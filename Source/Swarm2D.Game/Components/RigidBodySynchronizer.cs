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
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Library;
using Swarm2D.Network;
using System.IO;
using Swarm2D.Engine.Multiplayer;

namespace Swarm2D.Game.Components
{
    class RigidBodySynchronizer : Component, INetworkSynchronizeHandler
    {
        private NetworkView _networkView;
        private SceneEntity _sceneEntity;
        private PhysicsObject _rigidBody;

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            if (_sceneEntity == null)
            {
                _sceneEntity = Entity.GetComponent<SceneEntity>();
            }

            if (_rigidBody == null)
            {
                _rigidBody = Entity.GetComponent<PhysicsObject>();
            }

            if (_networkView == null)
            {
                _networkView = Entity.GetComponent<NetworkView>();
            }

            _networkView.SynchronizeHandler = this;
        }

        void INetworkSynchronizeHandler.OnSerializeToNetwork(DataWriter writer)
        {
            writer.WriteVector2(_sceneEntity.LocalPosition);
            writer.WriteFloat(_sceneEntity.LocalRotation);
            writer.WriteVector2(_rigidBody.Velocity);
        }

        void INetworkSynchronizeHandler.OnDeserializeFromNetwork(DataReader reader)
        {
            _sceneEntity.LocalPosition = reader.ReadVector2();
            _sceneEntity.LocalRotation = reader.ReadFloat();
            _rigidBody.Velocity = reader.ReadVector2();
        }
    }
}
