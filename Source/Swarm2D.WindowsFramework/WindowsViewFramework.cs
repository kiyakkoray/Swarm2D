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
using Swarm2D.Engine.View;
using Swarm2D.Library;

namespace Swarm2D.WindowsFramework
{
    public class WindowsViewFramework : Swarm2D.Engine.View.Framework
    {
        private GraphicsWindow _graphicsForm;
        private GraphicsContext _graphicsContext;

        private AudioContext _audioContext;

        #region Input

        public override void UpdateInput()
        {
            _graphicsForm.UpdateInput();
        }

        public override bool GetKeyDown(KeyCode keyCode)
        {
            return _graphicsForm.GetKeyDown(keyCode);
        }

        public override bool GetKey(KeyCode keyCode)
        {
            return _graphicsForm.GetKey(keyCode);
        }

        public override bool GetKeyUp(KeyCode keyCode)
        {
            return _graphicsForm.GetKeyUp(keyCode);
        }

        public override bool LeftMouse()
        {
            return _graphicsForm.LeftMouse();
        }

        public override bool LeftMouseDown()
        {
            return _graphicsForm.LeftMouseDown();
        }

        public override bool LeftMouseUp()
        {
            return _graphicsForm.LeftMouseUp();
        }

        public override bool RightMouse()
        {
            return _graphicsForm.RightMouse();
        }

        public override bool RightMouseDown()
        {
            return _graphicsForm.RightMouseDown();
        }

        public override bool RightMouseUp()
        {
            return _graphicsForm.RightMouseUp();
        }

        public override Vector2 MousePosition()
        {
            return _graphicsForm.MousePosition();
        }

        public override GamepadData GamepadData
        {
            get { return new GamepadData(); }
        }

        public override void FillInputData(InputData inputData)
        {
            _graphicsForm.FillInputDataFromCurrent(inputData);
        }

        #endregion

        #region Debug Render

        public override void ResetDebugRender()
        {
            DebugRender.Reset();
        }

        public override void DrawBufferedDebugObjects()
        {
            DebugRender.DrawBufferedDebugObjects();
        }

        public override void DrawDebugLine(Vector2 a, Vector2 b)
        {
            DebugRender.DrawDebugLine(a, b);
        }

        public override void DrawDebugPoint(Vector2 point)
        {
            DebugRender.DrawDebugPoint(point);
        }

        public override void DrawDebugPolygon(List<Vector2> vertices, Color color)
        {
            DebugRender.DrawDebugPolygon(vertices, color);
        }

        public override void DrawDebugCircle(float radius, Color color)
        {
            DebugRender.DrawDebugCircle(radius, color);
        }

        public override void DrawDebugQuad()
        {
            DebugRender.DrawDebugQuad();
        }

        public override void DrawDebugQuad(float x, float y, float width, float height)
        {
            DebugRender.DrawDebugQuad(x, y, width, height);
        }

        public override void AddDebugPoint(Vector2 point)
        {
            DebugRender.AddDebugPoint(point);
        }

        public override void AddDebugLine(Vector2 pointA, Vector2 pointB)
        {
            DebugRender.AddDebugLine(pointA, pointB);
        }

        #endregion

        #region Graphics Context

        public override bool SupportSeperatedRenderThread { get { return true; } }

        public override int Width { get { return _graphicsForm.Width; } }

        public override int Height { get { return _graphicsForm.Height; } }

        public override void BeginFrame()
        {
            _graphicsForm.BeginFrame();
        }

        public override void InitializeGraphicsContext()
        {
            _graphicsForm.InitializeGraphicsContext();
        }

        public override void CreateGraphics()
        {
            _graphicsForm = new GraphicsWindow(100, 100, 1280, 720);
            _graphicsContext = _graphicsForm.GraphicsContext;

            DebugRender.Initialize(_graphicsContext);
        }

        public override void UpdateGraphics()
        {
            _graphicsForm.Update();
        }

        public override void SwapBuffers()
        {
            _graphicsContext.SwapBuffers();
        }

        public override Matrix4x4 ViewMatrix
        {
            get { return _graphicsContext.ViewMatrix; }
            set { _graphicsContext.ViewMatrix = value; }
        }

        public override Matrix4x4 WorldMatrix
        {
            get { return _graphicsContext.WorldMatrix; }
            set { _graphicsContext.WorldMatrix = value; }
        }

        public override Matrix4x4 ProjectionMatrix
        {
            get { return _graphicsContext.ProjectionMatrix; }
            set { _graphicsContext.ProjectionMatrix = value; }
        }

        public override void PushScissor(int x, int y, int width, int heigt)
        {
            _graphicsContext.PushScissor(x, y, width, heigt);
        }

        public override void PopScissor()
        {
            _graphicsContext.PopScissor();
        }

        public override void DrawArrays(Texture texture, float[] vertices, float[] uvs, int vertexCount)
        {
            _graphicsContext.DrawArrays((OpenGLTexture)texture, vertices, uvs, vertexCount);
        }

        public override void DrawArrays(float x, float y, Texture texture, float[] vertices, float[] uvs, int vertexCount)
        {
            _graphicsContext.DrawArrays(x, y, (OpenGLTexture)texture, vertices, uvs, vertexCount);
        }

        public override void LoadTextureUsing(Texture texture, string resourcesName, string name)
        {
            _graphicsContext.LoadTextureUsing((OpenGLTexture)texture, resourcesName, name);
        }

        public override Texture LoadTexture(string name)
        {
            return _graphicsContext.LoadTexture(Resources.ResourcesName, name);
        }

        public override Texture LoadTexture(string resourcesName, string name)
        {
            return _graphicsContext.LoadTexture(resourcesName, name);
        }

        public override Texture GetTexture(string name)
        {
            return GetTexture(Resources.ResourcesName, name);
        }

        public override Texture GetTexture(string resourcesName, string name)
        {
            string textureName = resourcesName + @"/" + name;

            return _graphicsContext.GetTexture(textureName);
        }

        public override Texture CreateTexture()
        {
            return new OpenGLTexture();
        }

        #endregion

        #region Audio

        public override void InitializeAudioContext()
        {
            _audioContext = new AudioContext();
            _audioContext.Start();
        }

        public override AudioClip LoadAudioClip(string name)
        {
            return new OggAudioClip(name);
        }

        public override IAudioJob PlayOneShotAudio(AudioClip audioClip)
        {
            return _audioContext.PlayOneShotAudio((OggAudioClip)audioClip);
        }

        public override IAudioJob PlayOneShotAudio(AudioClip audioClip, Vector2 position)
        {
            return _audioContext.PlayOneShotAudio((OggAudioClip)audioClip, position);
        }

        public override void StopAllAudio()
        {
            _audioContext.StopAllAudio();
        }

        public override IAudioJob PlayAudio(AudioClip audioClip)
        {
            return _audioContext.PlayAudio((OggAudioClip)audioClip);
        }

        #endregion
    }
}
