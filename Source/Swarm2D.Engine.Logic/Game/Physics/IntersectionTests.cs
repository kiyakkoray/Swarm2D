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
        internal static bool CheckIntersectionAndProduceResult(ShapeInstance shapeA, ShapeInstance shapeB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
        {
            minimumTranslation = Vector2.Zero;
            intersectionPoint = Vector2.Zero;

            bool isIntersection = false;

            if (shapeA is PolygonInstance && shapeB is PolygonInstance)
            {
                isIntersection = CheckIntersectionAndProduceResult(shapeA as PolygonInstance, shapeB as PolygonInstance, out minimumTranslation, out intersectionPoint);
            }
            else if (shapeA is CircleInstance && shapeB is CircleInstance)
            {
                isIntersection = CheckIntersectionAndProduceResult(shapeA as CircleInstance, shapeB as CircleInstance, out minimumTranslation, out intersectionPoint);
            }
            else if (shapeA is PolygonInstance && shapeB is CircleInstance)
            {
                isIntersection = CheckIntersectionAndProduceResult(shapeA as PolygonInstance, shapeB as CircleInstance, out minimumTranslation, out intersectionPoint);
            }
            else if (shapeA is CircleInstance && shapeB is PolygonInstance)
            {
                isIntersection = CheckIntersectionAndProduceResult(shapeB as PolygonInstance, shapeA as CircleInstance, out minimumTranslation, out intersectionPoint);

                minimumTranslation *= -1.0f;
            }

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

        private static bool CheckIntersectionAndProduceResult(PolygonInstance polygonA, PolygonInstance polygonB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
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

        private static bool CheckIntersectionAndProduceResult(PolygonInstance polygonA, CircleInstance circleB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
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

        private static bool CheckIntersectionAndProduceResult(CircleInstance circleA, CircleInstance circleB, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
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
                isIntersection = CheckIntersection(shapeA as PolygonInstance, shapeB as PolygonInstance);
            }
            else if (shapeA is CircleInstance && shapeB is CircleInstance)
            {
                isIntersection = CheckIntersection(shapeA as CircleInstance, shapeB as CircleInstance);
            }
            else if (shapeA is PolygonInstance && shapeB is CircleInstance)
            {
                isIntersection = CheckIntersection(shapeA as PolygonInstance, shapeB as CircleInstance);
            }
            else if (shapeA is CircleInstance && shapeB is PolygonInstance)
            {
                isIntersection = CheckIntersection(shapeB as PolygonInstance, shapeA as CircleInstance);
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
}
