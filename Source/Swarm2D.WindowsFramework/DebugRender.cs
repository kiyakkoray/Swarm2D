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
using Swarm2D.WindowsFramework.Native.Opengl;

namespace Swarm2D.WindowsFramework
{
    public class DebugRender
    {
        private static float[] _debugVertices = new float[16];

        private static float[] _debugQuadVertices = new float[8] { -15, -15, -15, 15, 15, 15, 15, -15 };
        private static float[] _debugQuadLineVertices = new float[16] { -15, -15, -15, 15, -15, 15, 15, 15, 15, 15, 15, -15, 15, -15, -15, -15 };

        private static float[] _debugPointVertices2 = new float[8] { -2, -2, -2, 2, 2, 2, 2, -2 };

        private static List<Vector2> _debugPointList = new List<Vector2>();
        private static List<DebugLine> _debugLineList = new List<DebugLine>();

        public static bool Disabled { get; set; }

        private static GraphicsContext _graphicsContext;

        public static void Initialize(GraphicsContext graphicsContext)
        {
            _graphicsContext = graphicsContext;
        }

        public static void Reset()
        {
            _debugPointList.Clear();
            _debugLineList.Clear();
        }

        public static void DrawBufferedDebugObjects()
        {
            for (int i = 0; i < _debugPointList.Count; i++)
            {
                Vector2 point = _debugPointList[i];

                DrawDebugPoint(point);
            }

            for (int i = 0; i < _debugLineList.Count; i++)
            {
                DebugLine debugLine = _debugLineList[i];

                DrawDebugLine(debugLine.A, debugLine.B);
            }
        }

        public static void DrawDebugLine(Vector2 a, Vector2 b)
        {
            _debugPointVertices2[0] = a.X;
            _debugPointVertices2[1] = a.Y;
            _debugPointVertices2[2] = b.X;
            _debugPointVertices2[3] = b.Y;

            Opengl32.Color(1.0f, 0.0f, 0.0f);

            _graphicsContext.SetTextureCoordArrayClientState(true);

            Opengl32.VertexPointer(2, DataType.Float, 0, _debugPointVertices2);

            Opengl32.DrawArrays(BeginMode.Lines, 0, 2);
        }

        public static void DrawDebugPoint(Vector2 point)
        {
            _debugPointVertices2[0] = -2 + point.X;
            _debugPointVertices2[1] = -2 + point.Y;
            _debugPointVertices2[2] = -2 + point.X;
            _debugPointVertices2[3] = 2 + point.Y;
            _debugPointVertices2[4] = 2 + point.X;
            _debugPointVertices2[5] = 2 + point.Y;
            _debugPointVertices2[6] = 2 + point.X;
            _debugPointVertices2[7] = -2 + point.Y;

            Opengl32.Color(0.0f, 0.0f, 1.0f);

            _graphicsContext.SetTextureCoordArrayClientState(true);
            Opengl32.VertexPointer(2, DataType.Float, 0, _debugPointVertices2);

            Opengl32.DrawArrays(BeginMode.Quads, 0, 4);
        }

        private static float[] _debugPolygonVertices = new float[128];
        private static float[] _debugPolygonLineVertices = new float[256];

        public static void DrawDebugPolygon(List<Vector2> vertices, Color color)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                _debugPolygonVertices[i * 2] = vertices[i].X;
                _debugPolygonVertices[i * 2 + 1] = vertices[i].Y;
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 currentVertex = vertices[i];
                Vector2 nextVertex = vertices[i + 1 == vertices.Count ? 0 : i + 1];

                _debugPolygonLineVertices[i * 4 + 0] = currentVertex.X;
                _debugPolygonLineVertices[i * 4 + 1] = currentVertex.Y;

                _debugPolygonLineVertices[i * 4 + 2] = nextVertex.X;
                _debugPolygonLineVertices[i * 4 + 3] = nextVertex.Y;
            }

            const float byteToFloatCoeff = 1.0f / 255.0f;

            float r = ((float)color.Red) * byteToFloatCoeff;
            float g = ((float)color.Green) * byteToFloatCoeff;
            float b = ((float)color.Blue) * byteToFloatCoeff;
            float a = ((float)color.Alpha) * byteToFloatCoeff;

            Opengl32.Color(r, g, b, a);

            bool blend = true;

            _graphicsContext.SetBlending(blend);
            _graphicsContext.SetVertexArrayClientState(true);

            Opengl32.VertexPointer(2, DataType.Float, 0, _debugPolygonVertices);
            Opengl32.DrawArrays(BeginMode.Polygon, 0, vertices.Count);

            Opengl32.Color(0.5f, 1.0f, 0.0f, 0.5f);

            Opengl32.VertexPointer(2, DataType.Float, 0, _debugPolygonLineVertices);
            Opengl32.DrawArrays(BeginMode.Lines, 0, vertices.Count * 2);

            Opengl32.Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        private static List<Vector2> _debugCirclePoints = new List<Vector2>(64);
        private static List<Vector2> _referenceCirclePoints = new List<Vector2>(64);

