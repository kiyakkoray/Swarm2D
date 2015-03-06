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

namespace Swarm2D.Engine.Logic
{
    public class MultiplayerServerSession : MultiplayerSession, IServerSessionHandler, IEntityDomain
    {
        private Stopwatch _syncTimer;
        private long _lastSyncTime = 0;

        internal override INetworkSession Session
        {
            get
            {
                return _serverSession;
            }
        }

        private IServerSession _serverSession;

        private Dictionary<IServerSideClientSession, Peer> _serverPeers;

        private Peer _currentlyProcessingPeer = null;

        private EntityDomain _entityDomain;

        protected override void OnAdded()
        {
            base.OnAdded();

            IsServer = true;
            _serverPeers = new Dictionary<IServerSideClientSession, Peer>();

            _entityDomain = new EntityDomain(Entity);
            Entity.ChildDomain = this;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_serverSession != null)
            {
                _serverSession.Close();
            }
        }

        public void SendMessage(DomainMessage message)
        {
            _entityDomain.SendMessage(message);
        }

        internal void StartSession(INetworkDriver networkDriver, int port)
        {
            NetworkDriver = networkDriver;
            _serverSession = NetworkDriver.CreateServerSession(this, port);

            _syncTimer = new Stopwatch();
            _syncTimer.Start();
        }

        internal override void AddRPCEvent(NetworkID networkId, string methodName, object[] parameters)
        {
            foreach (Peer peer in _serverPeers.Values)
            {
                peer.AddRPCEvent(networkId, methodName, parameters);
            }
        }

        internal override void AddNetworkEntityMessageEvent(NetworkID networkId, NetworkEntityMessage message)
        {
            foreach (Peer peer in _serverPeers.Values)
            {
                IMultiplayerNode peerNode = peer;
                peerNode.AddNetworkEntityMessageEvent(networkId, message);
            }
        }

        internal void AddNetworkEntityMessageEvent(Peer peer, NetworkID networkId, Entity requester, NetworkEntityMessage message)
        {
            HandleRequestNetworkEntityMessage(message, requester);

            IMultiplayerNode peerNode = peer;
            peerNode.AddNetworkEntityMessageEvent(networkId, message);
        }

        void INetworkSessionHandler.ProcessEvent(IDataReader reader)
        {
            this.ProcessEvent(reader);
        }

        internal override void UpdateRead()
        {
            _serverSession.HandleConnectionEvents();

            _entityDomain.InitializeNonInitializedEntityComponents();

            #region Read Section

            foreach (Peer peer in _serverPeers.Values)
            {
                _currentlyProcessingPeer = peer;

                for (int i = 0; i < MaxReadEventsToProcess; i++)
                {
                    if (!peer.Session.ProcessEvent())
                    {
                        break;
                    }
                }

                _currentlyProcessingPeer = null;
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
                        DataWriter dataWriter = new DataWriter();

                        networkView.SynchronizeHandler.OnSerializeToNetwork(dataWriter);
                        Byte[] data = dataWriter.GetData();

                        IEnumerable<Peer> synchronizationPeers = _serverPeers.Values;

                        if (networkView.SynchronizationPeerGroup != null)
                        {
                            synchronizationPeers = networkView.SynchronizationPeerGroup.Peers;
                        }

                        foreach (Peer peer in synchronizationPeers)
                        {
                            if (peer.Id != null)
                            {
                                if (networkView.IsMine || !networkView.NetworkID.IsOwnedBy(peer.Id))
                                {
                                    peer.AddSynchronizeEvent(networkView.NetworkID, data);
                                }
                            }
                        }
                    }
                }
            }

            foreach (Peer peer in _serverPeers.Values)
            {
                peer.FinalizeAndSendMessage();
            }

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

        void IServerSessionHandler.OnClientConnect(IServerSideClientSession client)
        {
            Entity peerEntity = CreateChildEntity("PeerEntity");

            Peer peer = peerEntity.GetComponent<Peer>();
            peer.Session = client;

            _serverPeers.Add(client, peer);

            ClientConnectMessage clientConnectMessage = new ClientConnectMessage();
            clientConnectMessage.Peer = peer;
            clientConnectMessage.Session = this;

            NetworkController.Engine.InvokeMessage(clientConnectMessage);
        }

        void IEntityDomain.OnCreateChildEntity(Entity entity)
        {
            entity.Domain = this;

            Peer peer = entity.AddComponent<Peer>();
            peer.ServerSession = this;
        }

        void IServerSessionHandler.OnClientDisconnect(IServerSideClientSession client)
        {
            Peer peer = _serverPeers[client];
            _serverPeers.Remove(client);

            ClientDisconnectMessage clientDisconnectMessage = new ClientDisconnectMessage();
            clientDisconnectMessage.Peer = peer;
            clientDisconnectMessage.Session = this;

            NetworkController.Engine.InvokeMessage(clientDisconnectMessage);

            //peer entity may destroyed manually
            if (!peer.Entity.IsDestroyed)
            {
                peer.Entity.Destroy();
            }
        }

        void IEntityDomain.OnComponentCreated(Component component)
        {
            _entityDomain.OnComponentCreated(component);
        }

        void IEntityDomain.OnComponentDestroyed(Component component)
        {
            _entityDomain.OnComponentDestroyed(component);
        }

        Entity IEntityDomain.InstantiatePrefab(Entity prefab)
        {
            return _entityDomain.InstantiatePrefab(prefab);
        }

        protected override void OnEntityMessageEventRead(NetworkEntityMessage networkEntityMessage)
        {
            networkEntityMessage.Node = _currentlyProcessingPeer;

            if (networkEntityMessage is ClientEntityMessage)
            {
                ClientEntityMessage clientEntityMessage = networkEntityMessage as ClientEntityMessage;
                clientEntityMessage.Peer = _currentlyProcessingPeer;
            }
        }

        public Peer FindPeer(NetworkID peerId)
        {
            foreach (var peer in _serverPeers.Values)
            {
                if (peer.Id == peerId)
                {
                    return peer;
                }
            }

            return null;
        }

        void IEntityDomain.OnEntityParentChanged(Entity entity)
        {

        }
    }
}