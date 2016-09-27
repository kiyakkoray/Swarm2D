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
using System.Security.Policy;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.Multiplayer;
using Swarm2D.Engine.Multiplayer.Scene;
using Swarm2D.Library;

namespace Swarm2D.Test.FastMovingMultiplayerGameObjectTest
{
    public class ServerController : EngineComponent
    {
        public Role TestRole { get; internal set; }

        private TestController _testController;
        private NetworkController _networkController;
        private GameLogic _gameLogic;
        private Scene _scene;

        private Peer _clientTestPeer;
        private GameObject _clientTestAvatar;

        protected override void OnAdded()
        {
            base.OnAdded();

            _testController = GetComponent<TestController>();
            _networkController = GetComponent<NetworkController>();
        }

        [DomainMessageHandler(MessageType = typeof (UpdateMessage))]
        private void OnUpdate(Message message)
        {
            switch (TestRole.CurrentState)
            {
                case Role.State.WaitingServerToHost:
                    {
                        this.Host();
                        TestRole.CurrentState = Role.State.WaitingClientToConnect;
                    }
                    break;
                case Role.State.WaitingClientToConnect:
                    break;
                case Role.State.WaitingClientToFinishConnection:
                    break;
                case Role.State.WaitingServerToStartGame:
                    {
                        _gameLogic.StartGame();
                        TestRole.CurrentState = Role.State.WaitingClientToStartGame;
                    }
                    break;
                case Role.State.WaitingClientToStartGame:
                    break;
                case Role.State.WaitingServerToEnteringZone:
                    {
                        MakePeerToEnterScene();

                        TestRole.CurrentState = Role.State.WaitingFirstSynchronization;
                    }
                    break;
                case Role.State.WaitingServerToMoveAvatar1:
                    {
                        Debug.Log("Moving avatar to 300,10");
                        _clientTestAvatar.SceneEntity.LocalPosition = new Vector2(300, 10);
                        TestRole.CurrentState = Role.State.WaitingServerToMoveAvatar2;
                    }
                    break;
                case Role.State.WaitingServerToMoveAvatar2:
                    {
                        Debug.Log("Moving avatar to 540,10");
                        _clientTestAvatar.SceneEntity.LocalPosition = new Vector2(540, 10);
                        TestRole.CurrentState = Role.State.WaitingServerToMoveAvatar3;
                    }
                    break;
                case Role.State.WaitingServerToMoveAvatar3:
                    {
                        Debug.Log("Moving avatar to 2000,10");
                        _clientTestAvatar.SceneEntity.LocalPosition = new Vector2(2000, 10);
                        TestRole.CurrentState = Role.State.JustWaiting;
                    }
                    break;
            }
        }

        private void MakePeerToEnterScene()
        {
            _scene.GetComponent<GameSceneServer>().EnterPeerToScene(_clientTestPeer);
            var gameScenePeer = _clientTestPeer.GetComponent<GameScenePeer>();

            string name = "PeerAvatar" + _clientTestPeer.Id;
            NetworkID networkId = _networkController.GenerateNewNetworkId(_clientTestPeer.Id);

            _clientTestAvatar = InstantiateAvatar(_scene, name, networkId);
            gameScenePeer.Avatar = _clientTestAvatar;
        }

        public static GameObject InstantiateAvatar(Scene scene, string name, NetworkID networkId)
        {
            Debug.Log("Instantiating Avatar");

            Entity avatarEntity = scene.Engine.InstantiatePrefab("AvatarPrefab", scene);
            avatarEntity.GetComponent<SceneEntity>().LocalPosition = new Vector2(10, 10);

            NetworkView networkView = avatarEntity.GetComponent<NetworkView>();
            networkView.NetworkID = networkId;

            return avatarEntity.GetComponent<GameObject>();
        }

        private void Host()
        {
            //hosting
            {
                Debug.Log("Creating Server Session");

                _networkController.IsRoot = true;
                var session = _networkController.CreateServerSession(1538);
                _networkController.DefaultSession = session;
            }

            //game creation
            {
                Debug.Log("Creating Server Game");
                Entity gameLogicEntity = _testController.CreateGame();
                _gameLogic = gameLogicEntity.GetComponent<GameLogic>();
            }

            //scene creation
            {
                Debug.Log("Creating Server Scene");
                _scene = _gameLogic.CreateNewScene();
                NetworkView sceneNetworkView = _scene.AddComponent<NetworkView>();
                _scene.AddComponent<PhysicsWorld>();

                TestRole.SceneNetworkId = _networkController.GenerateNewNetworkId();
                sceneNetworkView.NetworkID = TestRole.SceneNetworkId;

                _scene.AddComponent<GameScene>();
                var test1SceneServer = _scene.AddComponent<SceneServer>();
                test1SceneServer.TestRole = TestRole;
            }
        }

        [GlobalMessageHandler(MessageType = typeof(PeerAuthorizedMessage))]
        private void OnPeerAuthorized(Message message)
        {
            Debug.Log("OnPeerAuthorized...");

            PeerAuthorizedMessage peerAuthorizedMessage = (PeerAuthorizedMessage)message;
            _clientTestPeer = peerAuthorizedMessage.Peer;
        }
    }
}
