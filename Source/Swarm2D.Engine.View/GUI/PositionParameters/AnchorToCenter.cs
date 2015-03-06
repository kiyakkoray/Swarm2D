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

namespace Swarm2D.Engine.View.GUI.PositionParameters
{
    public class AnchorToCenter : UIPositionParameter
    {
        public AnchorToCenter(UIWidget anchorTo, AnchorToCenterType typeOfAlignment, float offset, ScaleType typeOfScale = ScaleType.None)
        {
            AnchorTo = anchorTo;
            TypeOfCenterAnchor = typeOfAlignment;
            Value = offset;
            TypeOfScale = typeOfScale;
            PositionOrder = 4;

            if (typeOfAlignment == AnchorToCenterType.Horizontal)
            {
                TypeOfParameter = ParameterType.Horizontal;
            }
            else if (typeOfAlignment == AnchorToCenterType.Vertical)
            {
                TypeOfParameter = ParameterType.Vertical;
            }
        }

        internal override void DoWork(UIWidget widget, ref PositionUpdateState updateVariables, UIRegionDomain regionDomain)
        {
            float resultValue = widget.GetScaleResult(Value, TypeOfScale);

            float anchorX = AnchorTo.GetRegion(regionDomain).X;
            float anchorY = AnchorTo.GetRegion(regionDomain).Y;

            float anchorWidth = AnchorTo.GetRegion(regionDomain).Width;
            float anchorHeight = AnchorTo.GetRegion(regionDomain).Height;

            float anchorCenterX = anchorX + anchorWidth / 2;
            float anchorCenterY = anchorY + anchorHeight / 2;

            if (TypeOfCenterAnchor == AnchorToCenterType.Horizontal)
            {
                updateVariables.x1 = anchorCenterX - (updateVariables.width) / 2 + resultValue;
                updateVariables.x2 = anchorCenterX + (updateVariables.width) / 2 + resultValue;
                updateVariables.leftControl = updateVariables.rightControl = true;
            }
            else if (TypeOfCenterAnchor == AnchorToCenterType.Vertical)
            {
                updateVariables.y1 = anchorCenterY - (updateVariables.height) / 2 + resultValue;
                updateVariables.y2 = anchorCenterY + (updateVariables.height) / 2 + resultValue;
                updateVariables.topControl = updateVariables.bottomControl = true;
            }
        }
    }
}
