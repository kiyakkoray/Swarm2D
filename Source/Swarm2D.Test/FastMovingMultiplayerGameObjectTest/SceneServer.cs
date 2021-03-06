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
using System.Security.Policy;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.Multiplayer.Scene;
using Swarm2D.Library;

namespace Swarm2D.Test.FastMovingMultiplayerGameObjectTest
{
    public class SceneServer : SceneEntityComponent
    {
        public Role TestRole { get; internal set; }

        private bool _firstSynchronization = true;

        [EntityMessageHandler(MessageType = typeof (OnSynchronizeGameSceneToPeerMessage))]
        private void OnSynchronizeSmallBlocksToPeer(Message message)
        {
            Debug.Log("Synchronizing scene to client");

            if (TestRole.CurrentState == Role.State.WaitingFirstSynchronization)
            {
                Debug.Assert(_firstSynchronization, "It's not first synchronization!");

                TestRole.CurrentState = Role.State.WaitingServerToMoveAvatar1;
                _firstSynchronization = false;
            }
        }

        [EntityMessageHandler(MessageType = typeof (SceneControllerUpdateMessage))]
        private void OnUpdate(Message message)
        {
        }
    }
}
