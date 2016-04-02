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
    public static class Graphics2D
    {
        public static void DrawSprite(float mapX, float mapY, Sprite sprite, bool horizontalCenter = false, bool verticalCenter = false, float scale = 1.0f, bool flipped = false, float rotation = 0.0f)
        {
            DrawSprite(mapX, mapY, sprite, horizontalCenter, verticalCenter, scale, flipped, rotation, sprite.Width, sprite.Height);

            //for(int i = 0; i < sprite.SpriteParts.Count(); i++)
            //{
            //	SpritePartInfo* spritePartInfo = (*sprite.SpriteParts)[i];
            //	Sx2d::Graphics2D::DrawSpritePart(mapX, mapY, spritePartInfo.SpritePart, horizontalCenter, verticalCenter, scale, flipped, rotation);
            //}
        }

        public static void DrawSprite(float mapX, float mapY, Sprite sprite, bool horizontalCenter, bool verticalCenter, float scale, bool flipped, float rotation, float customWidth, float customHeight)
        {
            if (sprite != null)
            {
                sprite.DrawToScreen(mapX, mapY, horizontalCenter, verticalCenter, scale, flipped, rotation, customWidth, customHeight);
            }
        }

        public static void DrawSpritePart(SpritePart spritePart, float mapX, float mapY, float[] outVertices, float[] outUvs, int verticesStartIndex, int uvsStartIndex)
        {
            DrawSpritePart(spritePart, mapX, mapY, outVertices, outUvs, verticesStartIndex, uvsStartIndex, 0.0f, false, false, 1.0f, false, spritePart.Width, spritePart.Height);
        }

        public static void DrawSpritePart(SpritePart spritePart, float mapX, float mapY, float[] outVertices, float[] outUvs, int verticesStartIndex, int uvsStartIndex, float rotation, bool horizontalCenter, bool verticalCenter, float scale, bool flipped, float customWidth, float customHeight)
        {
            if (spritePart == null)
            {
                return;
            }

            Texture spriteTexture = spritePart.Texture;

            if (spriteTexture == null)
            {
                return;
            }

            float customWidthScale = (float)customWidth / (float)spritePart.Width;
            float customHeightScale = (float)customHeight / (float)spritePart.Height;

            float objectOriginalWidth = spritePart.Width * scale * customWidthScale;
            float objectOriginalHeight = spritePart.Height * scale * customHeightScale;

            float objectWidth = spritePart.RelativeWidth * scale * customWidthScale;
            float objectHeight = spritePart.RelativeHeight * scale * customHeightScale;

            float objectXOffset = spritePart.MinX * scale * customWidthScale;
            float objectYOffset = -spritePart.MinY * scale * customHeightScale;

            float uMin = spritePart.MinU;
            float uMax = spritePart.MaxU;
            float vMin = spritePart.MinV;
            float vMax = spritePart.MaxV;

            float screenX = 0.0f;
            float screenY = 0.0f;

            if (flipped)
            {
                if (spritePart.Rotated)
                {
                    objectXOffset = (spritePart.Width - spritePart.MaxX) * scale * customWidthScale;
                    vMax = spritePart.MinV;
                    vMin = spritePart.MaxV;
                }
                else
                {
                    objectXOffset = (spritePart.Width - spritePart.MaxX) * scale * customWidthScale;
                    uMin = spritePart.MaxU;
                    uMax = spritePart.MinU;
                }
            }

            screenX = mapX + objectXOffset;
            screenY = mapY - objectYOffset;

            if (horizontalCenter)
            {
                screenX = mapX + objectXOffset - objectOriginalWidth * 0.5f;
            }

            if (verticalCenter)
            {
                screenY = mapY - objectYOffset - objectOriginalHeight * 0.5f;
            }

            outVertices[verticesStartIndex + 0] = screenX + 0;
            outVertices[verticesStartIndex + 1] = screenY + 0;
            outVertices[verticesStartIndex + 2] = screenX + 0;
            outVertices[verticesStartIndex + 3] = screenY + objectHeight;
            outVertices[verticesStartIndex + 4] = screenX + objectWidth;
            outVertices[verticesStartIndex + 5] = screenY + objectHeight;
            outVertices[verticesStartIndex + 6] = screenX + objectWidth;
            outVertices[verticesStartIndex + 7] = screenY + 0;  

            if (spritePart.Rotated)
            {
                outUvs[uvsStartIndex + 0] = uMin;
                outUvs[uvsStartIndex + 1] = vMin;
                outUvs[uvsStartIndex + 2] = uMax;
                outUvs[uvsStartIndex + 3] = vMin;
                outUvs[uvsStartIndex + 4] = uMax;
                outUvs[uvsStartIndex + 5] = vMax;
                outUvs[uvsStartIndex + 6] = uMin;
                outUvs[uvsStartIndex + 7] = vMax;
            }
            else
            {
                outUvs[uvsStartIndex + 0] = uMin;
                outUvs[uvsStartIndex + 1] = vMin;
                outUvs[uvsStartIndex + 2] = uMin;
                outUvs[uvsStartIndex + 3] = vMax;
                outUvs[uvsStartIndex + 4] = uMax;
                outUvs[uvsStartIndex + 5] = vMax;
                outUvs[uvsStartIndex + 6] = uMax;
                outUvs[uvsStartIndex + 7] = vMin;
            }
        }
    }
}
