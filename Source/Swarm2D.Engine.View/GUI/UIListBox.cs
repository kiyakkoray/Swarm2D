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
    public class UIListBox : UIScrollViewer
    {
        public event ListBoxEvent ItemSelect;
        public event ListBoxEvent ItemMouseRightClick;

        public List<Object> ListBoxItems { get; private set; }
        public List<string> ListBoxItemNames { get; private set; }

        private List<UILabel> _listBoxLabels;

        private Object _selectedObject;
        private int _selectedIndex;

        public override void Initialize(List<UIPositionParameter> positionParameters)
        {
            base.Initialize(positionParameters);

            ListBoxItemNames = new List<string>();
            ListBoxItems = new List<object>();
            _listBoxLabels = new List<UILabel>();

            _selectedObject = null;
            _selectedIndex = -1;
        }

        public void AddItem(string name, Object data)
        {
            List<UIPositionParameter> itemFrameParameters = FastGUI.GenerateStandardParameters(Widget.HolderFrame, 5, 5 + ListBoxItems.Count * 22, Width - 40, 20);
            //SxBase::List<Sx2d::GUI::UIPositionParameter*>* itemFrameParameters = NULL;

            Entity itemLabelEntity = CreateChildEntity("itemLabelEntity");
            UILabel itemLabel = itemLabelEntity.AddComponent<UILabel>();
            itemLabel.Initialize(itemFrameParameters);

            itemLabel.DataObject = data;
            itemLabel.Name = name;
            itemLabel.Text = name;

            _listBoxLabels.Add(itemLabel);

            ListBoxItemNames.Add(name);
            ListBoxItems.Add(data);

            itemLabel.Widget.MouseClick += new UIMouseEvent(OnItemClick);
            itemLabel.Widget.MouseRightClick += new UIMouseEvent(OnItemRightClick);

            //GetScrollViewerHolderFrameBottomParameter()->ChangeAnchorToParameter(itemLabel);
            //GetScrollViewerHolderFrame()->SetNonUpdatedWithChildrenOnAllDomains();
            SetNonUpdatedWithChildrenOnAllDomains();
        }

        public void ClearItems()
        {
            _selectedIndex = -1;
            _selectedObject = null;
            ListBoxItems.Clear();
            ListBoxItemNames.Clear();

            while (Widget.LogicalChildren.Count > 0)
            {
                UIWidget itemWidget = LogicalChildren[0];
                itemWidget.Entity.Destroy();
            }
        }

        public Object GetSelectedObject()
        {
            return _selectedObject;
        }

        public int GetSelectedIndex()
        {
            return _selectedIndex;
        }

        public void SetSelectedObject(object o)
        {
            if (_selectedIndex != -1)
            {
                _listBoxLabels[_selectedIndex].ShowBox = false;
            }

            if (ListBoxItems.Contains(o))
            {
                _selectedObject = o;
                _selectedIndex = ListBoxItems.IndexOf(o);
                _listBoxLabels[_selectedIndex].ShowBox = true;
            }
            else
            {
                _selectedObject = null;
                _selectedIndex = -1;
            }
        }

        public void SynchronizeWithList<T>(List<T> sourceList)
        {
            bool anythingChanged = false;

            //first delete non exist items
            for (int i = ListBoxItems.Count - 1; i >= 0; i--)
            {
                Object listBoxItem = ListBoxItems[i];

                if (!sourceList.Contains((T)listBoxItem))
                {
                    if (!anythingChanged)
                    {
                        //GetScrollViewerHolderFrameBottomParameter()->ChangeAnchorToParameter(this);
                        //GetScrollViewerHolderFrame()->SetNonUpdatedWithChildrenOnAllDomains();
                        anythingChanged = true;
                    }

                    UIFrame itemFrame = _listBoxLabels[i];
                    itemFrame.Entity.Destroy();

                    ListBoxItems.RemoveAt(i);
                    ListBoxItemNames.RemoveAt(i);

                    _listBoxLabels.RemoveAt(i);
                }
            }
            //_holderFrameBottomParameter->ChangeAnchorToParameter(this);

            //add new items.
            for (int i = 0; i < sourceList.Count /*&& i < 16 && ListBoxItems.Count < 16*/; i++)
            {
                Object newObject = sourceList[i];
                string objectName = newObject.ToString();

                if (!ListBoxItems.Contains(newObject))
                {
                    if (!anythingChanged)
                    {
                        //GetScrollViewerHolderFrameBottomParameter()->ChangeAnchorToParameter(this);
                        //GetScrollViewerHolderFrame()->SetNonUpdatedWithChildrenOnAllDomains();
                        anythingChanged = true;
                    }

                    AddItem(objectName, newObject);
                }
            }

            if (anythingChanged)
            {
                RepositionItems();
            }
        }

        public void RepositionItems()
        {
            //GetScrollViewerHolderFrameBottomParameter()->ChangeAnchorToParameter(this);
            //GetScrollViewerHolderFrame()->SetNonUpdatedWithChildrenOnAllDomains();
            SetNonUpdatedWithChildrenOnAllDomains();

            for (int i = 0; i < _listBoxLabels.Count; i++)
            {
                UIFrame itemFrame = _listBoxLabels[i];
                itemFrame.SetNonUpdatedWithChildrenOnAllDomains();

                UIPositionParameter positionParameter = itemFrame.Widget.PositionParameters[0];
                positionParameter.Value = 5 + i * 22;

                if (i + 1 == _listBoxLabels.Count())
                {
                    //GetScrollViewerHolderFrameBottomParameter()->ChangeAnchorToParameter(itemFrame);
                    //GetScrollViewerHolderFrame()->SetNonUpdatedWithChildrenOnAllDomains();
                }
            }
        }

        protected override int CalculateScrollViewerHeight()
        {
            return ListBoxItems.Count * 22 + 5;
        }

        private void OnItemClick(UIWidget sender, MouseEventArgs e)
        {
            SetSelectedObject(sender.DataObject);

            if (ItemSelect != null)
            {
                ItemSelect(this, e);
            }
        }

        private void OnItemRightClick(UIWidget sender, MouseEventArgs e)
        {
            SetSelectedObject(sender.DataObject);

            if (ItemMouseRightClick != null)
            {
                ItemMouseRightClick(this, e);
            }
        }
    }

    public delegate void ListBoxEvent(UIListBox listBox, MouseEventArgs e);
}
