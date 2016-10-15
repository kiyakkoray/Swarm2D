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
using System.Drawing;
using Swarm2D.Engine.Logic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Swarm2D.Engine.View;

namespace Swarm2D.SpriteEditor
{
    public class BitmapOperations
    {
        public static Bitmap SpriteFilter(SpritePart spriteInfo, Bitmap sprite)
        {
            Bitmap newBitmap = new Bitmap(spriteInfo.MaxX - spriteInfo.MinX + 1, spriteInfo.MaxY - spriteInfo.MinY + 1, PixelFormat.Format32bppArgb);
            Color alphaColor = Color.FromArgb(0, 0, 0, 0);

            for (int i = spriteInfo.MinX; i <= spriteInfo.MaxX; i++)
            {
                for (int j = spriteInfo.MinY; j <= spriteInfo.MaxY; j++)
                {
                    Color c = sprite.GetPixel(i, j);
                    if (c.R == 255 && c.G == 0 && c.B == 255)
                    {
                        newBitmap.SetPixel(i - spriteInfo.MinX, j - spriteInfo.MinY, alphaColor);
                    }
                    else
                    {
                        newBitmap.SetPixel(i - spriteInfo.MinX, j - spriteInfo.MinY, Color.FromArgb(255, c.R, c.G, c.B));
                    }
                }
            }

            return newBitmap;
        }

        public static bool IsEmpty(Color color)
        {
            return (color.R == 255 && color.G == 0 && color.B == 255) || (color.R == 0 && color.B == 0 && color.G == 0 && color.A == 0);
        }

        public static int FindMinX(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color c = bmp.GetPixel(i, j);
                    if (!IsEmpty(c))
                    {
                        return i;
                    }
                }
            }

            return 0;
        }

        public static int FindMaxX(Bitmap bmp)
        {
            for (int i = bmp.Width - 1; i >= 0; i--)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color c = bmp.GetPixel(i, j);
                    if (!IsEmpty(c))
                    {
                        return i;
                    }
                }
            }

            return 0;
        }

        public static int FindMinY(Bitmap bmp)
        {
            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {
                    Color c = bmp.GetPixel(i, j);
                    if (!IsEmpty(c))
                    {
                        return j;
                    }
                }
            }

            return 0;
        }

        public static int FindMaxY(Bitmap bmp)
        {
            for (int j = bmp.Height - 1; j >= 0; j--)
            {
                for (int i = 0; i < bmp.Width; i++)
                {
                    Color c = bmp.GetPixel(i, j);
                    if (!IsEmpty(c))
                    {
                        return j;
                    }
                }
            }

            return 0;
        }

        public static byte[] ConvertBitmapToInternalFormat(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            
            byte[] bitmapAsSwarmFormat = new byte[bitmap.Width * bitmap.Height * 4 + sizeof(int) * 2];

            byte[] widthBytes = BitConverter.GetBytes(bitmap.Width);
            byte[] heightBytes = BitConverter.GetBytes(bitmap.Height);

            bitmapAsSwarmFormat[0] = widthBytes[0];
            bitmapAsSwarmFormat[1] = widthBytes[1];
            bitmapAsSwarmFormat[2] = widthBytes[2];
            bitmapAsSwarmFormat[3] = widthBytes[3];

            bitmapAsSwarmFormat[4] = heightBytes[0];
            bitmapAsSwarmFormat[5] = heightBytes[1];
            bitmapAsSwarmFormat[6] = heightBytes[2];
            bitmapAsSwarmFormat[7] = heightBytes[3];

            Marshal.Copy(bitmapData.Scan0, bitmapAsSwarmFormat, 8, bitmap.Width * bitmap.Height * 4);

            bitmap.UnlockBits(bitmapData);

            return bitmapAsSwarmFormat;
        }
    }
}
