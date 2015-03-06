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
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public readonly static Vector2 Zero = new Vector2(0.0f, 0.0f);

        public float Length
        {
            get
            {
                return Mathf.Sqrt(X * X + Y * Y);
            }
        }

        public float Magnitude
        {
            get
            {
                return Mathf.Sqrt(X * X + Y * Y);
            }
        }

        public float LengthSquared
        {
            get
            {
                return X * X + Y * Y;
            }
        }

        public Vector2 Normalized
        {
            get
            {
                Vector2 result = new Vector2(X, Y);
                result.Normalize();

                return result;
            }
        }

        public Vector2 Perpendicular
        {
            get
            {
                return new Vector2(Y, -X);
            }
        }

        public Vector2 CCWPerpendicular
        {
            get
            {
                return new Vector2(-Y, X);
            }
        }

        public float Angle
        {
            get
            {
                return Mathf.Atan2(Y, X);
            }
        }

        public void Normalize()
        {
            bool isXZero = Mathf.IsZero(X);
            bool isYZero = Mathf.IsZero(Y);

            if (!isXZero || !isYZero)
            {
                if (isXZero)
                {
                    Y = Mathf.Sign(Y);
                    X = 0.0f;
                }
                else if (isYZero)
                {
                    X = Mathf.Sign(X);
                    Y = 0.0f;
                }
                else
                {
                    float length = Length;

                    float invLength = 1.0f / length;

                    X *= invLength;
                    Y *= invLength;
                }
            }
            else
            {
                X = 0;
                Y = 0;
            }
        }

        public float Dot(Vector2 vector2)
        {
            return X * vector2.X + Y * vector2.Y;
        }

        public Vector2 Cross(float f)
        {
            return new Vector2(-f * Y, f * X);
        }

        public float Cross(Vector2 b)
        {
            return X * b.Y - Y * b.X;
        }

        public Vector2 RotateTowards(Vector2 target, float maxRadiansDelta, float maxMagnitudeDelta)
        {
            float asAngle = Angle;
            float targetAsAngle = target.Angle;

            float length = Length;
            float targetLength = target.Length;

            if (asAngle > targetAsAngle)
            {
                asAngle -= maxRadiansDelta;

                if (asAngle < targetAsAngle)
                {
                    asAngle = targetAsAngle;
                }
            }
            else if (asAngle < targetAsAngle)
            {
                asAngle += maxRadiansDelta;

                if (asAngle > targetAsAngle)
                {
                    asAngle = targetAsAngle;
                }
            }

            if (length > targetLength)
            {
                length -= maxMagnitudeDelta;

                if (length < targetLength)
                {
                    length = targetLength;
                }
            }
            else if (length < targetLength)
            {
                length += maxMagnitudeDelta;

                if (length > targetLength)
                {
                    length = targetLength;
                }
            }

            return Vector2.FromAngle(asAngle) * length;
        }

        public static Vector2 Interpolate(Vector2 from, Vector2 to, float alpha)
        {
            float oneMinusAlpha = 1 - alpha;

            float x = from.X * oneMinusAlpha + to.X * alpha;
            float y = from.Y * oneMinusAlpha + to.Y * alpha;

            return new Vector2(x, y);
        }

        public static Vector2 FromAngle(float angle)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public Vector2 Rotate(float angle)
        {
            float length = Length;
            return FromAngle(Mathf.Atan2(Y, X) + angle) * length;
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            return (b - a).Length;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static float operator *(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static Vector2 operator *(Vector2 a, float b)
        {
            return new Vector2(a.X * b, a.Y * b);
        }

        public static Vector2 operator *(float a, Vector2 b)
        {
            return new Vector2(a * b.X, a * b.Y);
        }

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public override string ToString()
        {
            return "Vector2: " + X + " " + Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Vector2))
            {
                return false;
            }

            Vector2 objAsVector2 = (Vector2)obj;

            return X == objAsVector2.X && Y == objAsVector2.Y;
        }

        public static Vector2 Lerp(Vector2 from, Vector2 to, float t)
        {
            float clampedT = Mathf.Clamp(t, 0, 1);

            return from * (1.0f - clampedT) + to * clampedT;
        }
    }
}
