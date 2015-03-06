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
using System.Runtime.InteropServices;

namespace Swarm2D.WindowsFramework.Native.Windows
{
    static class User32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern short GetAsyncKeyState(int vkey);

        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        public static extern IntPtr CreateWindowEx(int dwExStyle, [MarshalAs(UnmanagedType.LPTStr)] string lpClassName, string lpWindowName, WindowStyle dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(out NativeMessage lpMsg, [In] IntPtr hWnd, [In] uint wMsgFilterMin, [In]  uint wMsgFilterMax, [In]  uint wRemoveMsg);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TranslateMessage([In] ref NativeMessage lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage([In] ref NativeMessage lpMsg);

        [DllImport("user32.dll")]
        public static extern ushort RegisterClass([In] ref WindowClass lpWndClass);

        [DllImport("user32.dll")]
        public static extern bool UnregisterClass([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, IntPtr hInstance);

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMessage(out NativeMessage lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
    }

    delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}
