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
using Swarm2D.Library;
using Swarm2D.WindowsFramework.Native.OpenAL;

namespace Swarm2D.WindowsFramework
{
	class AudioSource
	{
        public uint[] Buffers { get; private set; }
        public int Source { get; private set; }

        public AudioSource()
		{
			Buffers = new uint[AudioContext.BufferCount];
		}

		public void Generate()
		{
			AL.GenBuffers(AudioContext.BufferCount, Buffers);
			Source = AL.GenSource();
		}

		public void Destroy()
		{
			AL.DeleteSource(Source);
			AL.DeleteBuffers(AudioContext.BufferCount, Buffers);
		}

	    public void SetPosition(Vector2 position)
	    {
	        AL.Source3f(Source, ALDefinitions.Position, position.X, position.Y, 50.0f);
	        int errorValue = AL.GetError();

            AL.Source3f(Source, ALDefinitions.Velocity, 0.0f, 0.0f, 0.0f);
            errorValue = AL.GetError();
        }
	}
}
