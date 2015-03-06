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
    internal class Collision
    {
        public Vector2 IntersectionPoint { get; internal set; }

        public Vector2 Normal { get; internal set; }

        public PhysicsObject PhysicsObjectA { get; internal set; }
        public PhysicsObject PhysicsObjectB { get; internal set; }

        internal LinkedListNode<Collision> NodeOnA { get; set; }
        internal LinkedListNode<Collision> NodeOnB { get; set; }
        internal LinkedListNode<Collision> NodeOnList { get; set; }

        internal bool FoundOnCurrentUpdate { get; set; }
        internal bool NewlyFoundOnCurrentUpdate { get; set; }

        internal Vector2 MinimumTranslation { get; set; }

        private Vector2 _collisionPointOnA;
        private Vector2 _collisionPointOnB;
        private Vector2 _relativeVelocity;

        private float _impulseScalar;
        private float _frictionImpulseScalar;
        private Vector2 _tangentVelocity;
        private Vector2 _tangent; //tangent vector for friction calculation
        private float _contactVelocity;

        public bool IsStaticCollision
        {
            get
            {
                return PhysicsObjectB.IsStatic;
            }
        }

        private void CalculateContactVelocity()
        {
            _relativeVelocity = (PhysicsObjectB.Velocity + _collisionPointOnB.Cross(PhysicsObjectB.AngularVelocity)) -
                (PhysicsObjectA.Velocity + _collisionPointOnA.Cross(PhysicsObjectA.AngularVelocity));

            _contactVelocity = _relativeVelocity.Dot(Normal);
        }

        private void CalculateImpulseScalar()
        {
            _impulseScalar = ImpulseScalar;
        }

        private void CalculateFrictionImpulseScalar()
        {
            _tangentVelocity = _relativeVelocity - Normal * _contactVelocity;
            _tangent = _tangentVelocity.Normalized;

            _frictionImpulseScalar = FrictionImpulseScalar;
        }

        internal void Resolve()
        {
            if (PhysicsObjectB.Type != PhysicsObject.PhysicsType.Trigger)
            {
                const float impulseCoeff = 1.0f;

                _collisionPointOnA = IntersectionPoint - PhysicsObjectA.SceneEntity.GlobalPosition;
                _collisionPointOnB = IntersectionPoint - PhysicsObjectB.SceneEntity.GlobalPosition;

                CalculateContactVelocity();

                if (_contactVelocity > Mathf.Epsilon)
                //if ((RigidBodyB.Velocity - RigidBodyA.Velocity).Dot(Normal) > Mathf.Epsilon)
                {

                    CalculateImpulseScalar();
                    //CalculateImpulseValues();

                    {
                        if (!Mathf.IsZero(_impulseScalar))
                        {
                            Vector2 impulseVector = _impulseScalar * Normal;

                            PhysicsObjectA.ApplyImpulse(-1.0f * impulseVector * impulseCoeff, _collisionPointOnA);

                            if (!PhysicsObjectB.IsStatic)
                            {
                                PhysicsObjectB.ApplyImpulse(impulseVector * impulseCoeff, _collisionPointOnB);
                            }
                        }
                    }

                    //if (false)
                    {
                        CalculateContactVelocity();
                        CalculateFrictionImpulseScalar();

                        if (!Mathf.IsZero(_frictionImpulseScalar))
                        {
                            float staticFriction = 0.4f;
                            float dynamicFriction = 0.2f;

                            Vector2 frictionImpulseVector;

                            if (Mathf.Abs(_frictionImpulseScalar) < _impulseScalar * staticFriction)
                            {
                                //Debug.Log("static friction apply");

                                frictionImpulseVector = _tangent * _frictionImpulseScalar;
                            }
                            else
                            {
                                //Debug.Log("dynamic friction apply");

                                frictionImpulseVector = _tangent * (-1.0f * _impulseScalar) * dynamicFriction;
                            }

                            PhysicsObjectA.ApplyImpulse(frictionImpulseVector * impulseCoeff, _collisionPointOnA);

                            if (!PhysicsObjectB.IsStatic)
                            {
                                PhysicsObjectB.ApplyImpulse(-1.0f * frictionImpulseVector * impulseCoeff, _collisionPointOnB);
                            }
                        }
                    }

                    //damping hack
                    if (false)
                    {
                        PhysicsObjectA.Velocity *= 0.9f;
                        PhysicsObjectA.AngularVelocity *= 0.9f;

                        if (!PhysicsObjectB.IsStatic)
                        {
                            PhysicsObjectB.Velocity *= 0.9f;
                            PhysicsObjectB.AngularVelocity *= 0.9f;
                        }
                    }
                }
            }
        }

        private float SumOfInverseMass
        {
            get
            {
                float rAcrossN = _collisionPointOnA * Normal;
                float rBcrossN = _collisionPointOnB * Normal;

                float result = 1.0f /
                (
                    PhysicsObjectA.InverseMass +
                    PhysicsObjectB.InverseMass +
                    rAcrossN * rAcrossN * PhysicsObjectA.InverseInertia +
                    rBcrossN * rBcrossN * PhysicsObjectB.InverseInertia
                );

                return result;
            }
        }

        private float ImpulseScalar
        {
            get
            {
                float rAcrossN = _collisionPointOnA * Normal;
                float rBcrossN = _collisionPointOnB * Normal;

                float j = 0.0f;

                j = -(1 + E) * _contactVelocity;
                j = j / (PhysicsObjectA.InverseMass + PhysicsObjectB.InverseMass + rAcrossN * rAcrossN * PhysicsObjectA.InverseInertia + rBcrossN * rBcrossN * PhysicsObjectB.InverseInertia);

                //j *= SumOfInverseMass;

                return j;
            }
        }

        private float FrictionImpulseScalar
        {
            get
            {
                float rAcrossT = _collisionPointOnA * _tangent;
                float rBcrossT = _collisionPointOnB * _tangent;

                //float rAcrossT = _collisionPointOnA * Normal;
                //float rBcrossT = _collisionPointOnB * Normal;

                float j = 0.0f;

                j = -(_relativeVelocity.Dot(_tangent));
                j = j / (PhysicsObjectA.InverseMass + PhysicsObjectB.InverseMass + rAcrossT * rAcrossT * PhysicsObjectA.InverseInertia + rBcrossT * rBcrossT * PhysicsObjectB.InverseInertia);

                //j *= SumOfInverseMass;

                if (Mathf.IsZero(j))
                {
                    return 0.0f;
                }

                return j;
            }
        }

        private float E
        {
            get
            {
                if (PhysicsObjectB.IsStatic)
                {
                    return PhysicsObjectA.Material.Restutition;
                }

                return Math.Min(PhysicsObjectA.Material.Restutition, PhysicsObjectB.Material.Restutition);
            }
        }
    }

    public struct CollisionInfo
    {
        public PhysicsObject CollidedBody { get; internal set; }
        public Vector2 IntersectionPoint { get; internal set; }
        public Vector2 Normal { get; internal set; }
    }
}
