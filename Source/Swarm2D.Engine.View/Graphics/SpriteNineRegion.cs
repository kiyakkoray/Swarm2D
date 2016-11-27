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
using Swarm2D.Library;

namespace Swarm2D.Engine.View
{
    public class SpriteNineRegion : Sprite
    {
        public SpritePart BaseSprite { get; private set; }

        public int LeftWidth { get; private set; }
        public int RightWidth { get; private set; }
        public int TopHeight { get; private set; }
        public int BottomHeight { get; private set; }

        private int _centerWidth;
        private int _centerHeight;

        private float _u0;
        private float _u1;
        private float _u2;
        private float _u3;

        private float _v0;
        private float _v1;
        private float _v2;
        private float _v3;

        #region Temporary Values

        private Texture _texture;
        private float[] _outUvs;
        private float[] _outVertices;

        private int _verticesStartIndex;
        private int _uvsStartIndex;
        private float _scale;

        private float _customWidth;
        private float _customHeight;

        #endregion

        public SpriteNineRegion(string name, SpritePart baseSprite, int leftWidth, int rightWidth, int topHeight, int bottomHeight)
            : base(name, baseSprite.Width, baseSprite.Height)
        {
            BaseSprite = baseSprite;
            LeftWidth = leftWidth;
            RightWidth = rightWidth;
            TopHeight = topHeight;
            BottomHeight = bottomHeight;

            _centerWidth = baseSprite.Width - leftWidth - rightWidth;
            _centerHeight = baseSprite.Height - topHeight - bottomHeight;

            Debug.Assert(baseSprite.MinX == 0, "baseSprite.MinX == 0, Nine region sprites does not support empty field prunning");
            Debug.Assert(baseSprite.MinY == 0, "baseSprite.MinY == 0, Nine region sprites does not support empty field prunning");
            Debug.Assert(baseSprite.MaxX == baseSprite.Width - 1, "baseSprite.MaxX == baseSprite.Width - 1, Nine region sprites does not support empty field prunning");
            Debug.Assert(baseSprite.MaxY == baseSprite.Height - 1, "baseSprite.MaxY == baseSprite.Height - 1, Nine region sprites does not support empty field prunning");
        }

        internal override void GetArrays(float mapX, float mapY, float scale, float customWidth, float customHeight, out Texture texture, out float[] outVertices, out float[] outUvs)
        {
            _texture = BaseSprite.Texture;
            _outVertices = new float[72];
            _verticesStartIndex = 0;
            _uvsStartIndex = 0;
            _scale = scale;
            _customWidth = customWidth;
            _customHeight = customHeight;

            Draw();

            if (_outUvs == null)
            {
                _outUvs = new float[72];
                CalculateTextureCoordinates();
            }

            for (int i = 0; i < 36; i++)
            {
                _outVertices[2 * i] += mapX;
                _outVertices[2 * i + 1] += mapY;
            }

            outVertices = _outVertices;
            outUvs = _outUvs;
            texture = _texture;

            _outVertices = null;
            _texture = null;
        }

        private void Draw()
        {
            DrawTopLeft();
            DrawTop();
            DrawTopRight();

            DrawLeft();
            DrawCenter();
            DrawRight();

            DrawBottomLeft();
            DrawBottom();
            DrawBottomRight();
        }

        private void CalculateTextureCoordinates()
        {
            float minU = BaseSprite.MinU;
            float minV = BaseSprite.MinV;
            float maxU = BaseSprite.MaxU;
            float maxV = BaseSprite.MaxV;

            if (BaseSprite.Rotated)
            {
                _u0 = minU;
                _u1 = minU + (maxU - minU) * ((float)TopHeight / (float)Height);
                _u2 = minU + (maxU - minU) * ((float)(TopHeight + _centerHeight) / (float)Height);
                _u3 = maxU;

                _v0 = minV;
                _v1 = minV + (maxV - minV) * ((float)LeftWidth / (float)Width);
                _v2 = minV + (maxV - minV) * ((float)(LeftWidth + _centerWidth) / (float)Width);
                _v3 = maxV;
            }
            else
            {
                _u0 = minU;
                _u1 = minU + (maxU - minU) * ((float)LeftWidth / (float)Width);
                _u2 = minU + (maxU - minU) * ((float)(LeftWidth + _centerWidth) / (float)Width);
                _u3 = maxU;

                _v0 = minV;
                _v1 = minV + (maxV - minV) * ((float)TopHeight / (float)Height);
                _v2 = minV + (maxV - minV) * ((float)(TopHeight + _centerHeight) / (float)Height);
                _v3 = maxV;
            }

            CalculateTextureCoordinatesTopLeft();
            CalculateTextureCoordinatesTop();
            CalculateTextureCoordinatesTopRight();

            CalculateTextureCoordinatesLeft();
            CalculateTextureCoordinatesCenter();
            CalculateTextureCoordinatesRight();

            CalculateTextureCoordinatesBottomLeft();
            CalculateTextureCoordinatesBottom();
            CalculateTextureCoordinatesBottomRight();
        }

