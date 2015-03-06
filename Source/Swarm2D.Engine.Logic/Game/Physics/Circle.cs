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
    public interface ICircle : IShape
    {
        float Radius { get; }
    }

    public class Circle : Shape, ICircle
    {
        public float Radius { get; set; }

        public Circle(string name)
            : base(name)
        {

        }

        internal override ShapeInstance CreateShapeInstance()
        {
            return new CircleInstance(this);
        }
    }

    public class CircleInstance : ShapeInstance
    {
        internal float RadiusWithTransformation { get; private set; }

        internal Vector2 CurrentCenter { get; private set; }

        private ICircle _circle;

        public override IShape Shape
        {
            get { return _circle; }
        }

        public CircleInstance(ICircle circle)
        {
            _circle = circle;
        }

        public override void Initialize()
        {
            BoundingCircleRadius = _circle.Radius;
            RadiusWithTransformation = _circle.Radius;
            Area = CalculateArea();
        }

        public override float CalculateInertia(PhysicsMaterial material)
        {
            return (Mathf.PI / 4.0f) * RadiusWithTransformation * RadiusWithTransformation * RadiusWithTransformation * RadiusWithTransformation * material.Density;
        }

        private float CalculateArea()
        {
            return RadiusWithTransformation * RadiusWithTransformation * Mathf.PI;
        }

        public override void PrepareTransformation(ref Matrix4x4 transform)
        {
            Matrix4x4 transformWithoutPosition = transform;

            transformWithoutPosition.M03 = 0;
            transformWithoutPosition.M13 = 0;

            Vector2 edgePoint = new Vector2(_circle.Radius, 0);
            edgePoint = transformWithoutPosition * edgePoint;

            RadiusWithTransformation = edgePoint.Length;
            BoundingCircleRadius = RadiusWithTransformation;

            PrepareTransformation(transform.Position);
        }

        internal void PrepareTransformation(Vector2 centerPosition)
        {
            CurrentCenter = centerPosition;
            RadiusWithTransformation = _circle.Radius;

            MinX = CurrentCenter.X - RadiusWithTransformation;
            MaxX = CurrentCenter.X + RadiusWithTransformation;

            MinY = CurrentCenter.Y - RadiusWithTransformation;
            MaxY = CurrentCenter.Y + RadiusWithTransformation;
        }

        internal Vector2 ProjectTo(Vector2 axis)
        {
            Vector2 p1 = CurrentCenter + axis * RadiusWithTransformation;
            Vector2 p2 = CurrentCenter - axis * RadiusWithTransformation;

            float f1 = p1.Dot(axis);
            float f2 = p2.Dot(axis);

            float min = f1;
            float max = f2;

            if (max < min)
            {
                min = f2;
                max = f1;
            }

            return new Vector2(min, max);
        }

        internal override bool IsInside(Vector2 worldPosition)
        {
            return (worldPosition - CurrentCenter).Length <= RadiusWithTransformation;
        }

        public override bool IsIntersects(LineSegment lineSegment)
        {
            Vector2 closestPoint = lineSegment.GetClosestPoint(CurrentCenter);
            return IsInside(closestPoint);
        }

        //TODO: complete method
        public override bool IsIntersects(LineSegment lineSegment, out Vector2 normal, out Vector2 intersectionPoint)
        {
            normal = Vector2.Zero;
            intersectionPoint = Vector2.Zero;

            Vector2 closestPoint = lineSegment.GetClosestPoint(CurrentCenter);
            return IsInside(closestPoint);
        }
    }
}
