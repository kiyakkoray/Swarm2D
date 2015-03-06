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
using Swarm2D.Engine.View.GUI.PositionParameters;
using Swarm2D.Library;

namespace Swarm2D.Engine.View.GUI
{
    public class UIContextMenu : UIFrame
    {
        private UIListBox _listBox;

        private int _itemCount;

        private SetWidth _widthParameter;
        private SetHeight _heightParameter;

        private Dictionary<UIWidget, ContextMenuItem> _items;

        public event UIContextMenuEvent ItemClick;

        public override void Initialize(List<UIPositionParameter> positionParameters)
        {
            base.Initialize(AddContextMenuPositionParameters(positionParameters));

            _widthParameter = positionParameters[positionParameters.Count - 2] as SetWidth;
            _heightParameter = positionParameters[positionParameters.Count - 1] as SetHeight;

            _itemCount = 0;

            _items = new Dictionary<UIWidget, ContextMenuItem>();

            Entity listBoxEntity = CreateChildEntity("ListBox");
            _listBox = listBoxEntity.AddComponent<UIListBox>();
            _listBox.Initialize(new List<UIPositionParameter>() { UIPositionParameter.FitTo(Widget) });

            _listBox.ItemSelect += OnListBoxItemSelect;
        }

        private float MenuHeight
        {
            get
            {
                return Mathf.Clamp(LastItemBottomY + 8.0f, 0, 400.0f);
            }
        }

        private float LastItemBottomY
        {
            get
            {
                float result = 5.0f;
                result += _itemCount * 20.0f;
                //result += _itemCount > 1 ? (_itemCount - 1) * 2.0f : 0.0f;
                result += _itemCount * 2.0f;

                return result;
            }
        }

        public ContextMenuItem AddItem(string text)
        {
            ContextMenuItem contextMenuItem = new ContextMenuItem();
            contextMenuItem.Text = text;

            _listBox.AddItem(text, contextMenuItem);
            _itemCount++;

            _heightParameter.Value = MenuHeight;
            SetNonUpdatedWithChildrenOnAllDomains();

            return contextMenuItem;
        }

        private void OnListBoxItemSelect(UIListBox listBox, MouseEventArgs e)
        {
            ContextMenuItem item = _listBox.GetSelectedObject() as ContextMenuItem;

            if (ItemClick != null)
            {
                ItemClick.Invoke(this, item);
            }

            if (item.Click != null)
            {
                item.Click.Invoke(item);
            }

            Widget.Enabled = false;
        }

        public void ClearItems()
        {
            _listBox.ClearItems();
            _items.Clear();
            _itemCount = 0;

            _heightParameter.Value = MenuHeight;
            SetNonUpdatedWithChildrenOnAllDomains();
        }

        private static List<UIPositionParameter> AddContextMenuPositionParameters(List<UIPositionParameter> positionParameters)
        {
            StayInOwner stayInOwner = UIPositionParameter.StayInOwner();
            SetWidth widthParameter = UIPositionParameter.SetWidth(250.0f);
            SetHeight heightParameter = UIPositionParameter.SetHeight(10.0f);

            positionParameters.Add(stayInOwner);
            positionParameters.Add(widthParameter);
            positionParameters.Add(heightParameter);

            return positionParameters;
        }
    }

    public delegate void UIContextMenuEvent(UIFrame frame, ContextMenuItem item);
    public delegate void ContextMenuItemEvent(ContextMenuItem item);

    public class ContextMenuItem
    {
        internal UILabel Label { get; set; }
        public string Text { get; set; }
        public object Data { get; set; }

        public ContextMenuItemEvent Click;
    }
}