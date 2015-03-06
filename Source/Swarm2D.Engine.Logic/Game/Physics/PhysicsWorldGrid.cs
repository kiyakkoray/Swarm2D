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

namespace Swarm2D.Engine.Logic
{
    internal class PhysicsWorldGrid
    {
        internal float Length { get; private set; }
        internal int Size { get; private set; }

        private PhysicsWorldGridCell[,] _grid;

        private PhysicsWorldGridCell _outterGridCell;

        internal PhysicsWorldGrid(int size, float length)
        {
            Size = size;
            Length = length;

            _grid = new PhysicsWorldGridCell[Size, Size];

            _outterGridCell = new PhysicsWorldGridCell();

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _grid[i, j] = new PhysicsWorldGridCell(i, j);
                }
            }
        }

        internal void UpdatePhysicsObject(IList<PhysicsWorldGridCell> oldCells, IList<PhysicsWorldGridCell> newCells, PhysicsObject physicsObject)
        {
            for (int i = 0; i < oldCells.Count; i++)
            {
                oldCells[i].RemovePhysicsObject(physicsObject);
            }

            for (int i = 0; i < newCells.Count; i++)
            {
                newCells[i].AddPhysicsObject(physicsObject);
            }
        }

        internal void Reset()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _grid[i, j].Reset();
                }
            }
        }

        internal PhysicsWorldGridCell this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= Size || y < 0 || y >= Size)
                {
                    return _outterGridCell;
                }

                return _grid[x, y];
            }
        }
    }
}
