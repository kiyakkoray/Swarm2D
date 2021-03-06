﻿/******************************************************************************
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
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.Engine.View
{
    [RequiresComponent(typeof(SceneEntity))]
    [PoolableComponent]
    public sealed class DebugRenderer : Renderer
    {
        public bool DebugPhysics { get; set; }

        private static Mesh _quadMesh;
        private static Mesh _quadLineMesh;

        public override Box BoundingBox
        {
            get
            {
                Box boundingBox = new Box();

                boundingBox.Size = new Vector2(30, 30);
                boundingBox.Position = SceneEntity.GlobalPosition - boundingBox.Size * 0.5f;

                return boundingBox;
            }
        }

        static DebugRenderer()
        {
            _quadMesh = new Mesh(MeshTopology.Quads, 4);

            _quadMesh.Vertices[0] = -15.0f;
            _quadMesh.Vertices[1] = -15.0f;

            _quadMesh.Vertices[2] = 15.0f;
            _quadMesh.Vertices[3] = -15.0f;

            _quadMesh.Vertices[4] = 15.0f;
            _quadMesh.Vertices[5] = 15.0f;

            _quadMesh.Vertices[6] = -15.0f;
            _quadMesh.Vertices[7] = 15.0f;

            _quadLineMesh = Mesh.CreateLineTopologyMeshWithQuadVertices(_quadMesh.Vertices, _quadMesh.VertexCount);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Width = 30.0f;
            Height = 30.0f;
        }

        protected override void OnDestroy()
        {
            DebugPhysics = false;

            base.OnDestroy();
        }

        public override void Render(RenderContext renderContext, Box renderBox)
        {
            ShapeFilter shapeFilter = Entity.GetComponent<ShapeFilter>();

            Color normalColor = new Color(127, 127, 127, 127);
            Color debugColor = new Color(32, 32, 32, 160);
            Color outlineColor = new Color(127, 255, 0, 127);

            Color selectedColor = DebugPhysics ? debugColor : normalColor;

            if (shapeFilter != null)
            {
                object shapeToRender = shapeFilter;

                if (shapeToRender is ResourceShapeFilter)
                {
                    shapeToRender = ((ResourceShapeFilter)shapeToRender).Shape;
                }

                if (shapeToRender is IPolygon)
                {
                    IPolygon polygon = (IPolygon)shapeToRender;

                    Mesh polygonMesh = Mesh.CreateTriangleTopologyMeshWithPolygonCoordinates(polygon.Vertices);
                    Mesh lineMesh = Mesh.CreateLineTopologyMeshWithPolygonCoordinates(polygon.Vertices);

                    renderContext.AddDrawMeshJob(SceneEntity.TransformMatrix, polygonMesh, new PrimitivePolygonMaterial(selectedColor, 100));
                    renderContext.AddDrawMeshJob(SceneEntity.TransformMatrix, lineMesh, new PrimitivePolygonMaterial(outlineColor, 101));

                    Width = 200;
                    Height = 200;

                    //Width = Math.Abs(polygonData.MaxX - polygonData.MinX);
                    //Height = Math.Abs(polygonData.MaxY - polygonData.MinY);
                }
                else if (shapeToRender is ICircle)
                {
                    ICircle circle = (ICircle)shapeToRender;

                    Width = circle.Radius;
                    Height = circle.Radius;

                    renderContext.AddDrawMeshJob(SceneEntity.TransformMatrix, Mesh.CreateTriangleTopologyMeshWithCircleRadius(circle.Radius), new PrimitivePolygonMaterial(selectedColor, 100));
                    renderContext.AddDrawMeshJob(SceneEntity.TransformMatrix, Mesh.CreateLineTopologyMeshWithCircleRadius(circle.Radius), new PrimitivePolygonMaterial(outlineColor, 101));
                }

                //if (DebugPhysics)
                //{
                //	foreach (Collision collision in physicsObject.Collisions)
                //	{
                //		Vector2 localIntersectionPoint = GetLocalPoint(collision.IntersectionPoint);
                //
                //		ioSystem.AddGraphicsCommand(new CommandDrawDebugPoint(localIntersectionPoint));
                //
                //		float collisionSign = collision.PhysicsObjectA == physicsObject ? 1.0f : -1.0f;
                //
                //		Vector2 normalEndPoint = collision.IntersectionPoint + collisionSign * collision.Normal * 10.0f;
                //		Vector2 localNormalEndPoint = GetLocalPoint(normalEndPoint);
                //
                //		ioSystem.AddGraphicsCommand(new CommandDrawDebugLine(localIntersectionPoint, localNormalEndPoint));
                //	}
                //}			
            }
            else
            {
                Width = 30.0f;
                Height = 30.0f;

                renderContext.AddDrawMeshJob(SceneEntity.TransformMatrix, _quadMesh, new PrimitivePolygonMaterial(selectedColor, 100));
                renderContext.AddDrawMeshJob(SceneEntity.TransformMatrix, _quadLineMesh, new PrimitivePolygonMaterial(outlineColor, 101));
            }
        }

        private Vector2 GetLocalPoint(Vector2 worldPoint)
        {
            Vector2 localPoint = (worldPoint - SceneEntity.LocalPosition);

            float length = localPoint.Length;
            float angle = localPoint.Angle - SceneEntity.LocalRotation * Mathf.Deg2Rad;

            localPoint = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * length;

            return localPoint;
        }
    }
}