        #region Drawing

        private void DrawTopLeft()
        {
            float x = 0;
            float y = 0;

            float objectWidth = LeftWidth;
            float objectHeight = TopHeight;

            Draw(x, y, objectWidth, objectHeight);
        }

        private void DrawTop()
        {
            float x = LeftWidth;
            float y = 0;

            float objectWidth = _customWidth - (LeftWidth + RightWidth);
            float objectHeight = TopHeight;

            Draw(x, y, objectWidth, objectHeight);
        }

        private void DrawTopRight()
        {
            float x = _customWidth - RightWidth;
            float y = 0;

            float objectWidth = RightWidth;
            float objectHeight = TopHeight;

            Draw(x, y, objectWidth, objectHeight);
        }

        private void DrawLeft()
        {
            float x = 0;
            float y = TopHeight;

            float objectWidth = LeftWidth;
            float objectHeight = _customHeight - (TopHeight + BottomHeight);

            Draw(x, y, objectWidth, objectHeight);
        }

        private void DrawCenter()
        {
            float x = LeftWidth;
            float y = TopHeight;

            float objectWidth = _customWidth - (LeftWidth + RightWidth);
            float objectHeight = _customHeight - (TopHeight + BottomHeight);

            Draw(x, y, objectWidth, objectHeight);
        }

        private void DrawRight()
        {
            float x = _customWidth - RightWidth;
            float y = TopHeight;

            float objectWidth = RightWidth;
            float objectHeight = _customHeight - (TopHeight + BottomHeight);

            Draw(x, y, objectWidth, objectHeight);
        }

        private void DrawBottomLeft()
        {
            float x = 0;
            float y = _customHeight - BottomHeight;

            float objectWidth = LeftWidth;
            float objectHeight = TopHeight;

            Draw(x, y, objectWidth, objectHeight);
        }

        private void DrawBottom()
        {
            float x = LeftWidth;
            float y = _customHeight - BottomHeight;

            float objectWidth = _customWidth - (LeftWidth + RightWidth);
            float objectHeight = TopHeight;

            Draw(x, y, objectWidth, objectHeight);
        }

        private void DrawBottomRight()
        {
            float x = _customWidth - RightWidth;
            float y = _customHeight - BottomHeight;

            float objectWidth = RightWidth;
            float objectHeight = TopHeight;

            Draw(x, y, objectWidth, objectHeight);
        }

        #endregion

        #region Texture Coordinates 

        private void CalculateTextureCoordinatesTopLeft()
        {
            float uMin = _u0;
            float vMin = _v0;
            float uMax = _u1;
            float vMax = _v1;

            CalculateTextureCoordinates(uMin, vMin, uMax, vMax);
        }

        private void CalculateTextureCoordinatesTop()
        {
            float uMin = _u1;
            float vMin = _v0;
            float uMax = _u2;
            float vMax = _v1;

            if (BaseSprite.Rotated)
            {
                uMin = _u0;
                vMin = _v1;
                uMax = _u1;
                vMax = _v2;
            }

            CalculateTextureCoordinates(uMin, vMin, uMax, vMax);
        }

        private void CalculateTextureCoordinatesTopRight()
        {
            float uMin = _u2;
            float vMin = _v0;
            float uMax = _u3;
            float vMax = _v1;

            if (BaseSprite.Rotated)
            {
                uMin = _u0;
                vMin = _v2;
                uMax = _u1;
                vMax = _v3;
            }

            CalculateTextureCoordinates(uMin, vMin, uMax, vMax);
        }

