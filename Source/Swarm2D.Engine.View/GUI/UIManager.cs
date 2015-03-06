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
    public class UIManager : Component, IEntityDomain
    {
        private UIWidget _mouseDownObject;

        private float lastMouseX;
        private float lastMouseY;

        private List<UIUpdateMethod> _updateMethods;

        private List<UIWidget> _popups;

        public Font Font
        {
            get;
            private set;
        }

        //public delegate void ScreenSizeChangeEventHandler(object sender, EventArgs eventArgs);

        public Dictionary<int, UIFrame> ObjectList
        {
            get;
            private set;
        }

        public UIWidget CurrentFocusObject
        {
            get;
            private set;
        }

        public float DefaultWidth
        {
            get;
            private set;
        }

        public float DefaultHeight
        {
            get;
            private set;
        }

        public UIRegionDomain DefaultDomain
        {
            get;
            private set;
        }

        public UIRegionDomain CurrentDomain
        {
            get;
            private set;
        }

        public bool ShowAllBoxes
        {
            get;
            set;
        }

        public UIWidget RootWidget { get; private set; }

        public UIFrame RootObject { get; private set; }

        public UIWidget MouseDownObject
        {
            get
            {
                return _mouseDownObject;
            }
            set
            {
                _mouseDownObject = value;

                if (value != null)
                {
                    if (CurrentFocusObject != value)
                    {
                        if (CurrentFocusObject != null)
                        {
                            CurrentFocusObject.OnLostFocus(CurrentFocusObject);
                        }

                        CurrentFocusObject = value;
                    }
                }
            }
        }

        public Vector2 PointerPosition
        {
            get
            {
                return IOSystem.MousePosition;
            }
        }

        public UISkin Skin { get; set; }

        public IIOSystem IOSystem { get; private set; }

        private EntityDomain _entityDomain;

        protected override void OnAdded()
        {
            base.OnAdded();
            _entityDomain = new EntityDomain(Entity);
            Entity.ChildDomain = this;

            _popups = new List<UIWidget>();

            Font = new Font("ArialKucuk");
            Skin = new UISkin();

            _updateMethods = new List<UIUpdateMethod>();
        }

        public void Initialize(IIOSystem ioSystem)
        {
            IOSystem = ioSystem;

            ShowAllBoxes = false;
            DefaultWidth = IOSystem.Width;
            DefaultHeight = IOSystem.Height;

            DefaultDomain = new UIRegionDomain(this);
            DefaultDomain.Width = DefaultWidth;
            DefaultDomain.Height = DefaultHeight;

            CurrentDomain = new UIRegionDomain(this);
            CurrentDomain.Width = DefaultWidth;
            CurrentDomain.Height = DefaultHeight;

            List<UIPositionParameter> rootObjectPositionParameters = new List<UIPositionParameter>();
            rootObjectPositionParameters.Add(UIPositionParameter.FitToDomain());

            Entity rootEntity = CreateChildEntity("RootUIFrame");
            RootWidget = rootEntity.GetComponent<UIWidget>();
            RootObject = rootEntity.AddComponent<UIFrame>();
            RootObject.Initialize(rootObjectPositionParameters);
            //rootUIObject.SetPositionInfo(new UIFrame(widthParameter, heightParameter));
            RootObject.Name = "root";
            RootObject.ShowBox = false;
        }

        public void Update()
        {
            if (IOSystem.LeftMouseDown)
            {
                MouseLeftButtonDownX = PointerPosition.X;
                MouseLeftButtonDownY = PointerPosition.Y;
            }

            bool widthChanged = false;
            bool heightChanged = false;

            if (IOSystem.Width != CurrentDomain.Width)
            {
                CurrentDomain.Width = IOSystem.Width;
                widthChanged = true;

            }

            if (IOSystem.Height != CurrentDomain.Height)
            {
                CurrentDomain.Height = IOSystem.Height;
                heightChanged = true;
            }

            if (widthChanged || heightChanged)
            {
                RootObject.CurrentRegion.SetNonUpdatedWithChildren();

                if (widthChanged && heightChanged)
                {
                    RootObject.CurrentRegion.UpdatePositionParameters();
                }
                else if (widthChanged)
                {
                    RootObject.CurrentRegion.UpdatePositionParameters();
                }
                else if (heightChanged)
                {
                    RootObject.CurrentRegion.UpdatePositionParameters();
                }
            }

            lastMouseX = PointerPosition.X;
            lastMouseY = PointerPosition.Y;
            RootWidget.MouseEnterLeaveController(false);

            RootObject.Update(_updateMethods);

            for (int i = 0; i < _updateMethods.Count; i++)
            {
                _updateMethods[i]();
            }

            _updateMethods.Clear();

            if (PointerDown())
            {
                UIWidget mouseDownObject = RootWidget.MouseDownController(PointerPosition.X, PointerPosition.Y);

                if (mouseDownObject != null && mouseDownObject != MouseDownObject)
                {
                    mouseDownObject.OnMouseDown(new MouseEventArgs(0, PointerPosition.X, PointerPosition.Y));
                }

                MouseDownObject = mouseDownObject;
            }
            else if (PointerRightDown())
            {
                UIWidget mouseDownObject = RootWidget.MouseDownController(PointerPosition.X, PointerPosition.Y, true);

                if (mouseDownObject != null && mouseDownObject != MouseDownObject)
                {
                    mouseDownObject.OnMouseRightDown(new MouseEventArgs(0, PointerPosition.X, PointerPosition.Y));
                }

                MouseDownObject = mouseDownObject;
            }

            if (PointerRelease())
            {
                RootWidget.MouseUpController(PointerPosition.X, PointerPosition.Y);
                MouseDownObject = null;
            }

            if (PointerRightRelease())
            {
                RootWidget.MouseUpController(PointerPosition.X, PointerPosition.Y, true);
                MouseDownObject = null;
            }

            foreach (UIWidget popup in _popups)
            {
                if (RootWidget.LogicalChildren.First() != popup)
                {
                    popup.Enabled = false;
                }
            }

            _entityDomain.InitializeNonInitializedEntityComponents();
            _entityDomain.StartNotStartedEntityComponents();

            UpdateMessage updateMessage = new UpdateMessage();
            updateMessage.Dt = 1.0f / 60.0f; //TODO

            SendMessage(updateMessage);
        }

        public void Render(IOSystem ioSystem)
        {
            RootObject.RenderController(ioSystem);
        }

        public float VerticalScaleValue()
        {
            return CurrentDomain.VerticalScaleValue;
        }

        public float HorizontalScaleValue()
        {
            return CurrentDomain.HorizontalScaleValue;
        }

        public float MouseLeftButtonDownX { get; private set; }

        public float MouseLeftButtonDownY { get; private set; }

        public bool PointerDown()
        {
            return IOSystem.LeftMouseDown;
        }

        public bool PointerRelease()
        {
            return IOSystem.LeftMouseUp;
        }

        public bool PointerRightDown()
        {
            return IOSystem.RightMouseDown;
        }

        public bool PointerRightRelease()
        {
            return IOSystem.RightMouseUp;
        }

        public void DoObjectDeletionJob(UIWidget widget)
        {
            if (CurrentFocusObject == widget)
            {
                CurrentFocusObject = null;
            }
        }

        public UIContextMenu CreateContextMenu(List<UIPositionParameter> positionParameters)
        {
            Entity contextMenuEntity = RootObject.CreateChildEntity("contextMenuEntity");
            UIContextMenu contextMenu = contextMenuEntity.AddComponent<UIContextMenu>();
            contextMenu.Initialize(positionParameters);
            contextMenu.BringToFront();
            _popups.Add(contextMenu.Widget);
            return contextMenu;
        }

        void IEntityDomain.OnCreateChildEntity(Entity entity)
        {
            entity.Domain = this;

            UIWidget widget = entity.AddComponent<UIWidget>();
            widget.Manager = this;
        }

        public void SendMessage(DomainMessage message)
        {
            _entityDomain.SendMessage(message);
        }

        void IEntityDomain.OnComponentCreated(Component component)
        {
            _entityDomain.OnComponentCreated(component);
        }

        void IEntityDomain.OnComponentDestroyed(Component component)
        {
            _entityDomain.OnComponentDestroyed(component);
        }

        Entity IEntityDomain.InstantiatePrefab(Entity prefab)
        {
            return _entityDomain.InstantiatePrefab(prefab);
        }

        void IEntityDomain.OnEntityParentChanged(Entity entity)
        {
            UIWidget widget = entity.GetComponent<UIWidget>();

            widget.OnEntityParentChanged();
        }
    }

    public delegate void UIUpdateMethod();
}
