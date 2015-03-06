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
using System.Runtime.InteropServices;

namespace Swarm2D.Library
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4x4
    {
        public float M00;
        public float M10;
        public float M20;
        public float M30;

        public float M01;
        public float M11;
        public float M21;
        public float M31;

        public float M02;
        public float M12;
        public float M22;
        public float M32;

        public float M03;
        public float M13;
        public float M23;
        public float M33;

        public Vector2 Position
        {
            get
            {
                Vector2 result = new Vector2();
                result.X = M03;
                result.Y = M13;

                return result;
            }
        }

        static Matrix4x4()
        {
            Matrix4x4 identity = new Matrix4x4();

            identity.M00 = 1;
            identity.M11 = 1;
            identity.M22 = 1;
            identity.M33 = 1;

            Identity = identity;
        }

        public static Matrix4x4 OrthographicProjection(float left, float right, float bottom, float top)
        {
            const float far = 1;
            const float near = -1;

            Matrix4x4 result = new Matrix4x4();

            result.M00 = 2 / (right - left);
            result.M11 = 2 / (top - bottom);
            result.M22 = -2 / (far - near);

            result.M03 = -(right + left) / (right - left);
            result.M13 = -(top + bottom) / (top - bottom);
            result.M23 = -(far + near) / (far - near);
            result.M33 = 1;

            return result;
        }

        public static Matrix4x4 Transformation2D(Vector2 scale, float rotation, Vector2 position)
        {
            //Matrix4x4 result = new Matrix4x4();
            //
            //result.M22 = 1;
            //result.M33 = 1;
            //
            //result.M00 = Mathf.Cos(rotation) * scale.X;
            //result.M01 = -Mathf.Sin(rotation);
            //
            //result.M10 = Mathf.Sin(rotation);
            //result.M11 = Mathf.Cos(rotation) * scale.Y;
            //
            //result.M03 = position.X;
            //result.M13 = position.Y;
            //
            //return result;

            return (Position2D(position) * RotationAboutZ(rotation)) * Scale2D(scale);
        }

        public static Matrix4x4 Position2D(Vector2 position)
        {
            Matrix4x4 result = new Matrix4x4();

            result.M00 = 1;
            result.M11 = 1;
            result.M22 = 1;
            result.M33 = 1;

            result.M03 = position.X;
            result.M13 = position.Y;

            return result;
        }

        public static Matrix4x4 Scale2D(Vector2 scale)
        {
            Matrix4x4 result = new Matrix4x4();

            result.M00 = scale.X;
            result.M11 = scale.Y;
            result.M22 = 1;
            result.M33 = 1;

            return result;
        }

        public static Matrix4x4 RotationAboutZ(float radian)
        {
            Matrix4x4 result = new Matrix4x4();

            result.M00 = Mathf.Cos(radian);
            result.M01 = -Mathf.Sin(radian);

            result.M10 = Mathf.Sin(radian);
            result.M11 = Mathf.Cos(radian);

            result.M22 = 1;
            result.M33 = 1;

            return result;
        }

        public static readonly Matrix4x4 Identity;

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 result;

            result.M00 = a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20 + a.M03 * b.M30;
            result.M01 = a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21 + a.M03 * b.M31;
            result.M02 = a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22 + a.M03 * b.M32;
            result.M03 = a.M00 * b.M03 + a.M01 * b.M13 + a.M02 * b.M23 + a.M03 * b.M33;

            result.M10 = a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20 + a.M13 * b.M30;
            result.M11 = a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31;
            result.M12 = a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32;
            result.M13 = a.M10 * b.M03 + a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33;

            result.M20 = a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20 + a.M23 * b.M30;
            result.M21 = a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31;
            result.M22 = a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32;
            result.M23 = a.M20 * b.M03 + a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33;

            result.M30 = a.M30 * b.M00 + a.M31 * b.M10 + a.M32 * b.M20 + a.M33 * b.M30;
            result.M31 = a.M30 * b.M01 + a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31;
            result.M32 = a.M30 * b.M02 + a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32;
            result.M33 = a.M30 * b.M03 + a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33;

            return result;
        }

        public static Vector2 operator *(Matrix4x4 a, Vector2 b)
        {
            Vector2 result = new Vector2();

            result.X = a.M00 * b.X + a.M01 * b.Y + a.M03;
            result.Y = a.M10 * b.X + a.M11 * b.Y + a.M13;

            return result;
        }
    }
}
