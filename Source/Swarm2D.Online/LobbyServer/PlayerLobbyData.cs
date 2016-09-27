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
using Swarm2D.Engine.Multiplayer;
using Swarm2D.Library;
using Swarm2D.Online.GameSchedulerServer;
using Swarm2D.Online.PlayerAccountServer;
using Swarm2D.Online.GameClient;

namespace Swarm2D.Online.LobbyServer
{
    class PlayerLobbyData : ClusterObjectProxy
    {
        public enum State
        {
            AtLobby,
            Disconnecting,
            NotAvailable, //meaning, its either offline or connected to another lobby server

            FindingGame,
            InGame
        }

        public string UserName { get; private set; }
        private ClusterObject _gameScheduler;

        public State CurrentState { get; private set; }

        public Peer Peer { get; private set; }

        private LobbyServerController _lobbyServerController;

        private bool _killingConnectionForPlayerDataServerRequest = false;

        protected override void OnAdded()
        {
            base.OnAdded();

            _lobbyServerController = Engine.RootEntity.GetComponent<LobbyServerController>();
        }

        public void Initialize(string userName, ClusterObject gameScheduler)
        {
            UserName = userName;
            _gameScheduler = gameScheduler;
        }

        [EntityMessageHandler(MessageType = typeof(KillPlayerConnectionMessage))]
        private void OnKillPlayerConnectionMessage(Message message)
        {
            Debug.Log("player connection kill request has arrived!");

            var killPlayerConnectionMessage = message as KillPlayerConnectionMessage;
            CoroutineManager.StartCoroutine(this, HandleKillPlayerConnection, killPlayerConnectionMessage);
        }

        private IEnumerator<CoroutineTask> HandleKillPlayerConnection(Coroutine coroutine)
        {
            var killPlayerConnectionMessage = coroutine.Parameter as KillPlayerConnectionMessage;

            PlayerPeer playerPeer = Peer.GetComponent<PlayerPeer>();
            PlayerLobbyData playerLobbyData = playerPeer.PlayerLobbyData;
            _killingConnectionForPlayerDataServerRequest = true;

            playerPeer.Entity.Destroy();

            while (playerLobbyData.CurrentState != State.NotAvailable)
            {
                yield return coroutine.AddTask<CoroutineTask>();
            }

            killPlayerConnectionMessage.Node.ResponseNetworkEntityMessageEvent(killPlayerConnectionMessage, new ResponseData());
        }

        internal Coroutine OnPlayerConnect(Peer peer)
        {
            return CoroutineManager.StartCoroutine(this, HandlePlayerConnection, peer);
        }

        private IEnumerator<CoroutineTask> HandlePlayerConnection(Coroutine coroutine)
        {
            Peer peer = coroutine.Parameter as Peer;
            LockObjectTask lockObjectTask = coroutine.AddTask<LockObjectTask>();
            lockObjectTask.Initialize(ClusterObject);

            yield return lockObjectTask;

            //login request from player data server
            var loginPlayerTask = coroutine.AddTask<ClusterObjectMessageTask>();
            loginPlayerTask.Initialize(ClusterObject, new LoginPlayerMessage());

            yield return loginPlayerTask;

            var loginResponse = loginPlayerTask.ResponseMessage as LoginPlayerResponseMessage;

            if (loginResponse.Successful)
            {
                CurrentState = State.AtLobby;
                Peer = peer;
            }

            //unlocking player account here,
            UnlockObjectTask unlockObjectTask = coroutine.AddTask<UnlockObjectTask>();
            unlockObjectTask.Initialize(ClusterObject);

            yield return unlockObjectTask;
        }

        internal void OnPlayerDisconnect()
        {
            CoroutineManager.StartCoroutine(this, HandlePlayerDisconnect);
        }

