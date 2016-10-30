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

namespace Swarm2D.Engine.View.GUI
{
    public class UITextureBox : UIFrame
    {
        public Texture Texture
        {
            get;
            set;
        }

        private Mesh _mesh;

        public UITextureBox()
        {
            _mesh = new Mesh(MeshTopology.Quads, 4);

            _mesh.TextureCoordinates[0] = 0;
            _mesh.TextureCoordinates[1] = 0;
            _mesh.TextureCoordinates[2] = 0;
            _mesh.TextureCoordinates[3] = 1;
            _mesh.TextureCoordinates[4] = 1;
            _mesh.TextureCoordinates[5] = 1;
            _mesh.TextureCoordinates[6] = 1;
            _mesh.TextureCoordinates[7] = 0;
        }

        protected override void Render(RenderContext renderContext)
        {
            if (Texture != null)
            {
                _mesh.Vertices[0] = X;
                _mesh.Vertices[1] = Y;

                _mesh.Vertices[2] = X + Texture.Width;
                _mesh.Vertices[3] = Y;

                _mesh.Vertices[4] = X + Texture.Width;
                _mesh.Vertices[5] = Y + Texture.Height;

                _mesh.Vertices[6] = X;
                _mesh.Vertices[7] = Y + Texture.Height;

                renderContext.AddDrawMeshJob(0, 0, _mesh, new SimpleMaterial(Texture));

            }
            else
            {
                base.Render(renderContext);
            }
        }
    }
}
