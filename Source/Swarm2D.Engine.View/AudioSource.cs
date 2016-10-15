﻿/******************************************************************************
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
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;

namespace Swarm2D.Engine.View
{
    public class AudioSource : Component
    {
        public AudioClip Clip { get; set; }

        private IOSystem _ioSystem;
        private IAudioJob _audioJob;

        protected override void OnAdded()
        {
            base.OnAdded();

            _ioSystem = Engine.RootEntity.GetComponent<IOSystem>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_audioJob != null)
            {
                _audioJob.Stop();
            }
        }

        public void PlayOneShot()
        {
            _audioJob = _ioSystem.PlayOneShotAudio(Clip);
        }

        public void Play()
        {
            _audioJob = _ioSystem.PlayAudio(Clip);
        }

        public void Stop()
        {
            if (_audioJob != null)
            {
                _audioJob.Stop();
                _audioJob = null;
            }
        }

        public static void PlayClipAtPoint(Scene scene, AudioClip clip, Vector2 position)
        {
            
        }
    }

    public interface IAudioJob
    {
        bool Finished { get; }
        void Stop();
    }
}
