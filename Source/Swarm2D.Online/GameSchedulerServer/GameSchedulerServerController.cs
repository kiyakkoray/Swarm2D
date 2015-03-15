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
using Swarm2D.Library;
using Debug = Swarm2D.Library.Debug;

namespace Swarm2D.Online.GameSchedulerServer
{
    public class GameSchedulerServerController : ClusterServerController
    {
        enum State
        {
            Idle,
            ConnectingToCluster,
            Ready
        }

        private State _state = State.Idle;

        private GameScheduler _gameScheduler;

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            if (_state == State.Idle)
            {
                Host();
            }
            else if (_state == State.Ready)
            {
                _gameScheduler.Update();
            }
        }

        private void Host()
        {
            Debug.Log("Connecting to cluster");
            _state = State.ConnectingToCluster;
            ClusterNode.ConnectToCluster("127.0.0.1", Parameters.MainServerPortForCluster, "127.0.0.1", Parameters.GameSchedulerServerPortForCluster);
        }

        [GlobalMessageHandler(MessageType = typeof(ClusterInitializedMessage))]
        private void OnClusterConnectionInitialized(Message message)
        {
            CoroutineManager.StartCoroutine(this, HandleClusterNode);
        }

        private IEnumerator<CoroutineTask> HandleClusterNode(Coroutine coroutine)
        {
            var createGameSchedulerTask = coroutine.AddTask<CreateChildObjectTask>();
            createGameSchedulerTask.Initialize(ClusterNode.RootClusterObject, "GameScheduler");
            yield return createGameSchedulerTask;

            var gameSchedulerObject = createGameSchedulerTask.CreatedChild;
            _gameScheduler = gameSchedulerObject.AddComponent<GameScheduler>();

            _state = State.Ready;
        }
    }
}