        private void CalculateTextureCoordinatesLeft()
        {
            float uMin = _u0;
            float vMin = _v1;
            float uMax = _u1;
            float vMax = _v2;

            if (BaseSprite.Rotated)
            {
                uMin = _u1;
                vMin = _v0;
                uMax = _u2;
                vMax = _v1;
            }

            CalculateTextureCoordinates(uMin, vMin, uMax, vMax);
        }

        private void CalculateTextureCoordinatesCenter()
        {
            float uMin = _u1;
            float vMin = _v1;
            float uMax = _u2;
            float vMax = _v2;

            CalculateTextureCoordinates(uMin, vMin, uMax, vMax);
        }

        private void CalculateTextureCoordinatesRight()
        {
            float uMin = _u2;
            float vMin = _v1;
            float uMax = _u3;
            float vMax = _v2;

            if (BaseSprite.Rotated)
            {
                uMin = _u1;
                vMin = _v2;
                uMax = _u2;
                vMax = _v3;
            }

            CalculateTextureCoordinates(uMin, vMin, uMax, vMax);
        }

        private void CalculateTextureCoordinatesBottomLeft()
        {
            float uMin = _u0;
            float vMin = _v2;
            float uMax = _u1;
            float vMax = _v3;

            if (BaseSprite.Rotated)
            {
                uMin = _u2;
                vMin = _v0;
                uMax = _u3;
                vMax = _v1;
            }

            CalculateTextureCoordinates(uMin, vMin, uMax, vMax);
        }

        private void CalculateTextureCoordinatesBottom()
        {
            float uMin = _u1;
            float vMin = _v2;
            float uMax = _u2;
            float vMax = _v3;

            if (BaseSprite.Rotated)
            {
                uMin = _u2;
                vMin = _v1;
                uMax = _u3;
                vMax = _v2;
            }

            CalculateTextureCoordinates(uMin, vMin, uMax, vMax);
        }

        private void CalculateTextureCoordinatesBottomRight()
        {
            float uMin = _u2;
            float vMin = _v2;
            float uMax = _u3;
            float vMax = _v3;

            CalculateTextureCoordinates(uMin, vMin, uMax, vMax);
        }

        #endregion

        private void Draw(float x, float y, float width, float height)
        {
            _outVertices[_verticesStartIndex + 0] = x + 0;
            _outVertices[_verticesStartIndex + 1] = y + 0;
            _outVertices[_verticesStartIndex + 2] = x + 0;
            _outVertices[_verticesStartIndex + 3] = y + height;
            _outVertices[_verticesStartIndex + 4] = x + width;
            _outVertices[_verticesStartIndex + 5] = y + height;
            _outVertices[_verticesStartIndex + 6] = x + width;
            _outVertices[_verticesStartIndex + 7] = y + 0;

            _verticesStartIndex += 8;
        }

        private void CalculateTextureCoordinates(float uMin, float vMin, float uMax, float vMax)
        {
            if (BaseSprite.Rotated)
            {
                _outUvs[_uvsStartIndex + 0] = uMin;
                _outUvs[_uvsStartIndex + 1] = vMin;
            
                _outUvs[_uvsStartIndex + 2] = uMax;
                _outUvs[_uvsStartIndex + 3] = vMin;
            
                _outUvs[_uvsStartIndex + 4] = uMax;
                _outUvs[_uvsStartIndex + 5] = vMax;
            
                _outUvs[_uvsStartIndex + 6] = uMin;
                _outUvs[_uvsStartIndex + 7] = vMax;
            }
            else
            {
                _outUvs[_uvsStartIndex + 0] = uMin;
                _outUvs[_uvsStartIndex + 1] = vMin;

                _outUvs[_uvsStartIndex + 2] = uMin;
                _outUvs[_uvsStartIndex + 3] = vMax;

                _outUvs[_uvsStartIndex + 4] = uMax;
                _outUvs[_uvsStartIndex + 5] = vMax;

                _outUvs[_uvsStartIndex + 6] = uMax;
                _outUvs[_uvsStartIndex + 7] = vMin;
            }

            _uvsStartIndex += 8;
        }
    }

}
