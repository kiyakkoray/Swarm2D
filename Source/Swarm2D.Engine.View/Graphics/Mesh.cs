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
    public class Mesh
    {
        public MeshTopology Topology { get; private set; }

        public float[] Vertices { get; protected set; }
        public float[] TextureCoordinates { get; protected set; }
        //public int[] Indices { get; private set; }

        public int VertexCount { get; set; }

        private static List<Vector2> _referenceCirclePoints;
        private static List<Vector2> _circlePolygonPoints;

        static Mesh()
        {
            _referenceCirclePoints = new List<Vector2>(64);
            _circlePolygonPoints = new List<Vector2>(64);

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

        protected Mesh()
        {
            Topology = MeshTopology.Quads;
        }

        public Mesh(MeshTopology topology, float[] vertices, float[] uvs, int vertexCount)
        {
            Topology = topology;
            Vertices = vertices;
            TextureCoordinates = uvs;
            VertexCount = vertexCount;
        }

        public Mesh(MeshTopology topology, int vertexCount)
        {
            Topology = topology;
            Vertices = new float[vertexCount * 2];
            TextureCoordinates = new float[vertexCount * 2];
            VertexCount = vertexCount;
        }

        public void SetVertexAt(int index, Vector2 vertex)
        {
            Vertices[2 * index] = vertex.X;
            Vertices[2 * index + 1] = vertex.Y;
        }

        public static Mesh CreateTriangleTopologyMeshWithPolygonCoordinates(List<Vector2> vertices)
        {
            int vertexCount = 3 * (vertices.Count - 2);

            float[] triangleTopologyVertices = new float[vertexCount * 2];
            float[] triangleTopologyTextureCoordinates = new float[vertexCount * 2];

            for (int i = 0; i < vertexCount / 3; i++)
            {
                triangleTopologyVertices[6 * i + 0] = vertices[0].X;
                triangleTopologyVertices[6 * i + 1] = vertices[0].Y;

                triangleTopologyVertices[6 * i + 2] = vertices[i + 1].X;
                triangleTopologyVertices[6 * i + 3] = vertices[i + 1].Y;

                triangleTopologyVertices[6 * i + 4] = vertices[i + 2].X;
                triangleTopologyVertices[6 * i + 5] = vertices[i + 2].Y;
            }

            Mesh mesh = new Mesh(MeshTopology.Triangles, triangleTopologyVertices, triangleTopologyTextureCoordinates, vertexCount);
            return mesh;
        }

        public static Mesh CreateLineTopologyMeshWithPolygonCoordinates(List<Vector2> vertices)
        {
            int vertexCount = 2 * vertices.Count;

            float[] lineTopologyVertices = new float[vertexCount * 2];
            float[] lineTopologyTextureCoordinates = new float[vertexCount * 2];

            FillLineTopologyMeshWithPolygonCoordinates(lineTopologyVertices, vertices);

            Mesh mesh = new Mesh(MeshTopology.Lines, lineTopologyVertices, lineTopologyTextureCoordinates, vertexCount);
            return mesh;
        }

        private static void FillLineTopologyMeshWithPolygonCoordinates(float[] lineTopologyVertices, List<Vector2> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                int currentVertexIndex = i;
                int nextVertexIndex = (i + 1 == vertices.Count) ? 0 : i + 1;

                lineTopologyVertices[i * 4 + 0] = vertices[currentVertexIndex].X;
                lineTopologyVertices[i * 4 + 1] = vertices[currentVertexIndex].Y;

                lineTopologyVertices[i * 4 + 2] = vertices[nextVertexIndex].X;
                lineTopologyVertices[i * 4 + 3] = vertices[nextVertexIndex].Y;
            }
        }

        public static Mesh CreateLineTopologyMeshWithQuadVertices(float[] quadVertices, int vertexCount)
        {
            float[] lineVertices = new float[vertexCount * 2 * 2];
            float[] textureCoordinates = new float[vertexCount * 2 * 2];

            QuadVerticesToLineVertices(quadVertices, vertexCount, lineVertices);

            Mesh mesh = new Mesh(MeshTopology.Lines, lineVertices, textureCoordinates, vertexCount);
            return mesh;
        }

        public static void QuadVerticesToLineVertices(float[] quadVertices, int vertexCount, float[] lineVertices)
        {
            for (int i = 0; i < vertexCount; i++)
            {
                int currentVertexIndex = 2 * i;
                int nextVertexIndex = i + 1 == vertexCount ? 0 : 2 * (i + 1);

                lineVertices[i * 4 + 0] = quadVertices[currentVertexIndex];
                lineVertices[i * 4 + 1] = quadVertices[currentVertexIndex + 1];

                lineVertices[i * 4 + 2] = quadVertices[nextVertexIndex];
                lineVertices[i * 4 + 3] = quadVertices[nextVertexIndex + 1];
            }
        }

        public static Mesh CreateTriangleTopologyMeshWithCircleRadius(float radius)
        {
            _circlePolygonPoints.Clear();

            for (int i = 0; i < _referenceCirclePoints.Count; i++)
            {
                Vector2 currentPoint = _referenceCirclePoints[i];

                currentPoint.X *= radius;
                currentPoint.Y *= radius;

                _circlePolygonPoints.Add(currentPoint);
            }

            return CreateTriangleTopologyMeshWithPolygonCoordinates(_circlePolygonPoints);
        }

        public static Mesh CreateLineTopologyMeshWithCircleRadius(float radius)
        {
            _circlePolygonPoints.Clear();

            for (int i = 0; i < _referenceCirclePoints.Count; i++)
            {
                Vector2 currentPoint = _referenceCirclePoints[i];

                currentPoint.X *= radius;
                currentPoint.Y *= radius;

                _circlePolygonPoints.Add(currentPoint);
            }

            int vertexCount = 2 * _circlePolygonPoints.Count + 2; //+2 for radius tracking

            float[] lineTopologyVertices = new float[vertexCount * 2];
            float[] lineTopologyTextureCoordinates = new float[vertexCount * 2];

            FillLineTopologyMeshWithPolygonCoordinates(lineTopologyVertices, _circlePolygonPoints);

            Vector2 radiusLine = new Vector2(1.0f, 0.0f);

            radiusLine.X *= radius;
            radiusLine.Y *= radius;

            lineTopologyVertices[lineTopologyVertices.Length - 4] = 0;
            lineTopologyVertices[lineTopologyVertices.Length - 3] = 0;

            lineTopologyVertices[lineTopologyVertices.Length - 2] = radiusLine.X;
            lineTopologyVertices[lineTopologyVertices.Length - 1] = radiusLine.Y;

            Mesh mesh = new Mesh(MeshTopology.Lines, lineTopologyVertices, lineTopologyTextureCoordinates, vertexCount);
            return mesh;
        }
    }

    public enum MeshTopology
    {
        Triangles,
        Quads,
        Lines
    }
}