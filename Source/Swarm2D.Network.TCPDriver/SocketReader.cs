/******************************************************************************
Copyright (c) 2015 Koray Kiyakoglu

http://www.swarm2d.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Swarm2D.Library;
using System.IO;
using Swarm2D.Network;

namespace Swarm2D.Network.TCPDriver
{
    internal class SocketReader
    {
        internal Socket Socket { get; set; }

        protected SocketAsyncEventArgs EventArguments = new SocketAsyncEventArgs();

        private NetworkMessage _currentNetworkMessage;

        private bool _currentlyReceiving = false;

        private int _currentlyReadData = -1;
        private short _currentMessageSize = 0;

        private Queue<NetworkMessage> _receivedMessages;

        private bool _currentlyReadingMultiplePacketEvent = false;
        private int _currentMultiplePacketEventSize = 0;
        private int _currentMultiplePacketEventCursor = 0;

        private byte[] _multiplePacketEvent;

        private bool _noError = true;

        internal SocketReader(Socket socket)
        {
            _receivedMessages = new Queue<NetworkMessage>();
            _multiplePacketEvent = new byte[32768];

            Socket = socket;
            EventArguments.Completed += new EventHandler<SocketAsyncEventArgs>(OnEventCompleted);

            RefreshBuffer();
        }

        private void RefreshBuffer()
        {
            _currentNetworkMessage = NetworkMessage.GetNetworkMessage();
            EventArguments.SetBuffer(_currentNetworkMessage.Buffer, 0, _currentNetworkMessage.Buffer.Length);
        }

        internal bool Loop()
        {
            if (!_currentlyReceiving)
            {
                //Debug.Log("Beginning to receive");

                if (_currentlyReadData == -1)
                {
                    _currentlyReadData = 0;
                    _currentMessageSize = 0;
                    EventArguments.SetBuffer(0, NetworkMessage.MessageHeaderSize);
                }

                _currentlyReceiving = true;

                bool operationPending = Socket.ReceiveAsync(EventArguments);

                if (!operationPending)
                {
                    //Debug.Log("operation pending on receive");
                    ReceiveData();
                }
            }

            return _noError;
        }

        void ReceiveData()
        {
            //Debug.Log("operation result " + EventArguments.LastOperation + " " + EventArguments.SocketError);
            //Debug.Log("EventArguments.BytesTransferred " + EventArguments.BytesTransferred + " " + EventArguments.Offset);

            if (EventArguments.SocketError == SocketError.Success)
            {
                _currentlyReadData += EventArguments.BytesTransferred;

                if (_currentlyReadData < NetworkMessage.MessageHeaderSize)
                {
                    EventArguments.SetBuffer(_currentlyReadData, NetworkMessage.MessageHeaderSize - _currentlyReadData);
                    //need to wait message size
                }
                else if (_currentlyReadData == NetworkMessage.MessageHeaderSize)
                {
                    _currentMessageSize = BitConverter.ToInt16(EventArguments.Buffer, 0);

                    EventArguments.SetBuffer(NetworkMessage.MessageHeaderSize, _currentMessageSize);

                    //Debug.Log("Message compressed size: " + _currentMessageSize);

                    if (_currentMessageSize == 0)
                    {
                        Debug.Log("OMFG");

                        _currentlyReadData = -1;
                        _currentlyReceiving = false;
                    }
                }
                else
                {
                    if (_currentlyReadData == _currentMessageSize + NetworkMessage.MessageHeaderSize)
                    {
                        //Message Received

                        _currentNetworkMessage.PrepareMessage(_currentMessageSize + NetworkMessage.MessageHeaderSize);

                        var newlyReceivedMessage = _currentNetworkMessage;
                        RefreshBuffer();

                        lock (_receivedMessages)
                        {
                            _receivedMessages.Enqueue(newlyReceivedMessage);
                        }

                        _currentlyReadData = -1;
                    }
                    else
                    {
                        EventArguments.SetBuffer(_currentlyReadData, _currentMessageSize + NetworkMessage.MessageHeaderSize - _currentlyReadData);
                    }
                }

                _currentlyReceiving = false;
            }
            else
            {
                _noError = false;
            }
        }

        void OnEventCompleted(object sender, SocketAsyncEventArgs e)
        {
            //Debug.Log("An event completed async on session " + EventArguments.LastOperation + " " + EventArguments.SocketError);

            ReceiveData();
        }

        //user thread
        internal bool ProcessMessageFromQueue(INetworkSessionHandler networkSessionHandler)
        {
            NetworkMessage networkMessage = null;

            lock (_receivedMessages)
            {
                if (_receivedMessages.Count > 0)
                {
                    networkMessage = _receivedMessages.Dequeue();
                }
            }

            if (networkMessage != null)
            {
                networkMessage.BeginRead();
                ReadAndProcessMessage(networkMessage, networkSessionHandler);
                NetworkMessage.FreeNetworkMessage(networkMessage);

                return true;
            }

            return false;
        }

        //user thread
        protected void ReadAndProcessMessage(NetworkMessage message, INetworkSessionHandler networkSessionHandler)
        {
            while (message.Cursor < message.Size)
            {
                if (!_currentlyReadingMultiplePacketEvent)
                {
                    bool isMultiplePacket = message.ReadBool();

                    if (!isMultiplePacket)
                    {
                        networkSessionHandler.ProcessEvent(message);
                    }
                    else
                    {
                        _currentlyReadingMultiplePacketEvent = true;
                        _currentMultiplePacketEventSize = message.ReadInt16();
                        _currentMultiplePacketEventCursor = 0;

                        if (_currentMultiplePacketEventSize > _multiplePacketEvent.Length)
                        {
                            _multiplePacketEvent = new byte[_currentMultiplePacketEventSize];
                        }
                    }
                }
                else
                {
                    // total data left in current packet
                    int currentPacketLeftData = message.Size - message.Cursor;

                    // total data left to read for current event
                    int leftEventDataToRead = _currentMultiplePacketEventSize - _currentMultiplePacketEventCursor;

                    //total data that can be read from current packet for current event
                    int currentPacketEventDataToRead = leftEventDataToRead > currentPacketLeftData ? currentPacketLeftData : leftEventDataToRead;

                    for (int i = 0; i < currentPacketEventDataToRead; i++)
                    {
                        _multiplePacketEvent[i + _currentMultiplePacketEventCursor] = message.ReadByte();
                    }

                    _currentMultiplePacketEventCursor += currentPacketEventDataToRead;

                    if (_currentMultiplePacketEventCursor == _currentMultiplePacketEventSize)
                    {
                        _currentlyReadingMultiplePacketEvent = false;
                        _currentMultiplePacketEventSize = 0;
                        _currentMultiplePacketEventCursor = 0;

                        networkSessionHandler.ProcessEvent(new DataReader(_multiplePacketEvent));
                    }
                }
            }
        }
    }
}
