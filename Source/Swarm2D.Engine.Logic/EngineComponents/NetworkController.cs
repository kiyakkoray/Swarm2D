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
using System.Threading;
using Swarm2D.Engine.Core;
using Swarm2D.Library;
using System.Diagnostics;
using Swarm2D.Network;
using Swarm2D.Network.TCPDriver;

namespace Swarm2D.Engine.Logic
{
    public class NetworkController : EngineComponent
    {
        public NetworkUpdateCheckType SessionUpdateCheckType { get; set; }
        
        public int SessionUpdatePeriod { get; set; } 

        private Stopwatch _timer;
        private long _lastNetworkUpdate = 0;
        private bool _sessionsNeedsUpdateOnThisFrame = false;

        private List<MultiplayerSession> _multiplayerSessions;

        private short _lastNetworkId = 1;

        private short _lastGivenPeerId = 0;

        protected override void OnAdded()
        {
            base.OnAdded();

            SessionUpdateCheckType = NetworkUpdateCheckType.Time;
            SessionUpdatePeriod = 50;
        }

        protected override void OnInitialize()
        {
            SynchronizableNetworkViews = new List<NetworkView>();

            _multiplayerSessions = new List<MultiplayerSession>();

            _timer = new Stopwatch();
            _timer.Start();

            if (DefaultNetworkDriver == null)
            {
                DefaultNetworkDriver = new TcpDriver();
            }
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            _sessionsNeedsUpdateOnThisFrame = DoesSessionsNeedsUpdate();
            
            if (_sessionsNeedsUpdateOnThisFrame)
            {
                for (int i = 0; i < _multiplayerSessions.Count; i++)
                {
                    MultiplayerSession session = _multiplayerSessions[i];

                    session.UpdateRead();
                }
            }
        }

        [EntityMessageHandler(MessageType = typeof(LateUpdateMessage))]
        private void OnLateUpdate(Message message)
        {
            if (_sessionsNeedsUpdateOnThisFrame)
            {
                for (int i = 0; i < _multiplayerSessions.Count; i++)
                {
                    MultiplayerSession session = _multiplayerSessions[i];

                    session.UpdateWrite();
                }

                _sessionsNeedsUpdateOnThisFrame = false;
            }
        }

        private bool DoesSessionsNeedsUpdate()
        {
            if (SessionUpdateCheckType == NetworkUpdateCheckType.Time)
            {
                long elapsedMiliseconds = _timer.ElapsedMilliseconds;

                if (elapsedMiliseconds - _lastNetworkUpdate >= SessionUpdatePeriod)
                {
                    _lastNetworkUpdate = elapsedMiliseconds;

                    return true;
                }

                return false;
            }

            if (SessionUpdateCheckType == NetworkUpdateCheckType.Frame)
            {
                if (Engine.CurrentFrame - _lastNetworkUpdate >= SessionUpdatePeriod)
                {
                    _lastNetworkUpdate = Engine.CurrentFrame;

                    return true;
                }

                return false;
            }

            return false;
        }

        public NetworkID GenerateNewNetworkId()
        {
            return GenerateNewNetworkId(PeerId);
        }

        public NetworkID GenerateNewNetworkId(NetworkID ownerPeerId) //TODO: possible intersection with peer owner
        {
            NetworkID networkId = NetworkID.GenerateNewNetworkId(ownerPeerId, _lastNetworkId);

            _lastNetworkId++;

            return networkId;
        }

        public NetworkID GenerateNewNetworkId(NetworkID parent, short id)
        {
            NetworkID networkId = NetworkID.GenerateNewNetworkId(parent, id);

            return networkId;
        }

        public MultiplayerServerSession CreateServerSession(int port)
        {
            return CreateServerSession(DefaultNetworkDriver, port);
        }

        public MultiplayerServerSession CreateServerSession(INetworkDriver driver, int port)
        {
            Entity multiplayerServerSessionEntity = Entity.CreateChildEntity("MultiplayerServerSession");
            MultiplayerServerSession session = multiplayerServerSessionEntity.AddComponent<MultiplayerServerSession>();
            NetworkView networkView = GetComponent<NetworkView>();

            if (DefaultSession == null)
            {
                networkView.NetworkID = NetworkID.Root;
            }

            session.StartSession(driver, port);

            if (DefaultSession == null)
            {
                DefaultSession = session;
            }

            _multiplayerSessions.Add(session);

            return session;
        }

        public MultiplayerClientSession CreateClientSession(string address, int port)
        {
            return CreateClientSession(DefaultNetworkDriver, address, port);
        }

        public MultiplayerClientSession CreateClientSession(INetworkDriver driver, string address, int port)
        {
            Entity multiplayerClientSessionEntity = Entity.CreateChildEntity("MultiplayerClientSession");
            MultiplayerClientSession session = multiplayerClientSessionEntity.AddComponent<MultiplayerClientSession>();

            NetworkView networkView = GetComponent<NetworkView>();

            if (DefaultSession == null)
            {
                networkView.NetworkID = NetworkID.Root;
            }

            session.StartSession(driver, address, port);

            if (DefaultSession == null)
            {
                DefaultSession = session;
            }

            _multiplayerSessions.Add(session);

            return session;
        }

        public NetworkView FindNetworkView(NetworkID networkId)
        {
            NetworkView rootNetworkView = GetComponent<NetworkView>();

            if (networkId.Type == NetworkID.IDType.Root)
            {
                return rootNetworkView;
            }

            return rootNetworkView.FindNetworkViewInChildren(networkId);
        }

        public MultiplayerSession DefaultSession { get; set; }

        private MultiplayerClientSession _parentSession;

