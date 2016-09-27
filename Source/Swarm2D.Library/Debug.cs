﻿/******************************************************************************
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

namespace Swarm2D.Library
{
    public static class Debug
    {
        private static IDebug _debug;

        public static void Initialize(IDebug debug)
        {
            _debug = debug;
        }

        public static void Log(object log)
        {
            if (_debug == null)
            {
#if !WINDOWS_UWP
                Console.WriteLine(log);
#endif
            }
            else
            {
                _debug.Log(log);
            }
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message = "assertion")
        {
            if (!condition)
            {
                if (_debug == null)
                {
#if !WINDOWS_UWP
                    Console.WriteLine(message);
#endif
                }
                else
                {
                    _debug.Assert(condition, message);
                }
            }
        }
    }

    public interface IDebug
    {
        void Log(object log);

        void Assert(bool condition, string message);
    }
}