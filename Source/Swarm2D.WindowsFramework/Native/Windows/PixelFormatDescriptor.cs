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
    [StructLayout(LayoutKind.Sequential)]
    struct PixelFormatDescriptor
    {
        public ushort nSize;
        public ushort nVersion;
        public uint dwFlags;
        public byte iPixelType;
        public byte cColorBits;
        public byte cRedBits;
        public byte cRedShift;
        public byte cGreenBits;
        public byte cGreenShift;
        public byte cBlueBits;
        public byte cBlueShift;
        public byte cAlphaBits;
        public byte cAlphaShift;
        public byte cAccumBits;
        public byte cAccumRedBits;
        public byte cAccumGreenBits;
        public byte cAccumBlueBits;
        public byte cAccumAlphaBits;
        public byte cDepthBits;
        public byte cStencilBits;
        public byte cAuxBuffers;
        public byte iLayerType;
        public byte bReserved;
        public uint dwLayerMask;
        public uint dwVisibleMask;
        public uint dwDamageMask;
    }

    enum PixelFormatDescriptorFlags : uint
    {
        DoubleBuffer = 0x00000001,
        Stereo = 0x00000002,
        DrawToWindow = 0x00000004,
        DrawToBitmap = 0x00000008,
        SupportGDI = 0x00000010,
        SupportOpengl = 0x00000020,
        GenericFormat = 0x00000040,
        NeedPalette = 0x00000080,
        NeedSystemPalette = 0x00000100,
        SwapExchange = 0x00000200,
        SwapCopy = 0x00000400,
        SwapLayerBuffers = 0x00000800,
        GenericAccelerated = 0x00001000,
        SupportDirectDraw = 0x00002000,
        Direct3DAccelerated = 0x00004000,
        SupportComposition = 0x00008000
    }

    enum PixelFormatDescriptorLayerTypes : sbyte
    {
        MainPlane = 0,
        OverlayPlane = 1,
        UnderlayPlane = (-1)
    }

    enum PixelFormatDescriptorPixelTypes : sbyte
    {
        RGBA = 0,
        ColorIndex = 1
    }
}
