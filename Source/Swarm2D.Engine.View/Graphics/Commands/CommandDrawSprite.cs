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
    class CommandDrawSprite : GraphicsCommand
    {
        private float _mapX;
        private float _mapY;
        private Sprite _sprite;
        private float _scale = 1.0f;
        private float _rotation = 0.0f;

        private float _width;
        private float _height;

        private Texture _texture;
        private float[] _vertices;
        private float[] _uvs;

        internal override bool Batchable { get { return true; } }

        private BatchedDrawTextureContext _batchedDrawSpriteContext;

        internal CommandDrawSprite(float mapX, float mapY, Sprite sprite, float scale = 1.0f)
            : this(mapX, mapY, sprite, scale, sprite.Width, sprite.Height)
        {
        }

        internal CommandDrawSprite(float mapX, float mapY, Sprite sprite, float scale, float width, float height)
        {
            _mapX = mapX;
            _mapY = mapY;
            _sprite = sprite;
            _scale = scale;

            _width = width;
            _height = height;
        }

        internal override void PrepareJob()
        {
            float[] vertices;
            float[] uvs;

            _sprite.GetArrays(_mapX, _mapY, _scale, _width, _height, out _texture, out vertices, out uvs);

            _vertices = new List<float>(vertices).ToArray();
            _uvs = new List<float>(uvs).ToArray();
        }

        internal override void DoJob()
        {
            if (_batchedDrawSpriteContext != null)
            {
                Graphics.DrawArrays(_texture, _batchedDrawSpriteContext.Vertices.ToArray(), _batchedDrawSpriteContext.Uvs.ToArray());
            }
            else if (_texture != null && _vertices != null && _uvs != null)
            {
                Graphics.DrawArrays(_texture, _vertices, _uvs);
            }
        }

        internal override bool TryBatch(GraphicsCommand command)
        {
            if (_texture != null && _vertices != null && _uvs != null)
            {
                if (command is CommandDrawSprite)
                {
                    CommandDrawSprite nextCommand = (CommandDrawSprite)command;

                    if (nextCommand._texture == _texture)
                    {
                        if (_batchedDrawSpriteContext == null)
                        {
                            _batchedDrawSpriteContext = new BatchedDrawTextureContext(_texture);
                            _batchedDrawSpriteContext.AddRange(_vertices, _uvs);
                        }

                        _batchedDrawSpriteContext.AddRange(nextCommand._vertices, nextCommand._uvs);

                        return true;
                    }
                }
            }

            return false;
        }
    }

    class BatchedDrawTextureContext
    {
        internal Texture Texture { get; private set; }

        internal List<float> Vertices { get; private set; }
        internal List<float> Uvs { get; private set; }

        internal BatchedDrawTextureContext(Texture texture)
        {
            Texture = texture;

            Vertices = new List<float>(8192);
            Uvs = new List<float>(8192);
        }

        internal void AddRange(float[] vertices, float[] uvs)
        {
            Vertices.AddRange(vertices);
            Uvs.AddRange(uvs);
        }
    }
}
