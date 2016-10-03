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
using System.IO;
using System.Linq;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.View.GUI.PositionParameters;
using System.Xml;
using Swarm2D.Library;

namespace Swarm2D.Engine.View.GUI
{
    public static class FastGUI
    {
        public static List<UIPositionParameter> GenerateStandardParameters(UIWidget anchorTo, float x, float y, float width, float height)
        {
            List<UIPositionParameter> frameParameters = new List<UIPositionParameter>();

            frameParameters.Add
            (
                UIPositionParameter.AnchorToSideParameter
                (
                    anchorTo,
                    AnchorSide.Top,
                    AnchorToSideType.Inner,
                    y
                )
            );

            frameParameters.Add
            (
                UIPositionParameter.AnchorToSideParameter
                (
                    anchorTo,
                    AnchorSide.Left,
                    AnchorToSideType.Inner,
                    x
                )
            );

            frameParameters.Add
            (
                UIPositionParameter.SetWidth
                (
                    width
                )
            );

            frameParameters.Add
            (
                UIPositionParameter.SetHeight
                (
                    height
                )
            );

            return frameParameters;
        }

        public static List<UIPositionParameter> GenerateTetraAnchorParameters(UIWidget left, UIWidget right, UIWidget top, UIWidget bottom, float leftGap, float rightGap, float topGap, float bottomGap)
        {
            List<UIPositionParameter> frameParameters = new List<UIPositionParameter>();

            frameParameters.Add
            (
                UIPositionParameter.AnchorToSideParameter
                (
                    top,
                    AnchorSide.Top,
                    AnchorToSideType.Inner,
                    topGap
                )
            );

            frameParameters.Add
            (
                UIPositionParameter.AnchorToSideParameter
                (
                    left,
                    AnchorSide.Left,
                    AnchorToSideType.Inner,
                    leftGap
                )
            );

            frameParameters.Add
            (
                UIPositionParameter.AnchorToSideParameter
                (
                    bottom,
                    AnchorSide.Bottom,
                    AnchorToSideType.Inner,
                    bottomGap
                )
            );

            frameParameters.Add
            (
                UIPositionParameter.AnchorToSideParameter
                (
                    right,
                    AnchorSide.Right,
                    AnchorToSideType.Inner,
                    rightGap
                )
            );

            return frameParameters;
        }

        public static UIFrame CreateFrame(UIWidget parent, string name, float x, float y, float width, float height)
        {
            List<UIPositionParameter> frameParameters = GenerateStandardParameters(parent, x, y, width, height);

            Entity frameEntity = parent.CreateChildEntity(name);
            UIFrame frame = frameEntity.AddComponent<UIFrame>();
            frame.Initialize(frameParameters);
            frame.Name = name;

            return frame;
        }

        public static UILabel CreateLabel(UIWidget parent, string name, float x, float y, float width, float height)
        {
            List<UIPositionParameter> labelParameters = GenerateStandardParameters(parent, x, y, width, height);

            Entity labelEntity = parent.CreateChildEntity(name);
            UILabel label = labelEntity.AddComponent<UILabel>();
            label.Initialize(labelParameters);
            label.Name = name;

            return label;
        }

        public static UIButton CreateButton(UIWidget parent, string name, float x, float y, float width, float height)
        {
            List<UIPositionParameter> labelParameters = GenerateStandardParameters(parent, x, y, width, height);

            Entity buttonEntity = parent.CreateChildEntity(name);
            UIButton button = buttonEntity.AddComponent<UIButton>();
            button.Initialize(labelParameters);
            button.Name = name;

            return button;
        }

        public static UIWidget CreateFromXmlFile(string fileName, UIWidget parent)
        {
            byte[] fileData = File.ReadAllBytes(fileName);
            MemoryStream memoryStream = new MemoryStream(fileData);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(memoryStream);
            //xmlDocument->DebugPrint();

            UIWidget widget = CreateFromXmlElement(xmlDocument.ChildNodes[0], parent);

            FillPositionParametersFromXml(widget, xmlDocument.ChildNodes[0], widget);

            return widget;
        }

