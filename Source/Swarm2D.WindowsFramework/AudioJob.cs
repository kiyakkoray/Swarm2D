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
using Swarm2D.Engine.View;
using Swarm2D.Library;
using Swarm2D.WindowsFramework.Native.OggVorbis;
using Swarm2D.WindowsFramework.Native.OpenAL;

namespace Swarm2D.WindowsFramework
{
    class AudioJob : IAudioJob
    {
        private AudioSource _audioSource;
        private OggStream _oggStream;

        private AudioContext _audioContext;

        private bool _firstLoop;

        public bool Finished { get; private set; }
        public bool Loop { get; private set; }
        public OggAudioClip Clip { get; private set; }

        public Vector2 Position { get; private set; }

        private byte[] _decodeBuffer;

        public AudioJob(AudioContext audioContext, bool loop, OggAudioClip clip)
            :this(audioContext, loop, clip, Vector2.Zero)
        {
        }

        public AudioJob(AudioContext audioContext, bool loop, OggAudioClip clip, Vector2 position)
        {
            _audioContext = audioContext;
            Loop = loop;
            Clip = clip;
            Finished = false;
            _oggStream = new OggStream(Resources.ResourcesPath + @"\" + clip.Name + ".ogg");
            _oggStream.Open();
            _firstLoop = true;
            _audioSource = null;
            Position = position;

            _decodeBuffer = new byte[65536];
        }

        public void DoJob()
        {
            if (_firstLoop)
            {
                _audioSource = _audioContext.GetAnAudioSource();
                _audioSource.SetPosition(Position);

                for (int iLoop = 0; iLoop < AudioContext.BufferCount; iLoop++)
                {
                    int bytesWritten = _oggStream.DecodeOggVorbis(_decodeBuffer);

                    if (bytesWritten > 0)
                    {
                        AL.BufferData(_audioSource.Buffers[iLoop], _oggStream.Format, _decodeBuffer, bytesWritten, _oggStream.Frequency);
                        AL.SourceQueueBuffers(_audioSource.Source, 1, new int[] { (int)_audioSource.Buffers[iLoop] });
                    }
                }

                AL.Sourcef(_audioSource.Source, ALDefinitions.Gain, Loop ? 1.0f : 0.3f);
                AL.SourcePlay(_audioSource.Source);

                _firstLoop = false;
            }
            else if (!Finished)
            {
                int buffersProcessed = 0;

                AL.GetSourcei(_audioSource.Source, ALDefinitions.BuffersProcessed, ref buffersProcessed);

                int[] dummyIntArray = new int[1];

                while (buffersProcessed > 0)
                {
                    int uiBuffer = 0;
                    AL.SourceUnqueueBuffers(_audioSource.Source, 1, dummyIntArray);
                    uiBuffer = dummyIntArray[0];

                    int bytesWritten = _oggStream.DecodeOggVorbis(_decodeBuffer);

                    if (Loop && bytesWritten <= 0)
                    {
                        _oggStream.SeekBegin();
                        bytesWritten = _oggStream.DecodeOggVorbis(_decodeBuffer);
                    }

                    if (bytesWritten > 0)
                    {
                        AL.BufferData((uint)uiBuffer, _oggStream.Format, _decodeBuffer, bytesWritten, _oggStream.Frequency);
                        AL.SourceQueueBuffers(_audioSource.Source, 1, dummyIntArray);
                    }

                    buffersProcessed--;
                }

                int state = 0;
                AL.GetSourcei(_audioSource.Source, ALDefinitions.SourceState, ref state);

                if (state != ALDefinitions.Playing)
                {
                    int queuedBuffers = 0;
                    AL.GetSourcei(_audioSource.Source, ALDefinitions.BuffersQueued, ref queuedBuffers);

                    if (queuedBuffers > 0)
                    {
                        AL.SourcePlay(_audioSource.Source);
                    }
                    else
                    {
                        if (Loop)
                        {

                        }
                        else
                        {
                            Finished = true;
                            _oggStream.Close();
                            _audioContext.FreeAudioSource(_audioSource);
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            if (!Finished)
            {
                AL.SourceStop(_audioSource.Source);

                int buffersProcessed = 0;

                AL.GetSourcei(_audioSource.Source, ALDefinitions.BuffersProcessed, ref buffersProcessed);

                int[] dummyIntArray = new int[1];

                while (buffersProcessed > 0)
                {
                    AL.SourceUnqueueBuffers(_audioSource.Source, 1, dummyIntArray);
                    buffersProcessed--;
                }

                _oggStream.Close();
                _audioContext.FreeAudioSource(_audioSource);
                Finished = true;
            }
        }
    }
}
