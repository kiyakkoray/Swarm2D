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
    public class Grid2D<T> where T : IGrid2DItem
    {
        public float Length = 64.0f;
        public int Size = 512;

        private Grid2DCell<T>[,] _grid;
        private Grid2DCell<T> _outterGridCell;

        internal Grid2D()
        {
            _grid = new Grid2DCell<T>[Size, Size];

            _outterGridCell = new Grid2DCell<T>();

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _grid[i, j] = new Grid2DCell<T>(i, j);
                }
            }
        }

        internal void UpdateItem(IList<Grid2DCell<T>> oldCells, IList<Grid2DCell<T>> newCells, T item)
        {
            for (int i = 0; i < oldCells.Count; i++)
            {
                oldCells[i].RemoveItem(item);
            }

            for (int i = 0; i < newCells.Count; i++)
            {
                newCells[i].AddItem(item);
            }
        }

        internal void Clear()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _grid[i, j].Clear();
                }
            }
        }

        internal Grid2DCell<T> this[int x, int y]
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

    public class Grid2DCell<T> where T : IGrid2DItem
    {
        internal bool Outter { get; private set; }
        internal int X { get; private set; }
        internal int Y { get; private set; }

        internal List<T> Items { get; private set; }

        internal Grid2DCell(int x, int y)
        {
            X = x;
            Y = y;

            Outter = false;
            Items = new List<T>(32);
        }

        internal Grid2DCell()
        {
            Outter = true;
            Items = new List<T>(32);
        }

        internal void AddItem(T item)
        {
            Items.Add(item);
        }

        internal void RemoveItem(T item)
        {
            Items.Remove(item);
        }

        internal void Clear()
        {
            Items.Clear();
        }
    }

    public interface IGrid2DItem
    {

    }
}
