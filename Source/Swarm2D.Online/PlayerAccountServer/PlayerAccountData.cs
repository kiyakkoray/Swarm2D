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
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Library;
using Swarm2D.Online.LobbyServer;

namespace Swarm2D.Online.PlayerAccountServer
{
    public class PlayerAccountData : ClusterObjectLogic
    {
        public enum State
        {
            Offline,
            Online
        }

        public string UserName { get; set; }
        public string Password { get; set; }

        public State CurrentState { get; private set; }
        public Peer CurrentLobbySession { get; private set; }

        protected override void OnAdded()
        {
            base.OnAdded();
        }

        [EntityMessageHandler(MessageType = typeof(LoginPlayerMessage))]
        private void OnPlayerLoginRequest(Message message)
        {
            CoroutineManager.StartCoroutine(this, HandlePlayerLoginRequest, message);
        }

        private IEnumerator<CoroutineTask> HandlePlayerLoginRequest(Coroutine coroutine)
        {
            var loginPlayerMessage = coroutine.Parameter as LoginPlayerMessage;

            Peer peerOfLobby = loginPlayerMessage.Node as Peer;

            Debug.Log("Received player login request of: " + UserName);

            if (CurrentState == State.Offline)
            {
                CurrentState = State.Online;
                CurrentLobbySession = peerOfLobby;
                peerOfLobby.ResponseNetworkEntityMessageEvent(loginPlayerMessage, new LoginPlayerResponseMessage(true));
            }
            else if (CurrentState == State.Online)
            {
                var messageTask = coroutine.CreateNetworkEntityMessageTask(new KillPlayerConnectionMessage(),
                    CurrentLobbySession, GetComponent<NetworkView>());

                yield return messageTask;

                CurrentLobbySession = peerOfLobby;
                peerOfLobby.ResponseNetworkEntityMessageEvent(loginPlayerMessage, new LoginPlayerResponseMessage(true));
            }
        }

        [EntityMessageHandler(MessageType = typeof(InformPlayerDisconnect))]
        private void OnPlayerDisconnectInformation(Message message)
        {
            var informPlayerDisconnect = message as InformPlayerDisconnect;
            IMultiplayerNode peerOfLobby = informPlayerDisconnect.Node;

            Debug.Log("Player disconnect inform of " + UserName);

            CurrentState = State.Offline;
            CurrentLobbySession = null;

            peerOfLobby.ResponseNetworkEntityMessageEvent(informPlayerDisconnect, new ResponseData());
        }
    }
}