        public static void FillFromXmlFile(string fileName, UIWidget existingWidget)
        {
            byte[] fileData = File.ReadAllBytes(fileName);
            MemoryStream memoryStream = new MemoryStream(fileData);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(memoryStream);
            //xmlDocument->DebugPrint();

            UIWidget widget = CreateFromXmlElement(xmlDocument.ChildNodes[0], existingWidget.Owner, true, existingWidget);

            FillPositionParametersFromXml(widget, xmlDocument.ChildNodes[0], widget);
        }

        private static UIWidget CreateFromXmlElement(XmlNode xmlNode, UIWidget parent, bool useExisting = false, UIWidget existingWidget = null)
        {
            UIWidget widget = null;
            UIFrame frame = null;

            if (!useExisting)
            {
                if (xmlNode.Name == "UIFrame")
                {
                    Entity frameEntity = parent.CreateChildEntity("UIFrame");
                    widget = frameEntity.GetComponent<UIWidget>();
                    frame = frameEntity.AddComponent<UIFrame>();
                    frame.Initialize(new List<UIPositionParameter>());

                    ApplyUIFrameAttributesFromXmlAttributes(widget, xmlNode.Attributes);
                }
                else if (xmlNode.Name == "UILabel")
                {
                    Entity frameEntity = parent.CreateChildEntity("UILabel");
                    widget = frameEntity.GetComponent<UIWidget>();
                    frame = frameEntity.AddComponent<UILabel>();
                    frame.Initialize(new List<UIPositionParameter>());

                    ApplyUIFrameAttributesFromXmlAttributes(widget, xmlNode.Attributes);
                }
                else if (xmlNode.Name == "UIListBox")
                {
                    Entity frameEntity = parent.CreateChildEntity("UIListBox");
                    widget = frameEntity.GetComponent<UIWidget>();
                    frame = frameEntity.AddComponent<UIListBox>();
                    frame.Initialize(new List<UIPositionParameter>());

                    ApplyUIFrameAttributesFromXmlAttributes(widget, xmlNode.Attributes);
                }
                else if (xmlNode.Name == "UIButton")
                {
                    Entity frameEntity = parent.CreateChildEntity("UIButton");
                    widget = frameEntity.GetComponent<UIWidget>();
                    frame = frameEntity.AddComponent<UIButton>();
                    frame.Initialize(new List<UIPositionParameter>());

                    ApplyUIFrameAttributesFromXmlAttributes(widget, xmlNode.Attributes);

                    XmlNode dataNode = PlatformHelper.SelectSingleNode(xmlNode, "Data");

                    if (dataNode != null)
                    {
                        ApplyUIButtonDataFromXmlElement(frame as UIButton, dataNode);
                    }

                }
                else if (xmlNode.Name == "UIEditBox")
                {
                    Entity frameEntity = parent.CreateChildEntity("UIEditBox");
                    widget = frameEntity.GetComponent<UIWidget>();
                    frame = frameEntity.AddComponent<UIEditBox>();
                    frame.Initialize(new List<UIPositionParameter>());

                    ApplyUIFrameAttributesFromXmlAttributes(widget, xmlNode.Attributes);
                }
                else if (xmlNode.Name == "UIScrollViewer")
                {
                    Entity frameEntity = parent.CreateChildEntity("UIScrollViewer");
                    widget = frameEntity.GetComponent<UIWidget>();
                    frame = frameEntity.AddComponent<UIScrollViewer>();
                    frame.Initialize(new List<UIPositionParameter>());

                    ApplyUIFrameAttributesFromXmlAttributes(widget, xmlNode.Attributes);
                }
                else if (xmlNode.Name == "UITreeView")
                {
                    Entity frameEntity = parent.CreateChildEntity("UITreeView");
                    widget = frameEntity.GetComponent<UIWidget>();
                    frame = frameEntity.AddComponent<UITreeView>();
                    frame.Initialize(new List<UIPositionParameter>());

                    ApplyUIFrameAttributesFromXmlAttributes(widget, xmlNode.Attributes);
                }
            }
            else
            {
                widget = existingWidget;
                ApplyUIFrameAttributesFromXmlAttributes(widget, xmlNode.Attributes);
            }

            if (xmlNode.ChildNodes.Count > 1)
            {
                XmlNode childrenNode = xmlNode.ChildNodes[1];

                for (int i = 0; i < childrenNode.ChildNodes.Count; i++)
                {
                    CreateFromXmlElement(childrenNode.ChildNodes[i], widget);
                }
            }

            return widget;
        }

