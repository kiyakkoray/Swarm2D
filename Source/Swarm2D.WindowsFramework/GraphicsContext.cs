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
using Swarm2D.WindowsFramework.Native;
using Swarm2D.WindowsFramework.Native.Windows;
using Swarm2D.WindowsFramework.Native.Opengl;
using System.Threading;
using Swarm2D.WindowsFramework.Native.OggVorbis;

namespace Swarm2D.WindowsFramework
{
    public class GraphicsContext
    {
        internal WindowsForm Control { get; set; }

        private float[] _debugVertices = new float[16];
        private float[] _textureUVs = new float[8] { 0, 0, 0, 1, 1, 1, 1, 0 };

        private IntPtr _handleDeviceContext;
        private IntPtr _handleRenderContext;

        public static GraphicsContext Active { get; private set; }

        internal Dictionary<string, OpenGLTexture> LoadedTextures { get; private set; }

        private List<ScissorTestInfo> _scissorStack;
        private bool _scissorTestEnabled;
        private int[] _scissorParameters = new int[4];

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

                Opengl32.MatrixMode(MatrixMode.Projection);
                Opengl32.LoadMatrix(ref _projectionMatrix);
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

                Opengl32.MatrixMode(MatrixMode.ModelView);
                Opengl32.LoadMatrix(ref _modelviewMatrix);
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

