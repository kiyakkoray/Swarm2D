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
    public class TestNetworkDriver : INetworkDriver
    {
        private Dictionary<int, ServerSession> _serverSessions;

        public TestNetworkDriver()
        {
            _serverSessions = new Dictionary<int, ServerSession>();
        }

        IServerSession INetworkDriver.CreateServerSession(IServerSessionHandler handler, int port)
        {
            var serverSession =  new ServerSession(this, handler);

            _serverSessions.Add(port, serverSession);

            return serverSession;
        }

        IClientSession INetworkDriver.CreateClientSession(IClientSessionHandler handler, string address, int port)
        {
            ServerSession serverSession = _serverSessions[port];

            var clientSession = new ClientSession(this, serverSession, handler);

            return clientSession;
        }
    }
}
