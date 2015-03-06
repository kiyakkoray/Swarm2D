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

namespace Swarm2D.Engine.View.GUI
{
    public class UIComboBox : UIFrame
    {
        private UILabel _label;
        private UIContextMenu _contextMenu;

        private List<ContextMenuItem> _items;

        private int _selectedIndex = -1;

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    _label.Text = _items[_selectedIndex].Text;

                    if (SelectedItemChanged != null)
                    {
                        SelectedItemChanged(this);
                    }
                }
            }
        }

        public string SelectedItem
        {
            get
            {
                return _items[_selectedIndex].Text;
            }
        }

        public override void Initialize(List<UIPositionParameter> positionParameters)
        {
            base.Initialize(positionParameters);

            Entity labelEntity = CreateChildEntity("labelEntity");
            _label = labelEntity.AddComponent<UILabel>();
            _label.Initialize(new List<UIPositionParameter>() { UIPositionParameter.FitTo(Widget) });

            _label.Widget.MouseDown += new UIMouseEvent(OnMouseDownHandler);
            _label.Widget.MouseUp += new UIMouseEvent(OnMouseUpHandler);

            List<UIPositionParameter> contextMenuPositionParameters = new List<UIPositionParameter>();

            contextMenuPositionParameters.Add(UIPositionParameter.AnchorToSideParameter(Widget, AnchorSide.Left, AnchorToSideType.Inner));
            contextMenuPositionParameters.Add(UIPositionParameter.AnchorToSideParameter(Widget, AnchorSide.Bottom, AnchorToSideType.Outer));

            Entity contextMenuEntity = Manager.RootObject.CreateChildEntity("contextMenu");
            _contextMenu = contextMenuEntity.AddComponent<UIContextMenu>();
            _contextMenu.Initialize(contextMenuPositionParameters);
            _contextMenu.ItemClick += new UIContextMenuEvent(OnContextMenuItemClick);
            _contextMenu.Enabled = false;

            _label.Text = "";

            _items = new List<ContextMenuItem>();
        }

        private void OnContextMenuItemClick(UIFrame frame, ContextMenuItem item)
        {
            SelectedIndex = _items.IndexOf(item);
            _contextMenu.Enabled = false;
        }

        public void AddItem(string text)
        {
            ContextMenuItem item = _contextMenu.AddItem(text);

            _items.Add(item);

            if (_items.Count == 1)
            {
                _selectedIndex = 0;
                _label.Text = item.Text;
            }
        }

        private void OnMouseUpHandler(UIWidget sender, MouseEventArgs e)
        {
        }

        private void OnMouseDownHandler(UIWidget sender, MouseEventArgs e)
        {
            if (_contextMenu.Enabled)
            {
                _contextMenu.Enabled = false;
            }
            else
            {
                _contextMenu.Enabled = true;
                _contextMenu.BringToFront();
            }
        }

        public event UIFrameEvent SelectedItemChanged;
    }
}
