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
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.Multiplayer;

namespace Swarm2D.Cluster
{
    public class ClusterNode : EngineComponent
    {
        enum ClusterType
        {
            Root,
            Node,
            NotAvailable
        }

        private NetworkController _networkController;
        private CoroutineManager _coroutineManager;

        private MultiplayerClientSession _rootSession;
        private MultiplayerServerSession _hostSession;

        private Dictionary<NetworkID, ClusterPeer> _clusterPeers;
        private Dictionary<IMultiplayerNode, ClusterPeer> _clusterPeersWithNodes;

        public ClusterPeer RootClusterPeer { get; private set; }
        public ClusterPeer MyClusterPeer { get; private set; }

        private NetworkView _networkView;

        private string _hostAddress;
        private int _hostPort;

        private ClusterType _clusterType;
        private ClusterObjectManager _clusterObjectManager;

        protected override void OnStart()
        {
            base.OnStart();

            _networkController = GetComponent<NetworkController>();
            _coroutineManager = Engine.FindComponent<CoroutineManager>();

            _clusterPeers = new Dictionary<NetworkID, ClusterPeer>();
            _clusterPeersWithNodes = new Dictionary<IMultiplayerNode, ClusterPeer>();

            _networkView = GetComponent<NetworkView>();

            _clusterType = ClusterType.NotAvailable;

            Entity clusterObjectManagerEntity = CreateChildEntity("ClusterObjectManager");
            clusterObjectManagerEntity.AddComponent<NetworkView>();
            _clusterObjectManager = clusterObjectManagerEntity.AddComponent<ClusterObjectManager>();
        }

        public void HostCluster(string hostAdress, int hostPort)
        {
            _clusterType = ClusterType.Root;

            _hostAddress = hostAdress;
            _hostPort = hostPort;

            _networkController.IsRoot = true;
            _hostSession = _networkController.CreateServerSession(hostPort);

            NetworkView clusterObjectManagerNetworkView = _clusterObjectManager.GetComponent<NetworkView>();
            clusterObjectManagerNetworkView.NetworkID = _networkController.GenerateNewNetworkId();

            NetworkID rootClusterObjectNetworkId = _networkController.GenerateNewNetworkId(clusterObjectManagerNetworkView.NetworkID);

            MyClusterPeer = new ClusterPeer();
            RootClusterPeer = MyClusterPeer;

            RootClusterPeer.Port = _hostPort;
            RootClusterPeer.Address = _hostAddress;
            RootClusterPeer.Session = null;
            RootClusterPeer.Id = _networkController.PeerId;

            _clusterPeers.Add(RootClusterPeer.Id, RootClusterPeer);

            _clusterObjectManager.CreateRootObject(RootClusterPeer, rootClusterObjectNetworkId);
        }

        public void ConnectToCluster(string rootAddress, int rootPort, string hostAdress, int hostPort)
        {
            _clusterType = ClusterType.Node;

            _hostAddress = hostAdress;
            _hostPort = hostPort;

            _rootSession = _networkController.CreateClientSession(rootAddress, rootPort);
            _networkController.ParentSession = _rootSession;

            _hostSession = _networkController.CreateServerSession(hostPort);
            _networkController.DefaultSession = _hostSession;
        }

        //this message will be invoked on client node when client node will authorized from root node
        [GlobalMessageHandler(MessageType = typeof(AuthorizedFromServerMessage))]
        private void OnAuthorizedFromServerMessage(Message message)
        {
            var authorizedFromServerMessage = message as AuthorizedFromServerMessage;

            if (_clusterType == ClusterType.Node)
            {
                if (authorizedFromServerMessage.ServerNode == _rootSession)
                {
                    _coroutineManager.StartCoroutine(this, HandleClusterInitialization);
                }
            }
            else if (_clusterType == ClusterType.Root)
            {

            }
        }

