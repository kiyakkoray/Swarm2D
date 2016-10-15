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

namespace Swarm2D.Engine.View
{
    [RequiresComponent(typeof(SceneEntity))]
    [PoolableComponent]
    public sealed class SpriteRenderer : Renderer
    {
        private Sprite _sprite;

        [ComponentProperty]
        public float RotationAsAngle { get; set; }

        [ComponentProperty]
        public Vector2 Offset { get; set; }

        [ComponentProperty]
        public Sprite Sprite
        {
            get { return _sprite; }
            set
            {
                _sprite = value;

                if (_sprite != null)
                {
                    Width = Sprite.Width;
                    Height = Sprite.Height;
                }
                else
                {
                    Width = 0;
                    Height = 0;
                }
            }
        }

        public override Box BoundingBox
        {
            get
            {
                Box boundingBox = new Box();

                if (Sprite != null)
                {
                    boundingBox.Size = new Vector2(Sprite.Width, Sprite.Height);
                    boundingBox.Position = SceneEntity.GlobalPosition + Offset - boundingBox.Size * 0.5f;
                }
                else
                {
                    boundingBox.Size = new Vector2(0, 0);
                    boundingBox.Position = SceneEntity.GlobalPosition + Offset;
                }

                return boundingBox;
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (Sprite != null)
            {
                Width = Sprite.Width;
                Height = Sprite.Height;
            }
        }

        protected override void OnDestroy()
        {
            _sprite = null;
            Offset = Vector2.Zero;
            RotationAsAngle = 0.0f;

            base.OnDestroy();
        }

        public override void Render(RenderContext renderContext, Box renderBox)
        {
            if (Sprite != null)
            {
                float x = -Sprite.Width / 2;
                float y = -Sprite.Height / 2;

                bool matrixSet = false;

                if (!Mathf.IsZero(RotationAsAngle) || !Mathf.IsZero(Offset.Length))
                {
                    var worldMatrix = SceneEntity.TransformMatrix * Matrix4x4.Position2D(Offset) * Matrix4x4.RotationAboutZ(RotationAsAngle * Mathf.Deg2Rad);
                    renderContext.AddGraphicsCommand(new CommandSetWorldMatrix(worldMatrix));
                    matrixSet = true;
                }
                else
                {
                    if (SceneEntity.Parent == null && Mathf.IsZero(SceneEntity.LocalRotation))
                    {
                        x += SceneEntity.GlobalPosition.X;
                        y += SceneEntity.GlobalPosition.Y;
                    }
                    else /*if (!SceneEntity.TransformMatrix.IsIdentity)*/
                    {
                        renderContext.AddGraphicsCommand(new CommandSetWorldMatrix(SceneEntity.TransformMatrix));
                        matrixSet = true;
                    }
                }

                renderContext.AddGraphicsCommand(new CommandDrawSprite(x, y, Sprite));

                if (matrixSet)
                {
                    renderContext.AddGraphicsCommand(new CommandSetWorldMatrixAsIdentity());
                }
            }
        }
    }
}
