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
    public interface IPolygon : IShape
    {
        List<Vector2> Vertices { get; }
    }

    public class Polygon : Shape, IPolygon
    {
        //In pixels
        public List<Vector2> Vertices { get; private set; }

        public Polygon(string name)
            : base(name)
        {
            Vertices = new List<Vector2>();
        }

        internal override ShapeInstance CreateShapeInstance()
        {
            return new PolygonInstance(this);
        }
    }

    public interface IPolygonInstance
    {
        List<LineSegment> Edges { get; }
        Vector2 CurrentCenter { get; }
    }

    public class PolygonInstance : ShapeInstance, IPolygonInstance
    {
        List<LineSegment> IPolygonInstance.Edges { get { return Edges; } }
        Vector2 IPolygonInstance.CurrentCenter { get { return CurrentCenter; } }

        internal List<LineSegment> Edges { get; private set; }
        internal Vector2 CurrentCenter { get; private set; }

        private IPolygon _polygon;

        public PolygonInstance(IPolygon polygon)
        {
            _polygon = polygon;
            Edges = new List<LineSegment>();
        }

        public override void Initialize()
        {
            Edges.Clear();

            float boundingCircleRadiusSquared = float.MinValue;

            for (int i = 0; i < _polygon.Vertices.Count; i++)
            {
                Vector2 currentVertex = _polygon.Vertices[i];
                Vector2 nextVertex = _polygon.Vertices[i + 1 != _polygon.Vertices.Count ? i + 1 : 0];

                LineSegment lineSegment = new LineSegment(currentVertex, nextVertex);

                Edges.Add(lineSegment);

                if (currentVertex.LengthSquared > boundingCircleRadiusSquared)
                {
                    boundingCircleRadiusSquared = currentVertex.LengthSquared;
                }
            }

            BoundingCircleRadius = Mathf.Sqrt(boundingCircleRadiusSquared);

            Area = CalculateArea();
        }

        public override void PrepareTransformation(ref Matrix4x4 transform)
        {
            CurrentCenter = transform.Position;

            MinX = float.MaxValue;
            MaxX = float.MinValue;

            MinY = float.MaxValue;
            MaxY = float.MinValue;

            float boundingCircleRadiusSquared = float.MinValue;

            for (int i = 0; i < Edges.Count; i++)
            {
                LineSegment edge = Edges[i];

                if (i != 0)
                {
                    LineSegment previousEdge = Edges[i - 1];
                    edge.P1OnWorld = previousEdge.P2OnWorld;
                }
                else
                {
                    edge.P1OnWorld = transform * edge.P1;
                }

                edge.P2OnWorld = transform * edge.P2;
                edge.EdgeVectorOnWorld = edge.P2OnWorld - edge.P1OnWorld;
                edge.NormalOnWorld = edge.EdgeVectorOnWorld.Perpendicular;

                if (MinX > edge.P1OnWorld.X)
                {
                    MinX = edge.P1OnWorld.X;
                }

                if (MinY > edge.P1OnWorld.Y)
                {
                    MinY = edge.P1OnWorld.Y;
                }

                if (MaxX < edge.P1OnWorld.X)
                {
                    MaxX = edge.P1OnWorld.X;
                }

                if (MaxY < edge.P1OnWorld.Y)
                {
                    MaxY = edge.P1OnWorld.Y;
                }

                if (edge.P1OnWorld.LengthSquared > boundingCircleRadiusSquared)
                {
                    boundingCircleRadiusSquared = edge.P1OnWorld.LengthSquared;
                }
            }

            BoundingCircleRadius = Mathf.Sqrt(boundingCircleRadiusSquared);
        }

        private float CalculateArea()
        {
            float area = 0.0f;

            Vector2 center = Vector2.Zero;

            for (int i = 0; i < Edges.Count; i++)
            {
                LineSegment edge = Edges[i];

                float heightOfCurrentTriangle = edge.GetSmallestDistance(center);

                area += heightOfCurrentTriangle*edge.Length*0.5f;
            }

            return area;
        }

        //http://lab.polygonal.de/?p=57
        public override float CalculateInertia(PhysicsMaterial material)
        {
            float totalInertia = 0.0f;

            Vector2 center = Vector2.Zero;

            for (int i = 0; i < Edges.Count; i++)
            {
                LineSegment edge = Edges[i];

                Vector2 closestPoint;

                float h = edge.GetSmallestDistance(center, out closestPoint);
                float b = edge.Length;
                float a = (closestPoint - edge.P1).Length;

                float areaOfCurrentTriangle = h*b*0.5f;

                Vector2 massCenterOfCurrentTriangle = (edge.P1 + edge.P2)*(1.0f/3.0f);

                float d = massCenterOfCurrentTriangle.Length;

                //b^3h - b^2ha + bha^2 + bh^3
                float inertiaOfCurrentTriangle = (h*((b*b*b) - (b*b*a) + (b*a*a) + (b*h*h)))/36.0f;

                totalInertia += inertiaOfCurrentTriangle + areaOfCurrentTriangle*d*d*material.Density;
            }

            return totalInertia;
        }

        public override bool IsInside(Vector2 worldPosition)
        {
            for (int i = 0; i < Edges.Count; i++)
            {
                Vector2 baseVector = Edges[i].EdgeVectorOnWorld;
                Vector2 pointVector = worldPosition - Edges[i].P1OnWorld;
                Vector2 checkVector = Edges[(i < Edges.Count - 1) ? i + 1 : 0].P2OnWorld - Edges[i].P1OnWorld;

                int signA = Math.Sign(pointVector * baseVector);
                int signB = Math.Sign(checkVector * baseVector);

                if (signA != signB)
                {
                    return false;
                }
            }

            return true;
        }

        //TODO: complete method
        public override bool IsIntersects(LineSegment lineSegment)
        {
            return false;
        }

        //TODO: complete method
        public override bool IsIntersects(LineSegment lineSegment, out Vector2 normal, out Vector2 intersectionPoint)
        {
            normal = Vector2.Zero;
            intersectionPoint = Vector2.Zero;

            return false;
        }
    }

    public static class PolygonExtensions
    {
        public static Vector2 CalculateIntersectionPoint(this IPolygonInstance shape, IPolygonInstance otherShape)
        {
            List<LineSegment> foundSegmentsOnA = new List<LineSegment>();
            List<LineSegment> foundSegmentsOnB = new List<LineSegment>();

            Vector2 intersectionPoint = Vector2.Zero;

            int intersectionPointCount = 0;

            for (int i = 0; i < shape.Edges.Count; i++)
            {
                LineSegment lineSegmentA = shape.Edges[i];

                for (int j = 0; j < otherShape.Edges.Count; j++)
                {
                    LineSegment lineSegmentB = otherShape.Edges[j];

                    Vector2 currentIntersectionPoint;

                    if (lineSegmentA.IsIntersects(lineSegmentB, out currentIntersectionPoint))
                    {
                        if (!foundSegmentsOnA.Contains(lineSegmentA))
                        {
                            foundSegmentsOnA.Add(lineSegmentA);
                        }

                        if (!foundSegmentsOnB.Contains(lineSegmentB))
                        {
                            foundSegmentsOnB.Add(lineSegmentB);
                        }

                        intersectionPoint += currentIntersectionPoint;
                        intersectionPointCount++;
                    }
                }
            }

            if (intersectionPointCount > 1)
            {
                intersectionPoint = intersectionPoint * (1.0f / (float)intersectionPointCount);
            }

            return intersectionPoint;
        }

        public static Vector2 ProjectTo(this IPolygonInstance shape, Vector2 axis)
        {
            float min = shape.Edges[0].P1OnWorld.Dot(axis);
            float max = min;

            for (int i = 1; i < shape.Edges.Count; i++)
            {
                Vector2 vertexOnWorld = shape.Edges[i].P1OnWorld;

                float p = vertexOnWorld.Dot(axis);

                if (p < min)
                {
                    min = p;
                }
                else if (p > max)
                {
                    max = p;
                }
            }

            return new Vector2(min, max);
        }
    }
}