                Opengl32.MatrixMode(MatrixMode.ModelView);
                Opengl32.LoadMatrix(ref _modelviewMatrix);
            }
        }

        public GraphicsContext()
        {
            LoadedTextures = new Dictionary<string, OpenGLTexture>();
            _scissorStack = new List<ScissorTestInfo>();
        }

        public void CreateContext()
        {
            _handleDeviceContext = User32.GetDC(Control.Handle);

            if (_handleDeviceContext == IntPtr.Zero)
            {
                Debug.Log("Can't get device context");
            }

            if (Opengl32.wglMakeCurrent(_handleDeviceContext, IntPtr.Zero))
            {
            }
            else
            {
                Debug.Log("Can't reset context");
            }

            PixelFormatDescriptor pfd = new PixelFormatDescriptor();
            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(PixelFormatDescriptor));
            pfd.nSize = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(typeof(PixelFormatDescriptor));
            pfd.nVersion = 1;
            pfd.dwFlags = (uint)(PixelFormatDescriptorFlags.DrawToWindow | PixelFormatDescriptorFlags.SupportOpengl | PixelFormatDescriptorFlags.DoubleBuffer);
            pfd.iPixelType = (byte)PixelFormatDescriptorPixelTypes.RGBA;
            pfd.cColorBits = 32;
            pfd.cRedBits = 0;
            pfd.cRedShift = 0;
            pfd.cGreenBits = 0;
            pfd.cGreenShift = 0;
            pfd.cBlueBits = 0;
            pfd.cBlueShift = 0;
            pfd.cAlphaBits = 0;
            pfd.cAlphaShift = 0;
            pfd.cAccumBits = 0;
            pfd.cAccumRedBits = 0;
            pfd.cAccumGreenBits = 0;
            pfd.cAccumBlueBits = 0;
            pfd.cAccumAlphaBits = 0;
            pfd.cDepthBits = 24;
            pfd.cStencilBits = 0;
            pfd.cAuxBuffers = 0;
            pfd.iLayerType = (byte)PixelFormatDescriptorLayerTypes.MainPlane;
            pfd.bReserved = 0;
            pfd.dwLayerMask = 0;
            pfd.dwVisibleMask = 0;
            pfd.dwDamageMask = 0;

            int pixelFormat = Gdi32.ChoosePixelFormat(_handleDeviceContext, ref pfd);


            if (!Gdi32.SetPixelFormat(_handleDeviceContext, pixelFormat, ref pfd))
            {
                Debug.Log("can't set pixel format");
            }

            _handleRenderContext = Opengl32.wglCreateContext(_handleDeviceContext);
            SetActive();

            //string extensionString = GL.GetString((uint)StringName.Extensions);
            //extensionList = extensionString.Split(new char[] { ' ' });

            //tell OpenGL to ignore padding at ends of rows
            //glPixelStorei(GL_UNPACK_ALIGNMENT, 4);
            //Opengl32.PixelStore(0x0CF5, 4);

            Opengl32.ShadeModel(ShadingModel.Smooth);
            Opengl32.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            Opengl32.ClearDepth(1.0);
            Opengl32.Disable(Target.DepthTest);

            //GL.Enable((uint)0x0BA1);//GL_NORMALIZE

            //Opengl32.DepthFunc(AlphaFunction.Allways);

            Opengl32.Hint((uint)Target.PERSPECTIVE_CORRECTION_HINT, (uint)HintMode.Nicest);

            ProjectionMatrix = Matrix4x4.Identity;
            //ModelViewMatrix = Matrix4x4.Identity;
            WorldMatrix = Matrix4x4.Identity;
            ViewMatrix = Matrix4x4.Identity;
        }

        public void SetActive()
        {
            if (Active != this)
            {
                if (Opengl32.wglMakeCurrent(_handleDeviceContext, _handleRenderContext))
                {
                    Active = this;
                }
                else
                {
                    Debug.Log("Can't activate context");
                }
            }
        }

        public void SwapBuffers()
        {
            Gdi32.SwapBuffers(_handleDeviceContext);
            Opengl32.Clear(AttribueMask.ColorBufferBit);
        }

        public bool IsActive
        {
            get
            {
                return Active == this;
            }

        }

        public void DestroyContext()
        {
            Opengl32.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            Opengl32.wglDeleteContext(_handleRenderContext);
            User32.ReleaseDC(Control.Handle, _handleDeviceContext);
        }

        public void PushScissor(int x, int y, int width, int heigt)
        {
            ScissorTestInfo newScissor = new ScissorTestInfo(x, y, width, heigt);

            if (_scissorStack.Count > 0)
            {
                ScissorTestInfo currentScissor = _scissorStack[_scissorStack.Count() - 1];

                int currentX2 = currentScissor.X + currentScissor.Width;
                int currentY2 = currentScissor.Y + currentScissor.Height;

                int newX2 = x + width;
                int newY2 = y + heigt;

                newScissor.X = newScissor.X > currentScissor.X ? newScissor.X : currentScissor.X;
                newScissor.Y = newScissor.Y > currentScissor.Y ? newScissor.Y : currentScissor.Y;

                int correctX2 = currentX2 > newX2 ? newX2 : currentX2;
                int correctY2 = currentY2 > newY2 ? newY2 : currentY2;

                newScissor.Width = correctX2 - newScissor.X;
                newScissor.Height = correctY2 - newScissor.Y;
            }

            _scissorStack.Add(newScissor);

            _scissorTestEnabled = true;

            Opengl32.GetInteger(Target.VIEWPORT, _scissorParameters);
            Opengl32.Scissor(newScissor.X, _scissorParameters[3] - newScissor.Height - newScissor.Y, newScissor.Width, newScissor.Height);
            Opengl32.Enable(Target.SCISSOR_TEST);
        }

        public void PopScissor()
        {
            _scissorStack.RemoveAt(_scissorStack.Count - 1);

            if (_scissorTestEnabled)
            {
                if (_scissorStack.Count > 0)
                {
                    ScissorTestInfo scissorTestInfo = _scissorStack[_scissorStack.Count - 1];

                    Opengl32.GetInteger(Target.VIEWPORT, _scissorParameters);
                    Opengl32.Scissor(scissorTestInfo.X, _scissorParameters[3] - scissorTestInfo.Height - scissorTestInfo.Y, scissorTestInfo.Width, scissorTestInfo.Height);
                }
                else
                {
                    Opengl32.Disable(Target.SCISSOR_TEST);
                    _scissorTestEnabled = false;
                }
            }
        }

        public void DrawArrays(float x, float y, OpenGLTexture texture, float[] vertices, float[] uvs, int vertexCount)
        {
            Opengl32.PushMatrix();
            Opengl32.Translate(x, y, 0.0f);

            DrawArrays(texture, vertices, uvs, vertexCount);

            Opengl32.PopMatrix();
        }

        public void DrawArrays(OpenGLTexture texture, float[] vertices, float[] uvs, int vertexCount)
        {
            SetBlending(true);

            if (texture != null)
            {
                Opengl32.Enable(Target.TEXTURE_2D);

                texture.MakeActive();
            }

            Opengl32.Color(1.0f, 1.0f, 1.0f, 1.0f);

            SetVertexArrayClientState(true);

            if (texture != null)
            {
                SetTextureCoordArrayClientState(true);
            }
            else
            {
                SetTextureCoordArrayClientState(false);
            }

            //Looks like pointer must be valid when you call draw arrays
            using (AutoPinner verticesPinner = new AutoPinner(vertices))
            {
                using (AutoPinner uvsPinner = new AutoPinner(uvs))
                {
                    Opengl32.VertexPointer(2, DataType.Float, 0, vertices);

                    if (texture != null)
                    {
                        Opengl32.TexCoordPointer(2, DataType.Float, 0, uvs);
                    }

                    Opengl32.DrawArrays(BeginMode.Quads, 0, vertexCount);
                }
            }

            if (texture != null)
            {
                SetTextureCoordArrayClientState(false);
            }

            if (texture != null)
            {
                Opengl32.Disable(Target.TEXTURE_2D);
            }
        }

        internal void Resize(int width, int height)
        {
            if (IsActive)
            {
                SetActive();
            }

            Opengl32.Viewport(0, 0, width, height);
        }

        public void LoadTextureUsing(OpenGLTexture texture, string resourcesName, string name)
        {
            string textureFullPath = Resources.MainPath + @"\" + resourcesName + @"\" + name;
            string textureName = resourcesName + @"/" + name;

            if (!LoadedTextures.ContainsKey(textureName))
            {
                texture.LoadFromFile(textureFullPath);
                LoadedTextures.Add(textureName, texture);
            }
            else
            {
                texture.CopyFrom(LoadedTextures[textureName]);
            }
        }

        public OpenGLTexture LoadTexture(string resourcesName, string name)
        {
            string textureFullPath = Resources.MainPath + @"\" + resourcesName + @"\" + name;
            string textureName = resourcesName + @"/" + name;

            OpenGLTexture texture = null;

            if (LoadedTextures.ContainsKey(textureName))
            {
                texture = LoadedTextures[textureName];
            }
            else
            {
                texture = OpenGLTexture.FromFile(textureFullPath);
                LoadedTextures.Add(textureName, texture);
            }

            return texture;
        }

        public OpenGLTexture GetTexture(string textureName)
        {
            OpenGLTexture texture = null;

            if (LoadedTextures.ContainsKey(textureName))
            {
                texture = LoadedTextures[textureName];
            }

            return texture;
        }

        private bool _blendingMode = false;

        public void SetBlending(bool enable)
        {
            if (_blendingMode != enable)
            {
                _blendingMode = enable;

                if (_blendingMode)
                {
                    Opengl32.Enable(Target.BLEND);
                    Opengl32.BlendFunc(BlendingSourceFactor.SourceAlpha, BlendingDestinationFactor.OneMinusSourceAlpha);
                }
                else
                {
                    Opengl32.Disable(Target.BLEND);
                }
            }
        }

        private bool _vertexArrayClientState = false;

        public void SetVertexArrayClientState(bool enable)
        {
            if (_vertexArrayClientState != enable)
            {
                _vertexArrayClientState = enable;

                if (_vertexArrayClientState)
                {
                    Opengl32.EnableClientState((uint)ArrayType.Vertex);
                }
                else
                {
                    Opengl32.DisableClientState((uint)ArrayType.Vertex);
                }
            }
        }

        private bool _textureCoordArrayClientState = false;

        public void SetTextureCoordArrayClientState(bool enable)
        {
            if (_textureCoordArrayClientState != enable)
            {
                _textureCoordArrayClientState = enable;

                if (_textureCoordArrayClientState)
                {
                    Opengl32.EnableClientState((uint)ArrayType.TextureCoord);
                }
                else
                {
                    Opengl32.DisableClientState((uint)ArrayType.TextureCoord);
                }
            }
        }
    }

    struct ScissorTestInfo
    {
        public ScissorTestInfo(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X;
        public int Y;
        public int Width;
        public int Height;
    }
}