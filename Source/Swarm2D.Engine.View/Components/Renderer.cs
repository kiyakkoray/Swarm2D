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
    public abstract class Renderer : SceneEntityComponent, IRenderer
    {
        public SceneRenderer SceneRenderer { get; private set; }

        public abstract Box BoundingBox { get; }

        public float Width { get; protected set; }
        public float Height { get; protected set; }

        private bool _addedToSceneRenderer = false;

        protected override void OnAdded()
        {
            base.OnAdded();

            if (!_addedToSceneRenderer)
            {
                AddToSceneRenderer();
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (!_addedToSceneRenderer)
            {
                AddToSceneRenderer();
            }
        }

        public virtual void Render(RenderContext renderContext, Box renderBox)
        {

        }

        protected override void OnDestroy()
        {
            if (_addedToSceneRenderer)
            {
                RemoveFromSceneRenderer();
            }

            SceneRenderer = null;
            Width = 0;
            Height = 0;

            base.OnDestroy();
        }

        private void AddToSceneRenderer()
        {
            Debug.Assert(!_addedToSceneRenderer);

            SceneRenderer = Scene.GetComponent<SceneRenderer>();

            if (SceneRenderer != null)
            {
                SceneRenderer.AddRenderer(this);
                _addedToSceneRenderer = true;
            }
        }

        private void RemoveFromSceneRenderer()
        {
            Debug.Assert(_addedToSceneRenderer);
            SceneRenderer.RemoveRenderer(this);
            _addedToSceneRenderer = false;
        }

        [DomainMessageHandler(MessageType = typeof(SceneRendererCreatedMessage))]
        private void OnSceneRendererCreatedMessage(Message message)
        {
            if (!_addedToSceneRenderer)
            {
                AddToSceneRenderer();
            }
        }
    }

    public interface IRenderer
    {
        Box BoundingBox { get; }

        float Width { get; }
        float Height { get; }

        void Render(RenderContext renderContext, Box renderBox);
    }
}
