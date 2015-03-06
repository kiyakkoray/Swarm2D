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
using System.IO.Compression;
using System.IO;
using Swarm2D.Library;
using Swarm2D.Network;

namespace Swarm2D.Network.TCPDriver
{
    internal class SocketWriter
    {
        private NetworkMessage _currentMessage;

        internal Socket Socket { get; set; }

        protected SocketAsyncEventArgs EventArguments = new SocketAsyncEventArgs();

        private Queue<NetworkMessage> _messageQueue;

        private bool _currentlySending = false;

        private bool _noError = true;

        NetworkMessage _currentMessageToSend = null;

        internal SocketWriter(Socket socket)
        {
            _currentMessage = NetworkMessage.GetNetworkMessage();
            _currentMessage.BeginWrite();

            _messageQueue = new Queue<NetworkMessage>();

            Socket = socket;

            EventArguments.Completed += new EventHandler<SocketAsyncEventArgs>(OnEventCompleted);
        }

        //Network Thread
        internal bool Loop()
        {
            if (!_currentlySending)
            {
                lock (_messageQueue) // shall i use monitor.tryenter?
                {
                    if (_messageQueue.Count > 0)
                    {
                        _currentMessageToSend = _messageQueue.Dequeue();
                    }
                }

                if (_currentMessageToSend != null)
                {
                    _currentlySending = true;

                    EventArguments.SetBuffer(_currentMessageToSend.Buffer, 0, _currentMessageToSend.Size + NetworkMessage.MessageHeaderSize);

                    bool operationPending = Socket.SendAsync(EventArguments);

                    if (!operationPending)
                    {
                        //Debug.Log("operation pending hmm");
                        SendData();
                    }
                }
            }

            return _noError;
        }

        //Async or Network Thread
        void SendData()
        {
            //Debug.Log("operation result " + EventArguments.LastOperation + " " + EventArguments.SocketError);
            //Debug.Log("EventArguments.BytesTransferred " + EventArguments.BytesTransferred + " " + EventArguments.Offset);
            //Debug.Log(_buffer[0]);

            if (EventArguments.SocketError == SocketError.Success)
            {
                NetworkMessage.FreeNetworkMessage(_currentMessageToSend);
                _currentMessageToSend = null;
                _currentlySending = false;
            }
            else
            {
                _noError = false;
            }
        }

        //Async Thread
        void OnEventCompleted(object sender, SocketAsyncEventArgs e)
        {
            //Debug.Log("An event completed async on session " + EventArguments.LastOperation + " " + EventArguments.SocketError);

            SendData();
        }

        //User Thread
        private void AddMessageToQueue(NetworkMessage message)
        {
            //Debug.Log("Adding message to queue, message size: " + message.Size);

            lock (_messageQueue)
            {
                _messageQueue.Enqueue(message);
            }
        }

        //User Thread
        internal void AddEvent(byte[] eventData)
        {
            const int eventHeaderSize = 1;
            const int multipleEventHeaderSize = 2;

            if (_currentMessage.EmptySize >= eventData.Length + eventHeaderSize)
            {
                _currentMessage.WriteBool(false); //isMultiplePacket = false;

                for (int i = 0; i < eventData.Length; i++)
                {
                    _currentMessage.WriteByte(eventData[i]);
                }
            }
            else
            {
                if (_currentMessage.EmptySize < eventHeaderSize + multipleEventHeaderSize)
                {
                    BeginNewMessage();
                }

                _currentMessage.WriteBool(true);
                _currentMessage.WriteInt16((short)eventData.Length);

                int currentDataCursor = 0;

                while (currentDataCursor < eventData.Length)
                {
                    int dataLeftSize = eventData.Length - currentDataCursor;

                    if (_currentMessage.EmptySize == 0)
                    {
                        BeginNewMessage();
                    }

                    int dataSizeInCurrentPacket = dataLeftSize < _currentMessage.EmptySize ? dataLeftSize : _currentMessage.EmptySize;

                    for (int i = 0; i < dataSizeInCurrentPacket; i++)
                    {
                        _currentMessage.WriteByte(eventData[i + currentDataCursor]);
                    }

                    currentDataCursor += dataSizeInCurrentPacket;
                }
            }
        }

        //User Thread
        private void BeginNewMessage()
        {
            _currentMessage.EndWrite();
            AddMessageToQueue(_currentMessage);

            _currentMessage = NetworkMessage.GetNetworkMessage();
            _currentMessage.BeginWrite();
        }

        //User Thread
        internal void FinalizeAndSendMessage()
        {
            _currentMessage.EndWrite();

            bool currentMessageIsEmpty = _currentMessage.Size == 0;

            if (!currentMessageIsEmpty)
            {
                AddMessageToQueue(_currentMessage);
            }

            if (_messageQueue.Count > 1)
            {
                Debug.Log("Message queue count: " + _messageQueue.Count);
            }

            if (!currentMessageIsEmpty)
            {
                _currentMessage = NetworkMessage.GetNetworkMessage();
                _currentMessage.BeginWrite();
            }
        }
    }
}
