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
using Swarm2D.Engine.View;
using Swarm2D.Engine.View.GUI;
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.SceneEditor.GUIControllers
{
    class ComponentEditorPanel : UIController
    {
        private Component _component;
        private float _panelLength;

        private UIContextMenu _componentPropertiesMenu;
        private UIPositionParameter _componentPropertiesMenuTopParameter;
        private UIPositionParameter _componentPropertiesMenuLeftParameter;

        private UILabel _componentLabel;
        private UIButton _componentPropertiesButton;
        private UIFrame _componentFrame;

        private UIPositionParameter _labelYPositionParameter;
        private UIPositionParameter _frameHeightPositionParameter;

        private List<ComponentPropertyEditor> _componentPropertyEditors;

        protected override void OnAdded()
        {
            base.OnAdded();

            _componentPropertyEditors = new List<ComponentPropertyEditor>();
        }

        public void Update()
        {

        }

        public void Refresh()
        {
            for (int i = 0; i < _componentPropertyEditors.Count; i++)
            {
                _componentPropertyEditors[i].Refresh();
            }
        }

        public static ComponentEditorPanel Create(UIFrame owner, Component component)
        {
            Entity componentEditorPanelEntity = owner.CreateChildEntity("ComponentEditorPanel");

            componentEditorPanelEntity.AddComponent<UIFrame>();

            ComponentEditorPanel componentEditorPanel = componentEditorPanelEntity.AddComponent<ComponentEditorPanel>();
            componentEditorPanel._component = component;
            componentEditorPanel.CreateUI();

            return componentEditorPanel;
        }

        private void CreateUI()
        {
            UIFrame owner = Entity.Parent.GetComponent<UIFrame>();
            ComponentInfo componentInfo = _component.GetComponentInfo();

            {
                List<UIPositionParameter> positionParameters = FastGUI.GenerateStandardParameters(owner.Widget, 5.0f, 35.0f, owner.Width - 10.0f, 5.0f);

                _labelYPositionParameter = positionParameters[0];
                _frameHeightPositionParameter = positionParameters[3];

                _componentFrame = this.GetComponent<UIFrame>();
                _componentFrame.Initialize(positionParameters);
            }

            {
                _componentLabel = FastGUI.CreateLabel(_componentFrame.Widget, componentInfo.Name, 5.0f, 5.0f, _componentFrame.Width - 45.0f, 30.0f);
                //_labelYPositionParameter = _componentLabel->GetPositionParameters()->GetItemAt(0);
                _componentLabel.Text = componentInfo.Name;
            }

            {
                List<UIPositionParameter> positionParameters = FastGUI.GenerateStandardParameters(_componentLabel.Widget, _componentLabel.Width + 5.0f, 0.0f, 30.0f, 30.0f);

                Entity componentPropertiesButtonEntity = _componentFrame.CreateChildEntity("componentPropertiesButton");
                _componentPropertiesButton = componentPropertiesButtonEntity.AddComponent<UIButton>();
                _componentPropertiesButton.Initialize(positionParameters);

                _componentPropertiesButton.MouseClick += new UIMouseEvent(OnComponentPropertiesButtonClick);
                _componentPropertiesButton.AddSprite(UIButtonRenderState.MouseDown, Resource.GetResource<Sprite>("gearButton"));
                _componentPropertiesButton.AddSprite(UIButtonRenderState.MouseOver, Resource.GetResource<Sprite>("gearButton"));
                _componentPropertiesButton.AddSprite(UIButtonRenderState.Normal, Resource.GetResource<Sprite>("gearButton"));
            }


            _panelLength = 40.0f;

            foreach (ComponentPropertyInfo propertyInfo in componentInfo.ComponentPropertyInfos.Values)
            {
                switch (propertyInfo.PropertyType)
                {
                    case ComponentPropertyType.Float:
                        AddPropertyEditorFloat(propertyInfo);
                        break;
                    case ComponentPropertyType.Int:
                        AddPropertyEditorInt(propertyInfo);
                        break;
                    case ComponentPropertyType.Vector2:
                        AddPropertyEditorVector2(propertyInfo);
                        break;
                    case ComponentPropertyType.Resource:
                        AddPropertyEditorResource(propertyInfo);
                        break;
                    case ComponentPropertyType.Enumerator:
                        AddPropertyEditorEnumerator(propertyInfo);
                        break;
                    case ComponentPropertyType.String:
                        if (propertyInfo.MemberOfGlobalList)
                        {
                            AddPropertyEditorStringMemberOfGlobalList(propertyInfo);
                        }
                        else
                        {
                            AddPropertyEditorString(propertyInfo);
                        }
                        break;
                    case ComponentPropertyType.Object:
                        break;
                    default:
                        break;
                }
            }

            _frameHeightPositionParameter.Value = _panelLength;

            CreateComponentPropertiesMenu();
        }

        public float RepositionUI(float y)
        {
            _labelYPositionParameter.Value = y;

            _componentFrame.SetNonUpdatedWithChildrenOnAllDomains();

            return _labelYPositionParameter.Value + _panelLength;
        }

        public virtual void Dispose()
        {
            _componentFrame.Entity.Destroy();
            _componentPropertiesMenu.Entity.Destroy();
        }

        private void AddPropertyEditorFloat(ComponentPropertyInfo componentFloatPropertyInfo)
        {
            ComponentBasicTypePropertyEditor<float> componentFloatPropertyEditor = new ComponentBasicTypePropertyEditor<float>(_component, componentFloatPropertyInfo);

            _panelLength = componentFloatPropertyEditor.CreateUI(_componentFrame, _panelLength);

            _componentPropertyEditors.Add(componentFloatPropertyEditor);
        }

        private void AddPropertyEditorInt(ComponentPropertyInfo componentIntPropertyInfo)
        {
            ComponentBasicTypePropertyEditor<int> componentIntPropertyEditor = new ComponentBasicTypePropertyEditor<int>(_component, componentIntPropertyInfo);

            _panelLength = componentIntPropertyEditor.CreateUI(_componentFrame, _panelLength);

            _componentPropertyEditors.Add(componentIntPropertyEditor);
        }

        private void AddPropertyEditorVector2(ComponentPropertyInfo componentVector2PropertyInfo)
        {
            ComponentVector2PropertyEditor componentVector2PropertyEditor = new ComponentVector2PropertyEditor(_component, componentVector2PropertyInfo);

            _panelLength = componentVector2PropertyEditor.CreateUI(_componentFrame, _panelLength);

            _componentPropertyEditors.Add(componentVector2PropertyEditor);
        }

        private void AddPropertyEditorResource(ComponentPropertyInfo componentResourcePropertyInfo)
        {
            ComponentResourcePropertyEditor componentResourcePropertyEditor = new ComponentResourcePropertyEditor(_component, componentResourcePropertyInfo);

            _panelLength = componentResourcePropertyEditor.CreateUI(_componentFrame, _panelLength);

            _componentPropertyEditors.Add(componentResourcePropertyEditor);
        }

        private void AddPropertyEditorString(ComponentPropertyInfo componentStringPropertyInfo)
        {
            ComponentStringPropertyEditor componentStringPropertyEditor = new ComponentStringPropertyEditor(_component, componentStringPropertyInfo);

            _panelLength = componentStringPropertyEditor.CreateUI(_componentFrame, _panelLength);

            _componentPropertyEditors.Add(componentStringPropertyEditor);
        }

        private void AddPropertyEditorStringMemberOfGlobalList(ComponentPropertyInfo componentStringPropertyInfo)
        {
            ComponentMemberOfGlobalListPropertyEditor componentMemberOfGlobalListPropertyEditor = new ComponentMemberOfGlobalListPropertyEditor(_component, componentStringPropertyInfo);

            _panelLength = componentMemberOfGlobalListPropertyEditor.CreateUI(_componentFrame, _panelLength);

            _componentPropertyEditors.Add(componentMemberOfGlobalListPropertyEditor);
        }

        private void AddPropertyEditorEnumerator(ComponentPropertyInfo componentSpritePropertyInfo)
        {
            ComponentEnumeratorPropertyEditor componentEnumeratorPropertyEditor = new ComponentEnumeratorPropertyEditor(_component, componentSpritePropertyInfo);

            _panelLength = componentEnumeratorPropertyEditor.CreateUI(_componentFrame, _panelLength);

            _componentPropertyEditors.Add(componentEnumeratorPropertyEditor);
        }

        private void CreateComponentPropertiesMenu()
        {
            UIFrame rootFrame = _componentFrame.Manager.RootObject;

            List<UIPositionParameter> menuParameters = new List<UIPositionParameter>();

            _componentPropertiesMenuTopParameter = UIPositionParameter.AnchorToSideParameter(rootFrame.Widget, AnchorSide.Top, AnchorToSideType.Inner, 50);
            _componentPropertiesMenuLeftParameter = UIPositionParameter.AnchorToSideParameter(rootFrame.Widget, AnchorSide.Left, AnchorToSideType.Inner, 50);

            menuParameters.Add(_componentPropertiesMenuTopParameter);
            menuParameters.Add(_componentPropertiesMenuLeftParameter);

            _componentPropertiesMenu = _componentFrame.Manager.CreateContextMenu(menuParameters);

            _componentPropertiesMenu.Name = "ComponentPropertiesMenu";

            ContextMenuItem deleteComponentButton = _componentPropertiesMenu.AddItem("Delete Component");
            ContextMenuItem resetComponentButton = _componentPropertiesMenu.AddItem("Reset");

            _componentPropertiesMenu.Enabled = false;
            _componentPropertiesMenu.BringToFront();

            deleteComponentButton.Click += new ContextMenuItemEvent(OnDeleteComponentButtonClick);
            resetComponentButton.Click += new ContextMenuItemEvent(OnResetComponentButtonClick);
        }

        private void OnComponentPropertiesButtonClick(UIWidget sender, MouseEventArgs e)
        {
            _componentPropertiesMenuLeftParameter.Value = e.X;
            _componentPropertiesMenuTopParameter.Value = e.Y;

            _componentPropertiesMenu.SetNonUpdatedWithChildrenOnAllDomains();
            _componentPropertiesMenu.Enabled = true;
            _componentPropertiesMenu.BringToFront();
        }

        private void OnDeleteComponentButtonClick(ContextMenuItem item)
        {
            _component.Entity.DeleteComponent(_component);
        }

        private void OnResetComponentButtonClick(ContextMenuItem item)
        {

        }
    }
}
