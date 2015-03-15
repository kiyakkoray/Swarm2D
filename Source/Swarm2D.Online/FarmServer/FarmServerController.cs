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
    public class FarmServerController : ClusterServerController
    {
        private ClusterObject _rootClusterObject;
        private ClusterObject _gameSchedulerObject;
        private ClusterObject _gameServerContainerObject;

        private MultiplayerServerSession _gameServerSession;

        enum State
        {
            Idle,
            ConnectingToCluster,
            Ready
        }

        private State _state;
        private int _lastGameServerId = 0;

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
            ClusterNode.ConnectToCluster("127.0.0.1", Parameters.MainServerPortForCluster, "127.0.0.1", Parameters.FarmServerPortForCluster);
        }

        [GlobalMessageHandler(MessageType = typeof(ClusterInitializedMessage))]
        private void OnClusterConnectionInitialized(Message message)
        {
            CoroutineManager.StartCoroutine(this, HandleClusterNode);
        }

        private IEnumerator<CoroutineTask> HandleClusterNode(Coroutine coroutine)
        {
            _rootClusterObject = ClusterNode.RootClusterObject;

            var getGameSchedulerTask = coroutine.AddTask<GetChildTask>();
            getGameSchedulerTask.Initialize(_rootClusterObject, "GameScheduler");
            yield return getGameSchedulerTask;

            _gameSchedulerObject = getGameSchedulerTask.Child;

            var getGameServerContainerTask = coroutine.AddTask<GetChildTask>();
            getGameServerContainerTask.Initialize(_gameSchedulerObject, "GameServers");
            yield return getGameServerContainerTask;

            _gameServerContainerObject = getGameServerContainerTask.Child;

            _gameServerSession = NetworkController.CreateServerSession(Parameters.FarmServerPortForGameServer);

            _state = State.Ready;
        }

        [GlobalMessageHandler(MessageType = typeof(PeerAuthorizedMessage))]
        private void OnGameServerConnected(Message message)
        {
            PeerAuthorizedMessage peerAuthorizedMessage = message as PeerAuthorizedMessage;

            if (_gameServerSession == peerAuthorizedMessage.Peer.ServerSession)
            {
                Debug.Log("a game server connected to farm server");
                CoroutineManager.StartCoroutine(this, HandleGameServerConnection);
            }
        }

        private IEnumerator<CoroutineTask> HandleGameServerConnection(Coroutine coroutine)
        {
            var createGameServerDataTask = coroutine.AddTask<CreateChildObjectTask>();
            createGameServerDataTask.Initialize(_gameServerContainerObject, "GameServer" + _lastGameServerId);
            _lastGameServerId++;
            yield return createGameServerDataTask;

            var gameServerDataObject = createGameServerDataTask.CreatedChild;
        }
    }
}
