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
    public static class IntersectionTests
    {
        private static Dictionary<Type, Dictionary<Type, IIntersectionChecker>> _intersectionCheckers;

        static IntersectionTests()
        {
            _intersectionCheckers = new Dictionary<Type, Dictionary<Type, IIntersectionChecker>>();

            AddIntersectionChecker(new PolygonPolygonIntersectionChecker());
            AddIntersectionChecker(new PolygonCircleIntersectionChecker());
            AddIntersectionChecker(new CirclePolygonIntersectionChecker());
            AddIntersectionChecker(new CircleCircleIntersectionChecker());
        }

        public static void AddIntersectionChecker(IIntersectionChecker intersectionChecker)
        {
            Type typeA = intersectionChecker.TypeA;
            Type typeB = intersectionChecker.TypeB;

            if (!_intersectionCheckers.ContainsKey(typeA))
            {
                _intersectionCheckers.Add(typeA, new Dictionary<Type, IIntersectionChecker>());
            }

            _intersectionCheckers[typeA].Add(typeB, intersectionChecker);
        }

        internal static bool CheckIntersectionAndProduceResult(ShapeInstance shapeA, ShapeInstance shapeB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
        {
            IIntersectionChecker intersectionChecker = _intersectionCheckers[shapeA.GetType()][shapeB.GetType()];

            bool isIntersection = intersectionChecker.CheckIntersection(shapeA, shapeB, out minimumTranslation, out intersectionPoint);

            if (isIntersection)
            {
                //DebugRender.AddDebugPoint(intersectionPoint);
            }

            return isIntersection;
        }

        private static bool DoesOverlap(Vector2 a, Vector2 b)
        {
            return (a.X >= b.X && a.X <= b.Y) || (a.Y >= b.X && a.Y <= b.Y)
                || (b.X >= a.X && b.X <= a.Y) || (b.Y >= a.X && b.Y <= a.Y);
        }

        private static float GetOverlap(Vector2 a, Vector2 b)
        {
            float maxX = a.X >= b.X ? a.X : b.X;
            float minY = a.Y <= b.Y ? a.Y : b.Y;

            return minY - maxX;
        }

        public static bool CheckIntersectionAndProduceResult(IPolygonInstance polygonA, IPolygonInstance polygonB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
        {
            minimumTranslation = Vector2.Zero;
            intersectionPoint = Vector2.Zero;

            float overlap = float.MaxValue;
            Vector2 direction = Vector2.Zero;

            int edgeCount = polygonA.Edges.Count + polygonB.Edges.Count;

            for (int i = 0; i < edgeCount; i++)
            {
                LineSegment edge = null;

                if (i < polygonA.Edges.Count)
                {
                    edge = polygonA.Edges[i];
                }
                else
                {
                    int j = i - polygonA.Edges.Count;

                    edge = polygonB.Edges[j];
                }

                Vector2 axis = edge.NormalOnWorld;
                axis.Normalize();

                Vector2 p1 = polygonA.ProjectTo(axis);
                Vector2 p2 = polygonB.ProjectTo(axis);

                if (!DoesOverlap(p1, p2))
                {
                    return false;
                }
                else
                {
                    float o = Mathf.Abs(GetOverlap(p1, p2));

                    if (o < overlap)
                    {
                        overlap = o;
                        direction = axis;
                    }
                }
            }

            if (Mathf.IsZero(overlap))
            {
                return false;
            }

            Vector2 centerDirection = polygonA.CurrentCenter - polygonB.CurrentCenter;

            if (direction.Dot(centerDirection) < 0)
            {
                direction = direction * (-1.0f);
            }

            minimumTranslation = direction * overlap;

            intersectionPoint = polygonA.CalculateIntersectionPoint(polygonB);

            return true;
        }

        public static bool CheckIntersectionAndProduceResult(IPolygonInstance polygonA, CircleInstance circleB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
        {
            minimumTranslation = Vector2.Zero;
            intersectionPoint = Vector2.Zero;

            float overlap = float.MaxValue;
            Vector2 direction = Vector2.Zero;

            int edgeCount = polygonA.Edges.Count;

            for (int i = 0; i < edgeCount; i++)
            {
                LineSegment edge = polygonA.Edges[i];

                Vector2 axis = edge.NormalOnWorld;
                axis.Normalize();

                Vector2 p1 = polygonA.ProjectTo(axis);
                Vector2 p2 = circleB.ProjectTo(axis);

                if (!DoesOverlap(p1, p2))
                {
                    return false;
                }
                else
                {
                    float o = Mathf.Abs(GetOverlap(p1, p2));

                    if (o < overlap)
                    {
                        overlap = o;
                        direction = axis;
                    }
                }
            }

            if (Mathf.IsZero(overlap))
            {
                return false;
            }

            Vector2 centerDirection = polygonA.CurrentCenter - circleB.CurrentCenter;

            if (direction.Dot(centerDirection) < 0)
            {
                direction = direction * (-1.0f);
            }

            minimumTranslation = direction * overlap;

            intersectionPoint = circleB.CurrentCenter + direction * (circleB.RadiusWithTransformation - overlap * 0.5f);

            return true;
        }

        public static bool CheckIntersectionAndProduceResult(CircleInstance circleA, CircleInstance circleB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
        {
            minimumTranslation = Vector2.Zero;
            intersectionPoint = Vector2.Zero;

            float sumOfRadiuses = circleA.RadiusWithTransformation + circleB.RadiusWithTransformation;
            float distanceOfCenters = Vector2.Distance(circleA.CurrentCenter, circleB.CurrentCenter);

            if (sumOfRadiuses >= distanceOfCenters)
            {
                intersectionPoint = (circleA.CurrentCenter + circleB.CurrentCenter) * 0.5f;

                Vector2 collisionDirection = circleA.CurrentCenter - circleB.CurrentCenter;
                collisionDirection.Normalize();

                minimumTranslation = collisionDirection * (sumOfRadiuses - distanceOfCenters);

                return true;
            }

            return false;
        }

        public static bool CheckIntersection(ShapeInstance shapeA, ShapeInstance shapeB)
        {
            bool isIntersection = false;

            if (shapeA is PolygonInstance && shapeB is PolygonInstance)
            {
                isIntersection = CheckIntersection((PolygonInstance)shapeA, (PolygonInstance)shapeB);
            }
            else if (shapeA is CircleInstance && shapeB is CircleInstance)
            {
                isIntersection = CheckIntersection((CircleInstance)shapeA, (CircleInstance)shapeB);
            }
            else if (shapeA is PolygonInstance && shapeB is CircleInstance)
            {
                isIntersection = CheckIntersection((PolygonInstance)shapeA, (CircleInstance)shapeB);
            }
            else if (shapeA is CircleInstance && shapeB is PolygonInstance)
            {
                isIntersection = CheckIntersection((PolygonInstance)shapeB, (CircleInstance)shapeA);
            }

            return isIntersection;
        }

        private static bool CheckIntersection(PolygonInstance polygonA, PolygonInstance polygonB)
        {
            float overlap = float.MaxValue;

            int edgeCount = polygonA.Edges.Count + polygonB.Edges.Count;

            for (int i = 0; i < edgeCount; i++)
            {
                LineSegment edge = null;

                if (i < polygonA.Edges.Count)
                {
                    edge = polygonA.Edges[i];
                }
                else
                {
                    int j = i - polygonA.Edges.Count;

                    edge = polygonB.Edges[j];
                }

                Vector2 axis = edge.NormalOnWorld;
                axis.Normalize();

                Vector2 p1 = polygonA.ProjectTo(axis);
                Vector2 p2 = polygonB.ProjectTo(axis);

                if (!DoesOverlap(p1, p2))
                {
                    return false;
                }
                else
                {
                    float o = Mathf.Abs(GetOverlap(p1, p2));

                    if (o < overlap)
                    {
                        overlap = o;
                    }
                }
            }

            if (Mathf.IsZero(overlap))
            {
                return false;
            }

            return true;
        }

        private static bool CheckIntersection(PolygonInstance polygonA, CircleInstance circleB)
        {
            float overlap = float.MaxValue;

            int edgeCount = polygonA.Edges.Count;

            for (int i = 0; i < edgeCount; i++)
            {
                LineSegment edge = polygonA.Edges[i];

                Vector2 axis = edge.NormalOnWorld;
                axis.Normalize();

                Vector2 p1 = polygonA.ProjectTo(axis);
                Vector2 p2 = circleB.ProjectTo(axis);

                if (!DoesOverlap(p1, p2))
                {
                    return false;
                }
                else
                {
                    float o = Mathf.Abs(GetOverlap(p1, p2));

                    if (o < overlap)
                    {
                        overlap = o;
                    }
                }
            }

            if (Mathf.IsZero(overlap))
            {
                return false;
            }

            return true;
        }

        private static bool CheckIntersection(CircleInstance circleA, CircleInstance circleB)
        {
            float sumOfRadiuses = circleA.RadiusWithTransformation + circleB.RadiusWithTransformation;
            float distanceOfCenters = Vector2.Distance(circleA.CurrentCenter, circleB.CurrentCenter);

            return sumOfRadiuses >= distanceOfCenters;
        }
    }

    public interface IIntersectionChecker
    {
        Type TypeA { get; }
        Type TypeB { get; }

        bool CheckIntersection(ShapeInstance shapeA, ShapeInstance shapeB, out Vector2 minimumTranslation, out Vector2 intersectionPoint);
    }

    public class PolygonPolygonIntersectionChecker : IIntersectionChecker
    {
        Type IIntersectionChecker.TypeA { get { return typeof(PolygonInstance); } }
        Type IIntersectionChecker.TypeB { get { return typeof(PolygonInstance); } }

        bool IIntersectionChecker.CheckIntersection(ShapeInstance shapeA, ShapeInstance shapeB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
        {
            var polygonA = (IPolygonInstance)shapeA;
            var polygonB = (IPolygonInstance)shapeB;

            return IntersectionTests.CheckIntersectionAndProduceResult(polygonA, polygonB, out minimumTranslation, out intersectionPoint);
        }
    }

    public class PolygonCircleIntersectionChecker : IIntersectionChecker
    {
        Type IIntersectionChecker.TypeA { get { return typeof(PolygonInstance); } }
        Type IIntersectionChecker.TypeB { get { return typeof(CircleInstance); } }

        bool IIntersectionChecker.CheckIntersection(ShapeInstance shapeA, ShapeInstance shapeB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
        {
            var polygon = (IPolygonInstance)shapeA;
            var circle = (CircleInstance)shapeB;

            return IntersectionTests.CheckIntersectionAndProduceResult(polygon, circle, out minimumTranslation, out intersectionPoint);
        }
    }

    public class CirclePolygonIntersectionChecker : IIntersectionChecker
    {
        Type IIntersectionChecker.TypeA { get { return typeof(CircleInstance); } }
        Type IIntersectionChecker.TypeB { get { return typeof(PolygonInstance); } }

        bool IIntersectionChecker.CheckIntersection(ShapeInstance shapeA, ShapeInstance shapeB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
        {
            var circle = (CircleInstance)shapeA;
            var polygon = (IPolygonInstance)shapeB;

            bool intersection = IntersectionTests.CheckIntersectionAndProduceResult(polygon, circle, out minimumTranslation, out intersectionPoint);

            if (intersection)
            {
                minimumTranslation *= -1.0f;
            }

            return intersection;
        }
    }

    public class CircleCircleIntersectionChecker : IIntersectionChecker
    {
        Type IIntersectionChecker.TypeA { get { return typeof(CircleInstance); } }
        Type IIntersectionChecker.TypeB { get { return typeof(CircleInstance); } }

        bool IIntersectionChecker.CheckIntersection(ShapeInstance shapeA, ShapeInstance shapeB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
        {
            var circleA = (CircleInstance)shapeA;
            var circleB = (CircleInstance)shapeB;

            return IntersectionTests.CheckIntersectionAndProduceResult(circleA, circleB, out minimumTranslation, out intersectionPoint);
        }
    }
}
