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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Swarm2D.WindowsFramework.Native;
using Swarm2D.WindowsFramework.Native.Windows;

namespace Swarm2D.WindowsFramework
{
    class WindowsForm
    {
        static int classNameCount = 0;

        WindowClass wc;
        string windowClassName;
        //MessageHandler messageHandler;

        WndProc _windowProcedure;

        public int Width
        {
            get;
            set;
        }

        public int Height
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }

        private WindowsFormMessageHandler _messageHandler;

        public IntPtr Handle
        {
            get;
            set;
        }

        public WindowsForm(int x, int y, int width, int height)
        {
            Handle = IntPtr.Zero;

            classNameCount++;

            Width = width;
            Height = height;

            Text = "Form";
            Width = Width - 16;
            Height = Height - 38;
            windowClassName = "Form" + classNameCount;

            wc = new WindowClass();
            _windowProcedure = new WndProc(WndProc);

            wc.style = 0;
            wc.lpfnWndProc = _windowProcedure;
            wc.cbClsExtra = 0;
            wc.cbWndExtra = 0;
            wc.hInstance = Kernel32.GetModuleHandle(null);
            wc.hbrBackground = new IntPtr(5);
            wc.lpszMenuName = null;
            wc.lpszClassName = windowClassName;

            User32.RegisterClass(ref wc);

            Handle = User32.CreateWindowEx(0, windowClassName, "Game Screen: " + Process.GetCurrentProcess().Id, WindowStyle.OverlappedWindow, x, y, width, height, IntPtr.Zero, IntPtr.Zero, Kernel32.GetModuleHandle(null), IntPtr.Zero);
        }

        public WindowsForm(int width, int height)
            : this(100, 100, width, height)
        {
        }

        public void Show()
        {
            User32.ShowWindow(Handle, WindowShowStyle.Show);
        }

        public void Hide()
        {
            User32.ShowWindow(Handle, WindowShowStyle.Hide);
        }

        public void Destroy()
        {
            User32.DestroyWindow(Handle);
            User32.UnregisterClass(windowClassName, IntPtr.Zero);
        }

        public void AddMessageHandler(WindowsFormMessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
        }

        IntPtr WndProc(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam)
        {
            int wParamAsInt = wParam.ToInt32();
            int lParamAsInt = lParam.ToInt32();

            switch (message)
            {
                case (uint)WindowMessage.Size:
                    {
                        int newWidth = lParamAsInt % 0x10000;
                        int newHeight = (lParamAsInt / 0x10000);

                        Width = newWidth;
                        Height = newHeight;
                        //_graphicsContext.Resize();
                    }
                    break;
                default:
                    break;
            }

            if (_messageHandler != null)
            {
                _messageHandler((WindowMessage)message, wParamAsInt, lParamAsInt);
            }

            return User32.DefWindowProc(hWnd, message, wParam, lParam);
        }
    }

    delegate void WindowsFormMessageHandler(WindowMessage message, int wParam, int lParam);
}