        private static void ApplyUIButtonDataFromXmlElement(UIButton button, XmlNode xmlNode)
        {
            for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
            {
                XmlNode spriteNode = xmlNode.ChildNodes[i];

                XmlAttribute stateAttribue = spriteNode.Attributes["State"];
                XmlAttribute nameAttribute = spriteNode.Attributes["Name"];

                UIButtonRenderState enumValue = (UIButtonRenderState)Enum.Parse(typeof(UIButtonRenderState), stateAttribue.Value);

                Sprite sprite = Resource.GetResource<Sprite>(nameAttribute.Value);

                button.AddSprite(enumValue, sprite);
            }
        }

        #region Position Parameters Section

        private static void AddParameterToFrameFromXmlElement(UIWidget frame, XmlNode parameterElement, UIWidget createdRoot)
        {
            UIPositionParameter positionParameter = null;

            if (parameterElement.Name == "AnchorToSideParameter")
            {
                positionParameter = ReadAnchorToSideParameterFromXmlAttributes(frame, parameterElement, createdRoot);
            }
            else if (parameterElement.Name == "AnchorToCenterParameter")
            {
                positionParameter = ReadAnchorToCenterParameterFromXmlAttributes(frame, parameterElement, createdRoot);
            }
            else if (parameterElement.Name == "SetWidth")
            {
                positionParameter = ReadSetWidthParameterFromXmlAttributes(frame, parameterElement, createdRoot);
            }
            else if (parameterElement.Name == "SetHeight")
            {
                positionParameter = ReadSetHeightParameterFromXmlAttributes(frame, parameterElement, createdRoot);
            }
            else if (parameterElement.Name == "StayInOwner")
            {
                positionParameter = UIPositionParameter.StayInOwner();
            }
            else if (parameterElement.Name == "FitTo")
            {
                positionParameter = ReadFitToParameterFromXmlAttributes(frame, parameterElement, createdRoot);
            }

            if (positionParameter != null)
            {
                frame.AddPositionParameter(positionParameter);
            }
        }

        private static void FillPositionParametersFromXml(UIWidget frame, XmlNode xmlElement, UIWidget createdRoot)
        {
            XmlNode parametersNode = PlatformHelper.SelectSingleNode(xmlElement, "Parameters");
            //wprintf(L"applying parameters to %s\n", frame->Name().GetText());

            for (int i = 0; i < parametersNode.ChildNodes.Count; i++)
            {
                AddParameterToFrameFromXmlElement(frame, parametersNode.ChildNodes[i], createdRoot);
            }

            XmlNode childrenNode = PlatformHelper.SelectSingleNode(xmlElement, "Children");

            if (childrenNode != null)
            {
                for (int i = 0; i < childrenNode.ChildNodes.Count; i++)
                {
                    FillPositionParametersFromXml(frame.LogicalChildren[childrenNode.ChildNodes.Count - i - 1],
                        childrenNode.ChildNodes[i], createdRoot);
                }
            }
        }

        private static void ApplyUIFrameAttributesFromXmlAttributes(UIWidget widget, XmlAttributeCollection attributes)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                string attributeName = attributes[i].Name;
                string attributeValue = attributes[i].Value;

