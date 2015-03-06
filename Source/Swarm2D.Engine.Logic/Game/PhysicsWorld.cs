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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Library;
using Debug = Swarm2D.Library.Debug;

namespace Swarm2D.Engine.Logic
{
    public enum DebugPhysicsWorldStep
    {
        UpdatePositions,
        CheckCollisions,
        ResolveCollisions
    }

    //100 pixel = 1 meter
    public class PhysicsWorld : SceneController
    {
        public const float PixelToMeter = 0.01f;
        public const float MeterToPixel = 100.0f;

        public LinkedList<PhysicsObject> PhysicsObjects { get; private set; }

        private LinkedList<PhysicsObject> _rigidBodies;
        private LinkedList<PhysicsObject> _staticBodies;
        private LinkedList<PhysicsObject> _triggers;

        private List<PhysicsObject> _orderdedRigidBodiesToReposition;

        private bool _checkParallel = false;
        private bool _makeParallelTransformations = false;
        private bool __resolveCollisionsParallel = false;

        [ComponentProperty]
        public Vector2 Gravity { get; set; }

        private float _dt = 0.0f;

        private LinkedList<Collision> _collisionList;

        private List<Collision> _removedCollisionsOnLastSimulate;
        private List<Collision> _addedCollisionsOnLastSimulate;

        private int _debugCount = 100;
        private double _totalDebug = 0.0f;

        public bool SimulationEnabled { get; set; }
        public DebugPhysicsWorldStep CurrentDebugState { get; private set; }
        public bool DoNextSimulationStep { get; set; }

        internal PhysicsWorldGrid Grid { get; private set; }

        public PhysicsMaterial DefaultPhysicsMaterial { get; private set; }

        private LinkedList<PhysicsObject> _physicsObjectsWithDirtyTransform;
        private List<LinkedListNode<PhysicsObject>> _freeLinkedListNodesForDirtyTransform;

        private Circle _checkCircle;
        private CircleInstance _checkCircleData;

        internal Circle DefaultCircle { get; private set; }

        private float _gridCellLength = 64.0f;
        private int _gridSize = 512;

        private int[] _layerCollisionControl = new int[32];

        [ComponentProperty]
        public float GridCellLength 
        {
            get { return _gridCellLength; }
            set
            {
                if (!IsInitialized)
                {
                    _gridCellLength = value;
                }
            }
        }

