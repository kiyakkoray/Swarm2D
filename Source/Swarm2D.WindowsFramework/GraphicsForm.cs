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
using Swarm2D.Engine.View;
using Swarm2D.Library;
using Swarm2D.WindowsFramework.Native.Opengl;
using Swarm2D.WindowsFramework.Native.Windows;

namespace Swarm2D.WindowsFramework
{
    public class GraphicsWindow
    {
        private WindowsForm _windowsForm;

        public GraphicsContext GraphicsContext
        {
            get;
            private set;
        }

        private InputData _currentInputData;
        private InputData _oldInputData;
        private InputData _messageLoopInputData;

        private object _inputDataLocker = new object();

        public GraphicsWindow(int x, int y, int width, int height)
        {
            _currentInputData = new InputData();
            _oldInputData = new InputData();
            _messageLoopInputData = new InputData();

            _windowsForm = new WindowsForm(x, y, width, height);
            _windowsForm.AddMessageHandler(MessageHandler);
            _windowsForm.Show();

            GraphicsContext = new GraphicsContext();
        }

        public void InitializeGraphicsContext()
        {
            GraphicsContext.Control = _windowsForm;
            GraphicsContext.CreateContext();

            GraphicsContext.ProjectionMatrix = Matrix4x4.OrthographicProjection(0, _windowsForm.Width, _windowsForm.Height, 0);

            Opengl32.MatrixMode(MatrixMode.ModelView);
        }

        public void BeginFrame()
        {
            if (GraphicsContext != null)
            {
                GraphicsContext.BeginFrame(_windowsForm.Width, _windowsForm.Height);
                GraphicsContext.Resize(_windowsForm.Width, _windowsForm.Height);
            }

            Matrix4x4 projectionMatrix = Matrix4x4.OrthographicProjection(0, _windowsForm.Width, _windowsForm.Height, 0);
            Matrix4x4 identityMatrix = Matrix4x4.Identity;

            Opengl32.MatrixMode(MatrixMode.Projection);
            Opengl32.LoadMatrix(ref projectionMatrix);

            Opengl32.MatrixMode(MatrixMode.ModelView);
            Opengl32.LoadMatrix(ref identityMatrix);
        }

        public void Update()
        {
        }

        public void UpdateInput()
        {
            InputData oldData = _oldInputData;
            _oldInputData = _currentInputData;
            _currentInputData = oldData;

            lock (_inputDataLocker)
            {
                _currentInputData.FillFrom(_messageLoopInputData);
            }
        }

        public bool GetKeyDown(KeyCode keyCode)
        {
            return _currentInputData.KeyData[(int)keyCode] && !_oldInputData.KeyData[(int)keyCode];
        }

        public bool GetKey(KeyCode keyCode)
        {
            return _currentInputData.KeyData[(int)keyCode];
        }

        public bool GetKeyUp(KeyCode keyCode)
        {
            return !_currentInputData.KeyData[(int)keyCode] && _oldInputData.KeyData[(int)keyCode];
        }

        public bool LeftMouse()
        {
            return _currentInputData.LeftMouse;
        }

        public bool LeftMouseDown()
        {
            return _currentInputData.LeftMouse && !_oldInputData.LeftMouse;
        }

        public bool LeftMouseUp()
        {
            return !_currentInputData.LeftMouse && _oldInputData.LeftMouse;
        }

        public bool RightMouse()
        {
            return _currentInputData.RightMouse;
        }

        public bool RightMouseDown()
        {
            return _currentInputData.RightMouse && !_oldInputData.RightMouse;
        }

        public bool RightMouseUp()
        {
            return !_currentInputData.RightMouse && _oldInputData.RightMouse;
        }

        public Vector2 MousePosition()
        {
            return new Vector2(_currentInputData.CursorX, _currentInputData.CursorY);
        }

        public void FillInputDataFromCurrent(InputData inputData)
        {
            inputData.FillFrom(_currentInputData);
        }

        internal void SwapInputs()
        {

        }

        private void MessageHandler(WindowMessage message, int wParam, int lParam)
        {
            switch (message)
            {
                case WindowMessage.Close:
                    {
                        //Running = false;
                    }
                    break;
                case WindowMessage.Size:
                    {
                        //if (GraphicsContext != null)
                        //{
                        //	GraphicsContext.Resize(_windowsForm.Width, _windowsForm.Height);
                        //}
                    }
                    break;
                case WindowMessage.KeyDown:
                    {
                        lock (_inputDataLocker)
                        {
                            _messageLoopInputData.KeyData[wParam] = true;
                        }
                    }
                    break;
                case WindowMessage.KeyUp:
                    {
                        lock (_inputDataLocker)
                        {
                            _messageLoopInputData.KeyData[wParam] = false;
                        }
                    }
                    break;
                case WindowMessage.RightButtonUp:
                    {
                        lock (_inputDataLocker)
                        {
                            _messageLoopInputData.RightMouse = false;

                            int xCoord = lParam % 0x10000;
                            int yCoord = (lParam / 0x10000);
                            _messageLoopInputData.CursorX = xCoord;
                            _messageLoopInputData.CursorY = yCoord;
                        }
                    }
                    break;
                case WindowMessage.RightButtonDown:
                    {
                        lock (_inputDataLocker)
                        {
                            _messageLoopInputData.RightMouse = true;

                            int xCoord = lParam % 0x10000;
                            int yCoord = (lParam / 0x10000);
                            _messageLoopInputData.CursorX = xCoord;
                            _messageLoopInputData.CursorY = yCoord;
                        }

                    }
                    break;
                case WindowMessage.LeftButtonUp:
                    {
                        lock (_inputDataLocker)
                        {
                            _messageLoopInputData.LeftMouse = false;

                            int xCoord = lParam % 0x10000;
                            int yCoord = (lParam / 0x10000);
                            _messageLoopInputData.CursorX = xCoord;
                            _messageLoopInputData.CursorY = yCoord;
                        }
                    }
                    break;
                case WindowMessage.LeftButtonDown:
                    {
                        lock (_inputDataLocker)
                        {
                            _messageLoopInputData.LeftMouse = true;

                            int xCoord = lParam % 0x10000;
                            int yCoord = (lParam / 0x10000);
                            _messageLoopInputData.CursorX = xCoord;
                            _messageLoopInputData.CursorY = yCoord;
                        }
                    }
                    break;
                case WindowMessage.MouseMove:
                    {
                        lock (_inputDataLocker)
                        {
                            int xCoord = lParam % 0x10000;
                            int yCoord = (lParam / 0x10000);
                            _messageLoopInputData.CursorX = xCoord;
                            _messageLoopInputData.CursorY = yCoord;
                        }
                    }
                    break;
                case WindowMessage.KillFocus:
                    {
                        lock (_inputDataLocker)
                        {
                            for (int i = 0; i < 256; i++)
                            {
                                _messageLoopInputData.KeyData[i] = false;
                                _messageLoopInputData.RightMouse = false;
                                _messageLoopInputData.LeftMouse = false;
                            }
                        }
                    }
                    break;
                case WindowMessage.SetFocus:
                    {
                        lock (_inputDataLocker)
                        {
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public int Width
        {
            get
            {
                return _windowsForm.Width;
            }
        }

        public int Height
        {
            get
            {
                return _windowsForm.Height;
            }
        }
    }
}
