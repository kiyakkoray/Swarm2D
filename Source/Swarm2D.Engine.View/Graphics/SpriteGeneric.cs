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

namespace Swarm2D.Engine.View
{
    public class SpriteGeneric : Sprite
    {
        public SpritePart SpritePart { get; private set; }

        private float[] _vertices;
        private float[] _uvs;

        public SpriteGeneric(string name, SpritePart spritePart)
            : base(name, spritePart.Width, spritePart.Height)
        {
            SpritePart = spritePart;

            _vertices = new float[8];
            _uvs = new float[8];
        }

        internal override void GetArrays(float mapX, float mapY, float scale, float width, float height, out Texture texture, out float[] outVertices, out float[] outUvs)
        {
            SpritePart.DrawSpritePart(mapX, mapY, _vertices, _uvs, 0, 0, scale, width, height);

            texture = SpritePart.Texture;
            outVertices = _vertices;
            outUvs = _uvs;
        }
    }
}
