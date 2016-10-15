/******************************************************************************
Copyright (c) 2016 Koray Kiyakoglu

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
using Windows.Gaming.Input;
using Swarm2D.Engine.View;
using Swarm2D.Library;
using Swarm2D.UniversalWindowsPlatform.DirectX;

namespace Swarm2D.UniversalWindowsPlatform.Framework
{
    public class ViewFramework : Swarm2D.Engine.View.Framework
    {
        public ViewFramework()
        {
            _loadedTextures = new Dictionary<string, DirectXTexture>();

            Windows.Gaming.Input.Gamepad.GamepadAdded += OnGamepadAdded;
            Windows.Gaming.Input.Gamepad.GamepadRemoved += OnGamepadRemoved;

            _debugVertexData = new float[4096];
        }

        private void OnGamepadRemoved(object sender, Windows.Gaming.Input.Gamepad e)
        {
        }

        private void OnGamepadAdded(object sender, Windows.Gaming.Input.Gamepad e)
        {
        }

        #region Input

        private GamepadReading _previousReading;
        private GamepadReading _currentReading;

        public override void UpdateInput()
        {
            _previousReading = _currentReading;
            _currentReading = new GamepadReading();

            foreach (var gamepad in Windows.Gaming.Input.Gamepad.Gamepads)
            {
                _currentReading = gamepad.GetCurrentReading();

                break;
            }
        }

        public override bool GetKeyDown(KeyCode keyCode)
        {
            return false;
        }

        public override bool GetKey(KeyCode keyCode)
        {
            return false;
        }

        public override bool GetKeyUp(KeyCode keyCode)
        {
            return false;
        }

        public override bool LeftMouse()
        {
            return false;
        }

        public override bool LeftMouseDown()
        {
            return false;
        }

        public override bool LeftMouseUp()
        {
            return false;
        }

        public override bool RightMouse()
        {
            return false;
        }

        public override bool RightMouseDown()
        {
            return false;
        }

        public override bool RightMouseUp()
        {
            return false;
        }

        public override Vector2 MousePosition()
        {
            return new Vector2(0, 0);
        }

        public override GamepadData GamepadData
        {
            get
            {
                GamepadData gamepadData = new GamepadData();

                gamepadData.LeftThumbstickX = _currentReading.LeftThumbstickX;
                gamepadData.LeftThumbstickY = _currentReading.LeftThumbstickY;
                gamepadData.LeftTrigger = _currentReading.LeftTrigger;
                gamepadData.RightThumbstickX = _currentReading.RightThumbstickX;
                gamepadData.RightThumbstickY = _currentReading.RightThumbstickY;
                gamepadData.RightTrigger = _currentReading.RightTrigger;

                return gamepadData;
            }
        }

        public override void FillInputData(InputData inputData)
        {
            inputData.GamepadData.LeftThumbstickX = _currentReading.LeftThumbstickX;
            inputData.GamepadData.LeftThumbstickY = _currentReading.LeftThumbstickY;
            inputData.GamepadData.LeftTrigger = _currentReading.LeftTrigger;
            inputData.GamepadData.RightThumbstickX = _currentReading.RightThumbstickX;
            inputData.GamepadData.RightThumbstickY = _currentReading.RightThumbstickY;
            inputData.GamepadData.RightTrigger = _currentReading.RightTrigger;
        }

        #endregion

        #region Debug Render

        private float[] _debugVertexData;

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
            for (int i = 0; i < vertices.Count; i++)
            {
                _debugVertexData[2 * i] = vertices[i].X;
                _debugVertexData[2 * i + 1] = vertices[i].Y;
            }
            
            DirectXApplication.DrawPolygon(_debugVertexData.ToArray(), vertices.Count, color.Red, color.Blue, color.Green, color.Alpha);
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

        private Dictionary<string, DirectXTexture> _loadedTextures;

        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _worldMatrix;
        private Matrix4x4 _projectionMatrix;

        public override bool SupportSeperatedRenderThread { get { return true; } }

        public override int Width { get { return DirectXApplication.Width(); } }

        public override int Height { get { return DirectXApplication.Height(); } }

        public override void BeginFrame()
        {
            DirectXApplication.BeginFrame();
            ProjectionMatrix = Matrix4x4.OrthographicProjection(0, Width, Height, 0);
        }

        public override void InitializeGraphicsContext()
        {
            
        }

        public override void CreateGraphics()
        {
            
        }

        public override void UpdateGraphics()
        {
            
        }

        public override void SwapBuffers()
        {
            DirectXApplication.SwapBuffers();
        }

        public override Matrix4x4 ViewMatrix
        {
            get { return _viewMatrix; }
            set
            {
                _viewMatrix = value;
                DirectXApplication.SetViewMatrix(
                    _viewMatrix.M00, _viewMatrix.M01, _viewMatrix.M02, _viewMatrix.M03,
                    _viewMatrix.M10, _viewMatrix.M11, _viewMatrix.M12, _viewMatrix.M13,
                    _viewMatrix.M20, _viewMatrix.M21, _viewMatrix.M22, _viewMatrix.M23,
                    _viewMatrix.M30, _viewMatrix.M31, _viewMatrix.M32, _viewMatrix.M33);
            }
        }

        public override Matrix4x4 WorldMatrix
        {
            get { return _worldMatrix; }
            set
            {
                _worldMatrix = value;
                DirectXApplication.SetWorldMatrix(
                    _worldMatrix.M00, _worldMatrix.M01, _worldMatrix.M02, _worldMatrix.M03,
                    _worldMatrix.M10, _worldMatrix.M11, _worldMatrix.M12, _worldMatrix.M13,
                    _worldMatrix.M20, _worldMatrix.M21, _worldMatrix.M22, _worldMatrix.M23,
                    _worldMatrix.M30, _worldMatrix.M31, _worldMatrix.M32, _worldMatrix.M33);
            }
        }

        public override Matrix4x4 ProjectionMatrix
        {
            get { return _projectionMatrix; }
            set
            {
                _projectionMatrix = value;
                DirectXApplication.SetProjectionMatrix(
                    _projectionMatrix.M00, _projectionMatrix.M01, _projectionMatrix.M02, _projectionMatrix.M03,
                    _projectionMatrix.M10, _projectionMatrix.M11, _projectionMatrix.M12, _projectionMatrix.M13,
                    _projectionMatrix.M20, _projectionMatrix.M21, _projectionMatrix.M22, _projectionMatrix.M23,
                    _projectionMatrix.M30, _projectionMatrix.M31, _projectionMatrix.M32, _projectionMatrix.M33);
            }
        }

        public override void PushScissor(int x, int y, int width, int heigt)
        {
            
        }

        public override void PopScissor()
        {
            
        }

        public override void DrawArrays(Texture texture, float[] vertices, float[] uvs, int vertexCount)
        {
            DirectXTexture directXTexture = (DirectXTexture)texture;
            DirectXApplication.DrawQuadArrays(vertices, uvs, vertexCount, directXTexture.InnerTexture);
        }

        public override void DrawArrays(float x, float y, Texture texture, float[] vertices, float[] uvs, int vertexCount)
        {
            DirectXTexture directXTexture = (DirectXTexture)texture;
            DirectXApplication.DrawQuadArrays(x, y, vertices, uvs, vertexCount, directXTexture.InnerTexture);
        }

        public override void LoadTextureUsing(Texture texture, string resourcesName, string name)
        {
            DirectXTexture directXTexture = (DirectXTexture) texture;

            //string textureFullPath = Resources.MainPath + @"\" + resourcesName + @"\" + name;
            string textureName = resourcesName + @"/" + name;
            Debug.Assert(!_loadedTextures.ContainsKey(textureName));

            directXTexture.LoadFromFile(textureName);
            _loadedTextures.Add(textureName, (DirectXTexture)texture);
        }

        public override Texture LoadTexture(string name)
        {
            return LoadTexture(LogicFramework.Current.ResourcesName, name);
        }

        public override Texture LoadTexture(string resourcesName, string name)
        {
            return null;
        }

        public override Texture GetTexture(string name)
        {
            return GetTexture(LogicFramework.Current.ResourcesName, name);
        }

        public override Texture GetTexture(string resourcesName, string name)
        {
            string textureName = resourcesName + @"/" + name;

            Texture texture = null;

            if (_loadedTextures.ContainsKey(textureName))
            {
                texture = _loadedTextures[textureName];
            }

            return texture;
        }

        public override Texture CreateTexture()
        {
            return new DirectXTexture();
        }

        #endregion

        #region Audio

        public override void InitializeAudioContext()
        {
            
        }

        public override AudioClip LoadAudioClip(string name)
        {
            return null;
        }

        public override IAudioJob PlayOneShotAudio(AudioClip audioClip)
        {
            return null;
        }

        public override IAudioJob PlayOneShotAudio(AudioClip audioClip, Vector2 position)
        {
            return null;
        }

        public override void StopAllAudio()
        {
            
        }

        public override IAudioJob PlayAudio(AudioClip audioClip)
        {
            return null;
        }

        #endregion
    }
}
