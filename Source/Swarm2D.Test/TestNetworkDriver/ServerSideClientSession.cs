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
using Swarm2D.Library;
using Swarm2D.Network;

namespace Swarm2D.Test.TestNetworkDriver
{
    class ServerSideClientSession : IServerSideClientSession
    {
        internal ClientSession TargetSession { get; private set; }

        private ServerSession _serverSession;
        private IServerSessionHandler _handler;

        private Queue<byte[]> _messagesToBeSent;
        private Queue<byte[]> _receivedMessages;

        internal bool ConnectionHandled { get; set; }
        internal bool Connected { get; set; }

        internal ServerSideClientSession(ServerSession serverSession, IServerSessionHandler handler, ClientSession clientSession)
        {
            _serverSession = serverSession;
            _handler = handler;
            _messagesToBeSent = new Queue<byte[]>();
            _receivedMessages = new Queue<byte[]>();

            Connected = true;
            TargetSession = clientSession;
        }

        bool INetworkNode.ProcessEvent()
        {
            if (_receivedMessages.Count > 0)
            {
                byte[] message = _receivedMessages.Dequeue();
                _handler.ProcessEvent(new DataReader(message));

                return true;
            }

            return false;
        }

        void INetworkNode.Close()
        {
            Connected = false;
            ConnectionHandled = false;

            TargetSession.Connected = false;
            TargetSession.ConnectionHandled = false;
        }

        void INetworkNode.FlushMessage()
        {
            if (TargetSession != null)
            {
                while (_messagesToBeSent.Count > 0)
                {
                    TargetSession.InsertMessage(_messagesToBeSent.Dequeue());
                }
            }
        }

        void INetworkNode.AddReliableEvent(byte[] eventData)
        {
            _messagesToBeSent.Enqueue(eventData);
        }

        void INetworkNode.AddUnreliableEvent(byte[] eventData)
        {
            _messagesToBeSent.Enqueue(eventData);
        }

        internal void InsertMessage(byte[] message)
        {
            _receivedMessages.Enqueue(message);
        }
    }
}