        //OnAuthorizedFromServerMessage coroutine
        private IEnumerator<CoroutineTask> HandleClusterInitialization(Coroutine coroutine)
        {
            var requestClusterJoinTask = coroutine.CreateNetworkEntityMessageTask(
                new RequestClusterJoinMessage(_hostAddress, _hostPort), _rootSession, GetComponent<NetworkView>());

            yield return requestClusterJoinTask;

            var clusterInformation = requestClusterJoinTask.ResponseMessage as ClusterJoinResponse;

            _clusterObjectManager.GetComponent<NetworkView>().NetworkID = clusterInformation.ClusterObjectManagerId;

            for (int i = 0; i < clusterInformation.ClusterNodeInformations.Length; i++)
            {
                var clusterNodeInformation = clusterInformation.ClusterNodeInformations[i];

                ClusterPeer clusterPeer = new ClusterPeer();

                clusterPeer.Port = clusterNodeInformation.Port;
                clusterPeer.Address = clusterNodeInformation.Address;
                clusterPeer.Id = clusterNodeInformation.Id;

                _clusterPeers.Add(clusterNodeInformation.Id, clusterPeer);

                if (i == 0)
                {
                    RootClusterPeer = clusterPeer;
                    RootClusterPeer.Session = _rootSession;

                    _clusterObjectManager.CreateRootObject(RootClusterPeer, clusterInformation.RootClusterObjectNetworkId);
                }
                else
                {
                    clusterPeer.Session = _networkController.CreateClientSession(clusterNodeInformation.Address,
                        clusterNodeInformation.Port);
                }

                _clusterPeersWithNodes.Add(clusterPeer.Session, clusterPeer);
            }

            {
                MyClusterPeer = new ClusterPeer();

                MyClusterPeer.Port = _hostPort;
                MyClusterPeer.Address = _hostAddress;
                MyClusterPeer.Session = null;
                MyClusterPeer.Id = _networkController.PeerId;

                _clusterPeers.Add(_networkController.PeerId, MyClusterPeer);
            }

            Engine.InvokeMessage(new ClusterInitializedMessage());
        }

        [GlobalMessageHandler(MessageType = typeof(ConnectedToServerMessage))]
        private void OnConnectedToServer(Message message)
        {

        }

        //this message will be invoked on root when client sends this message to root node
        [EntityMessageHandler(MessageType = typeof(RequestClusterJoinMessage))]
        private void OnRequestClusterInformationMessage(Message message)
        {
            var requestClusterJoinMessage = message as RequestClusterJoinMessage;
            var peerNode = requestClusterJoinMessage.Node;

            peerNode.ResponseNetworkEntityMessageEvent
            (
                requestClusterJoinMessage,
                new ClusterJoinResponse
                (
                    _clusterObjectManager.GetComponent<NetworkView>().NetworkID,
                    _clusterObjectManager.RootClusterObject.GetComponent<NetworkView>().NetworkID,
                    GenerateClusterNodeInformations()
                )
            );

            ClusterPeer clusterPeer = new ClusterPeer();

            clusterPeer.Address = requestClusterJoinMessage.Address;
            clusterPeer.Port = requestClusterJoinMessage.Port;
            clusterPeer.Session = peerNode;
            clusterPeer.Id = requestClusterJoinMessage.Peer.Id;

            _clusterPeers.Add(clusterPeer.Id, clusterPeer);
            _clusterPeersWithNodes.Add(clusterPeer.Session, clusterPeer);
        }

        //this message will be invoked on older nodes when a new node connects to them
        [GlobalMessageHandler(MessageType = typeof(PeerAuthorizedMessage))]
        private void OnClientConnected(Message message)
        {
            var peerAuthorizedMessage = message as PeerAuthorizedMessage;

            if (_clusterType == ClusterType.Node &&
                peerAuthorizedMessage.Peer.ServerSession == _hostSession)
            {
                ClusterPeer clusterPeer = new ClusterPeer();

                clusterPeer.Address = "NA";
                clusterPeer.Port = -1;
                clusterPeer.Session = peerAuthorizedMessage.Peer;
                clusterPeer.Id = peerAuthorizedMessage.Peer.Id;

                _clusterPeers.Add(peerAuthorizedMessage.Peer.Id, clusterPeer);
                _clusterPeersWithNodes.Add(peerAuthorizedMessage.Peer, clusterPeer);
            }
        }

        private ClusterNodeInformation[] GenerateClusterNodeInformations()
        {
            ClusterNodeInformation[] clusterNodes = new ClusterNodeInformation[_clusterPeers.Count];

            int i = 0;

            foreach (var clusterPeer in _clusterPeers.Values)
            {
                var clusterNodeInformation = new ClusterNodeInformation();

                clusterNodeInformation.Address = clusterPeer.Address;
                clusterNodeInformation.Port = clusterPeer.Port;
                clusterNodeInformation.Id = clusterPeer.Id;

                clusterNodes[i] = clusterNodeInformation;

                i++;
            }

            return clusterNodes;
        }

        public ClusterPeer GetClusterPeer(NetworkID peerId)
        {
            return _clusterPeers[peerId];
        }

        public ClusterPeer GetClusterPeer(IMultiplayerNode peerNode)
        {
            return _clusterPeersWithNodes[peerNode];
        }

        public ClusterObject RootClusterObject
        {
            get
            {
                return _clusterObjectManager.RootClusterObject;
            }
        }
    }
}
