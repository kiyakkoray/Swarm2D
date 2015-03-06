/******************************************************************************
Copyright (c) 2015 Koray Kiyakoglu

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
using UnityEngine;

namespace Swarm2D.UnityFramework
{
    public abstract class UnityEngineJob
    {
        internal bool Finished { get; set; }

        public abstract void DoJob();

        public void Wait()
        {
            while (!Finished)
            {

            }
        }
    }

    public class DebugLogJob : UnityEngineJob
    {
        private object _o;

        public DebugLogJob(object o)
        {
            _o = o;
        }

        public override void DoJob()
        {
            UnityEngine.Debug.Log(_o);
        }
    }

    public class LoadBinaryDataJob : UnityEngineJob
    {
        public byte[] LoadedBytes { get; private set; }

        public string Name { get; private set; }

        public LoadBinaryDataJob(string name)
        {
            Name = name;
        }

        public override void DoJob()
        {
            Debug.Log("loading binary " + Name);
            TextAsset binaryAsset = UnityEngine.Resources.Load(Name) as TextAsset;

            if (binaryAsset != null)
            {
                LoadedBytes = binaryAsset.bytes;
            }
        }
    }

    public class LoadTextDataJob : UnityEngineJob
    {
        public string LoadedText { get; private set; }

        public string Name { get; private set; }

        public LoadTextDataJob(string name)
        {
            Name = name;
        }

        public override void DoJob()
        {
            Debug.Log("loading text " + Name);
            TextAsset binaryAsset = UnityEngine.Resources.Load(Name) as TextAsset;

            if (binaryAsset != null)
            {
                LoadedText = binaryAsset.text;
            }
        }
    }
}
