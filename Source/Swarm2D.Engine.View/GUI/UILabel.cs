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
    public class UILabel : UIFrame
    {
        private TextMesh _textMesh;

        protected Font LabelFont;

        public override void Initialize(List<UIPositionParameter> positionParameters)
        {
            base.Initialize(positionParameters);

            Text = "Test UILabel";

            LabelFont = Manager.Font;

            _textMesh = new TextMesh(LabelFont);
            ShowBox = false;
        }

        public float FontHeight
        {
            get
            {
                return _textMesh.FontHeight;
            }
            set
            {
                _textMesh.FontHeight = value;
            }
        }

        protected override void Render(RenderContext renderContext)
        {
            base.Render(renderContext);

            _textMesh.Update(Width, Height, Text);
            renderContext.AddDrawMeshJob(X, Y, _textMesh, new SimpleMaterial(LabelFont.FontTexture));
        }
    }
}
