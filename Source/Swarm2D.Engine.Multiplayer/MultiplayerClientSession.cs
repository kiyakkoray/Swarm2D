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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Library;
using Swarm2D.Network;

namespace Swarm2D.Engine.Multiplayer
{
    [Serializable]
    public class MultiplayerClientSession : MultiplayerSession, IClientSessionHandler, IMultiplayerNode
    {
        [NonSerialized]
        private Stopwatch _syncTimer;
        private long _lastSyncTime = 0;

        private MultiplayerNode _multiplayerNode;

        internal override INetworkSession Session
        {
            get
            {
                return ClientSession;
            }
        }

        private IClientSession ClientSession
        {
            get { return _multiplayerNode.Session as IClientSession; }
            set { _multiplayerNode.Session = value; }
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            IsClient = true;

            _multiplayerNode = new MultiplayerNode();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (ClientSession != null)
            {
                ClientSession.Close();
            }
        }

        internal void StartSession(INetworkDriver networkDriver, string address, int port)
        {
            NetworkDriver = networkDriver;
            ClientSession = NetworkDriver.CreateClientSession(this, address, port);
            _multiplayerNode.Session = ClientSession;

            _syncTimer = new Stopwatch();
            _syncTimer.Start();
        }

        internal override void AddRPCEvent(NetworkID networkId, string methodName, object[] parameters)
        {
            _multiplayerNode.AddRPCEvent(networkId, methodName, parameters);
        }

        internal override void AddNetworkEntityMessageEvent(NetworkID networkId, NetworkEntityMessage message)
        {
            _multiplayerNode.AddNetworkEntityMessageEvent(networkId, message);
        }

        void IMultiplayerNode.AddNetworkEntityMessageEvent(NetworkID networkId, NetworkEntityMessage message)
        {
            _multiplayerNode.AddNetworkEntityMessageEvent(networkId, message);
        }

        void IMultiplayerNode.AddNetworkEntityMessageEvent(NetworkID networkId, Entity requester, NetworkEntityMessage message)
        {
            HandleRequestNetworkEntityMessage(message, requester);

            _multiplayerNode.AddNetworkEntityMessageEvent(networkId, message);
        }

        public void ResponseNetworkEntityMessageEvent(NetworkEntityMessage requestMessage, ResponseData responseData)
        {
            _multiplayerNode.ResponseNetworkEntityMessageEvent(requestMessage, responseData);
        }

        internal void AddSynchronizeEvent(NetworkID networkId, byte[] data)
        {
            _multiplayerNode.AddSynchronizeEvent(networkId, data);
        }

        void INetworkSessionHandler.ProcessEvent(IDataReader reader)
        {
            this.ProcessEvent(reader);
        }

        internal override void UpdateRead()
        {
            ClientSession.HandleConnectionEvents();

            #region Read Section

            for (int i = 0; i < MaxReadEventsToProcess; i++)
            {
                if (!ClientSession.ProcessEvent())
                {
                    break;
                }
            }

            #endregion
        }

        internal override void UpdateWrite()
        {
            #region Write Section

            if (IsDefaultSession)
            {
                if (DoesSynchronizeEventsNeedsToBeSend())
                {
                    for (int i = 0; i < NetworkController.SynchronizableNetworkViews.Count; i++)
                    {
                        NetworkView networkView = NetworkController.SynchronizableNetworkViews[i];

                        if (networkView.IsMine)
                        {
                            DataWriter dataWriter = new DataWriter();

                            networkView.SynchronizeHandler.OnSerializeToNetwork(dataWriter);
                            Byte[] data = dataWriter.GetData();

                            AddSynchronizeEvent(networkView.NetworkID, data);
                        }
                    }
                }
            }

            _multiplayerNode.FinalizeAndSendMessage();

            #endregion
        }

        private int MaxReadEventsToProcess
        {
            get
            {
                if (NetworkController.SessionUpdateCheckType == NetworkUpdateCheckType.Time)
                {
                    return 40;
                }

                if (NetworkController.SessionUpdateCheckType == NetworkUpdateCheckType.Frame)
                {
                    return int.MaxValue;
                }

                return 0;
            }
        }

        private bool DoesSynchronizeEventsNeedsToBeSend()
        {
            if (NetworkController.SessionUpdateCheckType == NetworkUpdateCheckType.Time)
            {
                long currentTime = _syncTimer.ElapsedMilliseconds;

                if (currentTime - _lastSyncTime > 100)
                {
                    _lastSyncTime = currentTime;

                    return true;
                }

                return false;
            }

            if (NetworkController.SessionUpdateCheckType == NetworkUpdateCheckType.Frame)
            {
                if (Engine.CurrentFrame - _lastSyncTime >= NetworkController.SessionUpdatePeriod)
                {
                    _lastSyncTime = Engine.CurrentFrame;

                    return true;
                }

                return false;
            }

            return false;
        }

        void IClientSessionHandler.OnConnectedToServer()
        {
            if (NetworkController.PeerId != null)
            {
                Swarm2D.Library.Debug.Log("Sending peer id to server");
                Engine.RootEntity.GetComponent<NetworkView>().NetworkEntityMessageEvent(this, new RegisterPeerIdMessage(NetworkController.PeerId));
            }

            Engine.InvokeMessage(new ConnectedToServerMessage(this));
        }

        void IClientSessionHandler.OnDisconnectedFromServer()
        {
            Engine.InvokeMessage(new DisconnectedFromServerMessage(this));
        }

        protected override void OnEntityMessageEventRead(NetworkEntityMessage networkEntityMessage)
        {
            base.OnEntityMessageEventRead(networkEntityMessage);

            networkEntityMessage.Node = this;
        }
    }

    public class ConnectedToServerMessage : GlobalMessage
    {
        public MultiplayerClientSession Session { get; private set; }

        public ConnectedToServerMessage(MultiplayerClientSession session)
        {
            Session = session;
        }
    }

    public class DisconnectedFromServerMessage : GlobalMessage
    {
        public MultiplayerClientSession Session { get; private set; }

        public DisconnectedFromServerMessage(MultiplayerClientSession session)
        {
            Session = session;
        }
    }
}
