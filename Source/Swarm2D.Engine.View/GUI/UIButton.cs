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
using System.Reflection.Emit;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.Engine.View.GUI
{
    public class UIButton : UIFrame
    {
        protected UIButtonRenderState _renderState;

        private List<Sprite> _normalSprites;
        private List<Sprite> _mouseDownSprites;
        private List<Sprite> _mouseOverSprites;

        private UILabel _label;

        public override void Initialize(List<UIPositionParameter> positionParameters)
        {
            base.Initialize(positionParameters);

            _renderState = UIButtonRenderState.Normal;

            Widget.MouseEnter += new UIMouseEvent(OnMouseEnterHandler);
            Widget.MouseLeave += new UIMouseEvent(OnMouseLeaveHandler);
            Widget.MouseDown += new UIMouseEvent(OnMouseDownHandler);
            Widget.MouseUp += new UIMouseEvent(OnMouseUpHandler);

            _normalSprites = new List<Sprite>();
            _mouseDownSprites = new List<Sprite>();
            _mouseOverSprites = new List<Sprite>();

            Sprite normalSprite = Manager.Skin.ButtonNormalSprite;
            Sprite mouseDownSprite = Manager.Skin.ButtonMouseDownSprite;
            Sprite mouseOverSprite = Manager.Skin.ButtonMouseOverSprite;

            if (normalSprite != null)
            {
                _normalSprites.Add(normalSprite);
            }

            if (mouseDownSprite != null)
            {
                _mouseDownSprites.Add(mouseDownSprite);
            }

            if (mouseOverSprite != null)
            {
                _mouseOverSprites.Add(mouseOverSprite);
            }

            //_normalSprite = Engine.Project.GetSprite(@"GUI\blackButtonNormal");
            //_mouseDownSprite =  Engine.Project.GetSprite(@"GUI\blackButtonMouseDown");
            //_mouseOverSprite =  Engine.Project.GetSprite(@"GUI\blackButtonMouseOver");

            Entity labelEntity = CreateChildEntity("buttonLabel");
            _label = labelEntity.AddComponent<UILabel>();
            _label.Initialize(new List<UIPositionParameter>() { UIPositionParameter.FitTo(Widget) });
            _label.Text = "";

            Widget.PassEventsToChildren = false;
        }

        public void AddSprite(UIButtonRenderState state, Sprite sprite)
        {
            List<Sprite> selectedSprites = null;

            switch (state)
            {
                case UIButtonRenderState.Normal:
                    selectedSprites = _normalSprites;
                    break;
                case UIButtonRenderState.MouseDown:
                    selectedSprites = _mouseDownSprites;
                    break;
                case UIButtonRenderState.MouseOver:
                    selectedSprites = _mouseOverSprites;
                    break;
            }

            selectedSprites.Add(sprite);
        }

        private void OnMouseUpHandler(UIWidget sender, MouseEventArgs e)
        {
            _renderState = UIButtonRenderState.MouseOver;
        }

        private void OnMouseDownHandler(UIWidget sender, MouseEventArgs e)
        {
            _renderState = UIButtonRenderState.MouseDown;
        }

        private void OnMouseLeaveHandler(UIWidget sender, MouseEventArgs e)
        {
            _renderState = UIButtonRenderState.Normal;
        }

        private void OnMouseEnterHandler(UIWidget sender, MouseEventArgs e)
        {
            if (Manager.MouseDownObject == Widget)
            {
                _renderState = UIButtonRenderState.MouseDown;
            }
            else
            {
                _renderState = UIButtonRenderState.MouseOver;
            }
        }

        protected override void Render(RenderContext renderContext)
        {
            List<Sprite> currentSprites = null;

            switch (_renderState)
            {
                case UIButtonRenderState.Normal:
                    currentSprites = _normalSprites;
                    break;
                case UIButtonRenderState.MouseDown:
                    currentSprites = _mouseDownSprites;
                    break;
                case UIButtonRenderState.MouseOver:
                    currentSprites = _mouseOverSprites;
                    break;
            }

            foreach (Sprite currentSprite in currentSprites)
            {
                renderContext.AddDrawSpriteJob(X, Y, currentSprite, 1.0f, Width, Height);
            }
        }

        protected internal override void OnTextChange()
        {
            base.OnTextChange();

            if (_label != null)
            {
                _label.Text = Text;
            }
        }
    }

    public enum UIButtonRenderState
    {
        Normal = 0,
        MouseDown = 1,
        MouseOver = 2,
    }
}
