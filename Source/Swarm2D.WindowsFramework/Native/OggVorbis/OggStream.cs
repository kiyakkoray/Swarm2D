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
using System.IO;
using System.Runtime.InteropServices;
using Swarm2D.WindowsFramework.Native.OpenAL;

namespace Swarm2D.WindowsFramework.Native.OggVorbis
{
    class OggStream
    {
        const int VorbisBufferCount = 64;

        private FileStream _stream;
        private CallBacks _callBacks;
        private byte[] _vorbisFile;
        private VorbisInfo _vorbisInfo;

        private static List<byte[]> _freeVorbisBuffers;

        public string FileName { get; private set; }
        public int Format { get; private set; }
        public int Frequency { get; private set; }
        public int Channels { get; private set; }
        public int BufferSize { get; private set; }
        public float Length { get; private set; }

        private GCHandle _vorbisFileHandle;

        static OggStream()
        {
            _freeVorbisBuffers = new List<byte[]>();
            for (int i = 0; i < VorbisBufferCount; i++)
            {
                byte[] buffer = new byte[1024];
                _freeVorbisBuffers.Add(buffer);
            }
        }

        private static byte[] GetVorbisBuffer()
        {
            byte[] value = null;

            if (_freeVorbisBuffers.Count > 0)
            {
                int index = _freeVorbisBuffers.Count - 1;

                value = _freeVorbisBuffers[index];
                _freeVorbisBuffers.RemoveAt(index);
            }
            else
            {
                value = new byte[1024];
            }

            return value;
        }

        private static void AddVorbisBuffer(byte[] buffer)
        {
            _freeVorbisBuffers.Add(buffer);
        }

        public OggStream(string fileName)
        {
            FileName = fileName;

            _callBacks = new CallBacks();
            _callBacks.ReadFunc = ReadFunc;
            _callBacks.SeekFunc = SeekFunc;
            _callBacks.CloseFunc = CloseFunc;
            _callBacks.TellFunc = TellFunc;

            _vorbisFile = GetVorbisBuffer();
        }

        public bool Open()
        {
            _stream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            _vorbisFileHandle = GCHandle.Alloc(_vorbisFile, GCHandleType.Pinned);
            IntPtr vorbisFilePtr = _vorbisFileHandle.AddrOfPinnedObject();

            if (VorbisFile.OpenCallbacks(new IntPtr(1), vorbisFilePtr, null, 0, _callBacks) == 0)
            {
                IntPtr vorbisInfoPtr = VorbisFile.Info(vorbisFilePtr, -1);

                if (vorbisInfoPtr != IntPtr.Zero)
                {
                    _vorbisInfo = VorbisInfo.FromUnmanagedMemory(vorbisInfoPtr);
                    Frequency = _vorbisInfo.Rate;
                    Channels = _vorbisInfo.Channels;
                    Length = (float)VorbisFile.TimeTotal(vorbisFilePtr, -1);

                    if (_vorbisInfo.Channels == 1)
                    {
                        Format = ALDefinitions.FormatMono16;
                        BufferSize = Frequency / 2;
                        BufferSize -= (BufferSize % 2);
                    }
                    else if (_vorbisInfo.Channels == 2)
                    {
                        Format = ALDefinitions.FormatStereo16;
                        BufferSize = Frequency;
                        BufferSize -= (BufferSize % 4);
                    }
                    else if (_vorbisInfo.Channels == 4)
                    {
                        Format = AL.GetEnumValue("AL_FORMAT_QUAD16");
                        BufferSize = Frequency * 2;
                        BufferSize -= (BufferSize % 8);
                    }
                    else if (_vorbisInfo.Channels == 6)
                    {
                        Format = AL.GetEnumValue("AL_FORMAT_51CHN16");
                        BufferSize = Frequency * 3;
                        BufferSize -= (BufferSize % 12);
                    }
                }

                if (Format != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void Close()
        {
            IntPtr vorbisFilePtr = _vorbisFileHandle.AddrOfPinnedObject();
            VorbisFile.Clear(vorbisFilePtr);

            _vorbisFileHandle.Free();
            AddVorbisBuffer(_vorbisFile);
        }

        private int ReadFunc(IntPtr ptr, int size, int nmemb, IntPtr datasource)
        {
            byte[] data = new byte[size * nmemb];

            int readCount = _stream.Read(data, 0, size * nmemb);
            Marshal.Copy(data, 0, ptr, readCount);
            return readCount;
        }

        private int SeekFunc(IntPtr datasource, long offset, int whence)
        {
            if (whence == 1)
            {
                _stream.Seek(offset, SeekOrigin.Current);
            }
            else if (whence == 2)
            {
                _stream.Seek(offset, SeekOrigin.End);
            }
            else if (whence == 0)
            {
                _stream.Seek(offset, SeekOrigin.Begin);
            }

            return 0;
        }

        private int CloseFunc(IntPtr datasource)
        {
            _stream.Close();
            return 0;
        }

        private int TellFunc(IntPtr datasource)
        {
            return (int)_stream.Position;
        }

        public void SeekBegin()
        {
            IntPtr vorbisFilePtr = _vorbisFileHandle.AddrOfPinnedObject();
            VorbisFile.TimeSeek(vorbisFilePtr, 0);
        }

        public int DecodeOggVorbis(byte[] decodeBuffer)
        {
            int currentSection = 0;
            int bytesDone = 0;

            while (true)
            {
                int decodeSize = 0;

                using (AutoPinner decodeBufferPinner = new AutoPinner(decodeBuffer))
                {
                    IntPtr decodeBufferPtr = decodeBufferPinner;

                    IntPtr vorbisFilePtr = _vorbisFileHandle.AddrOfPinnedObject();
                    decodeSize = VorbisFile.Read(vorbisFilePtr, new IntPtr(decodeBufferPtr.ToInt32() + bytesDone), BufferSize - bytesDone, 0, 2, 1, ref currentSection);
                }

                if (decodeSize > 0)
                {
                    bytesDone += decodeSize;

                    if (bytesDone >= BufferSize)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            if (Channels == 6)
            {
                //TODO
            }

            return bytesDone;
        }
    }
}