        static DebugRender()
        {
            for (int i = 0; i < 16; i++)
            {
                Vector2 currentPoint = new Vector2();

                float f = i;

                f *= (360.0f / 16.0f);

                currentPoint.X = Mathf.Cos(f * Mathf.Deg2Rad);
                currentPoint.Y = Mathf.Sin(f * Mathf.Deg2Rad);

                _referenceCirclePoints.Add(currentPoint);
            }
        }

        public static void DrawDebugCircle(float radius, Color color)
        {
            _debugCirclePoints.Clear();

            for (int i = 0; i < _referenceCirclePoints.Count; i++)
            {
                Vector2 currentPoint = _referenceCirclePoints[i];

                currentPoint.X *= radius;
                currentPoint.Y *= radius;

                _debugCirclePoints.Add(currentPoint);
            }

            DrawDebugPolygon(_debugCirclePoints, color);

            Vector2 radiusLine = new Vector2(1.0f, 0.0f);

            radiusLine.X *= radius;
            radiusLine.Y *= radius;

            DrawDebugLine(Vector2.Zero, radiusLine);
        }

        public static void DrawDebugQuad()
        {
            Opengl32.Color(0.5f, 0.5f, 0.5f, 0.5f);

            bool blend = true;

            _graphicsContext.SetBlending(blend);

            _graphicsContext.SetVertexArrayClientState(true);

            Opengl32.VertexPointer(2, DataType.Float, 0, _debugQuadVertices);
            Opengl32.DrawArrays(BeginMode.Quads, 0, 4);

            Opengl32.Color(0.5f, 1.0f, 0.0f, 0.5f);

            Opengl32.VertexPointer(2, DataType.Float, 0, _debugQuadLineVertices);
            Opengl32.DrawArrays(BeginMode.Lines, 0, 8);

            Opengl32.Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        public static void DrawDebugQuad(float x, float y, float width, float height)
        {
            bool blend = true;

            _graphicsContext.SetBlending(blend);

            if (false)
            {
                _debugVertices[0] = x;
                _debugVertices[1] = y;

                _debugVertices[2] = x + width;
                _debugVertices[3] = y;

                _debugVertices[4] = x + width;
                _debugVertices[5] = y + height;

                _debugVertices[6] = x;
                _debugVertices[7] = y + height;

                Opengl32.Color(0.0f, 1.0f, 0.0f, 1.0f);

                Opengl32.EnableClientState((uint)ArrayType.Vertex);
                Opengl32.VertexPointer(2, DataType.Float, 0, _debugVertices);

                Opengl32.DrawArrays(BeginMode.Quads, 0, 4);

                //debugVertices[0] = x;
                //debugVertices[1] = y;

                //debugVertices[2] = x + width;
                //debugVertices[3] = y;

                //debugVertices[4] = x + width;
                _debugVertices[5] = y;

                _debugVertices[6] = x + width;
                //debugVertices[7] = y + height;

                _debugVertices[8] = x + width;
                _debugVertices[9] = y + height;

                _debugVertices[10] = x;
                _debugVertices[11] = y + height;

                _debugVertices[12] = x;
                _debugVertices[13] = y + height;

                _debugVertices[14] = x;
                _debugVertices[15] = y;

                Opengl32.Color(0.0f, 0.0f, 0.0f, 1.0f);
                Opengl32.DrawArrays(BeginMode.Lines, 0, 8);

                Opengl32.DisableClientState((uint)ArrayType.Vertex);
            }

            if (true)
            {
                _graphicsContext.SetVertexArrayClientState(false);

                Opengl32.Begin(BeginMode.Quads);

                Opengl32.Color(0.0f, 1.0f, 0.0f, 1.0f);
                Opengl32.Vertex(x, y, 0);
                Opengl32.Vertex(x + width, y, 0);
                Opengl32.Vertex(x + width, y + height, 0);
                Opengl32.Vertex(x, y + height, 0);

                Opengl32.End();

                //Opengl32.Begin(BeginMode.Lines);
                //
                //Opengl32.Color(0.0f, 0.0f, 0.0f, 1.0f);
                //
                //Opengl32.Vertex(x, y, 0);
                //Opengl32.Vertex(x + width, y, 0);
                //
                //Opengl32.Vertex(x + width, y, 0);
                //Opengl32.Vertex(x + width, y + height, 0);
                //
                //Opengl32.Vertex(x + width, y + height, 0);
                //Opengl32.Vertex(x, y + height, 0);
                //
                //Opengl32.Vertex(x, y + height, 0);
                //Opengl32.Vertex(x, y, 0);
                //
                //Opengl32.Color(1.0f, 1.0f, 1.0f, 1.0f);
                //Opengl32.End();
            }
        }

        public static void AddDebugPoint(Vector2 point)
        {
            if (!Disabled)
            {
                _debugPointList.Add(point);
            }
        }

        public static void AddDebugLine(Vector2 pointA, Vector2 pointB)
        {
            if (!Disabled)
            {
                DebugLine debugLine = new DebugLine();

                debugLine.A = pointA;
                debugLine.B = pointB;

                _debugLineList.Add(debugLine);
            }
        }
    }

    struct DebugLine
    {
        public Vector2 A;
        public Vector2 B;
    }
}
