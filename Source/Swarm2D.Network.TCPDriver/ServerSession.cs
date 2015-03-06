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
using System.Threading;
using System.Net.Sockets;
using Swarm2D.Library;
using Swarm2D.Network;

namespace Swarm2D.Network.TCPDriver
{
    internal class ServerSession : NetworkSession, IServerSession
    {
        private const int MaxPendingConnectionCount = 16;

        private enum AcceptState
        {
            NotListening,
            Listening,
        }

        private AcceptState _currentAcceptState = AcceptState.NotListening;
        private object _acceptLock = new object();

        private LinkedList<ServerSideClientSession> _clients;

        private List<ServerSideClientSession> _newlyConnectedClients;
        private List<ServerSideClientSession> _disconnectedClients;

        private List<ServerSideClientSession> _handlerWaitingConnectedClients;
        private List<ServerSideClientSession> _handlerWaitingDisconnectedClients;

        internal IServerSessionHandler Handler { get; private set; }

        internal ServerSession(IServerSessionHandler handler)
        {
            Handler = handler;
        }

        internal void Listen(int port)
        {
            _clients = new LinkedList<ServerSideClientSession>();
            _newlyConnectedClients = new List<ServerSideClientSession>(128);
            _disconnectedClients = new List<ServerSideClientSession>(128);

            _handlerWaitingConnectedClients = new List<ServerSideClientSession>(128);
            _handlerWaitingDisconnectedClients = new List<ServerSideClientSession>(128);

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            Socket.Bind(ipEndPoint);

            Socket.Listen(MaxPendingConnectionCount);

            SessionThread.Start();
        }

        #region Listen Methods

        //Network Thread
        private void ListenLoop()
        {
            if (Monitor.TryEnter(_acceptLock)) //if we cant lock it lets skip
            {
                switch (_currentAcceptState)
                {
                    case AcceptState.NotListening:
                        {
                            bool operationPending = Socket.AcceptAsync(EventArguments);

                            //critical section,
                            //if async thread sets _currentAcceptState as NotListening while we are here
                            //we would not be able receive new connections because
                            //we are going to set _currentAcceptState as Listening while we are not listening
                            //thats why we need a lock beginning of this section!

                            Debug.Log("operationPending: " + operationPending);

                            if (!operationPending)
                            {
                                AcceptClient();
                            }
                            else
                            {
                                _currentAcceptState = AcceptState.Listening;
                            }
                        }
                        break;
                    case AcceptState.Listening:
                        break;
                    default:
                        break;
                }

                Monitor.Exit(_acceptLock);
            }
        }

        //Network and Async Thread
        private void AcceptClient()
        {
            if (EventArguments.LastOperation == SocketAsyncOperation.Accept)
            {
                if (EventArguments.SocketError == SocketError.Success)
                {
                    Socket clientSocket = EventArguments.AcceptSocket;

                    ServerSideClientSession clientSession = new ServerSideClientSession(this, clientSocket);

                    lock (_newlyConnectedClients)
                    {
                        _newlyConnectedClients.Add(clientSession);
                    }
                }
            }

            EventArguments.AcceptSocket = null;

            _currentAcceptState = AcceptState.NotListening;
        }

        //Async Thread
        protected override void OnSocketEventCompleted(object sender, SocketAsyncEventArgs eventArguments)
        {
            //Debug.Log("An event completed async on session " + eventArguments.LastOperation + " " + eventArguments.SocketError);

            lock (_acceptLock)
            {
                AcceptClient();
            }
        }

        #endregion

        //Network Thread
        protected override bool SessionLoop()
        {
            ListenLoop();

            foreach (ServerSideClientSession clientSession in _clients)
            {
                if (!clientSession.Loop())
                {
                    try
                    {
                        clientSession.DisposeSocket();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.Log("Exception on disposing socket: " + ex);
                    }

                    _disconnectedClients.Add(clientSession);

                    Debug.Log("A client disconnected");
                }
            }

            {
                for (int i = 0; i < _disconnectedClients.Count; i++)
                {
                    ServerSideClientSession clientSession = _disconnectedClients[i];
                    _clients.Remove(clientSession.ListNode);
                }

                if (_disconnectedClients.Count > 0)
                {
                    lock (_handlerWaitingDisconnectedClients)
                    {
                        _handlerWaitingDisconnectedClients.AddRange(_disconnectedClients);
                    }
                }

                _disconnectedClients.Clear();
            }

            lock (_newlyConnectedClients)
            {
                for (int i = 0; i < _newlyConnectedClients.Count; i++)
                {
                    ServerSideClientSession clientSession = _newlyConnectedClients[i];
                    clientSession.ListNode = _clients.AddLast(clientSession);
                }

                if (_newlyConnectedClients.Count > 0)
                {
                    lock (_handlerWaitingConnectedClients)
                    {
                        _handlerWaitingConnectedClients.AddRange(_newlyConnectedClients);
                    }
                }

                _newlyConnectedClients.Clear();
            }

            return true;
        }

        //User Thread
        void INetworkSession.HandleConnectionEvents()
        {
            lock (_handlerWaitingConnectedClients)
            {
                for (int i = 0; i < _handlerWaitingConnectedClients.Count; i++)
                {
                    ServerSideClientSession clientSession = _handlerWaitingConnectedClients[i];
                    Handler.OnClientConnect(clientSession);
                }

                _handlerWaitingConnectedClients.Clear();
            }

            lock (_handlerWaitingDisconnectedClients)
            {
                for (int i = 0; i < _handlerWaitingDisconnectedClients.Count; i++)
                {
                    ServerSideClientSession clientSession = _handlerWaitingDisconnectedClients[i];
                    Handler.OnClientDisconnect(clientSession);
                }

                _handlerWaitingDisconnectedClients.Clear();
            }
        }

        void IServerSession.Close()
        {
            base.OnClose();
        }
    }
}
