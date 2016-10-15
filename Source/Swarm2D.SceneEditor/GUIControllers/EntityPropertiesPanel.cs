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
using Swarm2D.Engine.View.GUI;
using Swarm2D.Engine.Logic;

namespace Swarm2D.SceneEditor.GUIControllers
{
    class EntityPropertiesPanel : UIController, IDataSynchronizerHandler
    {
        private SceneEditor _sceneEditor;

        private UIButton _closeObjectPropertiesPanel;
        private UIEditBox _selectedSceneObjectName;
        private UIButton _addComponentButton;

        private List<Component> _components;
        private List<ComponentEditorPanel> _componentEditorPanels;

        private UIContextMenu _addComponentMenu;
        private UIPositionParameter _addComponentMenuTopParameter;
        private UIPositionParameter _addComponentMenuLeftParameter;

        protected override void OnAdded()
        {
            base.OnAdded();

            _components = new List<Component>();
            _componentEditorPanels = new List<ComponentEditorPanel>();

            _sceneEditor = Entity.Parent.GetComponent<SceneEditor>();

            _closeObjectPropertiesPanel = (UIButton)ControllerFrame.GetChildAtPath("CloseEntityPropertiesPanel");
            _selectedSceneObjectName = (UIEditBox)ControllerFrame.GetChildAtPath("SelectedEntityName");
            _addComponentButton = (UIButton)ControllerFrame.GetChildAtPath("AddComponent");

            _closeObjectPropertiesPanel.MouseClick += new UIMouseEvent(OnCloseObjectPropertiesButtonClick);
            _addComponentButton.MouseClick += new UIMouseEvent(OnAddComponentMenuButtonClick);
            _selectedSceneObjectName.TextChanged += OnSelectedSceneObjectNameTextChanged;
            CreateAddComponentMenu();
        }

        private void OnSelectedSceneObjectNameTextChanged()
        {
            if (!String.IsNullOrEmpty(_selectedSceneObjectName.Text))
            {
                Entity selectedSceneObject = _sceneEditor.SelectedEntity;
                selectedSceneObject.Name = _selectedSceneObjectName.Text;
            }
        }

        private void CreateAddComponentMenu()
        {
            UIFrame rootFrame = ControllerFrame.Manager.RootObject;

            List<UIPositionParameter> menuParameters = new List<UIPositionParameter>();

            _addComponentMenuTopParameter = UIPositionParameter.AnchorToSideParameter(rootFrame.Widget, AnchorSide.Top, AnchorToSideType.Inner, 50);
            _addComponentMenuLeftParameter = UIPositionParameter.AnchorToSideParameter(rootFrame.Widget, AnchorSide.Left, AnchorToSideType.Inner, 50);

            menuParameters.Add(_addComponentMenuTopParameter);
            menuParameters.Add(_addComponentMenuLeftParameter);

            _addComponentMenu = ControllerFrame.Manager.CreateContextMenu(menuParameters);

            _addComponentMenu.Name = "AddComponentMenu";

            foreach (ComponentInfo componentInfo in ComponentInfo.ComponentInfos)
            {
                ContextMenuItem componentButton = _addComponentMenu.AddItem(componentInfo.Name);

                componentButton.Data = componentInfo;

                componentButton.Click += new ContextMenuItemEvent(OnAddComponentButtonClick);
            }

            _addComponentMenu.Enabled = false;
            _addComponentMenu.BringToFront();

            //deleteComponentButton.MouseClick += new UIMouseEvent(OnDeleteComponentButtonClick);
            //resetComponentButton.MouseClick += new UIMouseEvent(OnResetComponentButtonClick);
        }

        public void Update()
        {
            if (ControllerFrame.Enabled)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            Entity selectedSceneObject = _sceneEditor.SelectedEntity;

            if (selectedSceneObject != null)
            {
                _selectedSceneObjectName.Text = selectedSceneObject.Name;

                float currentBottom = _addComponentButton.Y + _addComponentButton.Height - ControllerFrame.Y + 5;

                if (DataSynchronizer.SynchronizeWithList(this, selectedSceneObject.Components))
                {
                    for (int i = 0; i < _components.Count; i++)
                    {
                        ComponentEditorPanel componentEditorPanel = _componentEditorPanels[i];

                        currentBottom = componentEditorPanel.RepositionUI(currentBottom) + 5.0f;
                    }
                }

                foreach (ComponentEditorPanel componentEditorPanel in _componentEditorPanels)
                {
                    componentEditorPanel.Refresh();
                }
            }
        }

        #region Component List Synchronizers

        public int GetObjectCount()
        {
            return _components.Count;
        }

        public bool ContainsItem(object newObject)
        {
            return _components.Contains((Component)newObject);
        }

        public object GetObjectAtIndex(int index)
        {
            return _components[index];
        }

        public void AddNewItem(object newObject)
        {
            Component component = (Component)newObject;

            ComponentEditorPanel componentEditorPanel = ComponentEditorPanel.Create(ControllerFrame.HolderFrame, component);
            _components.Add(component);
            _componentEditorPanels.Add(componentEditorPanel);
        }

        public void RemoveObject(int index, object deletedObject)
        {
            ComponentEditorPanel componentEditorPanel = _componentEditorPanels[index];

            componentEditorPanel.Dispose();

            _components.RemoveAt(index);
            _componentEditorPanels.RemoveAt(index);
        }

        #endregion

        private void OnCloseObjectPropertiesButtonClick(UIWidget sender, MouseEventArgs e)
        {
            _sceneEditor.HideEntityProperties();
        }

        private void OnAddComponentButtonClick(ContextMenuItem item)
        {
            ComponentInfo componentInfo = item.Data as ComponentInfo;

            Entity selectedEntity = _sceneEditor.SelectedEntity;

            selectedEntity.AddComponent(componentInfo.ComponentType);
        }

        private void OnAddComponentMenuButtonClick(UIWidget sender, MouseEventArgs e)
        {
            _addComponentMenuLeftParameter.Value = e.X;
            _addComponentMenuTopParameter.Value = e.Y;

            _addComponentMenu.Enabled = true;
            _addComponentMenu.BringToFront();

            _addComponentMenu.SetNonUpdatedWithChildrenOnAllDomains();
        }
    }
}
