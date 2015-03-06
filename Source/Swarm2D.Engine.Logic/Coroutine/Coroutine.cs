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
using Swarm2D.Engine.Core;

namespace Swarm2D.Engine.Logic
{
    public class Coroutine : Component
    {
        private IEnumerator<CoroutineTask> _enumerator;
        internal CoroutineMethod CoroutineMethod { get; set; }

        private CoroutineTask _currentTask;

        internal bool IsFinished { get; private set; }

        public object Parameter { get; internal set; }
        internal Component Owner { get; set; }

        internal void DoJob()
        {
            if (Owner.Entity.IsDestroyed)
            {
                IsFinished = true;
            }
            else
            {
                bool currentTaskIsNull = _currentTask == null;

                if (currentTaskIsNull)
                {
                    _enumerator = CoroutineMethod(this);
                }

                if (currentTaskIsNull || _currentTask.IsFinished)
                {
                    if (!currentTaskIsNull)
                    {
                        Entity.DeleteComponent(_currentTask);
                    }

                    IsFinished = !_enumerator.MoveNext();
                    _currentTask = _enumerator.Current;
                }

                if (!IsFinished)
                {
                    _currentTask.DoTask();
                }
            }
        }
    }

    public delegate IEnumerator<CoroutineTask> CoroutineMethod(Coroutine coroutine);
}