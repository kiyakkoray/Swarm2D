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
using System.Net;
using System.Net.Sockets;
using Swarm2D.Library;
using Swarm2D.Network;

namespace Swarm2D.Network.TCPDriver
{
    internal class ClientSession : NetworkSession, IClientSession
    {
        internal SocketReader Reader { get; private set; }
        internal SocketWriter Writer { get; private set; }

        private IClientSessionHandler _handler;

        private bool _connected = false;
        private bool _connectionHandled = true;

        internal ClientSession(IClientSessionHandler handler)
        {
            _handler = handler;

            Reader = new SocketReader(Socket);
            Writer = new SocketWriter(Socket);
        }

        protected override bool SessionLoop()
        {
            bool result = Reader.Loop() && Writer.Loop();

            if (!result)
            {
                _connected = false;
                _connectionHandled = false;
            }

            return result;
        }

        public void Connect(string address, int port)
        {
            IPAddress[] ipAddresses = new IPAddress[] { IPAddress.Parse(address) };
            //IPAddress[] ipAddresses = Dns.GetHostAddresses(address);

            if (ipAddresses.Length > 0)
            {
                for (int i = 0; i < ipAddresses.Length; i++)
                {
                    try
                    {
                        IPEndPoint ipEndPoint = new IPEndPoint(ipAddresses[i], port);

                        EventArguments.RemoteEndPoint = ipEndPoint;

                        Socket.ConnectAsync(EventArguments);
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        protected override void OnSocketEventCompleted(object sender, SocketAsyncEventArgs eventArguments)
        {
            if (eventArguments.LastOperation == SocketAsyncOperation.Connect && eventArguments.SocketError == SocketError.Success)
            {
                SessionThread.Start();

                _connected = true;
                _connectionHandled = false;
            }
            else if (eventArguments.LastOperation == SocketAsyncOperation.Disconnect && eventArguments.SocketError == SocketError.Success)
            {
                _connected = false;
                _connectionHandled = false;
            }

            //Debug.Log("An event completed on session " + eventArguments.LastOperation + " " + eventArguments.SocketError);
        }

        bool INetworkNode.ProcessEvent()
        {
            return Reader.ProcessMessageFromQueue(_handler);
        }

        public void HandleConnectionEvents()
        {
            if (!_connectionHandled)
            {
                if (_connected)
                {
                    _handler.OnConnectedToServer();
                }
                else
                {
                    _handler.OnDisconnectedFromServer();
                }

                _connectionHandled = true;
            }
        }

        void INetworkNode.Close()
        {
            base.OnClose();

            _connected = false;
            _connectionHandled = false;
        }

        void INetworkNode.FlushMessage()
        {
            Writer.FinalizeAndSendMessage();
        }

        void INetworkNode.AddReliableEvent(byte[] eventData)
        {
            Writer.AddEvent(eventData);
        }

        void INetworkNode.AddUnreliableEvent(byte[] eventData)
        {
            Writer.AddEvent(eventData);
        }
    }
}
