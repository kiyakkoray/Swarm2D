﻿/******************************************************************************
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

namespace Swarm2D.SceneEditor.GUIControllers
{
    class ComponentResourcePropertyEditor : ComponentPropertyEditor
    {
        private UIEditBox _propertyData;

        public ComponentResourcePropertyEditor(Component component, ComponentPropertyInfo componentFloatPropertyInfo)
            : base(component, componentFloatPropertyInfo)
        {

        }

        public override float CreateUI(UIFrame owner, float currentLegth)
        {
            {
                List<UIPositionParameter> positionParameters = FastGUI.GenerateStandardParameters(owner.Widget, 5.0f, currentLegth, owner.Width - 10.0f, 30.0f);

                Entity propertyLabelEntity = owner.CreateChildEntity("propertyLabel");
                UILabel propertyLabel = propertyLabelEntity.AddComponent<UILabel>();
                propertyLabel.Initialize(positionParameters);
                propertyLabel.Text = ComponentPropertyInfo.Name;

                currentLegth += 35.0f;
            }

            {
                List<UIPositionParameter> positionParameters = FastGUI.GenerateStandardParameters(owner.Widget, 5.0f, currentLegth, owner.Width - 10.0f, 30.0f);

                Entity propertyDataEntity = owner.CreateChildEntity("propertyData");
                _propertyData = propertyDataEntity.AddComponent<UIEditBox>();
                _propertyData.Initialize(positionParameters);

                Resource resource = ComponentPropertyInfo.PropertyInfo.GetGetMethod().Invoke(Component, new object[] { }) as Resource;

                if (resource != null)
                {
                    _propertyData.Text = resource.Name;
                }

                currentLegth += 35.0f;
            }

            {
                _propertyData.LostFocus += new UIEvent(OnDataLostFocus);
            }

            return currentLegth;
        }

        public override void Refresh()
        {
            if (_propertyData.Manager.CurrentFocusObject != _propertyData.Widget)
            {
                Resource resource = ComponentPropertyInfo.PropertyInfo.GetGetMethod().Invoke(Component, new object[] { }) as Resource;

                if (resource != null)
                {
                    _propertyData.Text = resource.Name;
                }
            }
        }

        void OnDataLostFocus(UIWidget sender)
        {
            UIFrame frame = sender.GetComponent<UIFrame>();

            Resource resource = Resource.GetResource(ComponentPropertyInfo.PropertyInfo.PropertyType, frame.Text);
            //Sprite sprite = Engine.Engine.Project.GetSprite(sender.Text);

            ComponentPropertyInfo.PropertyInfo.GetSetMethod().Invoke(Component, new object[] { resource });
        }
    }
}
