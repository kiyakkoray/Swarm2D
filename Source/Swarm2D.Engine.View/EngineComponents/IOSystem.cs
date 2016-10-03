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

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Library;
using Thread = Swarm2D.Library.Thread;

namespace Swarm2D.Engine.View
{
    public interface IIOSystem
    {
        int Width { get; }
        int Height { get; }

        bool GetKeyDown(KeyCode keyCode);
        bool GetKey(KeyCode keyCode);
        bool GetKeyUp(KeyCode keyCode);
        bool LeftMouse { get; }
        bool LeftMouseDown { get; }
        bool LeftMouseUp { get; }
        bool RightMouse { get; }
        bool RightMouseDown { get; }
        bool RightMouseUp { get; }
        Vector2 MousePosition { get; }

        //void FillInputData(InputData inputData);
    }

    public class IOSystem : EngineComponent, IIOSystem
    {
        private Thread _renderThread;
        private bool _doNotRender = false;

        private RenderContext _currentRootRenderContext;
        private RenderContext _nextRootRenderContext;

        private Framework _framework;

        private ManualResetEvent _renderThreadEvent;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _renderThreadEvent = new ManualResetEvent(false);
            _framework = Framework.Current;
            _currentRootRenderContext = null;
             _nextRootRenderContext = new RenderContext(this, _framework);
            Current = this;

            if (_framework.SupportSeperatedRenderThread)
            {
                _renderThread = new Thread(RenderThreadLoop);
                _renderThread.Name = "RenderThread";
                _renderThread.Start();
            }

            Graphics.CreateGraphics();
        }

        protected override void OnStart()
        {
            AddGraphicsCommand(new CommandInitializeGraphicsContext());
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            Engine.DoneJob();

            _framework.UpdateGraphics();

            UpdateInput();
        }

        private void AddGraphicsCommand(GraphicsCommand graphicsCommand)
        {
            _nextRootRenderContext.AddGraphicsCommand(graphicsCommand);
        }

        private void WaitPreviousRenderFrame()
        {
            if (_currentRootRenderContext != null)
            {
                _renderThreadEvent.WaitOne();
                _renderThreadEvent.Reset();
            }
        }

        [EntityMessageHandler(MessageType = typeof(LateUpdateMessage))]
        private void OnLateUpdate(Message message)
        {
            if (!_doNotRender)
            {
                WaitPreviousRenderFrame();

                var nextRenderContext = _nextRootRenderContext;
                _nextRootRenderContext = new RenderContext(this, _framework);

                var previousRenderContext = _nextRootRenderContext.AddChildRenderContext(-10000);
                previousRenderContext.AddGraphicsCommand(new CommandBeginFrame());

                var mainRenderContext = _nextRootRenderContext.AddChildRenderContext(0);
                Engine.SendMessage(new RenderMessage(mainRenderContext));

                var lastRenderContext = _nextRootRenderContext.AddChildRenderContext(10000);
                lastRenderContext.AddGraphicsCommand(new CommandSwapBuffers());

                _currentRootRenderContext = nextRenderContext;
            }
        }

        [EntityMessageHandler(MessageType = typeof(LastUpdateMessage))]
        private void OnLastUpdate(Message message)
        {
            if (!_framework.SupportSeperatedRenderThread)
            {
                DoRenderJob();
            }
        }

        [EntityMessageHandler(MessageType = typeof(OnGameFrameUpdate))]
        private void HanldeOnGameFrameUpdateMessage(Message message)
        {
            AddGraphicsCommand(new CommandResetDebugRender());
        }

        public void UpdateInput()
        {
            _framework.UpdateInput();
            //_graphicsForm.UpdateInput();
        }

        private void DoRenderJob()
        {
            if (_doNotRender)
            {
                AddGraphicsCommand(new CommandBeginFrame());
                AddGraphicsCommand(new CommandSwapBuffers());
            }

            if (_currentRootRenderContext != null)
            {
                var renderContext = _currentRootRenderContext;
                _currentRootRenderContext = null;
                _renderThreadEvent.Set();

                renderContext.Render();
            }
        }

        private void RenderThreadLoop()
        {
            while (true)
            {
                DoRenderJob();
            }
        }

        public Texture LoadTexture(string resourcesName, string name)
        {
            //string textureName = Resources.MainPath + @"\" + resourcesName + @"\" + name;
            //string textureName = name;

            Texture texture = Graphics.GetTexture(resourcesName, name);

            if (texture == null)
            {
                texture = Graphics.CreateTexture();

                AddGraphicsCommand(new CommandCreateAndLoadTexture(texture, resourcesName, name));
            }

            return texture;
        }

        public Texture LoadTexture(string name)
        {
            string resourcesName = Resources.ResourcesName;
            return LoadTexture(resourcesName, name);
        }

        public static IOSystem Current { get; private set; }

        public int Width
        {
            get
            {
                return _framework.Width;
                //return _graphicsForm.Width;
            }
        }

        public int Height
        {
            get
            {
                return _framework.Height;
                //return _graphicsForm.Height;
            }
        }

        public bool GetKeyDown(KeyCode keyCode)
        {
            return _framework.GetKeyDown(keyCode);
            //return _graphicsForm.GetKeyDown(keyCode);
        }

        public bool GetKey(KeyCode keyCode)
        {
            return _framework.GetKey(keyCode);
            //return _graphicsForm.GetKey(keyCode);
        }

        public bool GetKeyUp(KeyCode keyCode)
        {
            return _framework.GetKeyUp(keyCode);
            //return _graphicsForm.GetKeyUp(keyCode);
        }

        public bool LeftMouse
        {
            get
            {
                return _framework.LeftMouse();
                //return _graphicsForm.LeftMouse();
            }
        }

        public bool LeftMouseDown
        {
            get
            {
                return _framework.LeftMouseDown();
                //return _graphicsForm.LeftMouseDown();
            }
        }

        public bool LeftMouseUp
        {
            get
            {
                return _framework.LeftMouseUp();
                //return _graphicsForm.LeftMouseUp();
            }
        }

        public bool RightMouse
        {
            get
            {
                return _framework.RightMouse();
                //return _graphicsForm.RightMouse();
            }
        }

        public bool RightMouseDown
        {
            get
            {
                return _framework.RightMouseDown();
                //return _graphicsForm.RightMouseDown();
            }
        }

        public bool RightMouseUp
        {
            get
            {
                return _framework.RightMouseUp();
                //return _graphicsForm.RightMouseUp();
            }
        }

        public Vector2 MousePosition
        {
            get
            {
                return _framework.MousePosition();
                //return _graphicsForm.MousePosition();
            }
        }

        public void FillInputData(InputData inputData)
        {
            _framework.FillInputData(inputData);
        }

        public void AddDebugPoint(Vector2 point)
        {
            this.AddGraphicsCommand(new CommandDrawDebugPoint(point));
        }

        public void AddDebugLine(Vector2 pointA, Vector2 pointB)
        {
            this.AddGraphicsCommand(new CommandDrawDebugLine(pointA, pointB));
        }
    }

    public class RenderMessage : DomainMessage
    {
        public RenderContext RenderContext { get; set; }

        public RenderMessage(RenderContext renderContext)
        {
            RenderContext = renderContext;
        }
    }
}
