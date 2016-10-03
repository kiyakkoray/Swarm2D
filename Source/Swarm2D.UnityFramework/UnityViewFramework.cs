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
using System.Threading;
using System.Xml;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View;
using UnityEngine;
using Color = Swarm2D.Library.Color;
using Debug = Swarm2D.Library.Debug;
using KeyCode = Swarm2D.Library.KeyCode;
using Matrix4x4 = Swarm2D.Library.Matrix4x4;
using Texture = Swarm2D.Engine.View.Texture;
using Vector2 = Swarm2D.Library.Vector2;

namespace Swarm2D.UnityFramework
{
    public class UnityViewFramework : Swarm2D.Engine.View.Framework
    {
        #region Input

        public override void UpdateInput()
        {

        }

        public override bool GetKeyDown(KeyCode keyCode)
        {
            return UnityEngine.Input.GetKeyDown(ConvertKeyCode(keyCode));
        }

        public override bool GetKey(KeyCode keyCode)
        {
            return UnityEngine.Input.GetKey(ConvertKeyCode(keyCode));
        }

        public override bool GetKeyUp(KeyCode keyCode)
        {
            return UnityEngine.Input.GetKeyUp(ConvertKeyCode(keyCode));
        }

        public override bool LeftMouse()
        {
            return UnityEngine.Input.GetMouseButton(0);
        }

        public override bool LeftMouseDown()
        {
            return UnityEngine.Input.GetMouseButtonDown(0);
        }

        public override bool LeftMouseUp()
        {
            return UnityEngine.Input.GetMouseButtonUp(0);
        }

        public override bool RightMouse()
        {
            return UnityEngine.Input.GetMouseButton(1);
        }

        public override bool RightMouseDown()
        {
            return UnityEngine.Input.GetMouseButtonDown(1);
        }

        public override bool RightMouseUp()
        {
            return UnityEngine.Input.GetMouseButtonUp(1);
        }

        public override Vector2 MousePosition()
        {
            UnityEngine.Vector2 mousePosition = UnityEngine.Input.mousePosition;
            return new Vector2(mousePosition.x, Height - mousePosition.y);
        }

        public override void FillInputData(InputData inputData)
        {
            Vector2 mousePosition = MousePosition();

            inputData.CursorX = (int)mousePosition.X;
            inputData.CursorY = (int)mousePosition.Y;

            inputData.LeftMouse = LeftMouse();
            inputData.RightMouse = RightMouse();

            for (int i = 0; i < inputData.KeyData.Length; i++)
            {
                inputData.KeyData[i] = GetKey((Swarm2D.Library.KeyCode)i);
            }
        }

        public UnityEngine.KeyCode ConvertKeyCode(Swarm2D.Library.KeyCode keyCode)
        {
            UnityEngine.KeyCode result = UnityEngine.KeyCode.A;

            switch (keyCode)
            {
                case KeyCode.KeyShift:
                    result = UnityEngine.KeyCode.LeftShift;
                    break;
                case KeyCode.KeySpace:
                    result = UnityEngine.KeyCode.Space;
                    break;
                case KeyCode.KeyBackspace:
                    result = UnityEngine.KeyCode.Backspace;
                    break;
                case KeyCode.KeyLeftArrow:
                    result = UnityEngine.KeyCode.LeftArrow;
                    break;
                case KeyCode.KeyUpArrow:
                    result = UnityEngine.KeyCode.UpArrow;
                    break;
                case KeyCode.KeyRightArrow:
                    result = UnityEngine.KeyCode.RightArrow;
                    break;
                case KeyCode.KeyDownArrow:
                    result = UnityEngine.KeyCode.DownArrow;
                    break;
                case KeyCode.KeyDot:
                    break;
                case KeyCode.Key0:
                    result = UnityEngine.KeyCode.Keypad0;
                    break;
                case KeyCode.Key1:
                    result = UnityEngine.KeyCode.Keypad1;
                    break;
                case KeyCode.Key2:
                    result = UnityEngine.KeyCode.Keypad2;
                    break;
                case KeyCode.Key3:
                    result = UnityEngine.KeyCode.Keypad3;
                    break;
                case KeyCode.Key4:
                    result = UnityEngine.KeyCode.Keypad4;
                    break;
                case KeyCode.Key5:
                    result = UnityEngine.KeyCode.Keypad5;
                    break;
                case KeyCode.Key6:
                    result = UnityEngine.KeyCode.Keypad6;
                    break;
                case KeyCode.Key7:
                    result = UnityEngine.KeyCode.Keypad7;
                    break;
                case KeyCode.Key8:
                    result = UnityEngine.KeyCode.Keypad8;
                    break;
                case KeyCode.Key9:
                    result = UnityEngine.KeyCode.Keypad9;
                    break;
                case KeyCode.KeyA:
                    result = UnityEngine.KeyCode.A;
                    break;
                case KeyCode.KeyB:
                    result = UnityEngine.KeyCode.B;
                    break;
                case KeyCode.KeyC:
                    result = UnityEngine.KeyCode.C;
                    break;
                case KeyCode.KeyD:
                    result = UnityEngine.KeyCode.D;
                    break;
                case KeyCode.KeyE:
                    result = UnityEngine.KeyCode.E;
                    break;
                case KeyCode.KeyF:
                    result = UnityEngine.KeyCode.F;
                    break;
                case KeyCode.KeyG:
                    result = UnityEngine.KeyCode.G;
                    break;
                case KeyCode.KeyH:
                    result = UnityEngine.KeyCode.H;
                    break;
                case KeyCode.KeyI:
                    result = UnityEngine.KeyCode.I;
                    break;
                case KeyCode.KeyJ:
                    result = UnityEngine.KeyCode.J;
                    break;
                case KeyCode.KeyK:
                    result = UnityEngine.KeyCode.K;
                    break;
                case KeyCode.KeyL:
                    result = UnityEngine.KeyCode.L;
                    break;
                case KeyCode.KeyM:
                    result = UnityEngine.KeyCode.M;
                    break;
                case KeyCode.KeyN:
                    result = UnityEngine.KeyCode.N;
                    break;
                case KeyCode.KeyO:
                    result = UnityEngine.KeyCode.O;
                    break;
                case KeyCode.KeyP:
                    result = UnityEngine.KeyCode.P;
                    break;
                case KeyCode.KeyQ:
                    result = UnityEngine.KeyCode.Q;
                    break;
                case KeyCode.KeyR:
                    result = UnityEngine.KeyCode.R;
                    break;
                case KeyCode.KeyS:
                    result = UnityEngine.KeyCode.S;
                    break;
                case KeyCode.KeyT:
                    result = UnityEngine.KeyCode.T;
                    break;
                case KeyCode.KeyU:
                    result = UnityEngine.KeyCode.U;
                    break;
                case KeyCode.KeyV:
                    result = UnityEngine.KeyCode.V;
                    break;
                case KeyCode.KeyW:
                    result = UnityEngine.KeyCode.W;
                    break;
                case KeyCode.KeyX:
                    result = UnityEngine.KeyCode.X;
                    break;
                case KeyCode.KeyY:
                    result = UnityEngine.KeyCode.Y;
                    break;
                case KeyCode.KeyZ:
                    result = UnityEngine.KeyCode.Z;
                    break;
            }

            return result;
        }

