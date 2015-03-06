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

namespace Swarm2D.Library
{
    public static class Mathf
    {
        public const float PI = 3.14159265359f;
        public const float Deg2Rad = PI / 180.0f;
        public const float Rad2Deg = 180.0f / PI;

        public const float Epsilon = 0.00001f;

        public static float Sqrt(float f)
        {
            return (float)Math.Sqrt(f);
        }

        public static float Abs(float f)
        {
            return (float)Math.Abs(f);
        }

        public static float Cos(float radian)
        {
            return (float)Math.Cos(radian);
        }

        public static float Sin(float radian)
        {
            return (float)Math.Sin(radian);
        }

        public static float Acos(float f)
        {
            return (float)Math.Acos(f);
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        public static float Clamp(float value, float min, float max)
        {
            return value > max ? max : (value < min ? min : value);
        }

        public static int Clamp(int value, int min, int max)
        {
            return value > max ? max : (value < min ? min : value);
        }

        public static float Min(float a, float b)
        {
            return a > b ? b : a;
        }

        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }

        public static bool IsZero(float f)
        {
            return f < Epsilon && f > -Epsilon;
        }

        public static bool IsZero(Vector2 vector2)
        {
            return IsZero(vector2.X) && IsZero(vector2.Y);
        }

        public static float Sign(float f)
        {
            return Math.Sign(f);
        }

        public static float Ceil(float f)
        {
            return (float)Math.Ceiling(f);
        }
    }
}