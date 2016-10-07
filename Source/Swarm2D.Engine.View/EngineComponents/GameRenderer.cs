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
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.Engine.View
{
    public class GameRenderer : Component, IIOSystem
    {
        private int _currentGameLogicUpdateFrameCount = 0;

        private bool _renderToScreenPart = false;

        private Vector2 _renderToScreenPosition;
        private int _renderToScreenWidth = 0;
        private int _renderToScreenHeight = 0;

        private GameLogic _gameLogic;
        private IOSystem _ioSystem;

        private static InputData _currentInputData;
        private static InputData _oldInputData;

        private bool _inputEnabled = true;
        private bool _renderEnabled = true;

        public bool InputEnabled
        {
            get { return _inputEnabled; }
            set { _inputEnabled = value; }
        }

        public bool RenderEnabled
        {
            get { return _renderEnabled; }
            set { _renderEnabled = value; }
        }

        public Vector2 RenderTargetPosition
        {
            get
            {
                Vector2 renderTargetPosition = new Vector2(0.0f, 0.0f);

                if (_renderToScreenPart)
                {
                    renderTargetPosition = _renderToScreenPosition;
                }

                return renderTargetPosition;
            }
        }

        public int Width
        {
            get
            {
                if (_renderToScreenPart)
                {
                    return _renderToScreenWidth;
                }

                return _ioSystem.Width;
            }
        }

        public int Height
        {
            get
            {
                if (_renderToScreenPart)
                {
                    return _renderToScreenHeight;
                }

                return _ioSystem.Height;
            }
        }

        protected override void OnAdded()
        {
            RenderEnabled = true;
            InputEnabled = true;

            _gameLogic = Entity.GetComponent<GameLogic>();
            _ioSystem = Engine.RootEntity.GetComponent<IOSystem>();

            _currentInputData = new InputData();
            _oldInputData = new InputData();

            GameInput.Initialize(this);
            GameScreen.Initialize(this);
        }

        public void SetRenderToScreenPart(int x, int y, int width, int height)
        {
            _renderToScreenPart = true;
            _renderToScreenPosition = new Vector2(x, y);

            _renderToScreenWidth = width;
            _renderToScreenHeight = height;
        }

        public void ClearRenderToScreenPart()
        {
            _renderToScreenPart = false;
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            Engine.DoneJob();

            _currentGameLogicUpdateFrameCount = 0;
        }

        [DomainMessageHandler(MessageType = typeof(RenderMessage))]
        private void Render(Message message)
        {
            if (RenderEnabled)
            {
                RenderMessage renderMessage = message as RenderMessage;
                RenderContext renderContext = renderMessage.RenderContext;

                Render(renderContext);
            }
        }

        public void Render(RenderContext renderContext)
        {
            //Scene scene = _gameLogic.CurrentScene;

            foreach (var scene in _gameLogic.LoadedScenes)
            {
                if (scene.IsRunning)
                {
                    SceneRenderer sceneRenderer = scene.GetComponent<SceneRenderer>();

                    if (sceneRenderer != null && sceneRenderer.CameraComponents != null)
                    {
                        for (int i = 0; i < sceneRenderer.CameraComponents.Count; i++)
                        {
                            Camera camera = sceneRenderer.CameraComponents[i];

                            if (camera.Enabled)
                            {
                                Matrix4x4 cameraTransform = camera.GetViewMatrix(Width, Height, RenderTargetPosition);
                                Box renderBox = camera.GetRenderBox(Width, Height);

                                renderContext.AddGraphicsCommand(new CommandSetViewMatrix(cameraTransform));
                                //graphicsContext.ViewMatrix = Matrix4x4.Position2D(RenderTargetPosition + cameraTransform.GlobalPosition * -1.0f + new Vector2(Width * 0.5f, Height * 0.5f));

                                sceneRenderer.Render(renderContext, renderBox);

                                renderContext.AddGraphicsCommand(new CommandSetWorldMatrix(Matrix4x4.Identity));
                                //graphicsContext.WorldMatrix = Matrix4x4.Identity;

                                renderContext.AddGraphicsCommand(new CommandDrawBufferedDebugObjects());
                                //DebugRender.DrawBufferedDebugObjects();
                            }
                        }
                    }
                }
            }
        }

        [EntityMessageHandler(MessageType = typeof(OnGameFrameUpdate))]
        private void HanldeOnGameFrameUpdateMessage(Message message)
        {
            if (_currentGameLogicUpdateFrameCount != 0)
            {
                this.NextFrame();
            }
            else
            {
                this.UpdateInput();
            }

            _currentGameLogicUpdateFrameCount++;
        }

        private void UpdateInput()
        {
            InputData swapper = _oldInputData;
            _oldInputData = _currentInputData;
            _currentInputData = swapper;

            _ioSystem.FillInputData(_currentInputData);
        }

        private void NextFrame()
        {
            InputData swapper = _oldInputData;
            _oldInputData = _currentInputData;
            _currentInputData = swapper;

            _currentInputData.FillFrom(_oldInputData);
        }

        Vector2 IIOSystem.MousePosition
        {
            get
            {
                Vector2 result = new Vector2(0.0f, 0.0f);

                if (InputEnabled)
                {
                    if (_currentInputData != null)
                    {
                        result = new Vector2(_currentInputData.CursorX, _currentInputData.CursorY);
                    }

                    if (_renderToScreenPart)
                    {
                        result = result - _renderToScreenPosition;
                    }
                }

                return result;
            }
        }

        bool IIOSystem.LeftMouse
        {
            get
            {
                return InputEnabled && _currentInputData.LeftMouse;
            }
        }

        bool IIOSystem.LeftMouseDown
        {
            get
            {
                return InputEnabled && _currentInputData.LeftMouse && !_oldInputData.LeftMouse;
            }
        }

        bool IIOSystem.LeftMouseUp
        {
            get
            {
                return InputEnabled && !_currentInputData.LeftMouse && _oldInputData.LeftMouse;
            }
        }

        bool IIOSystem.RightMouse
        {
            get
            {
                return InputEnabled && _currentInputData.RightMouse;
            }
        }

        bool IIOSystem.RightMouseDown
        {
            get
            {
                return InputEnabled && _currentInputData.RightMouse && !_oldInputData.RightMouse;
            }
        }

        bool IIOSystem.RightMouseUp
        {
            get
            {
                return InputEnabled && !_currentInputData.RightMouse && _oldInputData.RightMouse;
            }
        }

        bool IIOSystem.GetKeyDown(KeyCode keyCode)
        {
            return InputEnabled && _currentInputData.KeyData[(int)keyCode] && !_oldInputData.KeyData[(int)keyCode];
        }

        bool IIOSystem.GetKey(KeyCode keyCode)
        {
            return InputEnabled && _currentInputData.KeyData[(int)keyCode];
        }

        bool IIOSystem.GetKeyUp(KeyCode keyCode)
        {
            return InputEnabled && !_currentInputData.KeyData[(int)keyCode] && _oldInputData.KeyData[(int)keyCode];
        }

        GamepadData IIOSystem.GamepadData
        {
            get
            {
                if (InputEnabled)
                {
                    return _currentInputData.GamepadData;
                }

                return new GamepadData();
            }
        }
    }
}
