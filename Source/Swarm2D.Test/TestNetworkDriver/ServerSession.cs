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
using Swarm2D.Network;

namespace Swarm2D.Test.TestNetworkDriver
{
    class ServerSession : IServerSession
    {
        internal TestNetworkDriver NetworkDriver { get; private set; }

        private IServerSessionHandler _handler;

        private List<ServerSideClientSession> _clients;
        private List<ServerSideClientSession> _disconnectedClients;

        internal ServerSession(TestNetworkDriver networkDriver, IServerSessionHandler handler)
        {
            _clients = new List<ServerSideClientSession>();
            _disconnectedClients = new List<ServerSideClientSession>();
            NetworkDriver = networkDriver;
            _handler = handler;
        }

        void INetworkSession.HandleConnectionEvents()
        {
            foreach (var serverSideClientSession in _clients)
            {
                if (!serverSideClientSession.ConnectionHandled)
                {
                    if (serverSideClientSession.Connected)
                    {
                        serverSideClientSession.TargetSession.Connected = true;
                        _handler.OnClientConnect(serverSideClientSession);
                    }
                    else
                    {
                        _disconnectedClients.Add(serverSideClientSession);
                        _handler.OnClientDisconnect(serverSideClientSession);                        
                    }

                    serverSideClientSession.ConnectionHandled = true;
                }
            }

            foreach (var serverSideClientSession in _disconnectedClients)
            {
                _clients.Remove(serverSideClientSession);
            }

            _disconnectedClients.Clear();
        }

        void IServerSession.Close()
        {
            foreach (var serverSideClientSession in _clients)
            {
                serverSideClientSession.Connected = false;
                serverSideClientSession.ConnectionHandled = false;
            }
        }

        internal ServerSideClientSession AcceptConnection(ClientSession clientSession)
        {
            ServerSideClientSession serversideSession = new ServerSideClientSession(this, _handler, clientSession);
            _clients.Add(serversideSession);
            
            return serversideSession;
        }
    }
}
