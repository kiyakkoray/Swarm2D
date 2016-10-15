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
using System.Runtime.InteropServices;

namespace Swarm2D.WindowsFramework.Native.OggVorbis
{
	internal struct VorbisInfo
	{
        public int Version;
        public int Channels;
        public int Rate;

        public int BitrateUpper;
        public int BitrateNominal;
        public int BitrateLower;
        public int BitrateWindow;

        public IntPtr CodecSetup;

        public static VorbisInfo FromUnmanagedMemory(IntPtr ptr)
		{
			VorbisInfo vorbisInfo = new VorbisInfo();

			vorbisInfo.Version = Marshal.ReadInt32(ptr, 0);
			vorbisInfo.Channels = Marshal.ReadInt32(ptr, 4);
			vorbisInfo.Rate = Marshal.ReadInt32(ptr, 8);
			vorbisInfo.BitrateUpper = Marshal.ReadInt32(ptr, 12);
			vorbisInfo.BitrateNominal = Marshal.ReadInt32(ptr, 16);
			vorbisInfo.BitrateLower = Marshal.ReadInt32(ptr, 20);
			vorbisInfo.BitrateWindow = Marshal.ReadInt32(ptr, 24);
			vorbisInfo.CodecSetup = Marshal.ReadIntPtr(ptr, 28);

			return vorbisInfo;
		}
	}
}
