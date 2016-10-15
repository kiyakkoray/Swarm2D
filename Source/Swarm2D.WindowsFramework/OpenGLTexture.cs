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
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View;
using Swarm2D.Library;
using Swarm2D.WindowsFramework.Native.Opengl;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Swarm2D.WindowsFramework
{
    public class OpenGLTexture : Texture
    {
        private int _width;
        private int _height;

        public override int Width { get { return _width; } }
        public override int Height { get { return _height; } }

        internal static OpenGLTexture ActiveTexture
        {
            get;
            private set;
        }

        private GraphicsContext _context;
        private int _id;

        public string Name { get; private set; }

        public OpenGLTexture()
        {

        }

        public void Initialize(string name, int width, int height)
        {
            _context = GraphicsContext.Active;
            Name = name;
            _id = 0;
            Opengl32.GenTextures(1, ref _id);

            _width = width;
            _height = height;
        }

        public void CopyFrom(OpenGLTexture texture)
        {
            _width = texture._width;
            _height = texture._height;

            Name = texture.Name;
            _id = texture._id;
            _context = texture._context;
        }

        public override void Delete()
        {
            Opengl32.DeleteTextures(1, new int[] { _id });
            Debug.Log("texture deleted! : " + Name);
        }

        internal void MakeActive()
        {
            if (ActiveTexture != this)
            {
                Opengl32.BindTexture(Target.TEXTURE_2D, _id);
                ActiveTexture = this;
            }
        }

        public static OpenGLTexture FromFile(string fileName)
        {
            OpenGLTexture texture = new OpenGLTexture();

            texture.LoadFromFile(fileName);

            return texture;
        }

        public void LoadFromFile(string fileName)
        {
            Bitmap bmp = null;

            if (File.Exists(fileName + ".png"))
            {
                bmp = Bitmap.FromFile(fileName + ".png") as Bitmap;
            }

            if (bmp != null)
            {
                int width = bmp.Width;
                int height = bmp.Height;

                this.Initialize(fileName, width, height);
                this.MakeActive();

                if (bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    System.Drawing.Imaging.BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                    //#define GL_BGR_EXT 0x80E0
                    Opengl32.TexImage2D(Target.TEXTURE_2D, 0, /*TextureInternalFormat.RGB8*/3, width, height, 0, (PixelFormat)0x80E0,
                        DataType.UnsignedByte, bitmapData.Scan0);
                    bmp.UnlockBits(bitmapData);
                }
                else if (bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    System.Drawing.Imaging.BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                    Opengl32.TexImage2D(Target.TEXTURE_2D, 0, /*TextureInternalFormat.RGBA8*/0x8058, width, height, 0,
                        (PixelFormat)0x80E1, DataType.UnsignedByte, bitmapData.Scan0);
                    bmp.UnlockBits(bitmapData);

                    //System.Drawing.Imaging.BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                    //Opengl32.TexImage2D(Target.TEXTURE_2D, 0, TextureInternalFormat.RGBA8, width, height, 0, (PixelFormat)OpenGL.EXT.Bgra.BGRA, DataType.UnsignedByte, bitmapData.Scan0);
                    //bmp.UnlockBits(bitmapData);
                }
                else if (bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                {
                    System.Drawing.Imaging.BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                    //#define GL_BGRA_EXT 0x80E1

                    Opengl32.TexImage2D(Target.TEXTURE_2D, 0, /*TextureInternalFormat.RGBA8*/4, width, height, 0, (PixelFormat)0x80E1,
                        DataType.UnsignedByte, bitmapData.Scan0);
                    bmp.UnlockBits(bitmapData);

                    //System.Drawing.Imaging.BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    //
                    ////#define GL_BGR_EXT 0x80E0
                    //Opengl32.TexImage2D(Target.TEXTURE_2D, 0, /*TextureInternalFormat.RGB8*/4, width, height, 0, (PixelFormat)0x80E0, DataType.UnsignedByte, bitmapData.Scan0);
                    //bmp.UnlockBits(bitmapData);
                }
                else
                {
                    Debug.Log("non 24 or 32 bit bitmap file");
                }

                Opengl32.TexParameteri(Target.TEXTURE_2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
                Opengl32.TexParameteri(Target.TEXTURE_2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                //Opengl32.TexParameteri(Target.TEXTURE_2D, TextureParameterName.TextureWrapS, OpenGL.EXT.TextureEdgeClamp.ClampToEdge);
                //Opengl32.TexParameteri(Target.TEXTURE_2D, TextureParameterName.TextureWrapT, OpenGL.EXT.TextureEdgeClamp.ClampToEdge);

                bmp.Dispose();
            }
        }
    }
}
