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
        private const bool _useSeparateRenderingThread = true;

        private bool _doNotRender = false;

        private Queue<GraphicsCommand> _graphicsCommands;

        private Framework _framework;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _framework = Framework.Current;
            Current = this;

            _graphicsCommands = new Queue<GraphicsCommand>(65536);

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
            //_graphicsForm.InitializeGraphicsContext();
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            Engine.DoneJob();

            _framework.UpdateGraphics();

            UpdateInput();
        }

        public void AddGraphicsCommand(GraphicsCommand graphicsCommand)
        {
            graphicsCommand.IOSystem = this;
            graphicsCommand.Framework = _framework;
            //graphicsCommand.GraphicsContext = GraphicsContext;
            //graphicsCommand.GraphicsWindow = _graphicsForm;

            if (_useSeparateRenderingThread && _framework.SupportSeperatedRenderThread)
            {
                lock (_graphicsCommands)
                {
                    _graphicsCommands.Enqueue(graphicsCommand);
                }
            }
            else
            {
                graphicsCommand.DoJob();
            }
        }

        internal void ExecuteBufferedCommands()
        {
        }

        private void ResetGraphicsContextMatrices()
        {
            Graphics.ViewMatrix = Matrix4x4.Identity;
            Graphics.WorldMatrix = Matrix4x4.Identity;
        }

        private void WaitPreviousRenderFrame()
        {
            while (_graphicsCommands.Count > 1)
            {
                Thread.Sleep(1);
            }
        }

        [EntityMessageHandler(MessageType = typeof(LateUpdateMessage))]
        private void OnLateUpdate(Message message)
        {
            if (!_doNotRender)
            {
                WaitPreviousRenderFrame();

                AddGraphicsCommand(new CommandBeginFrame());

                Entity.GetComponent<EngineController>().SendMessage(new RenderMessage(this));

                AddGraphicsCommand(new CommandSwapBuffers());
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

        private void RenderThreadLoop()
        {
            while (true)
            {
                if (_doNotRender)
                {
                    AddGraphicsCommand(new CommandBeginFrame());
                    AddGraphicsCommand(new CommandSwapBuffers());
                }

                GraphicsCommand graphicsCommand = null;

                lock (_graphicsCommands)
                {
                    if (_graphicsCommands.Count > 0)
                    {
                        graphicsCommand = _graphicsCommands.Dequeue();
                    }
                }

                if (graphicsCommand != null)
                {
                    graphicsCommand.DoJob();
                }
                else
                {
                    Thread.Sleep(1);
                }
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
        public IOSystem IOSystem { get; set; }

        public RenderMessage(IOSystem ioSystem)
        {
            IOSystem = ioSystem;
        }
    }
}
