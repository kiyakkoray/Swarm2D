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
    public class Role : TestRole
    {
        public enum State
        {
            WaitingServerToHost,
            WaitingClientToConnect,
            WaitingClientToFinishConnection,
            WaitingServerToStartGame,
            WaitingClientToStartGame,
            WaitingServerToEnteringZone,
            WaitingFirstSynchronization,
            WaitingServerToMoveAvatar1,
            WaitingServerToMoveAvatar2,
            WaitingServerToMoveAvatar3,

            JustWaiting,
            Over
        }

        public State CurrentState { get; internal set; }

        private Engine.Core.Engine _serverDomain;
        private Engine.Core.Engine _clientDomain;

        private TestNetworkDriver.TestNetworkDriver _testNetworkDriver;

        public NetworkID SceneNetworkId { get; internal set; }

        public override void DoTest()
        {
            Console.WriteLine("################################");
            Console.WriteLine("#         Running Test1        #");
            Console.WriteLine("################################");

            _testNetworkDriver = new TestNetworkDriver.TestNetworkDriver();

            {
                _serverDomain = new Engine.Core.Engine(this, false);

                Entity rootEntity = _serverDomain.RootEntity;

                NetworkController networkController = rootEntity.AddComponent<NetworkController>();
                rootEntity.AddComponent<NetworkView>();
                rootEntity.AddComponent<TestController>();
                rootEntity.AddComponent<Controller>();
                var test1Server = rootEntity.AddComponent<ServerController>();
                test1Server.TestRole = this;

                networkController.DefaultNetworkDriver = _testNetworkDriver;
            }

            {
                _clientDomain = new Engine.Core.Engine(this, false);

                Entity rootEntity = _clientDomain.RootEntity;

                NetworkController networkController = rootEntity.AddComponent<NetworkController>();
                rootEntity.AddComponent<NetworkView>();
                rootEntity.AddComponent<TestController>();
                rootEntity.AddComponent<Controller>();
                var test1Client = rootEntity.AddComponent<ClientController>();
                test1Client.TestRole = this;

                networkController.DefaultNetworkDriver = _testNetworkDriver;
            }

            this.Initialize("Test", new FrameworkDomain[] { _serverDomain, _clientDomain });

            CurrentState = State.WaitingServerToHost;

            while (CurrentState != State.Over)
            {
                this.Update();
            }
        }
    }
}
