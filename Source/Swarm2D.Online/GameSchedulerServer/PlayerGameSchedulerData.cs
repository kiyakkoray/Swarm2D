﻿/******************************************************************************
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
using Swarm2D.Online.LobbyServer;

namespace Swarm2D.Online.GameSchedulerServer
{
    public class PlayerGameSchedulerData : ClusterObjectProxy
    {
        public ClusterPeer Lobby { get; private set; }
        public string UserName { get; private set; }

        enum State
        {
            Idle,
            FindingGame,
            InGame,
        }

        private State _currentState = State.Idle;

        protected override void OnAdded()
        {
            base.OnAdded();
        }

        public void Initialize(ClusterPeer lobby, string userName)
        {
            Lobby = lobby;
            UserName = userName;
        }

        public void OnRequestFindGame()
        {
            _currentState = State.FindingGame;
        }

        public void OnCancelRequestFindGame()
        {
            _currentState = State.Idle;
        }


        public void OnQuitGame()
        {
            _currentState = State.Idle;
        }

        public void OnGameFound()
        {
            CoroutineManager.StartCoroutine(this, HandleGameFound);
        }

        private IEnumerator<CoroutineTask> HandleGameFound(Coroutine coroutine)
        {
            //locking player account here,
            {
                LockObjectTask lockObjectTask = coroutine.AddTask<LockObjectTask>();
                lockObjectTask.Initialize(ClusterObject);

                yield return lockObjectTask;
            }

            if (_currentState == State.FindingGame)
            {
                _currentState = State.InGame;

                var informLobbyObjectGameFoundTask = coroutine.AddTask<ClusterProxyMessageTask>();
                informLobbyObjectGameFoundTask.Initialize(ClusterObject, new InformGameFoundMessage(), Lobby);

                yield return informLobbyObjectGameFoundTask;
            }

            //unlocking player account here,
            {
                UnlockObjectTask unlockObjectTask = coroutine.AddTask<UnlockObjectTask>();
                unlockObjectTask.Initialize(ClusterObject);

                yield return unlockObjectTask;
            }
        }
    }
}
