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
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Swarm2D.WindowsFramework.Native.OpenAL
{
    internal static class AL
    {
        public const string DLLName = "OpenAL32.dll";

		[DllImport(DLLName, EntryPoint = "alEnable", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Enable(int capability);

		[DllImport(DLLName, EntryPoint = "alDisable", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Disable(int capability);

		[DllImport(DLLName, EntryPoint = "alIsEnabled", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsEnabled(int capability);

		[DllImport(DLLName, EntryPoint = "alHint", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Hint(int target, int mode);

		[DllImport(DLLName, EntryPoint = "alGetString", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetString(int param);

		[DllImport(DLLName, EntryPoint = "alGetBoolean", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetBoolean(int param);

		[DllImport(DLLName, EntryPoint = "alGetBooleanv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetBooleanv(int param, bool[] data);

		[DllImport(DLLName, EntryPoint = "alGetInteger", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetInteger(int param);

		[DllImport(DLLName, EntryPoint = "alGetIntegerv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetIntegerv(int param, int[] data);

		[DllImport(DLLName, EntryPoint = "alGetFloat", CallingConvention = CallingConvention.Cdecl)]
        public static extern float GetFloat(int param);

		[DllImport(DLLName, EntryPoint = "alGetFloatv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetFloatv(int param, float[] data);

		[DllImport(DLLName, EntryPoint = "alGetDouble", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetDouble(int param);

		[DllImport(DLLName, EntryPoint = "alGetDoublev", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetDoublev(int param, double[] data);

		[DllImport(DLLName, EntryPoint = "alGetError", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetError();

		[DllImport(DLLName, EntryPoint = "alIsExtensionPresent", CallingConvention = CallingConvention.Cdecl)]        
        public static extern bool IsExtensionPresent([MarshalAs(UnmanagedType.LPStr)] string fname);

		[DllImport(DLLName, EntryPoint = "alGetEnumValue", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetEnumValue(string ename);

		[DllImport(DLLName, EntryPoint = "alListeneri", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Listeneri(int param, int value);

		[DllImport(DLLName, EntryPoint = "alListenerf", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Listenerf(int param, float value);

		[DllImport(DLLName, EntryPoint = "alListener3f", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Listener3f(int param, float f1, float f2, float f3);

		[DllImport(DLLName, EntryPoint = "alListenerfv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Listenerfv(int param, float[] values);

		[DllImport(DLLName, EntryPoint = "alGetListeneriv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetListeneriv(int param, int[] values);

		[DllImport(DLLName, EntryPoint = "alGetListenerfv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetListenerfv(int param, float[] values);

		[DllImport(DLLName, EntryPoint = "alGenSources", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GenSources(int n, int[] sources);

		[DllImport(DLLName, EntryPoint = "alDeleteSources", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteSources(int n, int[] sources);

		[DllImport(DLLName, EntryPoint = "alIsSource", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsSource(int id);

		[DllImport(DLLName, EntryPoint = "alSourcei", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Sourcei(int source, int param, int value);

		[DllImport(DLLName, EntryPoint = "alSourcef", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Sourcef(int source, int param, float value);

		[DllImport(DLLName, EntryPoint = "alSource3f", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Source3f(int source, int param, float v1, float v2, float v3);

		[DllImport(DLLName, EntryPoint = "alSourcefv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Sourcefv(int source, int param, float[] values);

		[DllImport(DLLName, EntryPoint = "alGetSourcei", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetSourcei(int source, int param, ref int value);

		[DllImport(DLLName, EntryPoint = "alGetSourcef", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetSourcef(int source, int param, ref float value);

		[DllImport(DLLName, EntryPoint = "alGetSource3f", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetSource3f(int source, int param, float[] v1, float[] v2, float[] v3);

		[DllImport(DLLName, EntryPoint = "alGetSourcefv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetSourcefv(int source, int param, float[] values);

		[DllImport(DLLName, EntryPoint = "alSourcePlay", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourcePlay(int source);

		[DllImport(DLLName, EntryPoint = "alSourcePause", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourcePause(int source);

		[DllImport(DLLName, EntryPoint = "alSourceStop", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceStop(int source);

		[DllImport(DLLName, EntryPoint = "alSourceRewind", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceRewind(int source);

		[DllImport(DLLName, EntryPoint = "alSourcePlayv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourcePlayv(int n, int[] sources);

		[DllImport(DLLName, EntryPoint = "alSourceStopv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceStopv(int n, int[] sources);

		[DllImport(DLLName, EntryPoint = "alSourceRewindv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceRewindv(int n, int[] sources);

		[DllImport(DLLName, EntryPoint = "alSourcePausev", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourcePausev(int n, int[] sources);

		[DllImport(DLLName, EntryPoint = "alGenBuffers", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GenBuffers(int n, uint[] buffers);

		[DllImport(DLLName, EntryPoint = "alDeleteBuffers", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteBuffers(int n, uint[] buffers);

		[DllImport(DLLName, EntryPoint = "alIsBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsBuffer(int buffer);

		[DllImport(DLLName, EntryPoint = "alBufferData", CallingConvention = CallingConvention.Cdecl)]
        public static extern void BufferData(uint buffer, int format, byte[] data, int size, int freq);

		[DllImport(DLLName, EntryPoint = "alBufferData", CallingConvention = CallingConvention.Cdecl)]
		public static extern void BufferData(uint buffer, int format, IntPtr data, int size, int freq);

		[DllImport(DLLName, EntryPoint = "alGetBufferi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetBufferi(int buffer, int param, int[] value);

		[DllImport(DLLName, EntryPoint = "alGetBufferf", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetBufferf(int buffer, int param, float[] value);

		[DllImport(DLLName, EntryPoint = "alSourceQueueBuffers", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceQueueBuffers(int source, int n, int[] buffers);

		[DllImport(DLLName, EntryPoint = "alSourceUnqueueBuffers", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceUnqueueBuffers(int source, int n, int[] buffers);

		[DllImport(DLLName, EntryPoint = "alDistanceModel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DistanceModel(int value);

		[DllImport(DLLName, EntryPoint = "alDopplerFactor", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DopplerFactor(float value);

		[DllImport(DLLName, EntryPoint = "alDopplerVelocity", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DopplerVelocity(float value);

        public static void DeleteBuffer(uint buffer)
        {
            uint[] buffers = { buffer };
            AL.DeleteBuffers(1, buffers);
        }

        public static void DeleteSource(int source)
        {
            int[] sources = { source };
            AL.DeleteSources(1, sources);
        }

        public static uint GenBuffer()
        {
            uint[] buffers = new uint[1];
            AL.GenBuffers(1, buffers);
            return buffers[0];
        }

        public static int GenSource()
        {
            int[] sources = new int[1];
            AL.GenSources(1, sources);
            return sources[0];
        }
    }
}
