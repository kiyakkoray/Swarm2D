﻿/******************************************************************************
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
using Swarm2D.Engine.Core;
using Swarm2D.Library;

namespace Swarm2D.Engine.Logic
{
    public class BoxShapeFilter : ShapeFilter, IPolygon
    {
        [ComponentProperty]
        public float Width { get; set; }

        [ComponentProperty]
        public float Height { get; set; }

        private bool _initialized = false;

        private List<Vector2> _vertices;

        List<Vector2> IPolygon.Vertices
        {
            get
            {
                if (!_initialized)
                {
                    _vertices = new List<Vector2>();

                    _vertices.Add(new Vector2(+Width * 0.5f, +Height * 0.5f));
                    _vertices.Add(new Vector2(+Width * 0.5f, -Height * 0.5f));
                    _vertices.Add(new Vector2(-Width * 0.5f, -Height * 0.5f));
                    _vertices.Add(new Vector2(-Width * 0.5f, +Height * 0.5f));

                    _initialized = true;
                }

                return _vertices;
            }
        }

        public override ShapeInstance CreateShapeInstance()
        {
            PolygonInstance polygonInstance = new PolygonInstance(this);
            return polygonInstance;
        }
    }
}