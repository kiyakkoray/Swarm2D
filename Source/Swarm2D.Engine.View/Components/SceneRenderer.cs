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
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.Engine.View
{
    public class SceneRenderer : SceneController
    {
        internal List<Camera> CameraComponents { get; private set; }
        private List<IRenderer> _rendererComponents;

        private Circle _rendererCheckCircle;
        private Polygon _rendererCheckBox;

        private CircleInstance _rendererCheckCircleData;
        private PolygonInstance _rendererCheckBoxData;

        protected override void OnAdded()
        {
            base.OnAdded();

            CameraComponents = new List<Camera>();
            _rendererComponents = new List<IRenderer>();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _rendererCheckCircle = new Circle("rendererCheckCircle");
            _rendererCheckCircle.Radius = 1.0f;

            _rendererCheckBox = new Polygon("rendererCheckBox");
            _rendererCheckBox.Vertices.Add(new Vector2(0, 0));
            _rendererCheckBox.Vertices.Add(new Vector2(0, 1));
            _rendererCheckBox.Vertices.Add(new Vector2(1, 1));
            _rendererCheckBox.Vertices.Add(new Vector2(1, 0));

            _rendererCheckCircleData = new CircleInstance(_rendererCheckCircle);
            _rendererCheckBoxData = new PolygonInstance(_rendererCheckBox);

            _rendererCheckCircleData.Initialize();
            _rendererCheckBoxData.Initialize();
        }

        public override void OnReset()
        {
            base.OnReset();

            _rendererComponents.Clear();
            CameraComponents.Clear();
        }

        public void Render(IOSystem ioSystem, Box renderBox)
        {
            if (IsInitialized)
            {
                foreach (IRenderer renderer in _rendererComponents)
                {
                    if (renderBox.IsIntersects(renderer.BoundingBox))
                    {
                        renderer.Render(ioSystem, renderBox);
                    }
                }
            }
        }

        public void AddRenderer(IRenderer renderer)
        {
            _rendererComponents.Add(renderer);
        }

        public void RemoveRenderer(IRenderer renderer)
        {
            _rendererComponents.Remove(renderer);
        }

        internal void AddCamera(Camera camera)
        {
            CameraComponents.Add(camera);
        }

        internal void RemoveCamera(Camera camera)
        {
            CameraComponents.Remove(camera);
        }

        public void GetRendererIn(Vector2 position, List<Renderer> result)
        {
            _rendererCheckBoxData.Initialize();

            Matrix4x4 circleTransformMatrix = Matrix4x4.Position2D(position);
            _rendererCheckCircleData.PrepareTransformation(ref circleTransformMatrix);

            foreach (IRenderer rendererComponent in _rendererComponents)
            {
                if (rendererComponent is Renderer)
                {
                    Renderer renderer = rendererComponent as Renderer;

                    SceneEntity sceneEntity = renderer.SceneEntity;

                    Matrix4x4 transformMatrix = sceneEntity.TransformMatrix;

                    _rendererCheckBox.Vertices[0] = new Vector2(-renderer.Width * 0.5f, -renderer.Height * 0.5f);
                    _rendererCheckBox.Vertices[1] = new Vector2(-renderer.Width * 0.5f, renderer.Height * 0.5f);
                    _rendererCheckBox.Vertices[2] = new Vector2(renderer.Width * 0.5f, renderer.Height * 0.5f);
                    _rendererCheckBox.Vertices[3] = new Vector2(renderer.Width * 0.5f, -renderer.Height * 0.5f);

                    _rendererCheckBoxData.Initialize();
                    _rendererCheckBoxData.PrepareTransformation(ref transformMatrix);

                    if (IntersectionTests.CheckIntersection(_rendererCheckCircleData, _rendererCheckBoxData))
                    {
                        result.Add(renderer);
                    }
                }
            }
        }
    }
}
