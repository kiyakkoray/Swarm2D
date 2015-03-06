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
using UnityEngine;
using Matrix4x4 = Swarm2D.Library.Matrix4x4;

namespace Swarm2D.UnityFramework
{
    public class UnityRenderer
    {
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
        private Matrix4x4 _worldMatrix = Matrix4x4.Identity;
        private Matrix4x4 _viewMatrix = Matrix4x4.Identity;

        private Matrix4x4 _modelviewMatrix = Matrix4x4.Identity;

        public Matrix4x4 ProjectionMatrix
        {
            get
            {
                return _projectionMatrix;
            }
            set
            {
                _projectionMatrix = value;

                if (_currentlyRendering)
                {
                    LoadProjectionMatrix();
                }
            }
        }

        public Matrix4x4 ViewMatrix
        {
            get
            {
                return _viewMatrix;
            }
            set
            {
                _viewMatrix = value;

                _modelviewMatrix = _viewMatrix * _worldMatrix;

                if (_currentlyRendering)
                {
                    LoadModelViewMatrix();
                }
            }
        }

        public Matrix4x4 WorldMatrix
        {
            get
            {
                return _worldMatrix;
            }
            set
            {
                _worldMatrix = value;

                _modelviewMatrix = _viewMatrix * _worldMatrix;

                if (_currentlyRendering)
                {
                    LoadModelViewMatrix();
                }
            }
        }

        private bool _currentlyRendering = false;

        private Material _basicMaterial;

        void CreateBasicMaterial()
        {
            if (_basicMaterial == null)
            {
                string shader =
                "Shader \"Scythex\" {" +
                "    Properties {" +
                "        _Color (\"Main Color\", Color) = (1,1,1,1)" +
                "        _MainTex (\"Base (RGB)\", 2D) = \"white\" {}" +
                "    }" +
                "    Category {" +
                "       Lighting Off" +
                "       ZWrite Off" +
                "	   ZTest Always" +
                "       Cull Off" +
                "	   Blend SrcAlpha OneMinusSrcAlpha" +
                "	   Fog { Mode Off }" +
                "	   " +
                "       SubShader {" +
                "            Pass {" +
                "               SetTexture [_MainTex] {" +
                "                    constantColor [_Color]" +
                "                    Combine texture * constant, texture * constant " +
                "                 }" +
                "            }" +
                "        } " +
                "    }" +
                "}";

                _basicMaterial = new Material(shader);

                _basicMaterial.hideFlags = HideFlags.HideAndDontSave;
                _basicMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private Mesh _basicMesh;

        void CreateBasicMesh()
        {
            Vector3[] vertices = new Vector3[] { new Vector3(10, 10, 0f), new Vector3(10, -10, 0f), new Vector3(-10, 10, 0f), new Vector3(-10, -10, 0f) };
            Vector2[] uvs = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
            Color[] colors = new Color[] { Color.red, Color.red, Color.red, Color.red };
            int[] triangles = new int[] { 0, 1, 2, 3, 2, 1 };

            _basicMesh = new Mesh();
            _basicMesh.vertices = vertices;
            _basicMesh.uv = uvs;
            _basicMesh.colors = colors;
            _basicMesh.triangles = triangles;

            _basicMesh.RecalculateNormals();
            _basicMesh.RecalculateBounds();
        }

        public void Create()
        {
            CreateBasicMaterial();
            CreateBasicMesh();
        }

        public void BeginFrame()
        {
            ProjectionMatrix = Matrix4x4.OrthographicProjection(0, Screen.width, Screen.height, 0);

            _currentlyRendering = true;
            GL.PushMatrix();

            LoadProjectionMatrix();
            LoadModelViewMatrix();
        }

        private static UnityEngine.Matrix4x4 GetUnityMatrix(Matrix4x4 matrix)
        {
            UnityEngine.Matrix4x4 result = new UnityEngine.Matrix4x4();

            result.m00 = matrix.M00;
            result.m01 = matrix.M01;
            result.m02 = matrix.M02;
            result.m03 = matrix.M03;

            result.m10 = matrix.M10;
            result.m11 = matrix.M11;
            result.m12 = matrix.M12;
            result.m13 = matrix.M13;

            result.m20 = matrix.M20;
            result.m21 = matrix.M21;
            result.m22 = matrix.M22;
            result.m23 = matrix.M23;

            result.m30 = matrix.M30;
            result.m31 = matrix.M31;
            result.m32 = matrix.M32;
            result.m33 = matrix.M33;

            return result;
        }

        void LoadProjectionMatrix()
        {
            UnityEngine.Matrix4x4 projectionMatrix = GetUnityMatrix(_projectionMatrix);

            //Camera.current.orthographic = true;
            //Camera.current.projectionMatrix = projectionMatrix;

            GL.LoadProjectionMatrix(projectionMatrix);
        }

        void LoadModelViewMatrix()
        {
            //GL.LoadIdentity();
            //GL.MultMatrix(GetUnityMatrix(_modelviewMatrix));

            GL.modelview = GetUnityMatrix(_modelviewMatrix);
        }

        public void EndFrame()
        {
            GL.PopMatrix();
            _currentlyRendering = false;
        }

        public void DrawArrays(float x, float y, UnityTexture texture, float[] vertices, float[] uvs, int vertexCount)
        {
            _basicMaterial.SetTexture("_MainTex", texture.Texture);

            if (_basicMaterial.SetPass(0))
            {
                GL.Color(new UnityEngine.Color(1.0f, 1.0f, 1.0f, 1.0f));
                GL.Begin(GL.QUADS);

                for (int i = 0; i < vertexCount; i++)
                {
                    GL.TexCoord2(uvs[2 * i], 1.0f - uvs[2 * i + 1]);
                    GL.Vertex3(x + vertices[2 * i], y + vertices[2 * i + 1], 0);
                }

                GL.End();
            }

            return;
            _basicMaterial.SetTexture("_MainTex", texture.Texture);

            if (_basicMaterial.SetPass(0))
            {
                Graphics.DrawMeshNow(_basicMesh, GetUnityMatrix(_modelviewMatrix * Matrix4x4.Position2D(new Library.Vector2(x, y))));
                //Graphics.DrawMeshNow(_basicMesh, new Vector3(x,y,0), Quaternion.identity);
            }
        }
    }
}
