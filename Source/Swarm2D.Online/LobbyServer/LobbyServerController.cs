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
using Swarm2D.Online.GameClient;
using Debug = Swarm2D.Library.Debug;

namespace Swarm2D.Online.LobbyServer
{
    public class LobbyServerController : ClusterServerController
    {
        enum State
        {
            Idle,
            ConnectingToCluster,
            ConnectedToCluster,
            Ready
        }

        private State _state = State.Idle;

        private MultiplayerServerSession _gameClientSession;

        private ClusterObject _playerAccountManagerObject;
        private ClusterObject _gameSchedulerObject;

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            switch (_state)
            {
                case State.Idle:
                    {
                        _state = State.ConnectingToCluster;
                        ClusterNode.ConnectToCluster("127.0.0.1", Parameters.MainServerPortForCluster, "127.0.0.1", Parameters.LobbyServerPortForCluster);
                    }
                    break;
                case State.ConnectedToCluster:
                    {
                        _state = State.Ready;

                        Debug.Log("Creating to player lobby session...");
                        _gameClientSession = NetworkController.CreateServerSession(Parameters.LobbyServerPortForClient);
                        NetworkController.DefaultSession = _gameClientSession;
                    }
                    break;
                case State.Ready:
                    {

                    }
                    break;
            }
        }

        [GlobalMessageHandler(MessageType = typeof(ClusterInitializedMessage))]
        private void OnClusterConnectionInitialized(Message message)
        {
            CoroutineManager.StartCoroutine(this, HandleClusterNode);
        }

        private IEnumerator<CoroutineTask> HandleClusterNode(Coroutine coroutine)
        {
            {
                var getPlayerAccountManagerTask = coroutine.AddTask<GetChildTask>();
                getPlayerAccountManagerTask.Initialize(ClusterNode.RootClusterObject, "PlayerAccountManager");
                yield return getPlayerAccountManagerTask;

                _playerAccountManagerObject = getPlayerAccountManagerTask.Child;
            }

            {
                var getGameSchedulerTask = coroutine.AddTask<GetChildTask>();
                getGameSchedulerTask.Initialize(ClusterNode.RootClusterObject, "GameScheduler");
                yield return getGameSchedulerTask;

                _gameSchedulerObject = getGameSchedulerTask.Child;
            }

            _state = State.ConnectedToCluster;
        }

        [GlobalMessageHandler(MessageType = typeof(ConnectedToServerMessage))]
        private void OnConnectedToServerMessage(Message message)
        {
        }

        [GlobalMessageHandler(MessageType = typeof(PeerAuthorizedMessage))]
        private void OnClientAuthorized(Message message)
        {
            Debug.Log("a client conencted to lobby server");
            PeerAuthorizedMessage peerAuthorizedMessage = message as PeerAuthorizedMessage;

            CoroutineManager.StartCoroutine(this, HandlePlayerConnection, peerAuthorizedMessage.Peer);
        }

        private IEnumerator<CoroutineTask> HandlePlayerConnection(Coroutine coroutine)
        {
            Peer peer = coroutine.Parameter as Peer;

            NetworkEntityMessageTask requestInformationMessageTask =
                coroutine.CreateNetworkEntityMessageTask(new GameClient.Messages.FromLobbyServer.ToGameClient.RequestInformationMessage(), peer, NetworkView);

            yield return requestInformationMessageTask;

            var playerInformationResponse = requestInformationMessageTask.ResponseMessage as GameClient.Messages.FromGameClient.ToLobbyServer.PlayerInformationResponse;

            string playerName = playerInformationResponse.Name;

            {
                var getPlayerAccountTask = coroutine.AddTask<GetChildTask>();
                getPlayerAccountTask.Initialize(_playerAccountManagerObject, playerName);
                yield return getPlayerAccountTask;

                var playerAccount = getPlayerAccountTask.Child;
                PlayerLobbyData playerLobbyData = null;

                bool succesfullyLoggedIn = false;

                if (playerAccount != null)
                {
                    playerLobbyData = playerAccount.GetComponent<PlayerLobbyData>();

                    if (playerLobbyData == null)
                    {
                        playerLobbyData = playerAccount.AddComponent<PlayerLobbyData>();
                        playerLobbyData.Initialize(playerName, _gameSchedulerObject);
                    }

                    Coroutine playerConnectionCoroutine = playerLobbyData.OnPlayerConnect(peer);

                    WaitCoroutineTask waitCoroutineTask = coroutine.AddTask<WaitCoroutineTask>();
                    waitCoroutineTask.Initialize(playerConnectionCoroutine);

                    yield return waitCoroutineTask;

                    if (playerLobbyData.CurrentState == PlayerLobbyData.State.AtLobby && playerLobbyData.Peer == peer)
                    {
                        succesfullyLoggedIn = true;
                    }
                }

                if (succesfullyLoggedIn)
                {
                    PlayerPeer playerPeer = peer.AddComponent<PlayerPeer>();
                    playerPeer.PlayerLobbyData = playerLobbyData;
                    Debug.Log("player successfuly logged in");
                }
                else
                {
                    Debug.Log("couldn't logged in");
                    peer.Entity.Destroy();
                }
            }
        }

        [GlobalMessageHandler(MessageType = typeof(ClientDisconnectMessage))]
        private void OnClientDisconnect(Message message)
        {
            var clientDisconnectMessage = message as ClientDisconnectMessage;

            if (clientDisconnectMessage.Session == _gameClientSession)
            {
                Peer peer = clientDisconnectMessage.Peer;
                PlayerPeer playerPeer = peer.GetComponent<PlayerPeer>();
                PlayerLobbyData playerLobbyData = playerPeer.PlayerLobbyData;

                playerLobbyData.OnPlayerDisconnect();
            }
        }

        [EntityMessageHandler(MessageType = typeof(GameClient.Messages.FromGameClient.ToLobbyServer.FindGame))]
        private void OnPlayerFindGame(Message message)
        {
            var findGameMessage = message as GameClient.Messages.FromGameClient.ToLobbyServer.FindGame;

            Peer peer = findGameMessage.Node as Peer;
            PlayerPeer playerPeer = peer.GetComponent<PlayerPeer>();

            PlayerLobbyData playerLobbyData = playerPeer.PlayerLobbyData;
            playerLobbyData.OnFindGame();
        }

        [EntityMessageHandler(MessageType = typeof(GameClient.Messages.FromGameClient.ToLobbyServer.CancelFindGame))]
        private void OnPlayerCancelFindGame(Message message)
        {
            var cancelFindGameMessage = message as GameClient.Messages.FromGameClient.ToLobbyServer.CancelFindGame;

            Peer peer = cancelFindGameMessage.Node as Peer;
            PlayerPeer playerPeer = peer.GetComponent<PlayerPeer>();

            PlayerLobbyData playerLobbyData = playerPeer.PlayerLobbyData;
            playerLobbyData.OnCancelFindGame();
        }

        [EntityMessageHandler(MessageType = typeof(GameClient.Messages.FromGameClient.ToLobbyServer.QuitGame))]
        private void OnPlayerQuitGame(Message message)
        {
            var quitGameMessage = message as GameClient.Messages.FromGameClient.ToLobbyServer.QuitGame;

            Peer peer = quitGameMessage.Node as Peer;
            PlayerPeer playerPeer = peer.GetComponent<PlayerPeer>();

            PlayerLobbyData playerLobbyData = playerPeer.PlayerLobbyData;
            playerLobbyData.OnQuitGame();
        }
    }
}