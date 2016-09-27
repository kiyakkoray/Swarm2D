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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Swarm2D.Cluster;
using Swarm2D.Cluster.Tasks;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.Multiplayer;
using Swarm2D.Library;
using Debug = Swarm2D.Library.Debug;

namespace Swarm2D.Online.PlayerAccountServer
{
    public class PlayerAccountServerController : ClusterServerController
    {
        private bool _initialized = false;

        private ClusterObject _playerAccountManagerObjeect;
        private ClusterObject _gameSchedulerObject;

        private IEnumerator<CoroutineTask> HandleClusterNode(Coroutine coroutine)
        {
            {
                var getGameSchedulerTask = coroutine.AddTask<GetChildTask>();
                getGameSchedulerTask.Initialize(ClusterNode.RootClusterObject, "GameScheduler");
                yield return getGameSchedulerTask;

                _gameSchedulerObject = getGameSchedulerTask.Child;
            }

            {
                var createPlayerAccountManagerTask = coroutine.AddTask<CreateChildObjectTask>();
                createPlayerAccountManagerTask.Initialize(ClusterNode.RootClusterObject, "PlayerAccountManager");
                yield return createPlayerAccountManagerTask;

                _playerAccountManagerObjeect = createPlayerAccountManagerTask.CreatedChild;
            }

            Debug.Log("Loading user accounts...");

            const int playerCount = 512;

            for (int i = 0; i < playerCount; i++)
            {
                var createPlayerAccountTask = coroutine.AddTask<CreateChildObjectTask>();
                createPlayerAccountTask.Initialize(_playerAccountManagerObjeect, "player" + i);
                yield return createPlayerAccountTask;

                Entity playerAccountEntity = createPlayerAccountTask.CreatedChild.Entity;

                PlayerAccountData playerAccount = playerAccountEntity.AddComponent<PlayerAccountData>();
                playerAccount.UserName = "player" + i;
            }

            Debug.Log("finished loading user accounts!");
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            if (!_initialized)
            {
                _initialized = true;
                Host();
            }
        }

        private void Host()
        {
            Debug.Log("Creating Player Data Server Session");

            ClusterNode.ConnectToCluster("127.0.0.1", Parameters.MainServerPortForCluster, "127.0.0.1", Parameters.PlayerDataServerPortForCluster);

            //_networkController.ParentSession = _networkController.CreateClientSession("127.0.0.1", Parameters.MainServerPort);
            //_networkController.DefaultSession = _networkController.CreateServerSession(Parameters.PlayerDataServerPort);
        }

        [GlobalMessageHandler(MessageType = typeof(ClusterInitializedMessage))]
        private void OnClusterConnectionInitialized(Message message)
        {
            CoroutineManager.StartCoroutine(this, HandleClusterNode);
        }

        [GlobalMessageHandler(MessageType = typeof(ClientDisconnectMessage))]
        private void OnClientDisconnect(Message message)
        {
        }
    }
}