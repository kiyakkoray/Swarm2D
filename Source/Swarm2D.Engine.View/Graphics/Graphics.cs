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

namespace Swarm2D.Engine.View
{
    public static class Graphics
    {
        public static int Width { get { return Framework.Current.Width; } }

        public static int Height { get { return Framework.Current.Height; } }

        public static void BeginFrame()
        {
            Framework.Current.BeginFrame();
        }

        public static void InitializeGraphicsContext()
        {
            Framework.Current.InitializeGraphicsContext();
        }

        public static void CreateGraphics()
        {
            Framework.Current.CreateGraphics();
        }

        public static void UpdateGraphics()
        {
            Framework.Current.UpdateGraphics();
        }

        public static void SwapBuffers()
        {
            Framework.Current.SwapBuffers();
        }

        public static void PushScissor(int x, int y, int width, int height)
        {
            Framework.Current.PushScissor(x, y, width, height);
        }

        public static void PopScissor()
        {
            Framework.Current.PopScissor();
        }

        public static void DrawArrays(float x, float y, Texture texture, float[] vertices, float[] uvs)
        {
            DrawArrays(x, y, texture, vertices, uvs, vertices.Length / 2);
        }

        public static void DrawArrays(float x, float y, Texture texture, float[] vertices, float[] uvs, int vertexCount)
        {
            Framework.Current.DrawArrays(x, y, texture, vertices, uvs, vertexCount);
        }

        public static void DrawTextureOnScreen(float x, float y, float width, float height, Texture texture)
        {
            Framework.Current.DrawTextureOnScreen(x, y, width, height, texture);
        }

        public static void LoadTextureUsing(Texture texture, string resourcesName, string name)
        {
            Framework.Current.LoadTextureUsing(texture, resourcesName, name);
        }

        public static Texture LoadTexture(string name)
        {
            return Framework.Current.LoadTexture(name);
        }

        public static Texture LoadTexture(string resourcesName, string name)
        {
            return Framework.Current.LoadTexture(resourcesName, name);
        }

        public static Texture GetTexture(string resourcesName, string name)
        {
            return Framework.Current.GetTexture(resourcesName, name);
        }

        public static Texture CreateTexture()
        {
            return Framework.Current.CreateTexture();
        }

        public static Matrix4x4 ViewMatrix
        {
            get { return Framework.Current.ViewMatrix; }
            set { Framework.Current.ViewMatrix = value; }
        }

        public static Matrix4x4 WorldMatrix
        {
            get { return Framework.Current.WorldMatrix; }
            set { Framework.Current.WorldMatrix = value; }
        }

        public static Matrix4x4 ProjectionMatrix
        {
            get { return Framework.Current.ProjectionMatrix; }
            set { Framework.Current.ProjectionMatrix = value; }
        }
    }
}
