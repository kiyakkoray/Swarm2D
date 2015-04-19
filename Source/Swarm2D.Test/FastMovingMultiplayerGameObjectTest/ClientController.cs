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
using Swarm2D.Engine.Multiplayer.Scene;
using Swarm2D.Library;

namespace Swarm2D.Test.FastMovingMultiplayerGameObjectTest
{
    public class ClientController : EngineComponent
    {
        public Role TestRole { get; internal set; }

        private TestController _testController;
        private NetworkController _networkController;
        private GameLogic _gameLogic;

        private int _state = 0;

        protected override void OnAdded()
        {
            base.OnAdded();

            _testController = GetComponent<TestController>();
            _networkController = GetComponent<NetworkController>();
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            switch (TestRole.CurrentState)
            {
                case Role.State.WaitingServerToHost:
                    break;
                case Role.State.WaitingClientToConnect:
                    {
                        this.Connect();
                        TestRole.CurrentState = Role.State.WaitingClientToFinishConnection;
                    }
                    break;
                case Role.State.WaitingClientToFinishConnection:
                    {
                        if (_networkController.PeerId != null)
                        {
                            TestRole.CurrentState = Role.State.WaitingServerToStartGame;
                        }
                    }
                    break;
                case Role.State.WaitingServerToStartGame:
                    break;
                case Role.State.WaitingClientToStartGame:
                    {
                        _gameLogic.StartGame();
                        TestRole.CurrentState = Role.State.WaitingServerToEnteringZone;
                    }
                    break;
                case Role.State.WaitingServerToEnteringZone:
                    break;
            }
        }

        public void Connect()
        {
            //connecting
            {
                Debug.Log("Creating Client Session");

                var session = _networkController.CreateClientSession("test", 1538);
                _networkController.ParentSession = session;
            }

            //game creation
            {
                Debug.Log("Creating Client Game");
                Entity gameLogicEntity = _testController.CreateGame();
                _gameLogic = gameLogicEntity.GetComponent<GameLogic>();
            }

            //scene creation
            {
                Debug.Log("Creating Client Scene");
                Scene zoneScene = _gameLogic.CreateNewScene();
                NetworkView sceneNetworkView = zoneScene.AddComponent<NetworkView>();
                zoneScene.AddComponent<PhysicsWorld>();

                sceneNetworkView.NetworkID = TestRole.SceneNetworkId;

                zoneScene.AddComponent<GameScene>();
            }
        }
    }
}
