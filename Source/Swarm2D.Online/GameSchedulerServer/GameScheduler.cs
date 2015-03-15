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
using Swarm2D.Cluster;
using Swarm2D.Cluster.Tasks;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Debug = Swarm2D.Library.Debug;

namespace Swarm2D.Online.GameSchedulerServer
{
    public class GameScheduler : ClusterObjectLogic
    {
        private ClusterObject _playerAccountManager;

        private List<PlayerGameSchedulerData> _awaitingPlayers;

        private Stopwatch _timer;
        private long _lastScheduleTime = 0;

        private bool _initialized;

        protected override void OnAdded()
        {
            base.OnAdded();

            _awaitingPlayers = new List<PlayerGameSchedulerData>();

            _timer = new Stopwatch();
            CoroutineManager.StartCoroutine(this, InitializeGameScheduler);
        }

        private IEnumerator<CoroutineTask> InitializeGameScheduler(Coroutine coroutine)
        {
            var createGameServerContainerTask = coroutine.AddTask<CreateChildObjectTask>();
            createGameServerContainerTask.Initialize(ClusterObject, "GameServers");
            yield return createGameServerContainerTask;

            _timer.Start();

            _initialized = true;
        }

        private void ScheduleGames()
        {
            if (_awaitingPlayers.Count > 0)
            {
                foreach (var playerGameSchedulerData in _awaitingPlayers)
                {
                    playerGameSchedulerData.OnGameFound();
                }

                _awaitingPlayers.Clear();
            }
        }

        //[DomainMessageHandler(MessageType = typeof (UpdateMessage))]
        //private void OnUpdate(Message message)
        public void Update()
        {
            if (_initialized)
            {
                if (_timer.ElapsedMilliseconds - _lastScheduleTime > 1000)
                {
                    ScheduleGames();

                    _lastScheduleTime = _timer.ElapsedMilliseconds;
                }
            }
        }

        [EntityMessageHandler(MessageType = typeof(RequestFindGame))]
        private void OnRequestFindGame(Message message)
        {
            var requestFindGameMessage = message as RequestFindGame;

            CoroutineManager.StartCoroutine(this, HandleRequestFindGame, requestFindGameMessage);
        }

        private IEnumerator<CoroutineTask> HandleRequestFindGame(Coroutine coroutine)
        {
            var requestFindGameMessage = coroutine.Parameter as RequestFindGame;
            IMultiplayerNode peerOfLobby = requestFindGameMessage.Node;

            string playerName = requestFindGameMessage.UserName;
            Debug.Log("Player " + playerName + " requested to find a game");

            if (_playerAccountManager == null)
            {
                var getPlayerAccountManagerTask = coroutine.AddTask<GetChildTask>();
                getPlayerAccountManagerTask.Initialize(RootClusterObject, "PlayerAccountManager");
                yield return getPlayerAccountManagerTask;

                _playerAccountManager = getPlayerAccountManagerTask.Child;
            }

            var getPlayerAccountTask = coroutine.AddTask<GetChildTask>();
            getPlayerAccountTask.Initialize(_playerAccountManager, playerName);
            yield return getPlayerAccountTask;

            var playerAccount = getPlayerAccountTask.Child;
            var playerGameSchedulerData = playerAccount.GetComponent<PlayerGameSchedulerData>();

            if (playerGameSchedulerData == null)
            {
                playerGameSchedulerData = playerAccount.AddComponent<PlayerGameSchedulerData>();
            }

            ClusterPeer lobbyClusterPeer = ClusterNode.GetClusterPeer(peerOfLobby);

            playerGameSchedulerData.Initialize(lobbyClusterPeer, playerName);
            playerGameSchedulerData.OnRequestFindGame();

            _awaitingPlayers.Add(playerGameSchedulerData);

            peerOfLobby.ResponseNetworkEntityMessageEvent(requestFindGameMessage, new ResponseData());
        }

        [EntityMessageHandler(MessageType = typeof(CancelRequestFindGame))]
        private void OnCancelRequestFindGame(Message message)
        {
            var cancelRequestFindGame = message as CancelRequestFindGame;

            CoroutineManager.StartCoroutine(this, HandleCancelRequestFindGame, cancelRequestFindGame);
        }

        private IEnumerator<CoroutineTask> HandleCancelRequestFindGame(Coroutine coroutine)
        {
            var cancelRequestFindGameMessage = coroutine.Parameter as CancelRequestFindGame;
            IMultiplayerNode peerOfLobby = cancelRequestFindGameMessage.Node;

            string playerName = cancelRequestFindGameMessage.UserName;
            Debug.Log("Player " + playerName + " requested to cancel finding game");

            if (_playerAccountManager == null)
            {
                var getPlayerAccountManagerTask = coroutine.AddTask<GetChildTask>();
                getPlayerAccountManagerTask.Initialize(RootClusterObject, "PlayerAccountManager");
                yield return getPlayerAccountManagerTask;

                _playerAccountManager = getPlayerAccountManagerTask.Child;
            }

            var getPlayerAccountTask = coroutine.AddTask<GetChildTask>();
            getPlayerAccountTask.Initialize(_playerAccountManager, playerName);
            yield return getPlayerAccountTask;

            var playerAccount = getPlayerAccountTask.Child;
            var playerGameSchedulerData = playerAccount.GetComponent<PlayerGameSchedulerData>();

            ClusterPeer lobbyClusterPeer = ClusterNode.GetClusterPeer(peerOfLobby);
            playerGameSchedulerData.Initialize(lobbyClusterPeer, playerName);

            playerGameSchedulerData.OnCancelRequestFindGame();

            peerOfLobby.ResponseNetworkEntityMessageEvent(cancelRequestFindGameMessage, new ResponseData());
        }

        [EntityMessageHandler(MessageType = typeof(QuitGame))]
        private void OnQuitGame(Message message)
        {
            var quitGame = message as QuitGame;

            CoroutineManager.StartCoroutine(this, HandleQuitGame, quitGame);
        }

        private IEnumerator<CoroutineTask> HandleQuitGame(Coroutine coroutine)
        {
            var quitGameMessage = coroutine.Parameter as QuitGame;
            IMultiplayerNode peerOfLobby = quitGameMessage.Node;

            string playerName = quitGameMessage.UserName;
            Debug.Log("Player " + playerName + " requested to quit game");

            var getPlayerAccountTask = coroutine.AddTask<GetChildTask>();
            getPlayerAccountTask.Initialize(_playerAccountManager, playerName);
            yield return getPlayerAccountTask;

            var playerAccount = getPlayerAccountTask.Child;
            var playerGameSchedulerData = playerAccount.GetComponent<PlayerGameSchedulerData>();

            ClusterPeer lobbyClusterPeer = ClusterNode.GetClusterPeer(peerOfLobby);
            playerGameSchedulerData.Initialize(lobbyClusterPeer, playerName);

            playerGameSchedulerData.OnQuitGame();

            peerOfLobby.ResponseNetworkEntityMessageEvent(quitGameMessage, new ResponseData());
        }
    }
}