                if (attributeName == "Name")
                {
                    //wprintf(L"Applying name %s\n",attributeValue.GetText());
                    widget.Name = attributeValue;
                }
                else if (attributeName == "Enabled")
                {
                    if (attributeValue == "True")
                    {
                        widget.Enabled = true;
                    }
                    else if (attributeValue == "False")
                    {
                        widget.Enabled = false;
                    }
                }
                else if (attributeName == "Text")
                {
                    if (widget.GetComponent<UIFrame>() != null)
                    {
                        widget.GetComponent<UIFrame>().Text = attributeValue;
                    }
                    else
                    {
                        Debug.Log("TODO bak suna acil!!!");
                    }
                }
            }
        }

        private static AnchorToSide ReadAnchorToSideParameterFromXmlAttributes(UIWidget frame, XmlNode parameterElement, UIWidget createdRoot)
        {
            UIWidget anchorTo = null;
            AnchorSide anchorSide;
            AnchorToSideType typeOfAnchor;
            float offset = 0.0f;
            ScaleType scaleType = ScaleType.None;

            XmlAttribute anchorToAttribute = parameterElement.Attributes["anchorTo"];

            if (anchorToAttribute != null)
            {
                string anchorToName = anchorToAttribute.Value;

                if (anchorToName == ":Root")
                {
                    anchorTo = frame.Manager.RootWidget;
                }
                else if (anchorToName == ":Parent")
                {
                    anchorTo = createdRoot.Owner;
                }
                else if (anchorToName == ":LogicalOwner")
                {
                    anchorTo = frame.Owner;
                }
                else if (anchorToName == ":CreatedRoot")
                {
                    anchorTo = createdRoot;
                }
                else
                {
                    anchorTo = createdRoot.Owner.GetChildAtPath(anchorToName);
                }
            }

            if (anchorTo == null)
            {
                return null;
            }

            XmlAttribute sideAttribue = parameterElement.Attributes["Side"];

            if (sideAttribue != null)
            {
                string anchorSideName = sideAttribue.Value;

                if (anchorSideName == "Top")
                {
                    anchorSide = AnchorSide.Top;
                }
                else if (anchorSideName == "Bottom")
                {
                    anchorSide = AnchorSide.Bottom;
                }
                else if (anchorSideName == "Right")
                {
                    anchorSide = AnchorSide.Right;
                }
                else if (anchorSideName == "Left")
                {
                    anchorSide = AnchorSide.Left;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            XmlAttribute typeAttribue = parameterElement.Attributes["Type"];

            if (typeAttribue != null)
            {
                string anchorTypeName = typeAttribue.Value;

                if (anchorTypeName == "Inner")
                {
                    typeOfAnchor = AnchorToSideType.Inner;
                }
                else if (anchorTypeName == "Outer")
                {
                    typeOfAnchor = AnchorToSideType.Outer;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            XmlAttribute offsetAttribute = parameterElement.Attributes["Offset"];

            if (offsetAttribute != null)
            {
                string offsetValueName = offsetAttribute.Value;
                offset = ReadFloat(offsetValueName);
            }

            XmlAttribute typeOfScaleAttribute = parameterElement.Attributes["TypeOfScale"];

            if (typeOfScaleAttribute != null)
            {
                if (!ReadScaleTypeFromXmlAttributes(typeOfScaleAttribute, ref scaleType))
                {
                    return null;
                }
            }

            return new AnchorToSide(anchorTo, anchorSide, typeOfAnchor, offset, scaleType);
        }

        private static AnchorToCenter ReadAnchorToCenterParameterFromXmlAttributes(UIWidget frame, XmlNode parameterElement, UIWidget createdRoot)
        {
            UIWidget anchorTo = null;
            AnchorToCenterType typeOfAnchor;
            float offset = 0.0f;
            ScaleType scaleType = ScaleType.None;

            XmlAttribute anchorToAttribute = parameterElement.Attributes["anchorTo"];

            if (anchorToAttribute != null)
            {
                string anchorToName = anchorToAttribute.Value;

                if (anchorToName == ":Root")
                {
                    anchorTo = frame.Manager.RootWidget;
                }
                else if (anchorToName == ":Parent")
                {
                    anchorTo = createdRoot.Owner;
                }
                else if (anchorToName == ":LogicalOwner")
                {
                    anchorTo = frame.Owner;
                }
                else if (anchorToName == ":CreatedRoot")
                {
                    anchorTo = createdRoot;
                }
                else
                {
                    anchorTo = createdRoot.Owner.GetChildAtPath(anchorToName);
                }
            }

            if (anchorTo == null)
            {
                return null;
            }

            XmlAttribute typeAttribue = parameterElement.Attributes["Type"];

            if (typeAttribue != null)
            {
                string anchorTypeName = typeAttribue.Value;

                if (anchorTypeName == "Horizontal")
                {
                    typeOfAnchor = AnchorToCenterType.Horizontal;
                }
                else if (anchorTypeName == "Vertical")
                {
                    typeOfAnchor = AnchorToCenterType.Vertical;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            XmlAttribute offsetAttribute = parameterElement.Attributes["Offset"];

            if (offsetAttribute != null)
            {
                string offsetValueName = offsetAttribute.Value;
                offset = ReadFloat(offsetValueName);
            }

            XmlAttribute typeOfScaleAttribute = parameterElement.Attributes["TypeOfScale"];

            if (typeOfScaleAttribute != null)
            {
                if (!ReadScaleTypeFromXmlAttributes(typeOfScaleAttribute, ref scaleType))
                {
                    return null;
                }
            }

            return new AnchorToCenter(anchorTo, typeOfAnchor, offset, scaleType);
        }

        private static SetWidth ReadSetWidthParameterFromXmlAttributes(UIWidget frame, XmlNode parameterElement, UIWidget createdRoot)
        {
            float width = 0.0f;
            ScaleType scaleType = ScaleType.None;

            XmlAttribute widthAttribute = parameterElement.Attributes["Width"];

            if (widthAttribute != null)
            {
                String widthName = widthAttribute.Value;
                width = ReadFloat(widthName);
            }
            else
            {
                return null;
            }

            XmlAttribute typeOfScaleAttribute = parameterElement.Attributes["TypeOfScale"];

            if (typeOfScaleAttribute != null)
            {
                if (!ReadScaleTypeFromXmlAttributes(typeOfScaleAttribute, ref scaleType))
                {
                    return null;
                }
            }

            return new SetWidth(width, scaleType);
        }

        private static SetHeight ReadSetHeightParameterFromXmlAttributes(UIWidget frame, XmlNode parameterElement, UIWidget createdRoot)
        {
            float height = 0.0f;
            ScaleType scaleType = ScaleType.None;

            XmlAttribute heightAttribute = parameterElement.Attributes["Height"];

            if (heightAttribute != null)
            {
                String heightName = heightAttribute.Value;
                height = ReadFloat(heightName);
            }
            else
            {
                return null;
            }

            XmlAttribute typeOfScaleAttribute = parameterElement.Attributes["TypeOfScale"];

            if (typeOfScaleAttribute != null)
            {
                if (!ReadScaleTypeFromXmlAttributes(typeOfScaleAttribute, ref scaleType))
                {
                    return null;
                }
            }

            return new SetHeight(height, scaleType);
        }

        private static FitTo ReadFitToParameterFromXmlAttributes(UIWidget frame, XmlNode parameterElement, UIWidget createdRoot)
        {
            UIWidget anchorTo = null;

            XmlAttribute anchorToAttribute = parameterElement.Attributes["anchorTo"];

            if (anchorToAttribute != null)
            {
                string anchorToName = anchorToAttribute.Value;

                if (anchorToName == ":Root")
                {
                    anchorTo = frame.Manager.RootWidget;
                }
                else if (anchorToName == ":Parent")
                {
                    anchorTo = createdRoot.Owner;
                }
                else if (anchorToName == ":LogicalOwner")
                {
                    anchorTo = frame.Owner;
                }
                else if (anchorToName == ":CreatedRoot")
                {
                    anchorTo = createdRoot;
                }
                else
                {
                    anchorTo = createdRoot.Owner.GetChildAtPath(anchorToName);
                }
            }

            if (anchorTo == null)
            {
                return null;
            }

            return new FitTo(anchorTo);
        }

        private static bool ReadScaleTypeFromXmlAttributes(XmlAttribute scaleTypeAttribute, ref ScaleType output)
        {
            string scaleTypeName = scaleTypeAttribute.Name;

            if (scaleTypeName == "None")
            {
                output = ScaleType.None;
            }
            else if (scaleTypeName == "OwnerWidthScale")
            {
                output = ScaleType.OwnerWidthScale;
            }
            else if (scaleTypeName == "OwnerHeightScale")
            {
                output = ScaleType.OwnerHeightScale;
            }
            else if (scaleTypeName == "RootHorizontalScale")
            {
                output = ScaleType.RootHorizontalScale;
            }
            else if (scaleTypeName == "RootVerticalScale")
            {
                output = ScaleType.RootVerticalScale;
            }
            else
            {
                return false;
            }

            return true;
        }

        #endregion

        private static float ReadFloat(string text)
        {
            //float readValue = Convert.ToSingle(text.Remove(text.Length - 1)) / 10.0f;
            float readValue = Convert.ToSingle(text.Remove(text.Length - 1));
            Debug.Log("Read Value: " + readValue);
            return readValue;
        }
    }
}
