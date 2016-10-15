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
using System.Security;
using System.Runtime.InteropServices;

namespace Swarm2D.WindowsFramework.Native.OpenAL
{
	internal static class ALC
	{
		public const string DLLName = "OpenAL32.dll";

		[DllImport(DLLName, EntryPoint = "alcCreateContext", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr CreateContext(IntPtr device, int[] attrlist);

		[DllImport(DLLName, EntryPoint = "alcMakeContextCurrent", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern int MakeContextCurrent(IntPtr context);

		[DllImport(DLLName, EntryPoint = "alcProcessContext", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern void ProcessContext(IntPtr context);

		[DllImport(DLLName, EntryPoint = "alcSuspendContext", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern void SuspendContext(IntPtr context);

		[DllImport(DLLName, EntryPoint = "alcDestroyContext", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern void DestroyContext(IntPtr context);

		[DllImport(DLLName, EntryPoint = "alcGetError", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern int GetError(IntPtr device);

		[DllImport(DLLName, EntryPoint = "alcGetCurrentContext", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr GetCurrentContext();

		[DllImport(DLLName, EntryPoint = "alcOpenDevice", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr OpenDevice([MarshalAs(UnmanagedType.LPStr)]string deviceName);

		[DllImport(DLLName, EntryPoint = "alcCloseDevice", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern void CloseDevice(IntPtr device);

		[DllImport(DLLName, EntryPoint = "alcIsExtensionPresent", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern bool IsExtensionPresent(IntPtr device, string extName);

		[DllImport(DLLName, EntryPoint = "alcGetProcAddress", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr GetProcAddress(IntPtr device, byte[] funcName);

		[DllImport(DLLName, EntryPoint = "alcGetEnumValue", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern int GetEnumValue(IntPtr device, byte[] enumName);

		[DllImport(DLLName, EntryPoint = "alcGetContextDevice", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr GetContextsDevice(IntPtr context);

		[DllImport(DLLName, EntryPoint = "alcGetIntegerv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern void GetIntegerv(IntPtr device, int param, int intsize, out int data);

		[DllImport(DLLName, EntryPoint = "alcGetString", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr GetString(IntPtr device, int param);

		public static int GetIntegerv(IntPtr device, int param)
		{
			int result;
			ALC.GetIntegerv(device, param, sizeof(int), out result);

			return result;
		}

		public static string GetString(IntPtr device, int param, string seperator)
		{
			int offset = -1;

			IntPtr stringPtr = GetString(device, param);
			StringBuilder stringBuilder = new StringBuilder();

			while (true)
			{
                offset++;
				char c = (char)Marshal.ReadByte(stringPtr, offset);

				if (c == 0)
				{
					if ((char)Marshal.ReadByte(stringPtr, offset + 1) == 0)
					{
						return stringBuilder.ToString();
					}

                    stringBuilder.Append(seperator);
					continue;
				}

                stringBuilder = stringBuilder.Append(c);
			}
		}
	}
}
