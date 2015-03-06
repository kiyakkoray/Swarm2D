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
using System.Net;
using System.Threading;
using Swarm2D.Library;
using Swarm2D.Network;

namespace Swarm2D.Network.TCPDriver
{
    internal abstract class NetworkSession
    {
        internal Socket Socket { get; private set; }

        protected SocketAsyncEventArgs EventArguments { get; private set; }

        protected Thread SessionThread { get; private set; }

        private bool _closing = false;

        protected NetworkSession()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.NoDelay = true;
            EventArguments = new SocketAsyncEventArgs();

            EventArguments.Completed += new EventHandler<SocketAsyncEventArgs>(OnSocketEventCompleted);

            SessionThread = new Thread(Loop);
            SessionThread.Name = "Network Thread";
        }

        internal static ServerSession CreateServerSession(IServerSessionHandler handler, int port)
        {
            ServerSession session = new ServerSession(handler);

            session.Listen(port);

            return session;
        }

        internal static ClientSession CreateClientSession(IClientSessionHandler handler, string address, int port)
        {
            ClientSession session = new ClientSession(handler);

            session.Connect(address, port);

            return session;
        }

        protected abstract bool SessionLoop();

        private void Loop()
        {
            try
            {
                while (!_closing && SessionLoop())
                {
                    Thread.Sleep(1);
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log("Exception on Network Thread " + ex.Message + " " + ex.StackTrace);
            }

            Debug.Log("Closing network thread...");

            try
            {
                if (Socket != null)
                {
                    //Socket.Disconnect(false);
                    //Socket.Shutdown(SocketShutdown.Both);
                    Socket.Close();
                    Socket = null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log("exception on closing socket: " + ex);
            }

            Debug.Log("Network Thread is over");
        }

        protected abstract void OnSocketEventCompleted(object sender, SocketAsyncEventArgs eventArguments);

        internal void OnClose()
        {
            _closing = true;
        }
    }
}
