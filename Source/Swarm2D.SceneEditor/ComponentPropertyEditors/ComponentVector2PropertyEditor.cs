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
using Swarm2D.Engine.View.GUI;
using Swarm2D.Library;

namespace Swarm2D.SceneEditor.GUIControllers
{
    class ComponentVector2PropertyEditor : ComponentPropertyEditor
    {
        private UIEditBox _propertyDataX;
        private UIEditBox _propertyDataY;

        public ComponentVector2PropertyEditor(Component component, ComponentPropertyInfo componentFloatPropertyInfo)
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

            Vector2 vector2Value = (Vector2)ComponentPropertyInfo.PropertyInfo.GetGetMethod().Invoke(Component, new object[] { });

            int firstWidth = (int)((owner.Width - 15.0f) * 0.5f);
            int SecondWidth = (int)(owner.Width - 15.0f) - firstWidth;

            {
                List<UIPositionParameter> positionParameters = FastGUI.GenerateStandardParameters(owner.Widget, 5.0f, currentLegth, firstWidth, 30.0f);

                Entity propertyDataXEntity = owner.CreateChildEntity("propertyDataX");
                _propertyDataX = propertyDataXEntity.AddComponent<UIEditBox>();
                _propertyDataX.Initialize(positionParameters);
                _propertyDataX.Text = vector2Value.X.ToString();

                //currentLegth += 35.0f;
            }

            {
                List<UIPositionParameter> positionParameters = FastGUI.GenerateStandardParameters(owner.Widget, 10.0f + firstWidth, currentLegth, SecondWidth, 30.0f);

                Entity propertyDataYEntity = owner.CreateChildEntity("propertyDataY");
                _propertyDataY = propertyDataYEntity.AddComponent<UIEditBox>();
                _propertyDataY.Initialize(positionParameters);
                _propertyDataY.Text = vector2Value.Y.ToString();

                currentLegth += 35.0f;
            }

            {
                _propertyDataX.LostFocus += new UIEvent(OnDataXLostFocus);
                _propertyDataY.LostFocus += new UIEvent(OnDataYLostFocus);
            }

            return currentLegth;
        }

        public override void Refresh()
        {
            if (_propertyDataX.Manager.CurrentFocusObject != _propertyDataX.Widget)
            {
                Vector2 vector2Value = (Vector2)ComponentPropertyInfo.PropertyInfo.GetGetMethod().Invoke(Component, new object[] { });
                _propertyDataX.Text = vector2Value.X.ToString();
            }

            if (_propertyDataY.Manager.CurrentFocusObject != _propertyDataY.Widget)
            {
                Vector2 vector2Value = (Vector2)ComponentPropertyInfo.PropertyInfo.GetGetMethod().Invoke(Component, new object[] { });
                _propertyDataY.Text = vector2Value.Y.ToString();
            }
        }

        void OnDataXLostFocus(UIWidget sender)
        {
            UIFrame frame = sender.GetComponent<UIFrame>();

            float value = Convert.ToSingle(frame.Text);

            Vector2 vector2Value = (Vector2)ComponentPropertyInfo.PropertyInfo.GetGetMethod().Invoke(Component, new object[] { });

            vector2Value.X = value;

            ComponentPropertyInfo.PropertyInfo.GetSetMethod().Invoke(Component, new object[] { vector2Value });
        }

        void OnDataYLostFocus(UIWidget sender)
        {
            UIFrame frame = sender.GetComponent<UIFrame>();

            float value = Convert.ToSingle(frame.Text);

            Vector2 vector2Value = (Vector2)ComponentPropertyInfo.PropertyInfo.GetGetMethod().Invoke(Component, new object[] { });

            vector2Value.Y = value;

            ComponentPropertyInfo.PropertyInfo.GetSetMethod().Invoke(Component, new object[] { vector2Value });
        }
    }
}
