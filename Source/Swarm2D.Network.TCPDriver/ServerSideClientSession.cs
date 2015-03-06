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
using Swarm2D.Network;

namespace Swarm2D.Network.TCPDriver
{
    internal class ServerSideClientSession : IServerSideClientSession
    {
        internal SocketReader Reader { get; private set; }
        internal SocketWriter Writer { get; private set; }

        internal Socket Socket { get; private set; }

        internal LinkedListNode<ServerSideClientSession> ListNode { get; set; }

        private bool _closing;

        private ServerSession _serverSession;

        internal ServerSideClientSession(ServerSession serverSession, Socket socket)
        {
            _serverSession = serverSession;
            Socket = socket;

            Reader = new SocketReader(Socket);
            Writer = new SocketWriter(Socket);
        }

        internal bool Loop()
        {
            return !_closing && Socket.Connected && Reader.Loop() && Writer.Loop();
        }

        internal void DisposeSocket()
        {
            if (Socket != null)
            {
                Socket.Close();
                Socket = null;
            }
        }

        bool INetworkNode.ProcessEvent()
        {
            return Reader.ProcessMessageFromQueue(_serverSession.Handler);
        }

        void INetworkNode.Close()
        {
            _closing = true;
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
