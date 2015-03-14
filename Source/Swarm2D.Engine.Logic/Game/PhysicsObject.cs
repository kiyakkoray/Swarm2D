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
    [RequiresComponent(typeof(SceneEntity))]
    public class PhysicsObject : SceneEntityComponent
    {
        public enum PhysicsType
        {
            RigidBody,
            Static,
            Trigger
        }

        public Vector2 Velocity { get; set; }
        public float AngularVelocity { get; set; }

        public float Mass { get; private set; }
        public float InverseMass { get; private set; }

        public float Inertia { get; private set; }
        public float InverseInertia { get; private set; }

        private ShapeFilter _shapeFilter;
        internal ShapeInstance ShapeData { get; private set; }

        private void AssignShapeFilter(ShapeFilter shapeFilter)
        {
            _shapeFilter = shapeFilter;

            if (!Entity.IsPrefab)
            {
                if (shapeFilter != null)
                {
                    ShapeData = _shapeFilter.CreateShapeInstance();
                }
                else
                {
                    ShapeData = null;
                }
            }
        }

        public ShapeFilter ShapeFilter
        {
            get { return _shapeFilter; }
        }

        [ComponentProperty]
        public PhysicsMaterial Material { get; set; }

        public PhysicsWorld PhysicsWorld { get; private set; }

        private List<PhysicsWorldGridCell> _newGrids;
        internal List<PhysicsWorldGridCell> CurrentGrids { get; private set; }

        private PhysicsType _type = PhysicsType.RigidBody;

        [ComponentProperty]
        public PhysicsType Type
        {
            get
            {
                return _type;
            }
            set
            {
                if (value != _type)
                {
                    if (!Entity.IsPrefab)
                    {
                        PhysicsWorld.RemovePhysicsObject(this);
                    }

                    _type = value;

                    _minGridX = int.MinValue;
                    _minGridY = int.MinValue;

                    _maxGridX = int.MinValue;
                    _maxGridY = int.MinValue; 
                    
                    if (!Entity.IsPrefab)
                    {
                        PhysicsWorld.AddPhysicsObject(this);
                    }
                }
            }
        }

        [ComponentProperty]
        public bool FixedRotation { get; set; }

        [ComponentProperty]
        public int Layer { get; set; }

        public bool IsStatic
        {
            get
            {
                return _type == PhysicsType.Static;
            }
        }

        internal bool AddedToOrderdedRigidBodiesToRepositionList { get; set; }

        internal bool RepositionedOnCurrentUpdate { get; set; }

        internal LinkedList<Collision> Collisions { get; private set; }

        internal int StaticCollisionCount { get; private set; }

        internal int CollisionCount
        {
            get
            {
                return Collisions.Count;
            }
        }

        internal Vector2 MinimumTranslation { get; set; }

        internal Vector2 PreviousPosition { get; set; }

        internal float PreviousRotation { get; set; }

        private bool _isTransformDirty = false; //asigned to true at OnInitialize

        internal List<PhysicsObject> CurrentlyCollectedRigidBodiesToCheckWith { get; private set; }
        internal List<PhysicsObject> CurrentlyCollectedStaticBodiesToCheckWith { get; private set; }
        internal List<PhysicsObject> CurrentlyCollectedTriggersToCheckWith { get; private set; }

        internal LinkedListNode<PhysicsObject> NodeOnPhysicsObjectList { get; set; }
        internal LinkedListNode<PhysicsObject> NodeOnTypeList { get; set; } //rigid body, trigger list etc..
        internal LinkedListNode<PhysicsObject> NodeOnDirtyTransformList { get; private set; }

        private int _minGridX = int.MinValue;
        private int _minGridY = int.MinValue;

        private int _maxGridX = int.MinValue;
        private int _maxGridY = int.MinValue;

        protected override void OnAdded()
        {
            base.OnAdded();

            _newGrids = new List<PhysicsWorldGridCell>();
            CurrentGrids = new List<PhysicsWorldGridCell>();

            Collisions = new LinkedList<Collision>();
            StaticCollisionCount = 0;

            //those below had a parameter in their constructors as 64
            CurrentlyCollectedRigidBodiesToCheckWith = new List<PhysicsObject>();
            CurrentlyCollectedStaticBodiesToCheckWith = new List<PhysicsObject>();
            CurrentlyCollectedTriggersToCheckWith = new List<PhysicsObject>();

            PhysicsWorld = Scene.GetComponent<PhysicsWorld>();

            Debug.Assert(PhysicsWorld != null, "a physics objects added to a scene without physics world");

            PhysicsWorld.AddPhysicsObject(this);
            MakeTransformDirty();

            ShapeFilter shapeFilter = GetComponent<ShapeFilter>();

            if (shapeFilter != null)
            {
                AssignShapeFilter(shapeFilter);
            }

            Material = PhysicsWorld.DefaultPhysicsMaterial;

            ShapeData.Initialize();

            Refresh();
        }

        protected override void OnInitialize()
        {
            //PhysicsWorld.AddPhysicsObject(this);
            MakeTransformDirty();

            //if (Shape == null)
            //{
            //	if(false)
            //	{
            //		Polygon polygon = new Polygon(Resource.GenerateName<Polygon>());
            //
            //		polygon.Vertices.Add(new Vector2(15, 15));
            //		polygon.Vertices.Add(new Vector2(15, -15));
            //		polygon.Vertices.Add(new Vector2(-15, -15));
            //		polygon.Vertices.Add(new Vector2(-15, 15));
            //
            //		Shape = polygon;
            //	}
            //
            //	{
            //		Circle circle = new Circle(Resource.GenerateName<Circle>());
            //
            //		circle.Radius = 16.0f;
            //
            //		Shape = circle;
            //	}
            //}
            //
            //if (Material == null)
            //{
            //	Material = PhysicsWorld.DefaultPhysicsMaterial;
            //}

            ShapeData.Initialize();

            Refresh();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Debug.Assert(PhysicsWorld != null, "PhysicsWorld is null on OnDestroy");

            PhysicsWorld.RemovePhysicsObject(this);
        }

        public void Refresh()
        {
            if (Type != PhysicsType.RigidBody)
            {
                Mass = 0.0f;
                InverseMass = 0.0f;
                Inertia = 0.0f;
                InverseInertia = 0.0f;
            }
            else
            {
                Mass = ShapeData.Area * Material.Density;

                InverseMass = 1.0f / Mass;

                Inertia = ShapeData.CalculateInertia(Material);
                //Inertia = 1.0f;
                InverseInertia = 1.0f / Inertia;
            }
        }

        public void ApplyImpulse(Vector2 impulse, Vector2 point)
        {
            float pointCrossImpulse = point * impulse;

            Velocity += impulse * InverseMass;

            if (!FixedRotation)
            {
                AngularVelocity += InverseInertia * pointCrossImpulse;
            }
        }

        internal Collision FindExistingCollisionWith(PhysicsObject physicsObject)
        {
            foreach (Collision collisison in Collisions)
            {
                if (collisison.PhysicsObjectA == physicsObject || collisison.PhysicsObjectB == physicsObject)
                {
                    return collisison;
                }
            }

            return null;
        }

        private bool CheckBoundingCircleWith(PhysicsObject physicsObject)
        {
            float centerDistanceSquared = (SceneEntity.GlobalPosition - physicsObject.SceneEntity.GlobalPosition).LengthSquared;

            float boundingCircleRadiusSquared = ShapeData.BoundingCircleRadius + physicsObject.ShapeData.BoundingCircleRadius;
            boundingCircleRadiusSquared *= boundingCircleRadiusSquared;

            return boundingCircleRadiusSquared >= centerDistanceSquared;
        }

        private bool CheckAABBWith(PhysicsObject physicsObject)
        {
            return ((physicsObject.ShapeData.MinX <= ShapeData.MinX && ShapeData.MinX <= physicsObject.ShapeData.MaxX) ||
                   (ShapeData.MinX <= physicsObject.ShapeData.MinX && physicsObject.ShapeData.MinX <= ShapeData.MaxX)) &&

                   ((physicsObject.ShapeData.MinY <= ShapeData.MinY && ShapeData.MinY <= physicsObject.ShapeData.MaxY) ||
                   (ShapeData.MinY <= physicsObject.ShapeData.MinY && physicsObject.ShapeData.MinY <= ShapeData.MaxY));
        }

        internal Collision CheckIntersectionAndGetCollision(PhysicsObject physicsObject)
        {
            Collision collision = null;

            Vector2 minmumTranslation;
            Vector2 intersectionPoint;

            if (CheckIntersection(physicsObject, out minmumTranslation, out intersectionPoint))
            {
                collision = FindExistingCollisionWith(physicsObject);

                if (collision == null)
                {
                    collision = new Collision();

                    collision.PhysicsObjectA = this;
                    collision.PhysicsObjectB = physicsObject;

                    collision.NewlyFoundOnCurrentUpdate = true;

                }
                else
                {
                    collision.NewlyFoundOnCurrentUpdate = false;
                }

                collision.MinimumTranslation = minmumTranslation;
                collision.Normal = minmumTranslation.Normalized;
                collision.IntersectionPoint = intersectionPoint;

                collision.FoundOnCurrentUpdate = true;
            }

            return collision;
        }

        internal bool CheckIntersection(PhysicsObject physicsObject, out Vector2 minimumTranslation, out Vector2 intersectionPoint)
        {
            minimumTranslation = Vector2.Zero;
            intersectionPoint = Vector2.Zero;

            if (CheckBoundingCircleWith(physicsObject) && CheckAABBWith(physicsObject))
            {
                if (physicsObject.Type != PhysicsType.Trigger)
                {
                    if (ShapeData.CheckIntersectionAndProduceResult(physicsObject.ShapeData, out minimumTranslation, out intersectionPoint))
                    {
                        return true;
                    }
                }
                else
                {
                    if (ShapeData.CheckIntersection(physicsObject.ShapeData))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal void CheckAndMakeTransformation()
        {
            if (_isTransformDirty)
            {
                MakeTransformation();

                RemoveTransformDirtyInformation();
            }
        }

        private void MakeTransformation()
        {
            Matrix4x4 transform = SceneEntity.TransformMatrix;

            ShapeData.PrepareTransformation(ref transform);

            UpdateOnGrid(PhysicsWorld.Grid);
        }

        internal void RepositionUsingCollisionInformations()
        {
            MinimumTranslation = Vector2.Zero;

            CheckAndMakeTransformation();

            foreach (Collision collision in Collisions)
            {
                PhysicsObject physicsObjectA = collision.PhysicsObjectA;
                PhysicsObject physicsObjectB = collision.PhysicsObjectB;

                if (physicsObjectB.Type != PhysicsType.Trigger)
                {
                    Vector2 minimumTranslation;
                    Vector2 intersectionPoint;

                    if (physicsObjectA == this)
                    {
                        if (!physicsObjectB.RepositionedOnCurrentUpdate)
                        {
                            physicsObjectB.CheckAndMakeTransformation();
                        }
                    }
                    else
                    {
                        if (!physicsObjectA.RepositionedOnCurrentUpdate)
                        {
                            physicsObjectA.CheckAndMakeTransformation();
                        }
                    }

                    if (physicsObjectA.CheckIntersection(physicsObjectB, out minimumTranslation, out intersectionPoint))
                    {
                        collision.IntersectionPoint = intersectionPoint;
                        collision.MinimumTranslation = minimumTranslation;
                        collision.Normal = minimumTranslation.Normalized;

                        minimumTranslation *= 0.99f;

                        bool staticCollision = physicsObjectB.IsStatic;

                        if (staticCollision)
                        {
                            physicsObjectA.MinimumTranslation += minimumTranslation;
                        }
                        else
                        {
                            if (physicsObjectA.RepositionedOnCurrentUpdate)
                            {
                                physicsObjectB.MinimumTranslation -= minimumTranslation;
                            }
                            else if (physicsObjectB.RepositionedOnCurrentUpdate)
                            {
                                physicsObjectA.MinimumTranslation += minimumTranslation;
                            }
                            else
                            {
                                Vector2 collisionPointOnA = intersectionPoint - physicsObjectA.SceneEntity.GlobalPosition;
                                Vector2 collisionPointOnB = intersectionPoint - physicsObjectB.SceneEntity.GlobalPosition;

                                float speedOfAOnCollisionPoint = (physicsObjectA.Velocity + physicsObjectA.AngularVelocity * collisionPointOnA).Length;
                                float speedOfBOnCollisionPoint = (physicsObjectB.Velocity + physicsObjectB.AngularVelocity * collisionPointOnB).Length;

                                float ratioOfB = 0.5f;
                                float ratioOfA = 0.5f;

                                bool speedOfAIsZero = Mathf.IsZero(speedOfAOnCollisionPoint);
                                bool speedOfBIsZero = Mathf.IsZero(speedOfBOnCollisionPoint);

                                if (!speedOfAIsZero && !speedOfBIsZero)
                                {
                                    ratioOfA = speedOfAOnCollisionPoint / (speedOfAOnCollisionPoint + speedOfBOnCollisionPoint);
                                    ratioOfB = 1.0f - ratioOfA;
                                }
                                else if (speedOfAIsZero)
                                {
                                    ratioOfB = 1.0f;
                                    ratioOfA = 0.0f;
                                }
                                else if (speedOfBIsZero)
                                {
                                    ratioOfB = 0.0f;
                                    ratioOfA = 1.0f;
                                }

                                physicsObjectA.MinimumTranslation += minimumTranslation * ratioOfA;
                                physicsObjectB.MinimumTranslation -= minimumTranslation * ratioOfB;
                            }
                        }
                    }
                }
            }

            RepositionedOnCurrentUpdate = true;
            SceneEntity.LocalPosition += MinimumTranslation;
            CheckAndMakeTransformation();
        }

        internal LinkedListNode<Collision> AddCollision(Collision collision)
        {
            LinkedListNode<Collision> result = Collisions.AddLast(collision);

            if (collision.IsStaticCollision)
            {
                StaticCollisionCount++;
            }

            return result;
        }

        internal void RemoveCollision(LinkedListNode<Collision> collisionNode)
        {
            Collisions.Remove(collisionNode);

            if (collisionNode.Value.IsStaticCollision)
            {
                StaticCollisionCount--;
            }
        }

        public bool IsInside(Vector2 worldPosition)
        {
            CheckAndMakeTransformation();
            return ShapeData.IsInside(worldPosition);
        }

        internal bool IsInsideOfGrid(PhysicsWorldGridCell grid)
        {
            return false;
        }

        internal void UpdateOnGrid(PhysicsWorldGrid grid)
        {
            float inverseGridLength = 1.0f / grid.Length;
            int halfGridSize = grid.Size / 2;

            int minGridX = (int)(ShapeData.MinX * inverseGridLength) + halfGridSize; //TODO: duplication
            int minGridY = (int)(ShapeData.MinY * inverseGridLength) + halfGridSize;

            int maxGridX = (int)(ShapeData.MaxX * inverseGridLength) + halfGridSize;
            int maxGridY = (int)(ShapeData.MaxY * inverseGridLength) + halfGridSize;

            if (_minGridX != minGridX || _minGridY != minGridY || _maxGridX != maxGridX || _maxGridY != maxGridY)
            {
                _minGridX = minGridX;
                _minGridY = minGridY;

                _maxGridX = maxGridX;
                _maxGridY = maxGridY;

                for (int x = minGridX; x <= maxGridX; x++)
                {
                    for (int y = minGridY; y <= maxGridY; y++)
                    {
                        _newGrids.Add(grid[x, y]);
                    }
                }

                grid.UpdatePhysicsObject(CurrentGrids, _newGrids, this);

                CurrentGrids.Clear();

                List<PhysicsWorldGridCell> swapper = _newGrids;
                _newGrids = CurrentGrids;
                CurrentGrids = swapper;
            }
        }

        [EntityMessageHandler(MessageType = typeof(SceneEntityTransformMatrixChangeMesssage))]
        private void OnEntityTransformMatrixChange(Message message)
        {
            MakeTransformDirty();
        }

        internal void MakeTransformDirty() //TODO: check if something goes wrong when changing object type
        {
            if (!_isTransformDirty)
            {
                _isTransformDirty = true;
                NodeOnDirtyTransformList = PhysicsWorld.OnPhysicsObjectTransformDirty(this);
            }
        }

        internal void RemoveTransformDirtyInformation()
        {
            if (_isTransformDirty)
            {
                PhysicsWorld.OnPhysicsObjectTransformClean(this);
                NodeOnDirtyTransformList = null;
                _isTransformDirty = false;
            }
        }

        public Vector2 VelocityOfPoint(Vector2 point)
        {
            return Velocity + point.Cross(AngularVelocity);
        }
    }
}