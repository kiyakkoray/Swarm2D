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
using System.Runtime.InteropServices;
using System.Text;
using Swarm2D.Library;

namespace Swarm2D.Engine.View
{
    class CommandDrawTextureOnScreen : GraphicsCommand
    {
        private float _x;
        private float _y;
        private Texture _texture;

        private static readonly float[] _textureUVs;

        static CommandDrawTextureOnScreen()
        {
            _textureUVs = new float[8] { 0, 0, 0, 1, 1, 1, 1, 0 };
        }

        internal CommandDrawTextureOnScreen(float x, float y, Texture texture)
        {
            _x = x;
            _y = y;
            _texture = texture;
        }

        internal override void DoJob()
        {
            float[] vertices = new float[8];

            vertices[0] = _x;
            vertices[1] = _y;

            vertices[2] = _x + _texture.Width;
            vertices[3] = _y;

            vertices[4] = _x + _texture.Width;
            vertices[5] = _y + _texture.Height;

            vertices[6] = _x;
            vertices[7] = _y + _texture.Height;

            Graphics.DrawArrays(_x, _y, _texture, vertices, _textureUVs);
        }
    }
}
