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
    public class UIWidget : Component
    {
        #region Events

        public event UIMouseEvent MouseDown;
        public event UIMouseEvent MouseUp;
        public event UIMouseEvent MouseRightDown;
        public event UIMouseEvent MouseRightUp;
        public event UIMouseEvent MouseClick;
        public event UIMouseEvent MouseRightClick;
        public event UIMouseEvent MouseEnter;
        public event UIMouseEvent MouseLeave;
        public event UIEvent LostFocus;

        #endregion

        public UIManager Manager { get; internal set; }

        private static int _lastUniqueID;
        public int UniqueID { get; private set; }

        public List<UIPositionParameter> FirstPositionParameters { get; private set; }
        public List<UIPositionParameter> SecondPositionParameters { get; private set; }

        //our position parameters that has anchorto property
        public List<UIPositionParameter> DependedPositionParameters { get; private set; }

        //other frames position parameters that references us in their anchorto property
        public List<UIPositionParameter> ChildPositionParameters { get; private set; }

        public List<UIPositionParameter> PositionParameters { get; private set; }

        public bool DontCaptureEvents { get; set; }

        private bool _mouseInside;

        public bool PassEventsToChildren { get; set; }

        public Object DataObject { get; set; }

        public bool DontBringToFrontOnClick { get; set; }

        public bool Enabled { get; set; }

        public string Name { get { return Entity.Name; } set { Entity.Name = value; } }

        public float X { get { return CurrentRegion.X; } }

        public float Y { get { return CurrentRegion.Y; } }

        public UIRegion DefaultRegion { get; private set; }
        public UIRegion CurrentRegion { get; private set; }

        public float Width
        {
            get
            {
                return CurrentRegion.Width;
            }
        }

        public float Height
        {
            get
            {
                return CurrentRegion.Height;
            }
        }

        public float WidthScale { get; set; }

        public float HeightScale { get; set; }

        public float CenterX { get { return X + Width * 0.5f; } }
        public float CenterY { get { return Y + Height * 0.5f; } }

        public float RelativeMouseX
        {
            get
            {
                return Manager.PointerPosition.X - X;
            }
        }

        public float RelativeMouseY
        {
            get
            {
                return Manager.PointerPosition.Y - Y;
            }
        }

        public UIWidget Owner { get; private set; }

        public List<UIWidget> Children { get; private set; }

        public List<UIWidget> LogicalChildren { get { return HolderFrame.Children; } }

        public UIWidget HolderFrame { get; set; } //children added to holderFrame, default is object itself

        protected override void OnAdded()
        {
            base.OnAdded();

            Owner = Entity.Parent.GetComponent<UIWidget>();

            if (Owner != null)
            {
                Manager = Owner.Manager;
            }
            else
            {
                Manager = Entity.Parent.GetComponent<UIManager>();
            }

            {
                DependedPositionParameters = new List<UIPositionParameter>();
                ChildPositionParameters = new List<UIPositionParameter>();
                Children = new List<UIWidget>();

                WidthScale = 1.0f;
                HeightScale = 1.0f;
                _mouseInside = false;

                HolderFrame = this;

                CurrentRegion = new UIRegion(this);
                CurrentRegion.Domain = Manager.CurrentDomain;

                DefaultRegion = new UIRegion(this);
                DefaultRegion.Domain = Manager.DefaultDomain;

                DataObject = null;
                Enabled = true;
                PassEventsToChildren = true;
                DontCaptureEvents = false;
            }

            _lastUniqueID++;
            UniqueID = _lastUniqueID;

            if (Owner != null)
            {
                this.Owner = Owner.HolderFrame;
                Owner.LogicalChildren.Insert(0, this);
            }
        }

        internal void OnEntityParentChanged()
        {
            UIWidget newOwner = Entity.Parent.GetComponent<UIWidget>();
            UIWidget oldOwner = this.Owner;

            oldOwner.LogicalChildren.Remove(this);

            this.Owner = newOwner.HolderFrame;
            Owner.LogicalChildren.Insert(0, this);

            this.SetNonUpdatedWithChildrenOnAllDomains();
        }

        public void Initialize(List<UIPositionParameter> positionParameters)
        {
            if (positionParameters == null)
            {
                PositionParameters = new List<UIPositionParameter>();
                FirstPositionParameters = new List<UIPositionParameter>();
                SecondPositionParameters = new List<UIPositionParameter>();
            }
            else
            {
                PositionParameters = positionParameters.OrderBy(x => x.PositionOrder).ToList();

                for (int i = 0; i < PositionParameters.Count; i++)
                {
                    UIPositionParameter positionParameter = PositionParameters[i];
                    positionParameter.Owner = this;

                    if (positionParameter.AnchorTo != null)
                    {
                        DependedPositionParameters.Add(positionParameter);
                        positionParameter.AnchorTo.ChildPositionParameters.Add(positionParameter);
                        //if (!_dependedPositionInfos->Contains(positionParameter->anchorTo))
                        //{
                        //	_dependedPositionInfos->Add(positionParameter->anchorTo);
                        //	positionParameter->anchorTo->AddChildPositionInfo(this);
                        //}
                    }
                }

                FirstPositionParameters = PositionParameters.Where(x => x.ExecuteType == PositionParameterExecuteType.BeforeCalculations).ToList<UIPositionParameter>();
                SecondPositionParameters = PositionParameters.Where(x => x.ExecuteType == PositionParameterExecuteType.AfterCalculations).ToList<UIPositionParameter>();
            }
        }

        public void AddPositionParameter(UIPositionParameter positionParameter)
        {
            PositionParameters.Add(positionParameter);

            positionParameter.Owner = this;

            if (positionParameter.AnchorTo != null)
            {
                DependedPositionParameters.Add(positionParameter);
                positionParameter.AnchorTo.ChildPositionParameters.Add(positionParameter);
                //if (!_dependedPositionInfos->Contains(positionParameter->anchorTo))
                //{
                //	_dependedPositionInfos->Add(positionParameter->anchorTo);
                //	positionParameter->anchorTo->AddChildPositionInfo(this);
                //}
            }

            PositionParameters = PositionParameters.OrderBy(x => x.PositionOrder).ToList<UIPositionParameter>();

            FirstPositionParameters = PositionParameters.Where(x => x.ExecuteType == PositionParameterExecuteType.BeforeCalculations).ToList<UIPositionParameter>();
            SecondPositionParameters = PositionParameters.Where(x => x.ExecuteType == PositionParameterExecuteType.AfterCalculations).ToList<UIPositionParameter>();

            SetNonUpdatedWithChildrenOnAllDomains();
            UpdatePositionParametersOnAllDomains();
        }

        public void RemovePositionParameter(UIPositionParameter positionParameter)
        {
            PositionParameters.Remove(positionParameter);

            if (positionParameter.AnchorTo != null)
            {
                DependedPositionParameters.Remove(positionParameter);
                positionParameter.AnchorTo.ChildPositionParameters.Remove(positionParameter);
                //positionParameter->anchorTo->RemoveChildRegion(this);
            }

            SetNonUpdatedWithChildrenOnAllDomains();
            UpdatePositionParametersOnAllDomains();
        }

        public void SetNonUpdatedWithChildrenOnAllDomains()
        {
            CurrentRegion.SetNonUpdatedWithChildren();
            DefaultRegion.SetNonUpdatedWithChildren();
        }

        public void UpdatePositionParametersOnAllDomains()
        {
            CurrentRegion.UpdatePositionParameters();
            DefaultRegion.UpdatePositionParameters();
        }

        internal float GetScaleResult(float value, ScaleType typeOfScale)
        {
            float result = value;

            if (typeOfScale == ScaleType.RootHorizontalScale)
            {
                result = result * Manager.HorizontalScaleValue() * Mathf.Abs(WidthScale);
            }
            else if (typeOfScale == ScaleType.RootVerticalScale)
            {
                result = result * Manager.VerticalScaleValue() * Mathf.Abs(HeightScale);
            }
            else if (typeOfScale == ScaleType.OwnerWidthScale)
            {
                float scaleValue = Owner.CurrentRegion.Width / Owner.DefaultRegion.Width;
                result = result * scaleValue * Mathf.Abs(HeightScale);
            }
            else if (typeOfScale == ScaleType.OwnerHeightScale)
            {
                float scaleValue = Owner.CurrentRegion.Height / Owner.DefaultRegion.Height;
                result = result * scaleValue * Mathf.Abs(HeightScale);
            }

            return result;
        }

        internal UIRegion GetRegion(UIRegionDomain regionDomain)
        {
            if (regionDomain == CurrentRegion.Domain)
            {
                return CurrentRegion;
            }
            return DefaultRegion;
        }

        internal virtual bool CoordTestInside(float relativeCoordX, float relativeCoordY)
        {
            return (relativeCoordX >= 0 && relativeCoordY >= 0 && relativeCoordX <= Width && relativeCoordY <= Height);
        }

        public UIWidget GetFrontObject(float absoluteX, float absoluteY, UIWidget ignoreObject = null)
        {
            if (ignoreObject == this)
            {
                return null;
            }

            if (PassEventsToChildren)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    UIWidget widget = Children[i];

                    if (widget.Enabled && !widget.DontCaptureEvents)
                    {
                        if (widget == ignoreObject)
                        {
                            continue;
                        }

                        float childRelativeX = absoluteX - widget.X;
                        float childRelativeY = absoluteY - widget.Y;

                        if (widget.CoordTestInside(childRelativeX, childRelativeY))
                        {
                            return widget.GetFrontObject(absoluteX, absoluteY, ignoreObject);
                        }
                    }
                }
            }

            return this;
        }

        internal UIWidget MouseDownController(float mouseDownX, float mouseDownY, bool rightMouseButton = false)
        {
            bool childControl = false;
            UIWidget selectedChild = null;
            UIWidget mouseDownObject = null;

            if (PassEventsToChildren)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    UIWidget widget = Children[i];

                    if (widget.Enabled && !widget.DontCaptureEvents)
                    {
                        float childRelativeX = mouseDownX - widget.X;
                        float childRelativeY = mouseDownY - widget.Y;

                        if (widget.CoordTestInside(childRelativeX, childRelativeY))
                        {
                            mouseDownObject = widget.MouseDownController(mouseDownX, mouseDownY, rightMouseButton);
                            selectedChild = widget;
                            childControl = true;
                            break;
                        }
                    }
                }
            }

            if (!childControl)
            {
                mouseDownObject = this;
            }
            else
            {
                //moving its child to front
                if (!selectedChild.DontBringToFrontOnClick)
                {
                    Children.Remove(selectedChild);
                    Children.Insert(0, selectedChild);
                }
            }

            return mouseDownObject;
        }

        internal void MouseUpController(float mouseDownX, float mouseDownY, bool rightMouseButton = false)
        {
            bool childControl = false;

            if (PassEventsToChildren)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    UIWidget widget = Children[i];

                    if (widget.Enabled && !widget.DontCaptureEvents)
                    {
                        if (widget.CoordTestInside(widget.RelativeMouseX, widget.RelativeMouseY))
                        {
                            widget.MouseUpController(mouseDownX, mouseDownY, rightMouseButton);
                            childControl = true;
                            break;
                        }
                    }
                }
            }

            if (!childControl)
            {
                if (rightMouseButton)
                {
                    OnMouseRightUp(new MouseEventArgs(0, Manager.PointerPosition.X, Manager.PointerPosition.Y));
                }
                else
                {
                    OnMouseUp(new MouseEventArgs(0, Manager.PointerPosition.X, Manager.PointerPosition.Y));
                }

                if (Manager.MouseDownObject == this)
                {
                    if (CoordTestInside(RelativeMouseX, RelativeMouseY))
                    {
                        if (rightMouseButton)
                        {
                            OnMouseRightClick(new MouseEventArgs(0, Manager.PointerPosition.X, Manager.PointerPosition.Y));
                        }
                        else
                        {
                            OnMouseClick(new MouseEventArgs(0, Manager.PointerPosition.X, Manager.PointerPosition.Y));
                        }
                    }
                }
            }
        }

        internal bool MouseEnterLeaveController(bool ownerForceYou)
        {
            bool coordInside = CoordTestInside(RelativeMouseX, RelativeMouseY);
            bool mouseEnter = false;

            if (PassEventsToChildren)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    UIWidget widget = Children[i];

                    if (widget.Enabled && !widget.DontCaptureEvents)
                    {
                        if (ownerForceYou)
                        {
                            widget.MouseEnterLeaveController(true);
                        }
                        else
                        {
                            if (!mouseEnter)
                            {
                                mouseEnter = widget.MouseEnterLeaveController(false);
                            }
                            else
                            {
                                widget.MouseEnterLeaveController(true);
                            }
                        }
                    }
                }
            }

            if (coordInside && !mouseEnter && !ownerForceYou)
            {
                if (!_mouseInside)
                {
                    _mouseInside = true;

                    //todo mouse koordinatları ayarlanacak
                    OnMouseEnter(new MouseEventArgs(0, 0, 0));
                }
            }
            else
            {
                if (_mouseInside)
                {
                    _mouseInside = false;

                    //todo mouse koordinatları
                    OnMouseLeave(new MouseEventArgs(0, 0, 0));
                }
            }

            return coordInside;
        }

        private void DestroyChild(UIWidget child)
        {
            if (LogicalChildren.Contains(child))
            {
                Manager.DoObjectDeletionJob(child);

                for (int i = 0; i < child.ChildPositionParameters.Count; i++)
                {
                    UIPositionParameter childPositionParameter = child.ChildPositionParameters[i];
                    childPositionParameter.Owner.DependedPositionParameters.Remove(childPositionParameter);
                }

                for (int i = 0; i < child.DependedPositionParameters.Count; i++)
                {
                    UIPositionParameter dependedPositionInfo = child.DependedPositionParameters[i];
                    dependedPositionInfo.AnchorTo.ChildPositionParameters.Remove(dependedPositionInfo);
                }

                LogicalChildren.Remove(child);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Owner != null)
            {
                Owner.DestroyChild(this);
            }
        }

        public UIWidget GetChildAtPath(string path)
        {
            string[] splitPath = path.Split(new char[] { '\\' });

            UIWidget currentWidget = this;

            for (int i = 0; i < splitPath.Length; i++)
            {
                bool currentChildFound = false;

                for (int j = 0; j < currentWidget.LogicalChildren.Count && !currentChildFound; j++)
                {
                    UIWidget child = currentWidget.LogicalChildren[j];
                    if (child.Name == splitPath[i])
                    {
                        currentWidget = child;
                        currentChildFound = true;
                    }
                }

                if (i + 1 == splitPath.Length && currentChildFound)
                {
                    return currentWidget;
                }
            }

            return null;
        }

        public void BringToFront()
        {
            if (Owner != null)
            {
                Owner.LogicalChildren.Remove(this);
                Owner.LogicalChildren.Insert(0, this);
            }
        }

        public void BringToBack()
        {
            if (Owner != null)
            {
                Owner.LogicalChildren.Remove(this);
                Owner.LogicalChildren.Add(this);
            }
        }

        #region On Events

        protected internal virtual void OnMouseDown(MouseEventArgs e)
        {
            if (MouseDown != null)
            {
                MouseDown(this, e);
            }
        }

        protected internal virtual void OnMouseRightDown(MouseEventArgs e)
        {
            if (MouseRightDown != null)
            {
                MouseRightDown(this, e);
            }
        }

        protected internal virtual void OnMouseUp(MouseEventArgs e)
        {
            if (MouseUp != null)
            {
                MouseUp(this, e);
            }
        }

        protected internal virtual void OnMouseRightUp(MouseEventArgs e)
        {
            if (MouseRightUp != null)
            {
                MouseRightUp(this, e);
            }
        }

        protected internal virtual void OnMouseClick(MouseEventArgs e)
        {
            if (MouseClick != null)
            {
                MouseClick(this, e);
            }
        }

        protected internal virtual void OnMouseRightClick(MouseEventArgs e)
        {
            if (MouseRightClick != null)
            {
                MouseRightClick(this, e);
            }
        }

        protected internal virtual void OnMouseEnter(MouseEventArgs e)
        {
            if (MouseEnter != null)
            {
                MouseEnter(this, e);
            }
        }

        protected internal virtual void OnMouseLeave(MouseEventArgs e)
        {
            if (MouseLeave != null)
            {
                MouseLeave(this, e);
            }
        }

        protected internal virtual void OnLostFocus(UIWidget widget)
        {
            if (LostFocus != null)
            {
                LostFocus(widget);
            }
        }

        #endregion
    }

    public delegate void UIMouseEvent(UIWidget widget, MouseEventArgs e);
    public delegate void UIEvent(UIWidget widget);
}
