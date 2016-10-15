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
		const int VorbisFilePointerCount = 64;

		private FileStream _stream;
		private CallBacks _callBacks;
		private IntPtr _vorbisFile;
		private VorbisInfo _vorbisInfo;

        private static LinkedList<IntPtr> _freeVorbisFilePointers;

        public string FileName { get; private set; }
        public int Format { get; private set; }
        public int Frequency { get; private set; }
        public int Channels { get; private set; }
        public int BufferSize { get; private set; }
        public float Length { get; private set; }

        static OggStream()
		{
			_freeVorbisFilePointers = new LinkedList<IntPtr>();

			for (int i = 0; i < VorbisFilePointerCount; i++)
			{
				IntPtr newVorbisFile = Marshal.AllocHGlobal(1024);
				_freeVorbisFilePointers.AddLast(newVorbisFile);

				for (int j = 0; j < 1024; j++)
				{
					Marshal.WriteByte(newVorbisFile, j, 0);
				}
			}
		}

        private static IntPtr GetVorbisFilePointer()
		{
			IntPtr value = _freeVorbisFilePointers.Last.Value;
			_freeVorbisFilePointers.RemoveLast();
			return value;
		}

        private static void AddVorbisFilePointer(IntPtr pointer)
		{
			_freeVorbisFilePointers.AddLast(pointer);
		}

		public OggStream(string fileName)
		{
			_vorbisFile = IntPtr.Zero;

			FileName = fileName;

			_callBacks = new CallBacks();

			_callBacks.ReadFunc = ReadFunc;
			_callBacks.SeekFunc = SeekFunc;
			_callBacks.CloseFunc = CloseFunc;
			_callBacks.TellFunc = TellFunc;

			_vorbisFile = GetVorbisFilePointer();			
		}

		public bool Open()
		{
			_stream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);

			if (VorbisFile.OpenCallbacks(new IntPtr(1), _vorbisFile, null, 0, _callBacks) == 0)
			{
				IntPtr vorbisInfoPtr = VorbisFile.Info(_vorbisFile, -1);

				if (vorbisInfoPtr != IntPtr.Zero)
				{
					_vorbisInfo = VorbisInfo.FromUnmanagedMemory(vorbisInfoPtr);
					Frequency = _vorbisInfo.Rate;
					Channels = _vorbisInfo.Channels;
					Length = (float)VorbisFile.TimeTotal(_vorbisFile, -1);

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
			VorbisFile.Clear(_vorbisFile);
			AddVorbisFilePointer(_vorbisFile);
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
			VorbisFile.TimeSeek(_vorbisFile, 0);
		}

		public int DecodeOggVorbis(byte[] decodeBuffer)
		{
			int currentSection = 0;
			int bytesDone = 0;

			while (true)
			{
			    int decodeSize = 0;

			    using (AutoPinner autoPinner = new AutoPinner(decodeBuffer))
			    {
			        IntPtr decodeBufferPtr = autoPinner;
                    decodeSize = VorbisFile.Read(_vorbisFile, new IntPtr(decodeBufferPtr.ToInt32() + bytesDone), BufferSize - bytesDone, 0, 2, 1, ref currentSection);
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
