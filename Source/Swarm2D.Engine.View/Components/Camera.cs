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
    public class Camera : SceneEntityComponent
    {
        public SceneRenderer SceneRenderer { get; private set; }

        [ComponentProperty]
        public int Width { get; set; }

        [ComponentProperty]
        public int Height { get; set; }

        [ComponentProperty]
        public CameraSizeType Type { get; set; }

        public bool Enabled { get; set; }

        public Camera()
        {
            Width = 1024;
            Height = 768;
            Enabled = true;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            SceneRenderer = Scene.GetComponent<SceneRenderer>();
            SceneRenderer.AddCamera(this);
        }

        public Vector2 FromScreenPositionToMapPosition(Vector2 screenPosition)
        {
            Vector2 outputSize = new Vector2(GameScreen.Width, GameScreen.Height);
            //Vector2 outputSize = new Vector2();
            return screenPosition + (SceneEntity.LocalPosition - outputSize * 0.5f);
        }

        public Vector2 FromMapPositionToScreenPosition(Vector2 mapPosition)
        {
            Vector2 outputSize = new Vector2(GameScreen.Width, GameScreen.Height);
            //Vector2 outputSize = new Vector2();
            return mapPosition - SceneEntity.LocalPosition + outputSize * 0.5f;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SceneRenderer.RemoveCamera(this);
        }

        private void GetRenderWidthAndHeight(int targetWidth, int targetHeight, out int width, out int height)
        {
            width = Width;
            height = Height;

            switch (Type)
            {
                case CameraSizeType.TargetSize:
                    {
                        width = targetWidth;
                        height = targetHeight;
                    }
                    break;
                case CameraSizeType.SpecifiedSize:
                    {
                        width = Width;
                        height = Height;
                    }
                    break;
                case CameraSizeType.UseHeightAndKeepAspectRatio:
                    {
                        float targetAspectRatio = (float)targetWidth / (float)targetHeight;

                        width = (int)(targetAspectRatio * (float)Height);
                        height = Height;
                    }
                    break;
            }
        }

        internal Matrix4x4 GetViewMatrix(int targetWidth, int targetHeight, Vector2 targetPosition)
        {
            int width;
            int height;

            GetRenderWidthAndHeight(targetWidth, targetHeight, out width, out height);

            float xScale = (float)targetWidth / (float)width;
            float yScale = (float)targetHeight / (float)height;

            Vector2 cameraPosition = targetPosition + SceneEntity.GlobalPosition * -1.0f +
                                     new Vector2(targetWidth * 0.5f, targetHeight * 0.5f);

            return Matrix4x4.Transformation2D(new Vector2(xScale, yScale), 0.0f, cameraPosition);
        }

        internal Box GetRenderBox(int targetWidth, int targetHeight)
        {
            int width;
            int height;

            GetRenderWidthAndHeight(targetWidth, targetHeight, out width, out height);

            Box renderBox = new Box();

            renderBox.Size = new Vector2(width, height);
            renderBox.Position = SceneEntity.GlobalPosition - new Vector2(width * 0.5f, height * 0.5f);

            return renderBox;
        }
    }

    public enum CameraSizeType
    {
        TargetSize,
        SpecifiedSize,
        UseHeightAndKeepAspectRatio,
    }
}