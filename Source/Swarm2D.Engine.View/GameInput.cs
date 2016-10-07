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
    public static class GameInput
    {
        private static IIOSystem _gameRenderer;

        internal static void Initialize(GameRenderer gameRenderer)
        {
            _gameRenderer = gameRenderer;
        }

        public static Vector2 MousePosition
        {
            get
            {
                if (_gameRenderer == null)
                {
                    return new Vector2(0, 0);
                }

                return _gameRenderer.MousePosition;
            }
        }

        public static bool LeftMouse
        {
            get
            {
                return _gameRenderer != null && _gameRenderer.LeftMouse;
            }
        }

        public static bool LeftMouseDown
        {
            get
            {
                return _gameRenderer != null && _gameRenderer.LeftMouseDown;
            }
        }

        public static bool LeftMouseUp
        {
            get
            {
                return _gameRenderer != null && _gameRenderer.LeftMouseUp;
            }
        }

        public static bool RightMouse
        {
            get
            {
                return _gameRenderer != null && _gameRenderer.RightMouse;
            }
        }

        public static bool RightMouseDown
        {
            get
            {
                return _gameRenderer != null && _gameRenderer.RightMouseDown;
            }
        }

        public static bool RightMouseUp
        {
            get
            {
                return _gameRenderer != null && _gameRenderer.RightMouseUp;
            }
        }

        public static GamepadData GamepadData
        {
            get
            {
                if (_gameRenderer != null)
                {
                    return _gameRenderer.GamepadData;
                }
                
                return new GamepadData();
            }
        }

        public static bool GetKeyDown(KeyCode keyCode)
        {
            return _gameRenderer != null && _gameRenderer.GetKeyDown(keyCode);
        }

        public static bool GetKey(KeyCode keyCode)
        {
            return _gameRenderer != null && _gameRenderer.GetKey(keyCode);
        }

        public static bool GetKeyUp(KeyCode keyCode)
        {
            return _gameRenderer != null && _gameRenderer.GetKeyUp(keyCode);
        }
    }
}
