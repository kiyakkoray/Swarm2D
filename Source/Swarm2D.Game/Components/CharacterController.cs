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

using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View;
using Swarm2D.Library;

namespace Swarm2D.Game.Components
{
    [RequiresComponent(typeof(SceneEntity))]
    [RequiresComponent(typeof(Character))]
    class CharacterController : Component
    {
        public Scene Scene { get; private set; }
        public SceneEntity Transform { get; private set; }

        private bool _isInitialized = false;

        [ComponentProperty]
        public float CharacterSpeed { get; set; }

        private Vector2 _characterLookDirection;
        private Vector2 _characterSideDirection;

        private Camera _characterCamera;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            CharacterSpeed = 6.0f;

            Transform = Entity.GetComponent<SceneEntity>();
            Scene = Transform.Scene;
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            UpdateMessage updateMessage = message as UpdateMessage;

            if (!_isInitialized)
            {
                _isInitialized = true;
                _characterCamera = Scene.FindEntity("Camera").Entity.GetComponent<Camera>();
            }

            PhysicsObject rigidBody = Entity.GetComponent<PhysicsObject>();

            if (rigidBody == null)
            {
                return;
            }

            rigidBody.Velocity = new Vector2();
            rigidBody.AngularVelocity = 0.0f;

            Vector2 mousePositionOnScreen = GameInput.MousePosition;
            Vector2 mousePositionOnMap = _characterCamera.FromScreenPositionToMapPosition(mousePositionOnScreen);
            Vector2 currentPosition = Transform.LocalPosition;

            bool rightMouse = GameInput.RightMouse;
            bool leftMouse = GameInput.LeftMouse;

            if (leftMouse)
            {
                Debug.Log(mousePositionOnScreen);
            }

            if (rightMouse)
            {
                _characterLookDirection = (mousePositionOnMap - currentPosition);
                _characterLookDirection.Normalize();
                _characterSideDirection = _characterLookDirection.Perpendicular;
                Transform.LocalRotation = _characterLookDirection.Angle * Mathf.Rad2Deg + 180.0f;
            }

            if (GameInput.GetKey(KeyCode.KeyW))
            {
                currentPosition = currentPosition + _characterLookDirection * CharacterSpeed;
            }

            if (GameInput.GetKey(KeyCode.KeyS))
            {
                currentPosition = currentPosition - _characterLookDirection * CharacterSpeed;
            }

            if (GameInput.GetKey(KeyCode.KeyA))
            {
                if (rightMouse)
                {
                    //this scope means character turning around the point.
                    //
                    //if we don't execute this scope, 
                    //character's distance from the point will increase in time because of floating point error
                    //
                    //we are re-calculating the new distance from the point and fixing the position with calculated error

                    float firstRadius = (currentPosition - mousePositionOnMap).Length;
                    currentPosition = currentPosition + _characterSideDirection * CharacterSpeed;
                    float secondRadius = (currentPosition - mousePositionOnMap).Length;
                    float floatingFixValue = secondRadius - firstRadius;

                    currentPosition = currentPosition + _characterLookDirection * floatingFixValue;
                }
                else
                {
                    currentPosition = currentPosition + _characterSideDirection * CharacterSpeed;
                }
            }

            if (GameInput.GetKey(KeyCode.KeyD))
            {
                if (rightMouse)
                {
                    float firstRadius = (currentPosition - mousePositionOnMap).Length;
                    currentPosition = currentPosition - _characterSideDirection * CharacterSpeed;
                    float secondRadius = (currentPosition - mousePositionOnMap).Length;
                    float floatingFixValue = secondRadius - firstRadius;

                    currentPosition = currentPosition + _characterLookDirection * floatingFixValue;
                }
                else
                {
                    currentPosition = currentPosition - _characterSideDirection * CharacterSpeed;
                }
            }

            Vector2 velocity = (currentPosition - Transform.LocalPosition) * (1.0f / updateMessage.Dt);

            rigidBody.Velocity = velocity;

            //networkView.RPC("UpdateVelocity", velocity);

            if (GameInput.GetKey(KeyCode.KeyR))
            {
                Scene scene = this.Entity.GetComponent<SceneEntity>().Scene;
                PhysicsWorld physicsWorld = scene.Entity.GetComponent<PhysicsWorld>();

                foreach (PhysicsObject body in physicsWorld.PhysicsObjects)
                {
                    DebugRenderer debugRenderer = body.Entity.GetComponent<DebugRenderer>();

                    if (debugRenderer != null)
                    {
                        debugRenderer.DebugPhysics = body.IsInside(mousePositionOnMap);
                    }
                }
            }
            else if (GameInput.GetKeyUp(KeyCode.KeyR))
            {
                Scene scene = this.Entity.GetComponent<SceneEntity>().Scene;
                PhysicsWorld physicsWorld = scene.Entity.GetComponent<PhysicsWorld>();

                foreach (PhysicsObject body in physicsWorld.PhysicsObjects)
                {
                    DebugRenderer debugRenderer = body.Entity.GetComponent<DebugRenderer>();

                    if (debugRenderer != null)
                    {
                        debugRenderer.DebugPhysics = false;
                    }
                }
            }
            //Transform.LocalPosition = currentPosition;
        }

        private void UpdateVelocity(Vector2 velocity)
        {
            PhysicsObject rigidBody = Entity.GetComponent<PhysicsObject>();

            if (rigidBody != null)
            {
                rigidBody.Velocity = velocity;
            }
        }
    }
}
