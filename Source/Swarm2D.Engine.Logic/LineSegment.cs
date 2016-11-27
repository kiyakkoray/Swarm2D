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
using Swarm2D.Library;

namespace Swarm2D.Engine.Logic
{
    public class LineSegment
    {
        public Vector2 P1 { get; private set; }
        public Vector2 P2 { get; private set; }

        public Vector2 EdgeVector { get; private set; }
        public Vector2 EdgeDirection { get; private set; }
        public Vector2 Normal { get; private set; }

        public float Length { get; private set; }
        public float InverseLength { get; private set; }

        public Vector2 P1OnWorld { get; set; }
        public Vector2 P2OnWorld { get; set; }
        public Vector2 EdgeVectorOnWorld { get; set; }
        public Vector2 NormalOnWorld { get; set; }

        public LineSegment(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;

            EdgeVector = P2 - P1;
            Normal = EdgeVector.Perpendicular;

            Length = EdgeVector.Length;
            InverseLength = 1.0f / Length;

            EdgeDirection = EdgeVector.Normalized;
            Normal = Normal.Normalized;
        }

        public bool IsIntersects(LineSegment otherLineSegment, out Vector2 intersectionPoint)
        {
            //http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect

            Vector2 p = P1OnWorld;
            Vector2 r = EdgeVectorOnWorld;

            Vector2 q = otherLineSegment.P1OnWorld;
            Vector2 s = otherLineSegment.EdgeVectorOnWorld;

            if (!Mathf.IsZero(r * s))
            {
                float t = ((q - p) * s) / (r * s);
                float u = ((q - p) * r) / (r * s);

                if (t >= 0.0f && t <= 1.0f && u >= 0.0f && u <= 1.0f)
                {
                    intersectionPoint = p + r * t;
                    return true;
                }
            }

            intersectionPoint = Vector2.Zero;

            return false;
        }

        public Vector2 GetClosestPoint(Vector2 point)
        {
            Vector2 pointVector = point - P1;

            float u = pointVector.Dot(EdgeDirection) * InverseLength;

            u = Mathf.Clamp(u, 0.0f, 1.0f);

            Vector2 closestPoint = P1 + EdgeVector * u;

            return closestPoint;
        }

        public float GetSmallestDistance(Vector2 point)
        {
            Vector2 closestPoint = GetClosestPoint(point);

            return (closestPoint - point).Length;
        }

        public float GetSmallestDistance(Vector2 point, out Vector2 closestPoint)
        {
            closestPoint = GetClosestPoint(point);

            return (closestPoint - point).Length;
        }
    }
}
