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
using Swarm2D.Engine.Logic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Swarm2D.Engine.View;

namespace Swarm2D.SpriteEditor
{
    class SpriteSheet
    {
        const int spriteSheetWidth = 2048;
        const int spriteSheetHeight = 2048;
        const int spriteSheetEdgeSize = 4;

        private List<SpriteSheetRect> _freeRectangles;
        private Dictionary<SpritePart, SpriteSheetRect> _sheetSprites;

        private SpriteDataEditor _spriteDataEditor;

        public SpriteSheet(SpriteDataEditor spriteDataEditor)
        {
            _spriteDataEditor = spriteDataEditor;

            Width = spriteSheetWidth;
            Height = spriteSheetHeight;
            _freeRectangles = new List<SpriteSheetRect>();
            _sheetSprites = new Dictionary<SpritePart, SpriteSheetRect>();

            _freeRectangles.Add(new SpriteSheetRect(this, 0, 0, Width, Height));
        }

        public bool AddSpritePart(SpritePart spritePart)
        {
            int edgeSize = spriteSheetEdgeSize;

            SpriteSheetRect selectedRect = null;
            int selectedRectMinEdge = 0;

            bool rotated = false;

            int spriteRelativeWidth = spritePart.MaxX - spritePart.MinX;
            int spriteRelativeHeight = spritePart.MaxY - spritePart.MinY;

            int spriteWidth = spriteRelativeWidth + edgeSize;
            int spriteHeight = spriteRelativeHeight + edgeSize;

            for (int i = 0; i < _freeRectangles.Count; i++)
            {
                SpriteSheetRect currentRect = _freeRectangles[i];

                if ((spriteWidth) < currentRect.Width && (spriteHeight) < currentRect.Height)
                {
                    if (selectedRect != null)
                    {
                        if (currentRect.Width < selectedRectMinEdge || currentRect.Height < selectedRectMinEdge)
                        {
                            selectedRect = currentRect;
                            selectedRectMinEdge = selectedRect.Width;
                            if (selectedRect.Height < selectedRectMinEdge) selectedRectMinEdge = selectedRect.Height;
                            rotated = false;
                        }
                    }
                    else
                    {
                        selectedRect = currentRect;
                        selectedRectMinEdge = selectedRect.Width;
                        if (selectedRect.Height < selectedRectMinEdge) selectedRectMinEdge = selectedRect.Height;
                        rotated = false;
                    }
                }

                //rotated test
                if ((spriteHeight) < currentRect.Width && (spriteWidth) < currentRect.Height)
                {
                    if (selectedRect != null)
                    {
                        if (currentRect.Width < selectedRectMinEdge || currentRect.Height < selectedRectMinEdge)
                        {
                            selectedRect = currentRect;
                            selectedRectMinEdge = selectedRect.Width;
                            if (selectedRect.Height < selectedRectMinEdge) selectedRectMinEdge = selectedRect.Height;
                            rotated = true;
                        }
                    }
                    else
                    {
                        selectedRect = currentRect;
                        selectedRectMinEdge = selectedRect.Width;
                        if (selectedRect.Height < selectedRectMinEdge) selectedRectMinEdge = selectedRect.Height;
                        rotated = true;
                    }
                }
            }

            if (selectedRect == null)
            {
                return false;
            }

            if (rotated)
            {
                spriteHeight = spriteRelativeWidth + edgeSize;
                spriteWidth = spriteRelativeHeight + edgeSize;
            }

            spritePart.Rotated = rotated;
            spritePart.SheetX = selectedRect.X;
            spritePart.SheetY = selectedRect.Y;

            //insert to top left

            List<SpriteSheetRect> newRectangles = new List<SpriteSheetRect>();

            SpriteSheetRect firstFreeRect = new SpriteSheetRect(this, selectedRect.X + spriteWidth, selectedRect.Y, selectedRect.Width - spriteWidth, selectedRect.Height);
            SpriteSheetRect secondFreeRect = new SpriteSheetRect(this, selectedRect.X, selectedRect.Y + spriteHeight, selectedRect.Width, selectedRect.Height - spriteHeight);

            _freeRectangles.Remove(selectedRect);

            newRectangles.Add(firstFreeRect);
            newRectangles.Add(secondFreeRect);

            SpriteSheetRect spriteRect = new SpriteSheetRect(null, selectedRect.X, selectedRect.Y, spriteWidth, spriteHeight);
            _sheetSprites.Add(spritePart, spriteRect);

            spriteRect.Rotated = rotated;

            //diğer karelerle kesişiyorsa o kareleride parçalayalım
            for (int i = 0; i < _freeRectangles.Count; i++)
            {
                SpriteSheetRect freeRect = _freeRectangles[i];

                if (freeRect.IsCollide(spriteRect))
                {
                    SpriteSheetRect topRect = new SpriteSheetRect(this, freeRect.X, freeRect.Y, freeRect.Width, spriteRect.Y - freeRect.Y);
                    SpriteSheetRect bottomRect = new SpriteSheetRect(this, freeRect.X, spriteRect.Y2, freeRect.Width, freeRect.Y2 - spriteRect.Y2);

                    SpriteSheetRect leftRect = new SpriteSheetRect(this, freeRect.X, freeRect.Y, spriteRect.X - freeRect.X, freeRect.Height);
                    SpriteSheetRect rightRect = new SpriteSheetRect(this, spriteRect.X2, freeRect.Y, freeRect.X2 - spriteRect.X2, freeRect.Height);

                    newRectangles.Add(topRect);
                    newRectangles.Add(bottomRect);
                    newRectangles.Add(leftRect);
                    newRectangles.Add(rightRect);

                    _freeRectangles.RemoveAt(i);
                    i--;
                }
            }

            //yeni karelerin varlıklarına bakalım
            for (int i = 0; i < newRectangles.Count; i++)
            {
                SpriteSheetRect newRect = newRectangles[i];

                if (newRect.IsValid())
                {
                    _freeRectangles.Add(newRect);
                }
            }

            //eğer herhangibir kare başka bir kareyi içeriyorsa, onu silelim
            for (int i = 0; i < _freeRectangles.Count; i++)
            {
                SpriteSheetRect rectA = _freeRectangles[i];
                for (int j = 0; j < _freeRectangles.Count; j++)
                {
                    if (i == j) continue;

                    SpriteSheetRect rectB = _freeRectangles[j];
                    if (rectA.IsSubRectOf(rectB))
                    {
                        _freeRectangles.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }

            return true;
        }

        public Bitmap GetSheetBitmap()
        {
            Bitmap sheetBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(sheetBitmap);

            foreach (KeyValuePair<SpritePart, SpriteSheetRect> sheetSprite in _sheetSprites)
            {
                SpritePart spritePart = sheetSprite.Key;
                SpriteSheetRect spriteRect = sheetSprite.Value;

                Bitmap spriteBitmap = _spriteDataEditor.GetSpritePartBitmap(spritePart);

                DrawBitmapToBitmap(spriteBitmap, sheetBitmap, spriteRect.X, spriteRect.Y, spriteRect.Rotated, sheetSprite.Key, graphics);
                //DrawBitmapEdgesToBitmap(spriteBitmap, sheetBitmap, spriteRect.X, spriteRect.Y, spriteRect.Rotated, sheetSprite.Key, graphics);
                //DrawBitmapCornersToBitmap(spriteBitmap, sheetBitmap, spriteRect.X, spriteRect.Y, spriteRect.Rotated, sheetSprite.Key, graphics);

                if (spriteBitmap != null)
                {
                    spriteBitmap.Dispose();
                }
            }

            graphics.Dispose();

            return sheetBitmap;
        }

        public void SaveSheet()
        {
            string fileName = Path + @"\sheet" + ID + ".png";
            string fileInternalFormatName = Path + @"\sheet" + ID + ".stx";

            Bitmap sheetBitmap = GetSheetBitmap();
            sheetBitmap.Save(fileName, ImageFormat.Png);

            byte[] data = BitmapOperations.ConvertBitmapToInternalFormat(sheetBitmap);
            File.WriteAllBytes(fileInternalFormatName, data);

            sheetBitmap.Dispose();
        }

        private static void DrawBitmapToBitmap(Bitmap source, Bitmap destination, int offsetX, int offsetY, bool rotated,
            SpritePart spriteInfo, System.Drawing.Graphics graphics)
        {
            int width = (spriteInfo.MaxX - spriteInfo.MinX + 1);
            int height = (spriteInfo.MaxY - spriteInfo.MinY + 1);

            graphics.TranslateTransform(offsetX, offsetY);

            if (rotated)
            {
                graphics.ScaleTransform(-1.0f, 1.0f);
                graphics.RotateTransform(90.0f);
            }

            if (source != null)
            {
                graphics.DrawImage(source, new Rectangle(0, 0, width, height), spriteInfo.MinX, spriteInfo.MinY, width, height, GraphicsUnit.Pixel);
            }
            else
            {
                Pen pen = new Pen(Color.Red, 1.0f);
                graphics.DrawRectangle(pen, new Rectangle(0, 0, width, height));
            }

            graphics.ResetTransform();
        }

        private static void DrawBitmapEdgesToBitmap(Bitmap source, Bitmap destination, int offsetX, int offsetY, bool rotated,
            SpritePart spriteInfo, System.Drawing.Graphics graphics)
        {
            int width = (spriteInfo.MaxX - spriteInfo.MinX + 1);
            int height = (spriteInfo.MaxY - spriteInfo.MinY + 1);

            //left side
            {
                graphics.TranslateTransform(offsetX - 1, offsetY);

                if (rotated)
                {
                    graphics.ScaleTransform(-1.0f, 1.0f);
                    graphics.RotateTransform(90.0f);
                }

                if (source != null)
                {
                    graphics.DrawImage(source, new Rectangle(0, 0, 1, height), spriteInfo.MinX, spriteInfo.MinY, 1, height,
                        GraphicsUnit.Pixel);
                }
                else
                {
                    Pen pen = new Pen(Color.Red, 1.0f);
                    graphics.DrawRectangle(pen, new Rectangle(0, 0, 1, height));
                }

                graphics.ResetTransform();
            }

            //right side
            {
                graphics.TranslateTransform(offsetX + width, offsetY);

                if (rotated)
                {
                    graphics.ScaleTransform(-1.0f, 1.0f);
                    graphics.RotateTransform(90.0f);
                }

                if (source != null)
                {
                    graphics.DrawImage(source, new Rectangle(0, 0, 1, height), spriteInfo.MaxX, spriteInfo.MinY, 1, height,
                        GraphicsUnit.Pixel);
                }
                else
                {
                    Pen pen = new Pen(Color.Red, 1.0f);
                    graphics.DrawRectangle(pen, new Rectangle(0, 0, 1, height));
                }

                graphics.ResetTransform();
            }

            //top side
            {
                graphics.TranslateTransform(offsetX, offsetY - 1);

                if (rotated)
                {
                    graphics.ScaleTransform(-1.0f, 1.0f);
                    graphics.RotateTransform(90.0f);
                }

                if (source != null)
                {
                    graphics.DrawImage(source, new Rectangle(0, 0, width, 1), spriteInfo.MinX, spriteInfo.MinY, width, 1,
                        GraphicsUnit.Pixel);
                }
                else
                {
                    Pen pen = new Pen(Color.Red, 1.0f);
                    graphics.DrawRectangle(pen, new Rectangle(0, 0, width, 1));
                }

                graphics.ResetTransform();
            }

            //bottom side
            {
                graphics.TranslateTransform(offsetX, offsetY + height);

                if (rotated)
                {
                    graphics.ScaleTransform(-1.0f, 1.0f);
                    graphics.RotateTransform(90.0f);
                }

                if (source != null)
                {
                    graphics.DrawImage(source, new Rectangle(0, 0, width, 1), spriteInfo.MinX, spriteInfo.MaxY, width, 1,
                        GraphicsUnit.Pixel);
                }
                else
                {
                    Pen pen = new Pen(Color.Red, 1.0f);
                    graphics.DrawRectangle(pen, new Rectangle(0, 0, width, 1));
                }

                graphics.ResetTransform();
            }
        }

        private static void DrawBitmapCornersToBitmap(Bitmap source, Bitmap destination, int offsetX, int offsetY, bool rotated,
            SpritePart spriteInfo, System.Drawing.Graphics graphics)
        {
            int width = (spriteInfo.MaxX - spriteInfo.MinX + 1);
            int height = (spriteInfo.MaxY - spriteInfo.MinY + 1);

            //top left side
            {
                graphics.TranslateTransform(offsetX - 1, offsetY - 1);

                if (rotated)
                {
                    graphics.ScaleTransform(-1.0f, 1.0f);
                    graphics.RotateTransform(90.0f);
                }

                if (source != null)
                {
                    graphics.DrawImage(source, new Rectangle(0, 0, 1, 1), spriteInfo.MinX, spriteInfo.MinY, 1, 1,
                        GraphicsUnit.Pixel);
                }
                else
                {
                    Pen pen = new Pen(Color.Red, 1.0f);
                    graphics.DrawRectangle(pen, new Rectangle(0, 0, 1, 1));
                }

                graphics.ResetTransform();
            }

            //top right side
            {
                graphics.TranslateTransform(offsetX + width, offsetY - 1);

                if (rotated)
                {
                    graphics.ScaleTransform(-1.0f, 1.0f);
                    graphics.RotateTransform(90.0f);
                }

                if (source != null)
                {
                    graphics.DrawImage(source, new Rectangle(0, 0, 1, 1), spriteInfo.MaxX, spriteInfo.MinY, 1, 1,
                        GraphicsUnit.Pixel);
                }
                else
                {
                    Pen pen = new Pen(Color.Red, 1.0f);
                    graphics.DrawRectangle(pen, new Rectangle(0, 0, 1, 1));
                }

                graphics.ResetTransform();
            }

            //bottom left side
            {
                graphics.TranslateTransform(offsetX - 1, offsetY + height);

                if (rotated)
                {
                    graphics.ScaleTransform(-1.0f, 1.0f);
                    graphics.RotateTransform(90.0f);
                }

                if (source != null)
                {
                    graphics.DrawImage(source, new Rectangle(0, 0, 1, 1), spriteInfo.MinX, spriteInfo.MaxY, 1, 1,
                        GraphicsUnit.Pixel);
                }
                else
                {
                    Pen pen = new Pen(Color.Red, 1.0f);
                    graphics.DrawRectangle(pen, new Rectangle(0, 0, 1, 1));
                }

                graphics.ResetTransform();
            }

            //bottom right side
            {
                graphics.TranslateTransform(offsetX + width, offsetY + height);

                if (rotated)
                {
                    graphics.ScaleTransform(-1.0f, 1.0f);
                    graphics.RotateTransform(90.0f);
                }

                if (source != null)
                {
                    graphics.DrawImage(source, new Rectangle(0, 0, 1, 1), spriteInfo.MaxX, spriteInfo.MaxY, 1, 1,
                        GraphicsUnit.Pixel);
                }
                else
                {
                    Pen pen = new Pen(Color.Red, 1.0f);
                    graphics.DrawRectangle(pen, new Rectangle(0, 0, 1, 1));
                }

                graphics.ResetTransform();
            }
        }

        public int Width
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }

        public int ID
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

    }
}