        [ComponentProperty]
        public int GridSize
        {
            get { return _gridSize; }
            set
            {
                if (!IsInitialized)
                {
                    _gridSize = value;
                }
            }
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            _checkCircle = new Circle("checkCircle");
            _checkCircleData = new CircleInstance(_checkCircle);

            DefaultCircle = new Circle(Resource.GenerateName<Circle>());
            DefaultCircle.Radius = 16.0f;

            _physicsObjectsWithDirtyTransform = new LinkedList<PhysicsObject>();
            SimulationEnabled = true;
            CurrentDebugState = DebugPhysicsWorldStep.UpdatePositions;

            PhysicsObjects = new LinkedList<PhysicsObject>();

            _orderdedRigidBodiesToReposition = new List<PhysicsObject>(16384);
            _collisionList = new LinkedList<Collision>();

            _removedCollisionsOnLastSimulate = new List<Collision>();
            _addedCollisionsOnLastSimulate = new List<Collision>();

            _rigidBodies = new LinkedList<PhysicsObject>();
            _staticBodies = new LinkedList<PhysicsObject>();
            _triggers = new LinkedList<PhysicsObject>();

            DefaultPhysicsMaterial = new PhysicsMaterial("DefaultPhysicsMaterial");
            DefaultPhysicsMaterial.Density = 1.0f;
            DefaultPhysicsMaterial.Restutition = 1.0f;

            _freeLinkedListNodesForDirtyTransform = new List<LinkedListNode<PhysicsObject>>();

            for (int i = 0; i < 1024; i++)
            {
                _freeLinkedListNodesForDirtyTransform.Add(new LinkedListNode<PhysicsObject>(null));
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Grid = new PhysicsWorldGrid(GridSize, GridCellLength);
        }

        [EntityMessageHandler(MessageType = typeof(SceneControllerUpdateMessage))]
        private void OnUpdate(Message message)
        {
            int frameRate = GameLogic.FrameRate;
            const int iterationCount = 1;
            _dt = 1.0f / (float)(frameRate * iterationCount);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //if (GameInput.GetKeyDown(KeyCode.KeyP))
            //{
            //	_checkParallel = !_checkParallel;
            //
            //	Debug.Log("_checkParallel: " + _checkParallel);
            //}
            //
            //if (GameInput.GetKeyDown(KeyCode.KeyO))
            //{
            //	_makeParallelTransformations = !_makeParallelTransformations;
            //
            //	Debug.Log("_makeParallelTransformations: " + _makeParallelTransformations);
            //}
            //
            //if (GameInput.GetKeyDown(KeyCode.KeyL))
            //{
            //	__resolveCollisionsParallel = !__resolveCollisionsParallel;
            //
            //	Debug.Log("__resolveCollisionsParallel: " + __resolveCollisionsParallel);
            //}
            //
            //if (GameInput.GetKeyDown(KeyCode.KeyK))
            //{
            //	_debugCount = 0;
            //}

            //DebugRender.Disabled = false;

            if (SimulationEnabled)
            {
                for (int i = 0; i < iterationCount; i++)
                {
                    Simulate();
                }
            }

            if (!SimulationEnabled && DoNextSimulationStep)
            {
                Debug.Log("Current Physics World Step:" + CurrentDebugState);
                switch (CurrentDebugState)
                {
                    case DebugPhysicsWorldStep.UpdatePositions:
                        UpdatePositions();
                        CurrentDebugState = DebugPhysicsWorldStep.CheckCollisions;
                        break;
                    case DebugPhysicsWorldStep.CheckCollisions:
                        CurrentDebugState = DebugPhysicsWorldStep.ResolveCollisions;
                        CheckCollisions();
                        break;
                    case DebugPhysicsWorldStep.ResolveCollisions:
                        CurrentDebugState = DebugPhysicsWorldStep.UpdatePositions;
                        ResolveCollisions();
                        break;
                }
            }

            DoNextSimulationStep = false;

            //foreach (Collision collision in _collisionList)
            //{
            //	DebugRender.AddDebugLine(collision.RigidBodyA.SceneEntity.LocalPosition, collision.RigidBodyA.SceneEntity.LocalPosition + collision.MinimumTranslation);
            //}

            stopwatch.Stop();

            if (_debugCount < 60)
            {
                double elapsedMiliseconds = ((double)stopwatch.ElapsedTicks * 1000.0) / (double)Stopwatch.Frequency;
                _totalDebug += elapsedMiliseconds;
                Debug.Log(elapsedMiliseconds + " ms");
                _debugCount++;
            }
            else if (_debugCount == 60)
            {
                Debug.Log("Total " + _totalDebug + " ms");
                Debug.Log("Average " + (_totalDebug / 60.0) + " ms");
                _totalDebug = 0.0f;
                _debugCount = 100;
            }

            stopwatch = null;
        }

        public override void OnIdleUpdate()
        {
            base.OnIdleUpdate();

            MakeTransformations();
        }

        private void Simulate()
        {
            CurrentDebugState = DebugPhysicsWorldStep.UpdatePositions;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            UpdatePositions();

            long positionMs = sw.ElapsedMilliseconds;
            sw.Stop();
            sw.Start();

            CheckCollisions();
            long checkMs = sw.ElapsedMilliseconds;
            sw.Stop();
            sw.Start();

            ResolveCollisions();
            long resolveMs = sw.ElapsedMilliseconds;
            sw.Stop();
            sw.Start();
        }

        internal void AddPhysicsObject(PhysicsObject physicsObject)
        {
            physicsObject.NodeOnPhysicsObjectList = PhysicsObjects.AddLast(physicsObject);

            switch (physicsObject.Type)
            {
                case PhysicsObject.PhysicsType.RigidBody:
                    physicsObject.NodeOnTypeList = _rigidBodies.AddLast(physicsObject);
                    break;
                case PhysicsObject.PhysicsType.Static:
                    physicsObject.NodeOnTypeList = _staticBodies.AddLast(physicsObject);
                    break;
                case PhysicsObject.PhysicsType.Trigger:
                    physicsObject.NodeOnTypeList = _triggers.AddLast(physicsObject);
                    break;
            }

            physicsObject.MakeTransformDirty();
        }

        internal void RemovePhysicsObject(PhysicsObject physicsObject)
        {
            PhysicsObjects.Remove(physicsObject.NodeOnPhysicsObjectList);

            switch (physicsObject.Type)
            {
                case PhysicsObject.PhysicsType.RigidBody:
                    _rigidBodies.Remove(physicsObject.NodeOnTypeList);
                    break;
                case PhysicsObject.PhysicsType.Static:
                    _staticBodies.Remove(physicsObject.NodeOnTypeList);
                    break;
                case PhysicsObject.PhysicsType.Trigger:
                    _triggers.Remove(physicsObject.NodeOnTypeList);
                    break;
            }

            for (int i = 0; i < physicsObject.CurrentGrids.Count; i++)
            {
                PhysicsWorldGridCell physicsWorldGridCell = physicsObject.CurrentGrids[i];

                physicsWorldGridCell.RemovePhysicsObject(physicsObject);
            }

            physicsObject.CurrentGrids.Clear();

            if (physicsObject.Collisions.Count > 0)
            {
                LinkedListNode<Collision> currentNode = physicsObject.Collisions.First;

                while (currentNode != null)
                {
                    Collision collision = currentNode.Value;

                    LinkedListNode<Collision> nextNode = currentNode.Next;

                    physicsObject.RemoveCollision(currentNode);

                    PhysicsObject otherPhysicsObject = collision.PhysicsObjectA == physicsObject
                        ? collision.PhysicsObjectB
                        : collision.PhysicsObjectA;

                    LinkedListNode<Collision> nodeOnOther = collision.PhysicsObjectA == physicsObject
                        ? collision.NodeOnB
                        : collision.NodeOnA;

                    if (otherPhysicsObject.Type == PhysicsObject.PhysicsType.RigidBody)
                    {
                        otherPhysicsObject.RemoveCollision(nodeOnOther);
                    }

                    _collisionList.Remove(collision.NodeOnList);

                    currentNode = nextNode;
                }
            }

            physicsObject.RemoveTransformDirtyInformation();
        }

        public bool Raycast(Ray ray, out RaycastHit hitInfo, float distance)
        {
            hitInfo = new RaycastHit();

            LineSegment lineSegment = new LineSegment(ray.Origin, ray.Origin + ray.Direction * distance);

            int minGridX = (int)(lineSegment.P1.X / Grid.Length) + Grid.Size / 2;
            int minGridY = (int)(lineSegment.P1.Y / Grid.Length) + Grid.Size / 2;

            int maxGridX = (int)(lineSegment.P2.X / Grid.Length) + Grid.Size / 2;
            int maxGridY = (int)(lineSegment.P2.Y / Grid.Length) + Grid.Size / 2;

            if (minGridX > maxGridX)
            {
                int swapper = minGridX;
                minGridX = maxGridX;
                maxGridX = swapper;
            }

            if (minGridY > maxGridY)
            {
                int swapper = minGridY;
                minGridY = maxGridY;
                maxGridY = swapper;
            }

            for (int x = minGridX; x <= maxGridX; x++)
            {
                for (int y = minGridY; y <= maxGridY; y++)
                {
                    //TODO: it does not finds the nearest collided object
                    foreach (PhysicsObject physicsObject in Grid[x, y].PhysicsObjects)
                    {
                        Vector2 normal;
                        Vector2 intersectionPoint;

                        if (physicsObject.Type != PhysicsObject.PhysicsType.Trigger
                            && physicsObject.ShapeData.IsIntersects(lineSegment, out normal, out intersectionPoint))
                        {
                            hitInfo.Collider = physicsObject;
                            hitInfo.Normal = normal;
                            hitInfo.Point = intersectionPoint;

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void GetPhysicsObjectsIn(Vector2 position, float radius, List<PhysicsObject> result, bool onlyRigidAndStaticBodies = false)
        {
            Vector2 p1 = new Vector2(position.X - radius, position.Y - radius);
            Vector2 p2 = new Vector2(position.X + radius, position.Y + radius);

            float inverseGridLength = 1.0f / Grid.Length;

            int minGridX = (int)(p1.X * inverseGridLength) + Grid.Size / 2;
            int minGridY = (int)(p1.Y * inverseGridLength) + Grid.Size / 2;

            int maxGridX = (int)(p2.X * inverseGridLength) + Grid.Size / 2;
            int maxGridY = (int)(p2.Y * inverseGridLength) + Grid.Size / 2;

            if (minGridX > maxGridX)
            {
                int swapper = minGridX;
                minGridX = maxGridX;
                maxGridX = swapper;
            }

            if (minGridY > maxGridY)
            {
                int swapper = minGridY;
                minGridY = maxGridY;
                maxGridY = swapper;
            }

            _checkCircle.Radius = radius;
            _checkCircleData.PrepareTransformation(position);

            for (int x = minGridX; x <= maxGridX; x++)
            {
                for (int y = minGridY; y <= maxGridY; y++)
                {
                    Vector2 gridP1 = new Vector2();

                    gridP1.X = (x - Grid.Size / 2) * Grid.Length;
                    gridP1.Y = (y - Grid.Size / 2) * Grid.Length;

                    Vector2 gridP2 = gridP1 + new Vector2(Grid.Length, Grid.Length);

                    bool gridTotallyInside = Vector2.Distance(position, gridP1) < radius && Vector2.Distance(position, gridP2) < radius;

                    foreach (PhysicsObject physicsObject in Grid[x, y].PhysicsObjects) //TODO
                    {
                        if (!onlyRigidAndStaticBodies ||
                            onlyRigidAndStaticBodies && physicsObject.Type != PhysicsObject.PhysicsType.Trigger)
                        {
                            if (gridTotallyInside || physicsObject.ShapeData.CheckIntersection(_checkCircleData))
                            {
                                result.Add(physicsObject);
                            }
                        }
                    }
                }
            }
        }

        #region Update Positions

        private void UpdatePositions()
        {
            MakeUpdatePositionsJob();

            if (_makeParallelTransformations)
            {
                MakeTransformationsParallel();
            }
            else
            {
                MakeTransformations();
            }
        }

        private void MakeUpdatePositionsJob()
        {
            LinkedListNode<PhysicsObject> currentRigidBodyNode = _rigidBodies.First;

            while (currentRigidBodyNode != null)
            {
                PhysicsObject rigidBody = currentRigidBodyNode.Value;

                Debug.Assert(!rigidBody.Entity.IsDestroyed, "rigidbody entity: " + rigidBody.Entity.Name + " is destroyed");

                rigidBody.PreviousPosition = rigidBody.SceneEntity.LocalPosition;
                rigidBody.PreviousRotation = rigidBody.SceneEntity.LocalRotation;

                currentRigidBodyNode = currentRigidBodyNode.Next;
            }

            currentRigidBodyNode = _rigidBodies.First;

            while (currentRigidBodyNode != null)
            {
                PhysicsObject rigidBody = currentRigidBodyNode.Value;

                if (Mathf.IsZero(rigidBody.Velocity.Length))
                {
                    rigidBody.Velocity = Vector2.Zero;
                }

                if (Mathf.IsZero(rigidBody.AngularVelocity))
                {
                    rigidBody.AngularVelocity = 0.0f;
                }

                currentRigidBodyNode = currentRigidBodyNode.Next;
            }

            currentRigidBodyNode = _rigidBodies.First;

            while (currentRigidBodyNode != null)
            {
                PhysicsObject rigidBody = currentRigidBodyNode.Value;

                rigidBody.SceneEntity.LocalPosition += rigidBody.Velocity * _dt;
                rigidBody.SceneEntity.LocalRotation += rigidBody.AngularVelocity * Mathf.Rad2Deg * _dt;

                currentRigidBodyNode = currentRigidBodyNode.Next;
            }

            currentRigidBodyNode = _rigidBodies.First;

            while (currentRigidBodyNode != null)
            {
                PhysicsObject rigidBody = currentRigidBodyNode.Value;

                rigidBody.Velocity += Gravity * _dt;

                currentRigidBodyNode = currentRigidBodyNode.Next;
            }
        }

        private void MakeTransformationsParallel()
        {
            //Parallel.ForEach(PhysicsObjects, rigidBody =>
            //{
            //	rigidBody.MakeTransformation();
            //});
        }

        private void MakeTransformations()
        {
            //foreach (PhysicsObject rigidBody in PhysicsObjects)
            //{
            //	rigidBody.CheckAndMakeTransformation();
            //}

            while (_physicsObjectsWithDirtyTransform.Count > 0)
            {
                PhysicsObject physicsObject = _physicsObjectsWithDirtyTransform.First.Value;
                physicsObject.CheckAndMakeTransformation();
            }

            //_physicsObjectsWithDirtyTransform.Clear();
        }

        #endregion

        #region Collision Detection

        struct CollisionCheckData
        {
            public PhysicsObject PhysicsObjectA;
            public PhysicsObject PhysicsObjectB;
            public Collision Collision;
        }

        private List<CollisionCheckData> _collisionCheckDatas = new List<CollisionCheckData>(131288);

        private void CheckCollisions()
        {
            foreach (Collision collision in _collisionList)
            {
                collision.FoundOnCurrentUpdate = false;
            }

            _addedCollisionsOnLastSimulate.Clear();
            _removedCollisionsOnLastSimulate.Clear();

            {
                CollectCollisionBetweenRigidAndStaticBodies();
                CollectCollisionBetweenRigidBodies();
                CollectCollisionBetweenRigidBodiesAndTriggers();
            }
            
            LinkedListNode<PhysicsObject> currentRigidBodyNode = _rigidBodies.First;

            while (currentRigidBodyNode != null)
            {
                PhysicsObject rigidBody = currentRigidBodyNode.Value;

                rigidBody.CurrentlyCollectedRigidBodiesToCheckWith.Clear();
                rigidBody.CurrentlyCollectedStaticBodiesToCheckWith.Clear();
                rigidBody.CurrentlyCollectedTriggersToCheckWith.Clear();

                currentRigidBodyNode = currentRigidBodyNode.Next;
            }

            if (_checkParallel && false)
            {
                //Parallel.For(0, _collisionCheckDatas.Count, i =>
                //{
                //	CollisionCheckData collisionCheckData = _collisionCheckDatas[i];
                //
                //	PhysicsObject physicsObjectA = collisionCheckData.PhysicsObjectA;
                //	PhysicsObject physicsObjectB = collisionCheckData.PhysicsObjectB;
                //
                //	collisionCheckData.Collision = physicsObjectA.CheckIntersectionAndGetCollision(physicsObjectB);
                //
                //	_collisionCheckDatas[i] = collisionCheckData;
                //});
            }
            else
            {
                for (int i = 0; i < _collisionCheckDatas.Count; i++)
                {
                    CollisionCheckData collisionCheckData = _collisionCheckDatas[i];

                    PhysicsObject physicsObjectA = collisionCheckData.PhysicsObjectA;
                    PhysicsObject physicsObjectB = collisionCheckData.PhysicsObjectB;

                    collisionCheckData.Collision = physicsObjectA.CheckIntersectionAndGetCollision(physicsObjectB);

                    _collisionCheckDatas[i] = collisionCheckData;
                }
            }

            for (int i = 0; i < _collisionCheckDatas.Count; i++)
            {
                CollisionCheckData collisionCheckData = _collisionCheckDatas[i];

                Collision collision = collisionCheckData.Collision;

                if (collision != null && collision.NewlyFoundOnCurrentUpdate)
                {
                    PhysicsObject physicsObjectA = collisionCheckData.PhysicsObjectA;
                    PhysicsObject physicsObjectB = collisionCheckData.PhysicsObjectB;

                    collision.NodeOnA = physicsObjectA.AddCollision(collision);

                    if (physicsObjectB.Type == PhysicsObject.PhysicsType.RigidBody)
                    {
                        collision.NodeOnB = physicsObjectB.AddCollision(collision);
                    }

                    collision.NodeOnList = _collisionList.AddLast(collision);
                    _addedCollisionsOnLastSimulate.Add(collision);
                }
            }

            _collisionCheckDatas.Clear();

            RemoveSeperatedCollisions();
        }

        private void CollectCollisionBetweenRigidBodies()
        {
            LinkedListNode<PhysicsObject> currentRigidBodyANode = _rigidBodies.First;

            while (currentRigidBodyANode != null)
            {
                PhysicsObject rigidBodyA = currentRigidBodyANode.Value;

                for (int j = 0; j < rigidBodyA.CurrentGrids.Count; j++)
                {
                    LinkedListNode<PhysicsObject> currentRigidBodyBNode = rigidBodyA.CurrentGrids[j].RigidBodies.First;

                    while (currentRigidBodyBNode != null)
                    {
                        PhysicsObject rigidBodyB = currentRigidBodyBNode.Value;

                        if (rigidBodyA != rigidBodyB)
                        {
                            if (CheckIfLayersAllowCollision(rigidBodyA.Layer, rigidBodyB.Layer))
                            {
                                CollisionCheckData collisionCheckData = new CollisionCheckData();
                                collisionCheckData.PhysicsObjectA = rigidBodyA;
                                collisionCheckData.PhysicsObjectB = rigidBodyB;

                                if (!rigidBodyA.CurrentlyCollectedRigidBodiesToCheckWith.Contains(rigidBodyB))
                                {
                                    rigidBodyA.CurrentlyCollectedRigidBodiesToCheckWith.Add(rigidBodyB);
                                    rigidBodyB.CurrentlyCollectedRigidBodiesToCheckWith.Add(rigidBodyA);
                                    _collisionCheckDatas.Add(collisionCheckData);
                                }
                            }
                        }

                        currentRigidBodyBNode = currentRigidBodyBNode.Next;
                    }
                }

                currentRigidBodyANode = currentRigidBodyANode.Next;
            }
        }

        private void CollectCollisionBetweenRigidAndStaticBodies()
        {
            LinkedListNode<PhysicsObject> currentRigidBodyNode = _rigidBodies.First;

            while (currentRigidBodyNode != null)
            {
                PhysicsObject rigidBody = currentRigidBodyNode.Value;

                for (int j = 0; j < rigidBody.CurrentGrids.Count; j++)
                {
                    LinkedListNode<PhysicsObject> currentStaticBodyNode = rigidBody.CurrentGrids[j].StaticBodies.First;

                    while (currentStaticBodyNode != null)
                    {
                        PhysicsObject staticBody = currentStaticBodyNode.Value;

                        if (CheckIfLayersAllowCollision(rigidBody.Layer, staticBody.Layer))
                        {
                            CollisionCheckData collisionCheckData = new CollisionCheckData();
                            collisionCheckData.PhysicsObjectA = rigidBody;
                            collisionCheckData.PhysicsObjectB = staticBody;

                            if (!rigidBody.CurrentlyCollectedStaticBodiesToCheckWith.Contains(staticBody))
                            {
                                rigidBody.CurrentlyCollectedStaticBodiesToCheckWith.Add(staticBody);
                                _collisionCheckDatas.Add(collisionCheckData);
                            }
                        }

                        currentStaticBodyNode = currentStaticBodyNode.Next;
                    }
                }

                currentRigidBodyNode = currentRigidBodyNode.Next;
            }
        }

        private void CollectCollisionBetweenRigidBodiesAndTriggers()
        {
            LinkedListNode<PhysicsObject> currentRigidBodyNode = _rigidBodies.First;

            while (currentRigidBodyNode != null)
            {
                PhysicsObject rigidBody = currentRigidBodyNode.Value;

                for (int j = 0; j < rigidBody.CurrentGrids.Count; j++)
                {
                    LinkedListNode<PhysicsObject> currentTriggerNode = rigidBody.CurrentGrids[j].Triggers.First;

                    while (currentTriggerNode != null)
                    {
                        PhysicsObject trigger = currentTriggerNode.Value;

                        if (CheckIfLayersAllowCollision(rigidBody.Layer, trigger.Layer))
                        {
                            CollisionCheckData collisionCheckData = new CollisionCheckData();
                            collisionCheckData.PhysicsObjectA = rigidBody;
                            collisionCheckData.PhysicsObjectB = trigger;

                            if (!rigidBody.CurrentlyCollectedTriggersToCheckWith.Contains(trigger))
                            {
                                rigidBody.CurrentlyCollectedTriggersToCheckWith.Add(trigger);
                                _collisionCheckDatas.Add(collisionCheckData);
                            }
                        }

                        currentTriggerNode = currentTriggerNode.Next;
                    }
                }

                currentRigidBodyNode = currentRigidBodyNode.Next;
            }
        }

        private void RemoveSeperatedCollisions()
        {
            LinkedListNode<Collision> currentNode = _collisionList.First;

            while (currentNode != null)
            {
                Collision collision = currentNode.Value;

                LinkedListNode<Collision> nextNode = currentNode.Next;

                if (!collision.FoundOnCurrentUpdate)
                {
                    PhysicsObject physicsObjectA = collision.PhysicsObjectA;
                    PhysicsObject physicsObjectB = collision.PhysicsObjectB;

                    //Debug.Log("collision exit between " + rigidBodyA.Entity.Name + " " + rigidBodyB.Entity.Name + " !");
                    _collisionList.Remove(currentNode);

                    _removedCollisionsOnLastSimulate.Add(collision);

                    physicsObjectA.RemoveCollision(collision.NodeOnA);

                    if (physicsObjectB.Type == PhysicsObject.PhysicsType.RigidBody)
                    {
                        physicsObjectB.RemoveCollision(collision.NodeOnB);
                    }
                }

                currentNode = nextNode;
            }
        }

        #endregion

        #region Collision Resolution

        private void ResolveCollisions()
        {
            PrepareRepositionList();
            RepositionCollidedBodies();

            //for (int i = 0; i < 5; i++)
            {
                if (__resolveCollisionsParallel && false)
                {
                    //Parallel.ForEach(_collisionList, collision =>
                    //{
                    //	collision.Resolve();
                    //});
                }
                else
                {
                    foreach (Collision collision in _collisionList)
                    {
                        collision.Resolve();
                    }
                }
            }

            SendCollisionMessages();
        }

        private void SortRigidBodies()
        {
            //_rigidBodies.Sort(RigidBodySorterForCollisionResolution);

            if (_rigidBodies.Count > 1)
            {
                bool sort = true;

                while (sort) //bubble sort
                {
                    sort = false;

                    LinkedListNode<PhysicsObject> currentNode = _rigidBodies.First;

                    while (currentNode.Next != null)
                    {
                        LinkedListNode<PhysicsObject> nextNode = currentNode.Next;

                        if (RigidBodySorterForCollisionResolution(currentNode.Value, nextNode.Value) == 1)
                        {
                            PhysicsObject rigidBodySwapper = currentNode.Value;
                            currentNode.Value = nextNode.Value;
                            nextNode.Value = rigidBodySwapper;

                            currentNode.Value.NodeOnTypeList = currentNode;
                            nextNode.Value.NodeOnTypeList = nextNode;
                            sort = true;
                        }

                        currentNode = currentNode.Next;
                    }
                }
            }
        }

        private void PrepareRepositionList()
        {
            SortRigidBodies();

            _orderdedRigidBodiesToReposition.Clear();

            {
                LinkedListNode<PhysicsObject> currentRigidBodyNode = _rigidBodies.First;

                while (currentRigidBodyNode != null)
                {
                    PhysicsObject rigidBody = currentRigidBodyNode.Value;

                    rigidBody.RepositionedOnCurrentUpdate = false;
                    rigidBody.AddedToOrderdedRigidBodiesToRepositionList = false;

                    currentRigidBodyNode = currentRigidBodyNode.Next;
                }
            }

            {
                LinkedListNode<PhysicsObject> currentRigidBodyNode = _rigidBodies.Last;

                //first we'll add rigid bodies that has collision with static objects
                {
                    while (currentRigidBodyNode != null)
                    {
                        PhysicsObject rigidBody = currentRigidBodyNode.Value;

                        if (rigidBody.StaticCollisionCount == 0)
                        {
                            break;
                        }

                        rigidBody.AddedToOrderdedRigidBodiesToRepositionList = true;
                        _orderdedRigidBodiesToReposition.Add(rigidBody);

                        currentRigidBodyNode = currentRigidBodyNode.Previous;
                    }
                }


                //now we'll add rigid bodies that has indirect collision with static objects
                {
                    for (int i = 0; i < _orderdedRigidBodiesToReposition.Count; i++)
                    {
                        //enteredCurrentLoop = true;
                        PhysicsObject rigidBody = _orderdedRigidBodiesToReposition[i];

                        foreach (Collision collision in rigidBody.Collisions)
                        {
                            PhysicsObject neighbour = collision.PhysicsObjectA == rigidBody
                                ? collision.PhysicsObjectB
                                : collision.PhysicsObjectA;

                            if (!neighbour.AddedToOrderdedRigidBodiesToRepositionList)
                            {
                                neighbour.AddedToOrderdedRigidBodiesToRepositionList = true;
                                _orderdedRigidBodiesToReposition.Add(neighbour);
                            }
                        }
                    }
                }

                //finally we'll add the ones that has no direct or indirect collision with static objects
                {
                    while (currentRigidBodyNode != null)
                    {
                        PhysicsObject rigidBody = currentRigidBodyNode.Value;

                        if (!rigidBody.AddedToOrderdedRigidBodiesToRepositionList)
                        {
                            rigidBody.AddedToOrderdedRigidBodiesToRepositionList = true;
                            _orderdedRigidBodiesToReposition.Add(rigidBody);
                        }

                        currentRigidBodyNode = currentRigidBodyNode.Previous;
                    }
                }
            }
        }

        private void RepositionCollidedBodies()
        {
            //bool enableDebugRenderAfter = false;
            //
            //if (!DebugRender.Disabled)
            //{
            //	enableDebugRenderAfter = true;
            //	DebugRender.Disabled = true;
            //}

            for (int i = 0; i < _orderdedRigidBodiesToReposition.Count; i++)
            {
                PhysicsObject rigidBody = _orderdedRigidBodiesToReposition[i];

                rigidBody.RepositionUsingCollisionInformations();
            }

            //if (enableDebugRenderAfter)
            //{
            //	DebugRender.Disabled = false;
            //}
        }

        private int RigidBodySorterForCollisionResolution(PhysicsObject a, PhysicsObject b)
        {
            if (a.StaticCollisionCount > b.StaticCollisionCount)
            {
                return 1;
            }
            else if (a.StaticCollisionCount == b.StaticCollisionCount)
            {
                if (a.CollisionCount > b.CollisionCount)
                {
                    return 1;
                }
                else if (a.CollisionCount < b.CollisionCount)
                {
                    return -1;
                }
            }
            else if (a.StaticCollisionCount < b.StaticCollisionCount)
            {
                return -1;
            }

            return 0;
        }

        #endregion

        public void SetLayerCollision(int layerA, int layerB, bool collides)
        {
            if (collides)
            {
                int layerAMask = ~(1 << layerA);
                int layerBMask = ~(1 << layerB);

                _layerCollisionControl[layerA] = _layerCollisionControl[layerA] & layerBMask;
                _layerCollisionControl[layerB] = _layerCollisionControl[layerB] & layerAMask;
            }
            else
            {
                int layerAMask = (1 << layerA);
                int layerBMask = (1 << layerB);

                _layerCollisionControl[layerA] = _layerCollisionControl[layerA] | layerBMask;
                _layerCollisionControl[layerB] = _layerCollisionControl[layerB] | layerAMask;
            }
        }

        private bool CheckIfLayersAllowCollision(int layerA, int layerB)
        {
            int layerAMask = 1 << layerA;
            int layerBMask = 1 << layerB;

            int layerCollisionMask = _layerCollisionControl[layerA];

            if ((layerCollisionMask & layerBMask) == 0)
            {
                return true;
            }

            return false;
        }

        public override void OnReset()
        {
            PhysicsObjects.Clear();
            _rigidBodies.Clear();
            _staticBodies.Clear();
            _triggers.Clear();
            _collisionList.Clear();
            Grid.Reset();
        }

        private void SendCollisionMessages()
        {
            for (int i = 0; i < _removedCollisionsOnLastSimulate.Count; i++)
            {
                var collision = _removedCollisionsOnLastSimulate[i];

                if (collision.PhysicsObjectB.Type == PhysicsObject.PhysicsType.Trigger)
                {
                    collision.PhysicsObjectB.Entity.SendMessage("OnTriggerExit", collision.PhysicsObjectA);
                }
                else
                {
                    {
                        CollisionInfo collisionInfo = new CollisionInfo();

                        collisionInfo.CollidedBody = collision.PhysicsObjectB;
                        collisionInfo.Normal = collision.Normal;
                        collisionInfo.IntersectionPoint = collision.IntersectionPoint;

                        collision.PhysicsObjectA.Entity.SendMessage("OnCollisionExit", collisionInfo);
                    }

                    {
                        CollisionInfo collisionInfo = new CollisionInfo();

                        collisionInfo.CollidedBody = collision.PhysicsObjectA;
                        collisionInfo.Normal = collision.Normal;
                        collisionInfo.IntersectionPoint = collision.IntersectionPoint;

                        collision.PhysicsObjectB.Entity.SendMessage("OnCollisionExit", collisionInfo);
                    }
                }
            }

            for (int index = 0; index < _addedCollisionsOnLastSimulate.Count; index++)
            {
                var collision = _addedCollisionsOnLastSimulate[index];

                if (collision.PhysicsObjectB.Type == PhysicsObject.PhysicsType.Trigger)
                {
                    collision.PhysicsObjectB.Entity.SendMessage("OnTriggerEnter", collision.PhysicsObjectA);
                }
                else
                {
                    {
                        CollisionInfo collisionInfo = new CollisionInfo();

                        collisionInfo.CollidedBody = collision.PhysicsObjectB;
                        collisionInfo.Normal = collision.Normal;
                        collisionInfo.IntersectionPoint = collision.IntersectionPoint;

                        collision.PhysicsObjectA.Entity.SendMessage("OnCollisionEnter", collisionInfo);
                    }

                    {
                        CollisionInfo collisionInfo = new CollisionInfo();

                        collisionInfo.CollidedBody = collision.PhysicsObjectA;
                        collisionInfo.Normal = collision.Normal;
                        collisionInfo.IntersectionPoint = collision.IntersectionPoint;

                        collision.PhysicsObjectB.Entity.SendMessage("OnCollisionEnter", collisionInfo);
                    }
                }
            }
        }

        internal LinkedListNode<PhysicsObject> OnPhysicsObjectTransformDirty(PhysicsObject physicsObject)
        {
            Debug.Assert(!physicsObject.Entity.IsDestroyed, "a destroyed physicsObject used at PhysicsWorld::OnPhysicsObjectTransformDirty!!");

            LinkedListNode<PhysicsObject> nodeOnDirtyTransformList = null;

            if (_freeLinkedListNodesForDirtyTransform.Count > 0)
            {
                nodeOnDirtyTransformList = _freeLinkedListNodesForDirtyTransform[_freeLinkedListNodesForDirtyTransform.Count - 1];
                _freeLinkedListNodesForDirtyTransform.RemoveAt(_freeLinkedListNodesForDirtyTransform.Count - 1);
            }
            else
            {
                nodeOnDirtyTransformList = new LinkedListNode<PhysicsObject>(null);
            }

            nodeOnDirtyTransformList.Value = physicsObject;

            _physicsObjectsWithDirtyTransform.AddLast(nodeOnDirtyTransformList);

            return nodeOnDirtyTransformList;
        }

        internal void OnPhysicsObjectTransformClean(PhysicsObject physicsObject)
        {
            _freeLinkedListNodesForDirtyTransform.Add(physicsObject.NodeOnDirtyTransformList);
            _physicsObjectsWithDirtyTransform.Remove(physicsObject.NodeOnDirtyTransformList);
        }
    }
}