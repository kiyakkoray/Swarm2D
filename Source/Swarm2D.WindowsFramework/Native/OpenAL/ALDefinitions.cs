/******************************************************************************
Copyright (c) 2016 Koray Kiyakoglu

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
using System.Threading.Tasks;

namespace Swarm2D.WindowsFramework.Native.OpenAL
{
    internal static class ALDefinitions
    {
        public const int Invalid = -1;
        public const int None = 0;
        public const int False = 0;
        public const int True = 1;
        public const int NoError = False;

        public const int SourceType = 0x0200;
        public const int SourceAbsolute = 0x0201;
        public const int SourceRelative = 0x0202;

        public const int ConeInnerAngle = 0x1001;
        public const int ConeOuterAngle = 0x1002;
        public const int Pitch = 0x1003;
        public const int Position = 0x1004;
        public const int Direction = 0x1005;
        public const int Velocity = 0x1006;
        public const int Looping = 0x1007;
        public const int Buffer = 0x1009;
        public const int Gain = 0x100A;
        public const int MinGain = 0x100D;
        public const int MaxGain = 0x100E;
        public const int Orientation = 0x100F;

        public const int SourceState = 0x1010;
        public const int Initial = 0x1011;
        public const int Playing = 0x1012;
        public const int Paused = 0x1013;
        public const int Stopped = 0x1014;

        public const int BuffersQueued = 0x1015;
        public const int BuffersProcessed = 0x1016;

        public const int FormatMono8 = 0x1100;
        public const int FormatMono16 = 0x1101;
        public const int FormatStereo8 = 0x1102;
        public const int FormatStereo16 = 0x1103;

        public const int ReferenceDistance = 0x1020;
        public const int RolloffFactor = 0x1021;
        public const int ConeOuterGain = 0x1022;
        public const int MaxDistanceE = 0x1023;

        public const int Frequency = 0x2001;
        public const int Bits = 0x2002;
        public const int Channels = 0x2003;
        public const int Size = 0x2004;
        public const int Data = 0x2005;

        public const int Unused = 0x2010;
        public const int Pending = 0x2011;
        public const int Processed = 0x2012;

        public const int ChannelMask = 0x3000;

        public const int InvalidName = 0xA001;
        public const int IllegalEnum = 0xA002;
        public const int InvalidEnum = 0xA002;
        public const int InvalidValue = 0xA003;
        public const int IllegalCommand = 0xA004;
        public const int InvalidOperation = 0xA004;
        public const int OutOfMemory = 0xA005;

        public const int Vendor = 0xB001;
        public const int Version = 0xB002;
        public const int Renderer = 0xB003;
        public const int Extensions = 0xB004;

        public const int DopplerFactor = 0xC000;
        public const int DopplerVelocity = 0xC001;

        public const int DistanceModel = 0xD000;
        public const int InverseDistance = 0xD001;
        public const int InverseDistanceClamped = 0xD002;
    }
}
