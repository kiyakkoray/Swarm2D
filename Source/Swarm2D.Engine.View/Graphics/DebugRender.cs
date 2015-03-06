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

namespace Swarm2D.Engine.View
{
    public class DebugRender
    {
        public static void Reset()
        {
            Framework.Current.ResetDebugRender();
        }

        public static void DrawBufferedDebugObjects()
        {
            Framework.Current.DrawBufferedDebugObjects();
        }

        public static void DrawDebugLine(Vector2 a, Vector2 b)
        {
            Framework.Current.DrawDebugLine(a, b);
        }

        public static void DrawDebugPoint(Vector2 point)
        {
            Framework.Current.DrawDebugPoint(point);
        }

        public static void DrawDebugPolygon(List<Vector2> vertices, Color color)
        {
            Framework.Current.DrawDebugPolygon(vertices, color);
        }

        public static void DrawDebugCircle(float radius, Color color)
        {
            Framework.Current.DrawDebugCircle(radius, color);
        }

        public static void DrawDebugQuad()
        {
            Framework.Current.DrawDebugQuad();
        }

        public static void DrawDebugQuad(float x, float y, float width, float height)
        {
            Framework.Current.DrawDebugQuad(x, y, width, height);
        }

        public static void AddDebugPoint(Vector2 point)
        {
            Framework.Current.AddDebugPoint(point);
        }

        public static void AddDebugLine(Vector2 pointA, Vector2 pointB)
        {
            Framework.Current.AddDebugLine(pointA, pointB);
        }
    }
}
