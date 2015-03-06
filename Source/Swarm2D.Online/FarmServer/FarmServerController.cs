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
using Swarm2D.Cluster;
using Swarm2D.Cluster.Tasks;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.Online.FarmServer
{
    public class FarmServerController : Component
    {
        private CoroutineManager _coroutineManager;

        private ClusterNode _clusterNode;

        private ClusterObject _rootClusterObject;
        private ClusterObject _gameSchedulerObject;
        private ClusterObject _gameServerContainerObject;

        private NetworkController _networkController;
        private MultiplayerServerSession _gameServerSession;

        enum State
        {
            Idle,
            ConnectingToCluster,
            Ready
        }

        private State _state;
        private int _lastGameServerId = 0;

        protected override void OnAdded()
        {
            base.OnAdded();

            _networkController = Entity.GetComponent<NetworkController>();
            _clusterNode = Entity.GetComponent<ClusterNode>();
        }

        protected override void OnStart()
        {
            base.OnStart();

            _coroutineManager = Engine.FindComponent<CoroutineManager>();
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            if (_state == State.Idle)
            {
                Host();
            }
            else if (_state == State.Ready)
            {
            }
        }

        private void Host()
        {
            Debug.Log("Connecting to cluster");
            _state = State.ConnectingToCluster;
            _clusterNode.ConnectToCluster("127.0.0.1", Parameters.MainServerPortForCluster, "127.0.0.1", Parameters.FarmServerPortForCluster);
        }

        [GlobalMessageHandler(MessageType = typeof(ClusterInitializedMessage))]
        private void OnClusterConnectionInitialized(Message message)
        {
            _coroutineManager.StartCoroutine(this, HandleClusterNode);
        }

        private IEnumerator<CoroutineTask> HandleClusterNode(Coroutine coroutine)
        {
            _rootClusterObject = _clusterNode.RootClusterObject;

            var getGameSchedulerTask = coroutine.AddComponent<GetChildTask>();
            getGameSchedulerTask.Initialize(_rootClusterObject, "GameScheduler");
            yield return getGameSchedulerTask;

            _gameSchedulerObject = getGameSchedulerTask.Child;

            var getGameServerContainerTask = coroutine.AddComponent<GetChildTask>();
            getGameServerContainerTask.Initialize(_gameSchedulerObject, "GameServers");
            yield return getGameServerContainerTask;

            _gameServerContainerObject = getGameServerContainerTask.Child;

            _gameServerSession = _networkController.CreateServerSession(Parameters.FarmServerPortForGameServer);

            _state = State.Ready;
        }

        [GlobalMessageHandler(MessageType = typeof(PeerAuthorizedMessage))]
        private void OnGameServerConnected(Message message)
        {
            PeerAuthorizedMessage peerAuthorizedMessage = message as PeerAuthorizedMessage;

            if (_gameServerSession == peerAuthorizedMessage.Peer.ServerSession)
            {
                Debug.Log("a game server connected to farm server");
                _coroutineManager.StartCoroutine(this, HandleGameServerConnection);

            }
        }

        private IEnumerator<CoroutineTask> HandleGameServerConnection(Coroutine coroutine)
        {
            var createGameServerDataTask = coroutine.AddComponent<CreateChildObjectTask>();
            createGameServerDataTask.Initialize(_gameServerContainerObject, "GameServer" + _lastGameServerId);
            _lastGameServerId++;
            yield return createGameServerDataTask;

            var gameServerDataObject = createGameServerDataTask.CreatedChild;
        }
    }
}
