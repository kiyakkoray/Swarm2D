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
        private bool _horizontalCenter = false;
        private bool _verticalCenter = false;
        private float _scale = 1.0f;
        private bool _flipped = false;
        private float _rotation = 0.0f;

        private float _width;
        private float _height;

        internal CommandDrawSprite(float mapX, float mapY, Sprite sprite, bool horizontalCenter = false,
            bool verticalCenter = false, float scale = 1.0f, bool flipped = false, float rotation = 0.0f)
            : this(mapX, mapY, sprite, horizontalCenter, verticalCenter, scale, flipped, rotation, sprite.Width, sprite.Height)
        {
        }

        internal CommandDrawSprite(float mapX, float mapY, Sprite sprite, bool horizontalCenter,
            bool verticalCenter, float scale, bool flipped, float rotation, float width, float height)
        {
            _mapX = mapX;
            _mapY = mapY;
            _sprite = sprite;
            _horizontalCenter = horizontalCenter;
            _verticalCenter = verticalCenter;
            _scale = scale;
            _flipped = flipped;
            _rotation = rotation;

            _width = width;
            _height = height;
        }

        internal override void DoJob()
        {
            Graphics2D.DrawSprite(_mapX, _mapY, _sprite, _horizontalCenter, _verticalCenter, _scale, _flipped, _rotation, _width, _height);
        }
    }
}