        private IEnumerator<CoroutineTask> HandlePlayerDisconnect(Coroutine coroutine)
        {
            State oldState = CurrentState;
            CurrentState = State.Disconnecting;

            switch (oldState)
            {
                case State.AtLobby:
                    {
                    }
                    break;
                case State.FindingGame:
                    {

                    }
                    break;
                case State.InGame:
                    {

                    }
                    break;
            }

            if (!_killingConnectionForPlayerDataServerRequest)
            {
                var informPlayerDisconnectTask = coroutine.AddTask<ClusterObjectMessageTask>();
                informPlayerDisconnectTask.Initialize(ClusterObject, new InformPlayerDisconnect());

                yield return informPlayerDisconnectTask;
            }

            CurrentState = State.NotAvailable;
            Peer = null;
        }

        public void OnFindGame()
        {
            Debug.Log("Player" + UserName + " wants to join a game");
            CoroutineManager.StartCoroutine(this, HandlePlayerFindGame);
        }

        private IEnumerator<CoroutineTask> HandlePlayerFindGame(Coroutine coroutine)
        {
            //locking player account here,
            {
                LockObjectTask lockObjectTask = coroutine.AddTask<LockObjectTask>();
                lockObjectTask.Initialize(ClusterObject);

                yield return lockObjectTask;
            }

            if (CurrentState == State.AtLobby)
            {
                CurrentState = State.FindingGame;

                var requestFindGameTask = coroutine.AddTask<ClusterObjectMessageTask>();
                requestFindGameTask.Initialize(_gameScheduler, new RequestFindGame(UserName));

                yield return requestFindGameTask;
            }

            //unlocking player account here,
            {
                UnlockObjectTask unlockObjectTask = coroutine.AddTask<UnlockObjectTask>();
                unlockObjectTask.Initialize(ClusterObject);

                yield return unlockObjectTask;
            }
        }

        public void OnCancelFindGame()
        {
            Debug.Log("Player" + UserName + " wants to cancel joining a game");
            CoroutineManager.StartCoroutine(this, HandlePlayerCancelFindGame);
        }

        private IEnumerator<CoroutineTask> HandlePlayerCancelFindGame(Coroutine coroutine)
        {
            //locking player account here,
            {
                LockObjectTask lockObjectTask = coroutine.AddTask<LockObjectTask>();
                lockObjectTask.Initialize(ClusterObject);

                yield return lockObjectTask;
            }

            if (CurrentState == State.FindingGame)
            {
                var requestFindGameTask = coroutine.AddTask<ClusterObjectMessageTask>();
                requestFindGameTask.Initialize(_gameScheduler, new CancelRequestFindGame(UserName));

                yield return requestFindGameTask;

                CurrentState = State.AtLobby;
            }

            //unlocking player account here,
            {
                UnlockObjectTask unlockObjectTask = coroutine.AddTask<UnlockObjectTask>();
                unlockObjectTask.Initialize(ClusterObject);

                yield return unlockObjectTask;
            }
        }

        [EntityMessageHandler(MessageType = typeof(InformGameFoundMessage))]
        private void OnFoundGameMessage(Message message)
        {
            if (CurrentState == State.FindingGame)
            {
                var informGameFoundMessage = message as InformGameFoundMessage;
                var rootNetworkView = _lobbyServerController.GetComponent<NetworkView>();

                rootNetworkView.NetworkEntityMessageEvent(Peer, new GameClient.Messages.FromLobbyServer.ToGameClient.GameFindResult());

                CurrentState = State.InGame;

                informGameFoundMessage.Node.ResponseNetworkEntityMessageEvent(informGameFoundMessage, new ResponseData());
            }
        }

        public void OnQuitGame()
        {
            Debug.Log("Player" + UserName + " wants to quit game");
            CoroutineManager.StartCoroutine(this, HandlePlayerQuitGame);
        }

        private IEnumerator<CoroutineTask> HandlePlayerQuitGame(Coroutine coroutine)
        {
            //locking player account here,
            {
                LockObjectTask lockObjectTask = coroutine.AddTask<LockObjectTask>();
                lockObjectTask.Initialize(ClusterObject);

                yield return lockObjectTask;
            }

            if (CurrentState == State.InGame)
            {
                var requestQuitGameTask = coroutine.AddTask<ClusterObjectMessageTask>();
                requestQuitGameTask.Initialize(_gameScheduler, new QuitGame(UserName));

                yield return requestQuitGameTask;

                CurrentState = State.AtLobby;
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
