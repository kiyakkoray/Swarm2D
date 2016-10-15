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
	class VorbisFile
	{
		const string DllName = "vorbisfile.dll";

		[DllImport(DllName, EntryPoint = "ov_open_callbacks", CallingConvention = CallingConvention.Cdecl)]
		public static extern int OpenCallbacks(IntPtr dataSource, IntPtr vf, [MarshalAs(UnmanagedType.LPArray)]byte[] initial, int ibytes, CallBacks callBacks);

		[DllImport(DllName, EntryPoint = "ov_info", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr Info(IntPtr vf, int link);

		[DllImport(DllName, EntryPoint = "ov_read", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Read(IntPtr vf, IntPtr buffer, int length, int bigendianp, int word, int sgned, ref int bitstream);

        [DllImport(DllName, EntryPoint = "ov_time_total", CallingConvention = CallingConvention.Cdecl)]
		public static extern double TimeTotal(IntPtr vf, int link);

		[DllImport(DllName, EntryPoint = "ov_clear", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Clear(IntPtr vf);

		[DllImport(DllName, EntryPoint = "ov_time_seek", CallingConvention = CallingConvention.Cdecl)]
		public static extern int TimeSeek(IntPtr vf, double s);
	}
}
