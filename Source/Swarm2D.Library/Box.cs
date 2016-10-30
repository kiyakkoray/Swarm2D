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

namespace Swarm2D.Library
{
    public struct Box
    {
        public Vector2 Position;
        public Vector2 Size;

        public bool IsIntersects(Box box)
        {
            bool xIntersects = false;

            xIntersects = Position.X >= box.Position.X && Position.X <= box.Position.X + box.Size.X;

            if (!xIntersects)
            {
                xIntersects = box.Position.X >= Position.X && box.Position.X <= Position.X + Size.X;
            }

            bool intersects = false;

            if (xIntersects)
            {
                intersects = Position.Y >= box.Position.Y && Position.Y <= box.Position.Y + box.Size.Y;

                if (!intersects)
                {
                    intersects = box.Position.Y >= Position.Y && box.Position.Y <= Position.Y + Size.Y;
                }
            }

            return intersects;
        }

        public Box GetIntersection(Box otherBox)
        {
            Vector2 ap1 = Position;
            Vector2 ap2 = Position + Size;

            Vector2 bp1 = otherBox.Position;
            Vector2 bp2 = otherBox.Position + otherBox.Size;

            Vector2 rp1 = ap1;
            Vector2 rp2 = ap2;

            if (ap1.X < bp1.X)
            {
                rp1.X = bp1.X;
            }

            if (ap2.X > bp2.X)
            {
                rp2.X = bp2.X;
            }

            if (ap1.Y < bp1.Y)
            {
                rp1.Y = bp1.Y;
            }

            if (ap2.Y > bp2.Y)
            {
                rp2.Y = bp2.Y;
            }

            Box box = new Box();

            box.Position = rp1;
            box.Size = rp2 - rp1;

            return box;
        }
    }
}
