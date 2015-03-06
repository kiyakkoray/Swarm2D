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
    internal class PhysicsWorldGridCell
    {
        internal bool Outter { get; private set; }
        internal int X { get; private set; }
        internal int Y { get; private set; }

        internal LinkedList<PhysicsObject> RigidBodies { get; private set; }
        internal LinkedList<PhysicsObject> StaticBodies { get; private set; }
        internal LinkedList<PhysicsObject> Triggers { get; private set; }

        internal LinkedList<PhysicsObject> PhysicsObjects { get; private set; }

        private List<LinkedListNode<PhysicsObject>> _freeLinkedListNodes;

        internal PhysicsWorldGridCell(int x, int y)
        {
            Configure(x, y, false);
        }

        internal PhysicsWorldGridCell()
        {
            Configure(0, 0, true);
        }

        private void Configure(int x, int y, bool outter)
        {
            X = x;
            Y = y;
            Outter = outter;
            RigidBodies = new LinkedList<PhysicsObject>();
            StaticBodies = new LinkedList<PhysicsObject>();
            Triggers = new LinkedList<PhysicsObject>();
            PhysicsObjects = new LinkedList<PhysicsObject>();

            _freeLinkedListNodes = new List<LinkedListNode<PhysicsObject>>();
        }

        private LinkedListNode<PhysicsObject> GetNewNode(PhysicsObject physicsObject)
        {
            LinkedListNode<PhysicsObject> result = null;

            if (_freeLinkedListNodes.Count > 0)
            {
                result = _freeLinkedListNodes[_freeLinkedListNodes.Count - 1];
                _freeLinkedListNodes.RemoveAt(_freeLinkedListNodes.Count - 1);
            }
            else
            {
                result = new LinkedListNode<PhysicsObject>(null);
            }

            result.Value = physicsObject;

            return result;
        }

        private void ReleaseNode(LinkedListNode<PhysicsObject> node)
        {
            node.Value = null;
            _freeLinkedListNodes.Add(node);
        }

        internal void AddPhysicsObject(PhysicsObject physicsObject)
        {
            Debug.Assert(!physicsObject.Entity.IsDestroyed, "a destroyed physicsObject used at PhysicsWorldGridCell::AddPhysicsObject!!");
            PhysicsObjects.AddLast(GetNewNode(physicsObject));

            switch (physicsObject.Type)
            {
                case PhysicsObject.PhysicsType.RigidBody:
                    RigidBodies.AddLast(GetNewNode(physicsObject));
                    break;
                case PhysicsObject.PhysicsType.Static:
                    StaticBodies.AddLast(GetNewNode(physicsObject));
                    break;
                case PhysicsObject.PhysicsType.Trigger:
                    Triggers.AddLast(GetNewNode(physicsObject));
                    break;
            }
        }

        internal void RemovePhysicsObject(PhysicsObject physicsObject)
        {
            LinkedListNode<PhysicsObject> nodeOnPhysicsObjects = PhysicsObjects.Find(physicsObject);
            PhysicsObjects.Remove(nodeOnPhysicsObjects);
            ReleaseNode(nodeOnPhysicsObjects);

            switch (physicsObject.Type)
            {
                case PhysicsObject.PhysicsType.RigidBody:
                    {
                        LinkedListNode<PhysicsObject> nodeOnTypeList = RigidBodies.Find(physicsObject);
                        RigidBodies.Remove(nodeOnTypeList);
                        ReleaseNode(nodeOnTypeList);
                    }
                    break;
                case PhysicsObject.PhysicsType.Static:
                    {
                        LinkedListNode<PhysicsObject> nodeOnTypeList = StaticBodies.Find(physicsObject);
                        StaticBodies.Remove(nodeOnTypeList);
                        ReleaseNode(nodeOnTypeList);
                    }
                    break;
                case PhysicsObject.PhysicsType.Trigger:
                    {
                        LinkedListNode<PhysicsObject> nodeOnTypeList = Triggers.Find(physicsObject);
                        Triggers.Remove(nodeOnTypeList);
                        ReleaseNode(nodeOnTypeList);
                    }
                    break;
            }
        }

        internal void Reset()
        {
            RigidBodies.Clear();
            StaticBodies.Clear();
            Triggers.Clear();
            PhysicsObjects.Clear();
        }
    }

}
