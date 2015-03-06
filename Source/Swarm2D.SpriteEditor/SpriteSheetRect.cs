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

namespace Swarm2D.SpriteEditor
{
    class SpriteSheetRect
    {
        public SpriteSheetRect(SpriteSheet spriteSheet, int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            SpriteSheet = spriteSheet;
            Rotated = false;
        }

        public bool IsCollide(SpriteSheetRect other)
        {
            int minX = X;
            int maxX = other.X2;
            int minY = Y;
            int maxY = other.Y2;

            if (X > other.X) minX = other.X;
            if (X2 > other.X2) maxX = X2;
            if (Y > other.Y) minY = other.Y;
            if (Y2 > other.Y2) maxY = Y2;

            return ((maxX - minX) < (Width + other.Width)) && ((maxY - minY) < (Height + other.Height));
        }

        public bool IsSubRectOf(SpriteSheetRect other)
        {
            return (other.X <= X) && (other.X2 >= X2) && (other.Y <= Y) && (other.Y2 >= Y2);
        }

        public bool IsValid()
        {
            return Width > 0 && Height > 0;
        }

        public SpriteSheet SpriteSheet
        {
            get;
            private set;
        }

        public int X
        {
            get;
            private set;
        }

        public int Y
        {
            get;
            private set;
        }

        public int X2
        {
            get
            {
                return X + Width;
            }
        }

        public int Y2
        {
            get
            {
                return Y + Height;
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

        public bool Rotated
        {
            get;
            set;
        }
    }
}
