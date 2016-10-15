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
using System.Threading;
using System.Runtime.InteropServices;
using Swarm2D.Library;
using Swarm2D.WindowsFramework.Native.OpenAL;

namespace Swarm2D.WindowsFramework
{
	class AudioContext
	{
		internal const int BufferCount = 32;
		internal const int SourceCount = 256;

        private Thread _audioThread;

		private List<AudioSource> _freeAudioSources;
        private List<AudioSource> _runningAudioSources;

        private List<AudioJob> _audioJobs;

        private bool _stopAllAudioEvent = false;

	    private bool _running;

        private IntPtr _device = IntPtr.Zero;
        private IntPtr _context = IntPtr.Zero;

        public IntPtr Device { get { return _device; } }
        public IntPtr Context { get { return _context; } }

	    private Vector2 _listenerPosition;

	    public Vector2 ListenerPosition
	    {
	        get
	        {
	            return _listenerPosition;
	        }
	        set
	        {
	            int errorValue = 0;

	            _listenerPosition = value;
                AL.Listener3f(ALDefinitions.Position, _listenerPosition.X, _listenerPosition.Y, 0.0f);
	            errorValue = AL.GetError();

                float[] orientation = new float[6];

	            orientation[0] = 0;
	            orientation[1] = 0;
	            orientation[2] = 1;
            
                orientation[3] = 0;
                orientation[4] = 1;
                orientation[5] = 0;

                AL.Listenerfv(ALDefinitions.Orientation, orientation);
                errorValue = AL.GetError();

                AL.Listener3f(ALDefinitions.Velocity, 0.0f, 0.0f, 0.0f);
                errorValue = AL.GetError();
            }
	    }

        private void CreateAudioContext()
        {
            _device = ALC.OpenDevice("");
            _context = ALC.CreateContext(_device, null);

            ALC.MakeContextCurrent(_context);
        }

        private void DisposeAudioContext()
        {
            if ((_context == IntPtr.Zero) || (_device == IntPtr.Zero))
            {
                return;
            }

            ALC.DestroyContext(_context);
            ALC.CloseDevice(_device);

            _context = IntPtr.Zero;
            _device = IntPtr.Zero;
        }

        internal void Start()
		{
            _freeAudioSources = new List<AudioSource>(SourceCount);
			_runningAudioSources = new List<AudioSource>(SourceCount);
			_audioJobs = new List<AudioJob>(SourceCount);

            _running = true;

            _audioThread = new Thread(AudioLoop);
		    _audioThread.Name = "Audio";
            _audioThread.Start();
		}

		internal void Stop()
		{
		    _running = false;
		}

		internal AudioSource GetAnAudioSource()
		{
			if (_freeAudioSources.Count > 0)
			{
				AudioSource freeAudioSource = _freeAudioSources[_freeAudioSources.Count - 1];
				_freeAudioSources.RemoveAt(_freeAudioSources.Count - 1);
				return freeAudioSource;
			}

			return null;
		}

		internal void FreeAudioSource(AudioSource audioSource)
		{
			_runningAudioSources.Remove(audioSource);
			_freeAudioSources.Add(audioSource);
		}

		private void AudioLoop()
		{
		    CreateAudioContext();

            ListenerPosition = Vector2.Zero;

            for (int i = 0; i < SourceCount; i++)
			{
				AudioSource audioSource = new AudioSource();
				_freeAudioSources.Add(audioSource);
				audioSource.Generate();
			}

			while (_running)
			{
				Thread.Sleep(1);

				if (_stopAllAudioEvent)
				{
					_stopAllAudioEvent = false;

					lock (_audioJobs)
					{
						for (int i = 0; i < _audioJobs.Count; i++)
						{
							_audioJobs[i].Stop();
						}

						_audioJobs.Clear();
					}
				}

				for (int i = 0; i < _audioJobs.Count; i++)
				{
					AudioJob audioJob = null;

					lock (_audioJobs)
					{
						audioJob = _audioJobs[i];
					}

					audioJob.DoJob();

					if (audioJob.Finished)
					{
						lock (_audioJobs)
						{
							_audioJobs.RemoveAt(i);
						}

						i--;
					}
				}
			}

		    DisposeAudioContext();
		}

		public AudioJob PlayOneShotAudio(OggAudioClip audioClip)
		{
			AudioJob audioJob = new AudioJob(this, false, audioClip);

			lock (_audioJobs)
			{
				_audioJobs.Add(audioJob);
			}

		    return audioJob;
		}

        public AudioJob PlayOneShotAudio(OggAudioClip audioClip, Vector2 position)
        {
            AudioJob audioJob = new AudioJob(this, false, audioClip, position);

            lock (_audioJobs)
            {
                _audioJobs.Add(audioJob);
            }

            return audioJob;
        }

        public void StopAllAudio()
		{
			_stopAllAudioEvent = true;
		}

		public AudioJob PlayAudio(OggAudioClip audioClip)
		{
			AudioJob audioJob = new AudioJob(this, true, audioClip);

			lock (_audioJobs)
			{
				_audioJobs.Add(audioJob);
			}

            return audioJob;
        }
	}
}
