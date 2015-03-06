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
    public class SceneEntity : Component
    {
        public SceneEntity Parent { get; private set; }

        internal LinkedListNode<SceneEntity> NodeOnSceneEntitiesList { get; set; }

        public Scene Scene { get; internal set; }

        private Matrix4x4 _transformMatrix;
        private Matrix4x4 _localTransform;
        private Vector2 _localPosition;
        private Vector2 _localScale = new Vector2(1, 1);
        private float _localRotation;
        private bool _localTransformDirty;
        private bool _parentTransformWasDirty;

        private Vector2 _globalPosition;

        private readonly SceneEntityTransformMatrixChangeMesssage _entityTransformMatrixChangeMesssage = new SceneEntityTransformMatrixChangeMesssage();

        [ComponentProperty]
        public Vector2 LocalPosition
        {
            get
            {
                return _localPosition;
            }
            set
            {
                if (_localPosition != value)
                {
                    _localPosition = value;
                    SetLocalTransformDirty();

                    Entity.SendMessage(_entityTransformMatrixChangeMesssage);
                }
            }
        }

        [ComponentProperty]
        public float LocalRotation
        {
            get
            {
                return _localRotation;
            }
            set
            {
                if (_localRotation != value)
                {
                    _localRotation = value;
                    SetLocalTransformDirty();

                    Entity.SendMessage(_entityTransformMatrixChangeMesssage);
                }
            }
        }

        public float LocalRotationAsRadian
        {
            get
            {
                return LocalRotation * Mathf.Deg2Rad;
            }
            set
            {
                LocalRotation = value * Mathf.Rad2Deg;
            }
        }

        [ComponentProperty]
        public Vector2 LocalScale
        {
            get
            {
                return _localScale;
            }
            set
            {
                if (_localScale != value)
                {
                    _localScale = value;
                    SetLocalTransformDirty();

                    Entity.SendMessage(_entityTransformMatrixChangeMesssage);
                }
            }
        }

        private void SetLocalTransformDirty()
        {
            if (!_localTransformDirty)
            {
                _localTransformDirty = true;

                LinkedListNode<Entity> childEntityNode = Entity.Children.First;

                while (childEntityNode != null)
                {
                    Entity childEntity = childEntityNode.Value;

                    childEntity.GetComponent<SceneEntity>().OnParentTransformDirty();

                    childEntityNode = childEntityNode.Next;
                }
            }
        }

        private void OnParentTransformDirty()
        {
            if (!_parentTransformWasDirty)
            {
                _parentTransformWasDirty = true;

                LinkedListNode<Entity> childEntityNode = Entity.Children.First;

                while (childEntityNode != null)
                {
                    Entity childEntity = childEntityNode.Value;

                    childEntity.GetComponent<SceneEntity>().OnParentTransformDirty();

                    childEntityNode = childEntityNode.Next;
                }
            }
        }

        public Vector2 GlobalPosition
        {
            get
            {
                if (Parent != null)
                {
                    if (IsTransformDirty)
                    {
                        RecalculateTransform();
                    }

                    return _globalPosition;
                }

                return _localPosition;
            }
        }

        public bool IsTransformDirty
        {
            get
            {
                SceneEntity parent = Parent;

                if (parent != null)
                {
                    return parent.IsTransformDirty || _localTransformDirty || _parentTransformWasDirty;
                }

                return _localTransformDirty || _parentTransformWasDirty;
            }
        }

        public Matrix4x4 TransformMatrix
        {
            get
            {
                if (IsTransformDirty)
                {
                    RecalculateTransform();
                }

                return _transformMatrix;
            }
        }

        protected override void OnInitialize()
        {
            Parent = Entity.Parent.GetComponent<SceneEntity>();

            SetLocalTransformDirty();
            _transformMatrix = Matrix4x4.Identity;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Scene.OnRemoveSceneEntity(this);
        }

        private void RecalculateTransform()
        {
            if (Parent != null && Parent.IsTransformDirty)
            {
                Parent.RecalculateTransform();
            }

            if (_localTransformDirty)
            {
                _localTransform = Matrix4x4.Transformation2D(_localScale, _localRotation * Mathf.Deg2Rad, _localPosition);
                _localTransformDirty = false;
            }

            if (Parent != null && _parentTransformWasDirty)
            {
                _parentTransformWasDirty = false;

                _transformMatrix = Parent.TransformMatrix * _localTransform;
                _globalPosition = Parent.TransformMatrix * _localPosition;
            }
            else
            {
                _transformMatrix = _localTransform;
                _globalPosition = _localPosition;
            }
        }

        public override string ToString()
        {
            return Entity.Name;
        }
    }

    public class SceneEntityTransformMatrixChangeMesssage : EntityMessage
    {

    }
}