        #endregion

        #region Debug Render

        public override void ResetDebugRender()
        {
        }

        public override void DrawBufferedDebugObjects()
        {
        }

        public override void DrawDebugLine(Vector2 a, Vector2 b)
        {
        }

        public override void DrawDebugPoint(Vector2 point)
        {
        }

        public override void DrawDebugPolygon(List<Vector2> vertices, Color color)
        {
        }

        public override void DrawDebugCircle(float radius, Color color)
        {
        }

        public override void DrawDebugQuad()
        {
        }

        public override void DrawDebugQuad(float x, float y, float width, float height)
        {
        }

        public override void AddDebugPoint(Vector2 point)
        {
        }

        public override void AddDebugLine(Vector2 pointA, Vector2 pointB)
        {
        }

        #endregion

        #region Graphics Context

        public override bool SupportSeperatedRenderThread { get { return false; } }

        public override int Width { get { return UnityEngine.Screen.width; } }

        public override int Height { get { return UnityEngine.Screen.height; } }

        private UnityRenderer _renderer;

        public override void CreateGraphics()
        {
            Debug.Log("CreateGraphics");
            _renderer = new UnityRenderer();
            _renderer.Create();
        }

        public override void BeginFrame()
        {
            _renderer.BeginFrame();
        }

        public override void InitializeGraphicsContext()
        {
        }

        public override void UpdateGraphics()
        {
        }

        public override void SwapBuffers()
        {
            _renderer.EndFrame();
        }

        public override Matrix4x4 ViewMatrix
        {
            get { return _renderer.ViewMatrix; }
            set { _renderer.ViewMatrix = value; }
        }

        public override Matrix4x4 WorldMatrix
        {
            get { return _renderer.WorldMatrix; }
            set { _renderer.WorldMatrix = value; }
        }

        public override Matrix4x4 ProjectionMatrix
        {
            get { return _renderer.ProjectionMatrix; }
            set { _renderer.ProjectionMatrix = value; }
        }

        public override void PushScissor(int x, int y, int width, int heigt)
        {
        }

        public override void PopScissor()
        {
        }

        public override void DrawArrays(Texture texture, float[] vertices, float[] uvs, int vertexCount)
        {
            _renderer.DrawArrays(0, 0, (UnityTexture)texture, vertices, uvs, vertexCount);
        }

        public override void DrawArrays(float x, float y, Texture texture, float[] vertices, float[] uvs, int vertexCount)
        {
            _renderer.DrawArrays(x, y, (UnityTexture)texture, vertices, uvs, vertexCount);
        }

        public override void DrawTextureOnScreen(float x, float y, float width, float height, Texture texture)
        {
            Debug.Log("x: " + x + "y: " + y);
        }

        public override void LoadTextureUsing(Texture texture, string resourcesName, string name)
        {
            UnityTexture unityTexture = texture as UnityTexture;
            unityTexture.LoadTexture(resourcesName, name);
        }

        public override Texture LoadTexture(string name)
        {
            UnityTexture unityTexture = new UnityTexture();
            unityTexture.LoadTexture(Engine.Core.Framework.Current.ResourcesName, name);

            return unityTexture;
        }

        public override Texture LoadTexture(string resourcesName, string name)
        {
            UnityTexture unityTexture = new UnityTexture();
            unityTexture.LoadTexture(resourcesName, name);

            return unityTexture;
        }

        public override Texture GetTexture(string name)
        {
            return null;
        }

        public override Texture GetTexture(string resourcesName, string name)
        {
            return null;
        }

        public override Texture CreateTexture()
        {
            return new UnityTexture();
        }

        #endregion

    }
}
