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
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.Engine.View.GUI
{
    public class UIFrame : Component
    {
        #region Widget Events

        public event UIMouseEvent MouseDown
        {
            add
            {
                Widget.MouseDown += value;
            }
            remove
            {
                Widget.MouseDown -= value;
            }
        }

        public event UIMouseEvent MouseUp
        {
            add
            {
                Widget.MouseUp += value;
            }
            remove
            {
                Widget.MouseUp -= value;
            }
        }

        public event UIMouseEvent MouseRightDown
        {
            add
            {
                Widget.MouseRightDown += value;
            }
            remove
            {
                Widget.MouseRightDown -= value;
            }
        }

        public event UIMouseEvent MouseRightUp
        {
            add
            {
                Widget.MouseRightUp += value;
            }
            remove
            {
                Widget.MouseRightUp -= value;
            }
        }

        public event UIMouseEvent MouseClick
        {
            add
            {
                Widget.MouseClick += value;
            }
            remove
            {
                Widget.MouseClick -= value;
            }
        }

        public event UIMouseEvent MouseRightClick
        {
            add
            {
                Widget.MouseRightClick += value;
            }
            remove
            {
                Widget.MouseRightClick -= value;
            }
        }

        public event UIMouseEvent MouseEnter
        {
            add
            {
                Widget.MouseEnter += value;
            }
            remove
            {
                Widget.MouseEnter -= value;
            }
        }

        public event UIMouseEvent MouseLeave
        {
            add
            {
                Widget.MouseLeave += value;
            }
            remove
            {
                Widget.MouseLeave -= value;
            }
        }

        public event UIEvent LostFocus
        {
            add
            {
                Widget.LostFocus += value;
            }
            remove
            {
                Widget.LostFocus -= value;
            }
        }

        #endregion

        #region Widget Properties

        public UIFrame HolderFrame
        {
            get
            {
                return Widget.HolderFrame.GetComponent<UIFrame>();
            }
            set
            {
                Widget.HolderFrame = value.Widget;
            }
        }

        public object DataObject
        {
            get
            {
                return Widget.DataObject;
            }
            set
            {
                Widget.DataObject = value;
            }
        }

        public float X
        {
            get
            {
                return Widget.X;
            }
        }

        public float Y
        {
            get
            {
                return Widget.Y;
            }
        }

        public float Width
        {
            get
            {
                return Widget.Width;
            }
        }

        public float Height
        {
            get
            {
                return Widget.Height;
            }
        }

        public string Name
        {
            get
            {
                return Widget.Name;
            }
            set
            {
                Widget.Name = value;
            }
        }

        public UIRegion DefaultRegion
        {
            get
            {
                return Widget.DefaultRegion;
            }
        }

        public UIRegion CurrentRegion
        {
            get
            {
                return Widget.CurrentRegion;
            }
        }

        public UIFrame Owner
        {
            get
            {
                return Widget.Owner.GetComponent<UIFrame>();
            }
        }

        public List<UIWidget> Children
        {
            get
            {
                return Widget.Children;
            }
        }

        public List<UIWidget> LogicalChildren
        {
            get
            {
                return Widget.LogicalChildren;
            }
        }

        public bool Enabled
        {
            get
            {
                return Widget.Enabled;
            }
            set
            {
                Widget.Enabled = value;
            }
        }

        public bool PassEventsToChildren
        {
            get
            {
                return Widget.PassEventsToChildren;
            }
            set
            {
                Widget.PassEventsToChildren = value;
            }
        }

        #endregion

        public event UIRenderEvent AfterRender;

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnTextChange();
                }
            }
        }

        private bool _showBox = true;

        public bool ShowBox
        {
            get
            {
                return (Manager.ShowAllBoxes || _showBox) && UseParentShowDebug;
            }
            set
            {
                _showBox = value;
            }
        }

        public float Alpha { get; set; }

        public bool UseParentShowDebug { get; set; }

        public IIOSystem IOSystem
        {
            get
            {
                return Manager.IOSystem;
            }
        }

        public UIManager Manager { get { return Widget.Manager; } }

        public bool ScissorTestOnRender { get; set; }

        public UIWidget Widget { get; private set; }

        protected override void OnAdded()
        {
            base.OnAdded();

            Widget = GetComponent<UIWidget>();

            Alpha = 1.0f;
            _showBox = true;
            UseParentShowDebug = true;
            ScissorTestOnRender = false;

            Text = "";
            ShowBox = true;
        }

        public virtual void Initialize(List<UIPositionParameter> positionParameters)
        {
            Widget.Initialize(positionParameters);
        }

        internal void Update(List<UIUpdateMethod> updateMethods)
        {
            updateMethods.Add(ObjectUpdate);

            for (int i = 0; i < Widget.Children.Count; i++)
            {
                UIWidget widget = Widget.Children[i];
                UIFrame frame = widget.GetComponent<UIFrame>();

                if (frame != null && widget.Enabled)
                {
                    frame.Update(updateMethods);
                }
            }
        }

        protected internal virtual void OnTextChange()
        {

        }

        protected internal virtual void OnAfterRender(RenderContext renderContext)
        {
            if (AfterRender != null)
            {
                AfterRender(this, renderContext);
            }
        }

        internal void RenderController(RenderContext renderContext)
        {
            if (ScissorTestOnRender)
            {
                renderContext.AddGraphicsCommand(new CommandPushScissor((int)Widget.X, (int)Widget.Y, (int)Widget.Width, (int)Widget.Height));
                //graphicsContext.PushScissor((int)X, (int)Y, (int)Width, (int)Height);
            }

            Render(renderContext);

            for (int i = Widget.Children.Count - 1; i >= 0; i--)
            {
                UIWidget child = Widget.Children[i];

                if (child.Enabled)
                {
                    UIFrame frame = child.GetComponent<UIFrame>();

                    if (frame != null)
                    {
                        frame.RenderController(renderContext);
                    }
                }
            }

            if (ScissorTestOnRender)
            {
                renderContext.AddGraphicsCommand(new CommandPopScissor());
                //graphicsContext.PopScissor();
            }
        }

        protected virtual void Render(RenderContext renderContext)
        {
            if (ShowBox)
            {
                Sprite sprite = Manager.Skin.FrameBoxSprite;

                if (sprite != null)
                {
                    int x = (int)Widget.X;
                    int y = (int)Widget.Y;
                    int width = (int)Widget.Width;
                    int height = (int)Widget.Height;

                    //ioSystem.AddGraphicsCommand(new CommandDrawSprite(Math.Floor(X), Math.Floor(Y), sprite, false, false, 1.0f, false, 0.0f, Math.Floor(Width), Math.Floor(Height)));
                    renderContext.AddGraphicsCommand(new CommandDrawSprite(x, y, sprite, false, false, 1.0f, false, 0.0f, width, height));
                    //Graphics2D.DrawSprite(X, Y, sprite, false, false, 1.0f, false, 0.0f, Width, Height);
                }
            }

            OnAfterRender(renderContext);
        }

        protected internal virtual void ObjectUpdate()
        {

        }

        public UIFrame GetChildAtPath(string path)
        {
            UIWidget widget = Widget.GetChildAtPath(path);

            if (widget != null)
            {
                return widget.GetComponent<UIFrame>();
            }

            return null;
        }

        #region Widget Methods

        public void SetNonUpdatedWithChildrenOnAllDomains()
        {
            Widget.SetNonUpdatedWithChildrenOnAllDomains();
        }

        public void BringToFront()
        {
            Widget.BringToFront();
        }

        public void BringToBack()
        {
            Widget.BringToBack();
        }

        public void AddPositionParameter(UIPositionParameter positionParameter)
        {
            Widget.AddPositionParameter(positionParameter);
        }

        #endregion
    }

    public delegate void UIFrameEvent(UIFrame frame);
    public delegate void UIRenderEvent(UIFrame frame, RenderContext renderContext);
}
