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
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.Engine.View
{
    public abstract class Framework
    {
        internal static Framework Current { get; set; }

        protected Framework()
        {
            Current = this;
        }

        #region Input

        public abstract void UpdateInput();

        public abstract bool GetKeyDown(KeyCode keyCode);

        public abstract bool GetKey(KeyCode keyCode);

        public abstract bool GetKeyUp(KeyCode keyCode);

        public abstract bool LeftMouse();

        public abstract bool LeftMouseDown();

        public abstract bool LeftMouseUp();

        public abstract bool RightMouse();

        public abstract bool RightMouseDown();

        public abstract bool RightMouseUp();

        public abstract Vector2 MousePosition();

        public abstract GamepadData GamepadData { get; }

        public abstract void FillInputData(InputData inputData);

        #endregion

        #region Debug Render

        public abstract void ResetDebugRender();

        public abstract void DrawBufferedDebugObjects();

        public abstract void DrawDebugLine(Vector2 a, Vector2 b);

        public abstract void DrawDebugPoint(Vector2 point);

        public abstract void DrawDebugPolygon(List<Vector2> vertices, Color color);

        public abstract void DrawDebugCircle(float radius, Color color);

        public abstract void DrawDebugQuad();

        public abstract void DrawDebugQuad(float x, float y, float width, float height);

        public abstract void AddDebugPoint(Vector2 point);

        public abstract void AddDebugLine(Vector2 pointA, Vector2 pointB);

        #endregion

        #region Graphics Context

        public abstract bool SupportSeperatedRenderThread { get; }

        public abstract int Width { get; }

        public abstract int Height { get; }

        public abstract void BeginFrame();

        public abstract void InitializeGraphicsContext();

        public abstract void CreateGraphics();

        public abstract void UpdateGraphics();

        public abstract void SwapBuffers();

        public abstract Matrix4x4 ViewMatrix { get; set; }

        public abstract Matrix4x4 WorldMatrix { get; set; }

        public abstract Matrix4x4 ProjectionMatrix { get; set; }

        public abstract void PushScissor(int x, int y, int width, int heigt);

        public abstract void PopScissor();

        public abstract void DrawArrays(Texture texture, float[] vertices, float[] uvs, int vertexCount);

        public abstract void DrawArrays(float x, float y, Texture texture, float[] vertices, float[] uvs, int vertexCount);

        public abstract void LoadTextureUsing(Texture texture, string resourcesName, string name);

        public abstract Texture LoadTexture(string name);

        public abstract Texture LoadTexture(string resourcesName, string name);

        public abstract Texture GetTexture(string name);

        public abstract Texture GetTexture(string resourcesName, string name);

        public abstract Texture CreateTexture();

        #endregion

        #region Audio

        public abstract void InitializeAudioContext();

        public abstract AudioClip LoadAudioClip(string name);

        public abstract IAudioJob PlayOneShotAudio(AudioClip audioClip);

        public abstract IAudioJob PlayOneShotAudio(AudioClip audioClip, Vector2 position);

        public abstract void StopAllAudio();

        public abstract IAudioJob PlayAudio(AudioClip audioClip);

        #endregion
    }

    interface ITextFramework
    {
        short GetHashCodeOf(string text);
    }
}
