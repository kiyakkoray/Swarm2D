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
using Swarm2D.Engine.Multiplayer;
using Swarm2D.Library;

namespace Swarm2D.Game
{
    [RequiresComponent(typeof(SceneEntity))]
    public class Character : Component
    {
        private NetworkView _networkView;
        private SceneEntity _sceneEntity;

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            if (_sceneEntity == null)
            {
                _sceneEntity = Entity.GetComponent<SceneEntity>();
            }

            if (_networkView == null)
            {
                _networkView = Entity.GetComponent<NetworkView>();
            }
            else
            {
                if (_networkView.NetworkController.DefaultSession != null)
                {
                    if (_networkView.NetworkController.DefaultSession.IsServer && _networkView.NetworkID.Id == 1 || _networkView.NetworkController.DefaultSession.IsClient && _networkView.NetworkID.Id == 2)
                    {
                        _networkView.RPC("UpdatePosition", _sceneEntity.LocalPosition.X, _sceneEntity.LocalPosition.Y, _sceneEntity.LocalRotation);
                    }
                }
            }
        }

        [RPCMethod]
        private void UpdatePosition(float x, float y, float rotation)
        {
            if (_sceneEntity != null)
            {
                _sceneEntity.LocalPosition = new Vector2(x, y);
                _sceneEntity.LocalRotation = rotation;
            }
        }

        [EntityMessageHandler(MessageType = typeof(CollisionEnterMessage))]
        private void OnCollisionEnter(Message message)
        {
            CollisionEnterMessage collisionEnterMessage = (CollisionEnterMessage)message;
            CollisionInfo collisionInfo = collisionEnterMessage.CollisionInfo;

            Debug.Log("Character collided with: " + collisionInfo.CollidedBody.Entity.Name);

            //collision.CollidedBody.Entity.Destroy();
        }

        [EntityMessageHandler(MessageType = typeof(CollisionExitMessage))]
        private void OnCollisionExit(Message message)
        {
            CollisionExitMessage collisionExitMessage = (CollisionExitMessage)message;
            CollisionInfo collisionInfo = collisionExitMessage.CollisionInfo;

            Debug.Log("Character removed with: " + collisionInfo.CollidedBody.Entity.Name);
        }
    }
}
