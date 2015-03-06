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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Swarm2D.Library;

namespace Swarm2D.WindowsFramework
{
    public class WindowsDebug : IDebug
    {
        private Thread _debugThread;

        private Queue<string> _logQueue;

        public WindowsDebug()
        {
            _logQueue = new Queue<string>();
            _debugThread = new Thread(DebugLoop);
            _debugThread.Start();
        }

        void IDebug.Log(object log)
        {
            lock (_logQueue)
            {
                _logQueue.Enqueue(log.ToString());
            }
        }

        void IDebug.Assert(bool condition, string message)
        {
            StackTrace stackTrace = new StackTrace();
            System.Windows.Forms.MessageBox.Show(message, "Assertion Failed!\n" + stackTrace.ToString(), MessageBoxButtons.OK);
        }

        private void DebugLoop()
        {
            while (true)
            {
                int currentLogCount = 0;

                lock (_logQueue)
                {
                    currentLogCount = _logQueue.Count;
                }

                while (currentLogCount > 0)
                {
                    string currentMessage = "";

                    lock (_logQueue)
                    {
                        currentMessage = _logQueue.Dequeue();
                    }

                    Console.WriteLine(currentMessage);

                    currentLogCount--;
                }

                Thread.Sleep(1);
            }
        }
    }
}