        public MultiplayerClientSession ParentSession
        {
            get
            {
                return _parentSession;
            }
            set
            {
                if (value != null)
                {
                    Swarm2D.Library.Debug.Assert(_parentSession == null, "parent session already assigned");
                    _parentSession = value;

                    NetworkView networkView = GetComponent<NetworkView>();

                    networkView.NetworkEntityMessageEvent(_parentSession, new RequestPeerIdMessage());
                }
                else
                {
                    _parentSession = null;
                    PeerId = null;
                }
            }
        }

        internal List<NetworkView> SynchronizableNetworkViews { get; private set; }

        [EntityMessageHandler(MessageType = typeof(RegisterPeerIdMessage))]
        private void RegisterPeerId(Message message)
        {
            Swarm2D.Library.Debug.Log("peer id registering...");
            RegisterPeerIdMessage registerPeerIdMessage = message as RegisterPeerIdMessage;

            registerPeerIdMessage.Peer.Id = registerPeerIdMessage.NetworkID;

            NetworkView rootNetworkView = GetComponent<NetworkView>();

            rootNetworkView.NetworkEntityMessageEvent(registerPeerIdMessage.Peer, new ResponsePeerIdMessage(registerPeerIdMessage.Peer.Id));

            PeerAuthorizedMessage peerAuthorizedMessage = new PeerAuthorizedMessage();
            peerAuthorizedMessage.Peer = registerPeerIdMessage.Peer;

            Engine.InvokeMessage(peerAuthorizedMessage);

            Swarm2D.Library.Debug.Log("peer id registered...");
        }

        [EntityMessageHandler(MessageType = typeof(RequestPeerIdMessage))]
        private void RequestPeerId(Message message)
        {
            Swarm2D.Library.Debug.Log("peer id requested");
            RequestPeerIdMessage requestPeerIdMessage = message as RequestPeerIdMessage;

            NetworkView rootNetworkView = GetComponent<NetworkView>();

            _lastGivenPeerId++;
            short newPeerId = _lastGivenPeerId;

            NetworkID childrenBaseId = NetworkID.GenerateNewNetworkId(PeerId, 0);
            requestPeerIdMessage.Peer.Id = NetworkID.GenerateNewNetworkId(childrenBaseId, newPeerId);

            rootNetworkView.NetworkEntityMessageEvent(requestPeerIdMessage.Peer, new ResponsePeerIdMessage(requestPeerIdMessage.Peer.Id));

            PeerAuthorizedMessage peerAuthorizedMessage = new PeerAuthorizedMessage();
            peerAuthorizedMessage.Peer = requestPeerIdMessage.Peer;

            Engine.InvokeMessage(peerAuthorizedMessage);
        }

        [EntityMessageHandler(MessageType = typeof(ResponsePeerIdMessage))]
        private void ResponsePeerId(Message message)
        {
            Swarm2D.Library.Debug.Log("peer id responsed");
            ResponsePeerIdMessage responsePeerIdMessage = message as ResponsePeerIdMessage;

            PeerId = responsePeerIdMessage.NetworkID;

            Engine.InvokeMessage(new AuthorizedFromServerMessage(responsePeerIdMessage.Node));
        }

        public NetworkReference<T> CreateNetworkReference<T>(NetworkID id) where T : Component
        {
            return new NetworkReference<T>(this, id);
        }

        public NetworkReference<T> CreateNetworkReference<T>(T component) where T : Component
        {
            return new NetworkReference<T>(this, component.GetComponent<NetworkView>().NetworkID);
        }

        public NetworkID PeerId { get; private set; }

        private bool _isRoot = false;

        public bool IsRoot
        {
            get
            {
                return _isRoot;
            }
            set
            {
                _isRoot = value;

                if (_isRoot)
                {
                    PeerId = NetworkID.Root;
                }
            }
        }

        public INetworkDriver DefaultNetworkDriver { get; set; }
    }

    public enum NetworkUpdateCheckType
    {
        Time,
        Frame
    }

    public class AuthorizedFromServerMessage : GlobalMessage
    {
        public IMultiplayerNode ServerNode { get; private set; }

        public AuthorizedFromServerMessage(IMultiplayerNode serverNode)
        {
            ServerNode = serverNode;
        }
    }

    public class PeerAuthorizedMessage : GlobalMessage
    {
        public Peer Peer { get; set; }
    }

    public class RequestPeerIdMessage : ClientEntityMessage
    {
        public RequestPeerIdMessage()
        {
        }

        protected override void OnSerialize(IDataWriter writer)
        {
        }

        protected override void OnDeserialize(IDataReader reader)
        {
        }
    }

    public class RegisterPeerIdMessage : ClientEntityMessage
    {
        public NetworkID NetworkID { get; private set; }

        public RegisterPeerIdMessage()
        {
        }

        public RegisterPeerIdMessage(NetworkID networkId)
        {
            NetworkID = networkId;
        }

        protected override void OnSerialize(IDataWriter writer)
        {
            writer.WriteNetworkID(NetworkID);
        }

        protected override void OnDeserialize(IDataReader reader)
        {
            NetworkID = reader.ReadNetworkID();
        }
    }

    public class ResponsePeerIdMessage : NetworkEntityMessage
    {
        public NetworkID NetworkID { get; private set; }

        public ResponsePeerIdMessage()
        {
        }

        public ResponsePeerIdMessage(NetworkID networkId)
        {
            NetworkID = networkId;
        }

        protected override void OnSerialize(IDataWriter writer)
        {
            writer.WriteNetworkID(NetworkID);
        }

        protected override void OnDeserialize(IDataReader reader)
        {
            NetworkID = reader.ReadNetworkID();
        }
    }
}