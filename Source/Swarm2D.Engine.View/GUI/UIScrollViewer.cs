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
    public class UIScrollViewer : UIFrame
    {
        private UIFrame _itemHolderFrame;
        private UIPositionParameter _holderFrameHeightParameter;
        private UIPositionParameter _holderFrameTopParameter;

        private UIFrame _moveUpButton;
        private UIFrame _moveDownButton;

        private UIFrame _scrollButton;
        private UIPositionParameter _scrollButtonHeightParameter;
        private UIPositionParameter _scrollButtonTopParameter;

        private int _moveValue;

        private bool _scrolling;
        private bool _scrollingWithScrollButton;

        private int _mouseDownScrollButtonY;
        private int _scrollDirection;

        public override void Initialize(List<UIPositionParameter> positionParameters)
        {
            base.Initialize(positionParameters);

            _scrolling = false;
            _scrollingWithScrollButton = false;
            _scrollDirection = 0;
            //_selectedObject = NULL;
            //_selectedIndex = -1;

            List<UIPositionParameter> itemHolderFrameParameters = new List<UIPositionParameter>();

            UIPositionParameter leftParameter;
            UIPositionParameter rightParameter;

            leftParameter = UIPositionParameter.AnchorToSideParameter(Widget, AnchorSide.Left, AnchorToSideType.Inner);
            rightParameter = UIPositionParameter.AnchorToSideParameter(Widget, AnchorSide.Right, AnchorToSideType.Inner, 30);

            _holderFrameTopParameter = UIPositionParameter.AnchorToSideParameter(Widget, AnchorSide.Top, AnchorToSideType.Inner);
            _holderFrameHeightParameter = UIPositionParameter.SetHeight(100);

            itemHolderFrameParameters.Add(_holderFrameTopParameter);
            itemHolderFrameParameters.Add(leftParameter);
            itemHolderFrameParameters.Add(rightParameter);
            itemHolderFrameParameters.Add(_holderFrameHeightParameter);

            Entity itemHolderFrameEntity = CreateChildEntity("itemHolderFrameEntity");
            _itemHolderFrame = itemHolderFrameEntity.AddComponent<UIFrame>();
            _itemHolderFrame.Initialize(itemHolderFrameParameters);
            ScissorTestOnRender = true;
            _itemHolderFrame.ScissorTestOnRender = true;

            _moveValue = 0;

            {
                List<UIPositionParameter> currentPositionParameters = new List<UIPositionParameter>();

                currentPositionParameters.Add(UIPositionParameter.AnchorToSideParameter(Widget, AnchorSide.Top, AnchorToSideType.Inner, 2));
                currentPositionParameters.Add(UIPositionParameter.AnchorToSideParameter(Widget, AnchorSide.Right, AnchorToSideType.Inner, 2));
                currentPositionParameters.Add(UIPositionParameter.SetWidth(26));
                currentPositionParameters.Add(UIPositionParameter.SetHeight(26));

                Entity moveUpButtonEntity = CreateChildEntity("moveUpButtonEntity");
                _moveUpButton = moveUpButtonEntity.AddComponent<UIButton>();
                _moveUpButton.Initialize(currentPositionParameters);

                _moveUpButton.Widget.MouseDown += new UIMouseEvent(OnMoveUpButtonDown);
                _moveUpButton.Widget.MouseUp += new UIMouseEvent(OnMoveUpButtonUp);
                _moveUpButton.Widget.MouseLeave += new UIMouseEvent(OnMoveUpButtonLeave);
            }

            {
                List<UIPositionParameter> currentPositionParameters = new List<UIPositionParameter>();

                currentPositionParameters.Add(UIPositionParameter.AnchorToSideParameter(Widget, AnchorSide.Bottom, AnchorToSideType.Inner, 2));
                currentPositionParameters.Add(UIPositionParameter.AnchorToSideParameter(Widget, AnchorSide.Right, AnchorToSideType.Inner, 2));
                currentPositionParameters.Add(UIPositionParameter.SetWidth(26));
                currentPositionParameters.Add(UIPositionParameter.SetHeight(26));

                Entity moveDownButtonEntity = Entity.CreateChildEntity("moveDownButtonEntity");
                _moveDownButton = moveDownButtonEntity.AddComponent<UIButton>();
                _moveDownButton.Initialize(currentPositionParameters);

                _moveDownButton.Widget.MouseDown += new UIMouseEvent(OnMoveDownButtonDown);
                _moveDownButton.Widget.MouseUp += new UIMouseEvent(OnMoveDownButtonUp);
                _moveDownButton.Widget.MouseLeave += new UIMouseEvent(OnMoveDownButtonLeave);
            }

            {
                List<UIPositionParameter> currentPositionParameters = new List<UIPositionParameter>();

                _scrollButtonTopParameter = UIPositionParameter.AnchorToSideParameter(Widget, AnchorSide.Top, AnchorToSideType.Inner, 30);
                _scrollButtonHeightParameter = UIPositionParameter.SetHeight(26);

                currentPositionParameters.Add(_scrollButtonTopParameter);
                currentPositionParameters.Add(UIPositionParameter.AnchorToSideParameter(Widget, AnchorSide.Right, AnchorToSideType.Inner, 2));
                currentPositionParameters.Add(UIPositionParameter.SetWidth(26));
                currentPositionParameters.Add(_scrollButtonHeightParameter);

                Entity scrollButtonEntity = Entity.CreateChildEntity("scrollButtonEntity");
                _scrollButton = scrollButtonEntity.AddComponent<UIButton>();
                _scrollButton.Initialize(currentPositionParameters);

                _scrollButton.Widget.MouseDown += new UIMouseEvent(OnScrollButtonDown);
                _scrollButton.Widget.MouseUp += new UIMouseEvent(OnScrollButtonUp);
                _scrollButton.Widget.MouseLeave += new UIMouseEvent(OnScrollButtonLeave);

            }

            HolderFrame = _itemHolderFrame;
            _itemHolderFrame.ShowBox = false;
        }

        protected virtual int CalculateScrollViewerHeight()
        {
            float highestHeight = 0;

            foreach (UIWidget widget in LogicalChildren)
            {
                float height = widget.Y + widget.Height;

                if (height > highestHeight)
                {
                    highestHeight = height;
                }
            }
            return (int)(highestHeight - _holderFrameTopParameter.Value);
        }

        protected internal override void ObjectUpdate()
        {
            base.ObjectUpdate();

            float height = Height;
            //float itemHolderFrameHeight = CalculateListBoxHeight();//  _itemHolderFrame.Height();
            float itemHolderFrameHeight = CalculateScrollViewerHeight();

            int maxScrollValue = (int)(itemHolderFrameHeight - height);

            int scrollHeight = 0;

            int maxScrollButtonHeight = (int)(_moveDownButton.Y - (_moveUpButton.Y + _moveUpButton.Height) - 4);

            float scrollButtonRatio = height / itemHolderFrameHeight;

            if (maxScrollValue <= 0)
            {
                scrollHeight = maxScrollButtonHeight;
            }
            else
            {
                scrollHeight = (int)((float)maxScrollButtonHeight * scrollButtonRatio);
            }

            if (scrollHeight < 4)
            {
                scrollHeight = 4;
            }

            if (_scrollingWithScrollButton)
            {
                int mouseDownNewY = (int)Manager.PointerPosition.Y;
                int difference = mouseDownNewY - _mouseDownScrollButtonY;
                _mouseDownScrollButtonY = mouseDownNewY;

                _moveValue += difference;
            }
            else if (_scrolling)
            {
                _moveValue += _scrollDirection;
            }

            float maxMoveValue = ((float)maxScrollButtonHeight - (float)scrollHeight);

            if (_moveValue >= maxMoveValue)
            {
                _moveValue = (int)maxMoveValue;
            }

            if (_moveValue < 0)
            {
                _moveValue = 0;
            }

            float moveRatio = (float)_moveValue / (float)maxMoveValue;

            if (maxMoveValue == 0.0f)
            {
                moveRatio = 0.0f;
            }

            if (_scrollButtonHeightParameter.Value != scrollHeight)
            {
                _scrollButtonHeightParameter.Value = scrollHeight;
                _scrollButton.SetNonUpdatedWithChildrenOnAllDomains();
            }

            if (_scrollButtonTopParameter.Value != _moveValue + 30)
            {
                _scrollButtonTopParameter.Value = _moveValue + 30;
                _scrollButton.SetNonUpdatedWithChildrenOnAllDomains();
            }

            if (_holderFrameTopParameter.Value != -maxScrollValue * moveRatio)
            {
                _holderFrameTopParameter.Value = -maxScrollValue * moveRatio;
                _itemHolderFrame.SetNonUpdatedWithChildrenOnAllDomains();
            }

            if (itemHolderFrameHeight != _holderFrameHeightParameter.Value)
            {
                _holderFrameHeightParameter.Value = itemHolderFrameHeight;
                _itemHolderFrame.SetNonUpdatedWithChildrenOnAllDomains();
            }
        }

        private void OnMoveUpButtonDown(UIWidget sender, MouseEventArgs e)
        {
            _scrolling = true;
            _scrollDirection = -1;
        }

        private void OnMoveUpButtonUp(UIWidget sender, MouseEventArgs e)
        {
            _scrolling = false;
            _scrollDirection = 0;
        }

        private void OnMoveUpButtonLeave(UIWidget sender, MouseEventArgs e)
        {
            _scrolling = false;
            _scrollDirection = 0;
        }

        private void OnMoveDownButtonDown(UIWidget sender, MouseEventArgs e)
        {
            _scrolling = true;
            _scrollDirection = 1;
        }

        private void OnMoveDownButtonUp(UIWidget sender, MouseEventArgs e)
        {
            _scrolling = false;
            _scrollDirection = 0;
        }

        private void OnMoveDownButtonLeave(UIWidget sender, MouseEventArgs e)
        {
            _scrolling = false;
            _scrollDirection = 0;
        }

        private void OnScrollButtonDown(UIWidget sender, MouseEventArgs e)
        {
            _mouseDownScrollButtonY = (int)e.Y;
            _scrollingWithScrollButton = true;
        }

        private void OnScrollButtonUp(UIWidget sender, MouseEventArgs e)
        {
            _scrollingWithScrollButton = false;
        }

        private void OnScrollButtonLeave(UIWidget sender, MouseEventArgs e)
        {
            _scrollingWithScrollButton = false;
        }
    }
}
