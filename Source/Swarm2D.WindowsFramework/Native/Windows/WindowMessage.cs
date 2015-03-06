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

namespace Swarm2D.WindowsFramework.Native.Windows
{
    enum WindowMessage : uint
    {
        Quit = 0x0012,
        Close = 0x0010,
        Size = 0x0005,
        KeyDown = 0x0100,
        KeyUp = 0x0101,
        RightButtonUp = 0x0205,
        RightButtonDown = 0x0204,
        LeftButtonUp = 0x0202,
        LeftButtonDown = 0x0201,
        MouseMove = 0x0200,
        KillFocus = 0x0008,
        SetFocus = 0x0007,
    }
}