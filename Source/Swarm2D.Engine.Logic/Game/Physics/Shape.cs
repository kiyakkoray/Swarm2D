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
using Swarm2D.Engine.Core;
using Swarm2D.Library;

namespace Swarm2D.Engine.Logic
{
    public interface IShape
    {
        ShapeInstance CreateShapeInstance();
    }

    public abstract class Shape : Resource, IShape
    {
        protected Shape(string name)
            : base(name)
        {

        }

        ShapeInstance IShape.CreateShapeInstance()
        {
            return this.CreateShapeInstance();
        }

        internal abstract ShapeInstance CreateShapeInstance();
    }

    public abstract class ShapeInstance
    {
        //AABB variables
        public float MinX { get; protected set; }
        public float MaxX { get; protected set; }

        public float MinY { get; protected set; }
        public float MaxY { get; protected set; }

        public float BoundingCircleRadius { get; protected set; }

        public float Area { get; protected set; }

        public abstract void Initialize();

        public abstract void PrepareTransformation(ref Matrix4x4 transform);

        public abstract float CalculateInertia(PhysicsMaterial material);

        public bool CheckIntersectionAndProduceResult(ShapeInstance otherShape, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
        {
            return IntersectionTests.CheckIntersectionAndProduceResult(this, otherShape, out minimumTranslation, out intersectionPoint);
        }

        public bool CheckIntersection(ShapeInstance otherShape)
        {
            return IntersectionTests.CheckIntersection(this, otherShape);
        }

        public abstract bool IsIntersects(LineSegment lineSegment);

        public abstract bool IsIntersects(LineSegment lineSegment, out Vector2 normal, out Vector2 intersectionPoint);

        public abstract bool IsInside(Vector2 worldPosition);
    }
}