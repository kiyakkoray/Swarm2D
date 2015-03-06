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
using Swarm2D.Engine.View;
using Swarm2D.Library;

namespace Swarm2D.Game.Components
{
    [RequiresComponent(typeof(SceneEntity))]
    [RequiresComponent(typeof(Camera))]
    class CharacterCamera : Component
    {
        public Scene Scene { get; private set; }
        public SceneEntity Transform { get; private set; }

        private bool _initialized = false;
        private Entity _character;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Transform = Entity.GetComponent<SceneEntity>();
            Scene = Transform.Scene;
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            if (!_initialized)
            {
                _initialized = true;
                _character = Scene.FindEntity("Character1").Entity;
            }

            Vector2 characterPosition = _character.GetComponent<SceneEntity>().LocalPosition;
            Vector2 cameraPosition = Transform.LocalPosition;

            Vector2 characterDistance = characterPosition - cameraPosition;

            float outputWidth = GameScreen.Width;
            float outputHeight = GameScreen.Height;

            if (characterDistance.X >= outputWidth * 0.25f)
            {
                cameraPosition.X += characterDistance.X - outputWidth * 0.25f;
            }
            else if (characterDistance.X <= -outputWidth * 0.25f)
            {
                cameraPosition.X += characterDistance.X + outputWidth * 0.25f;
            }

            if (characterDistance.Y >= outputHeight * 0.25f)
            {
                cameraPosition.Y += characterDistance.Y - outputHeight * 0.25f;
            }
            else if (characterDistance.Y <= -outputHeight * 0.25f)
            {
                cameraPosition.Y += characterDistance.Y + outputHeight * 0.25f;
            }

            //cameraPosition  characterPosition
            Transform.LocalPosition = cameraPosition;
        }
    }
}
