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
    public class SpritePart
    {
        public string Name { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int SheetID { get; set; }

        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }

        public int SheetX { get; set; }
        public int SheetY { get; set; }
        public bool Rotated { get; set; }

        public float MinU { get; private set; }
        public float MinV { get; private set; }
        public float MaxU { get; private set; }
        public float MaxV { get; private set; }

        public int RelativeWidth { get; private set; }
        public int RelativeHeight { get; private set; }

        public Texture Texture
        {
            get
            {
                if (_category != null)
                {
                    if (_category.IsLoaded)
                    {
                        if (_category.SpriteSheets != null)
                        {
                            if (_category.SpriteSheets.Count >= SheetID)
                            {
                                return _category.SpriteSheets[SheetID - 1];
                            }
                        }
                    }
                }

                return null;
            }
        }

        public SpriteCategory Category
        {
            get
            {
                return _category;
            }
        }

        private SpriteCategory _category;

        public SpritePart(string name, SpriteCategory category, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
            _category = category;
            _category.SpriteParts.Add(this);
        }

        internal void Initialize()
        {
            RelativeWidth = MaxX - MinX + 1;
            RelativeHeight = MaxY - MinY + 1;

            //float division = 1.0f / (float)Parameters.SpriteSheetWidth;
            const double inverseLength = 1.0 / 2048.0;
            const double adder = 0;

            double minU, maxU, minV, maxV;

            if (Rotated)
            {
                minU = ((double)SheetX + adder) * inverseLength;
                maxU = ((double)(SheetX + RelativeHeight + adder)) * inverseLength;
                //MaxV = /*1.0f - */((float) SheetY) * inverseLength;
                //MinV = /*1.0f - */((float)(SheetY + RelativeWidth)) * inverseLength;

                minV = /*1.0f - */((double)SheetY + adder) * inverseLength;
                maxV = /*1.0f - */((double)(SheetY + RelativeWidth + adder)) * inverseLength;
            }
            else
            {
                minU = ((double)SheetX + adder) * inverseLength;
                maxU = ((double)(SheetX + RelativeWidth + adder)) * inverseLength;
                //MaxV = /*1.0f - */((float)SheetY) * inverseLength;
                //MinV = /*1.0f -*/ ((float)(SheetY + RelativeHeight)) * inverseLength;

                minV = /*1.0f - */((double)SheetY + adder) * inverseLength;
                maxV = /*1.0f -*/ ((double)(SheetY + RelativeHeight + adder)) * inverseLength;
            }

            MinU = (float)minU;
            MaxU = (float)maxU;
            MinV = (float)minV;
            MaxV = (float)maxV;
        }

        public void DrawSpritePart(float mapX, float mapY, float[] outVertices, float[] outUvs, int verticesStartIndex, int uvsStartIndex)
        {
            DrawSpritePart(mapX, mapY, outVertices, outUvs, verticesStartIndex, uvsStartIndex, 1.0f, Width, Height);
        }

        public void DrawSpritePart(float mapX, float mapY, float[] outVertices, float[] outUvs, int verticesStartIndex, int uvsStartIndex, float scale, float customWidth, float customHeight)
        {
            if (Texture != null)
            {
                float customWidthScale = (float)customWidth / (float)Width;
                float customHeightScale = (float)customHeight / (float)Height;

                float objectWidth = RelativeWidth * scale * customWidthScale;
                float objectHeight = RelativeHeight * scale * customHeightScale;

                float objectXOffset = MinX * scale * customWidthScale;
                float objectYOffset = -MinY * scale * customHeightScale;

                float screenX = mapX + objectXOffset;
                float screenY = mapY - objectYOffset;

                outVertices[verticesStartIndex + 0] = screenX + 0;
                outVertices[verticesStartIndex + 1] = screenY + 0;
                outVertices[verticesStartIndex + 2] = screenX + 0;
                outVertices[verticesStartIndex + 3] = screenY + objectHeight;
                outVertices[verticesStartIndex + 4] = screenX + objectWidth;
                outVertices[verticesStartIndex + 5] = screenY + objectHeight;
                outVertices[verticesStartIndex + 6] = screenX + objectWidth;
                outVertices[verticesStartIndex + 7] = screenY + 0;


                FillTextureCoordinates(outUvs, uvsStartIndex);
            }
        }

        public void FillTextureCoordinates(float[] outUVs, int uvsStartIndex)
        {
            float uMin = MinU;
            float uMax = MaxU;
            float vMin = MinV;
            float vMax = MaxV;

            if (Rotated)
            {
                outUVs[uvsStartIndex + 0] = uMin;
                outUVs[uvsStartIndex + 1] = vMin;
                outUVs[uvsStartIndex + 2] = uMax;
                outUVs[uvsStartIndex + 3] = vMin;
                outUVs[uvsStartIndex + 4] = uMax;
                outUVs[uvsStartIndex + 5] = vMax;
                outUVs[uvsStartIndex + 6] = uMin;
                outUVs[uvsStartIndex + 7] = vMax;
            }
            else
            {
                outUVs[uvsStartIndex + 0] = uMin;
                outUVs[uvsStartIndex + 1] = vMin;
                outUVs[uvsStartIndex + 2] = uMin;
                outUVs[uvsStartIndex + 3] = vMax;
                outUVs[uvsStartIndex + 4] = uMax;
                outUVs[uvsStartIndex + 5] = vMax;
                outUVs[uvsStartIndex + 6] = uMax;
                outUVs[uvsStartIndex + 7] = vMin;
            }
        }
    }
}
