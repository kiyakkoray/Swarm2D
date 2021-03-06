﻿/******************************************************************************
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

namespace Swarm2D.Engine.View
{
    public class InputData
    {
        public GamepadData GamepadData { get; set; }
        public bool[] KeyData { get; set; }
        public bool LeftMouse { get; set; }
        public bool RightMouse { get; set; }
        public int CursorX { get; set; }
        public int CursorY { get; set; }

        public InputData()
        {
            GamepadData = new GamepadData();
            KeyData = new bool[256];
            CursorX = 0;
            CursorY = 0;
            LeftMouse = false;
            RightMouse = false;

            for (int i = 0; i < 256; i++)
            {
                KeyData[i] = false;
            }
        }

        public void FillFrom(InputData inputData)
        {
            GamepadData.FillFrom(inputData.GamepadData);
            CursorX = inputData.CursorX;
            CursorY = inputData.CursorY;
            LeftMouse = inputData.LeftMouse;
            RightMouse = inputData.RightMouse;

            for (int i = 0; i < 256; i++)
            {
                KeyData[i] = inputData.KeyData[i];
            }
        }
    }
}
