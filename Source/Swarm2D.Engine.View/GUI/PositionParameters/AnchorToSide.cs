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

namespace Swarm2D.Engine.View.GUI.PositionParameters
{
    public class AnchorToSide : UIPositionParameter
    {
        public AnchorToSide(UIWidget anchorTo, AnchorSide sideOfAnchor, AnchorToSideType typeOfAnchor, float offset = 0, ScaleType typeOfScale = ScaleType.None)
        {
            AnchorTo = anchorTo;
            SideOfAnchor = sideOfAnchor;
            Value = offset;
            TypeOfScale = typeOfScale;
            PositionOrder = 2;
            TypeOfAnchor = typeOfAnchor;

            switch (SideOfAnchor)
            {
                case AnchorSide.Left:
                    TypeOfParameter = ParameterType.Horizontal;
                    break;
                case AnchorSide.Right:
                    TypeOfParameter = ParameterType.Horizontal;
                    break;
                case AnchorSide.Top:
                    TypeOfParameter = ParameterType.Vertical;
                    break;
                case AnchorSide.Bottom:
                    TypeOfParameter = ParameterType.Vertical;
                    break;
                default:
                    break;
            }
        }

        internal override void DoWork(UIWidget widget, ref PositionUpdateState updateVariables, UIRegionDomain regionDomain)
        {
            float resultValue = widget.GetScaleResult(Value, TypeOfScale);

            float anchorX = AnchorTo.GetRegion(regionDomain).X;
            float anchorY = AnchorTo.GetRegion(regionDomain).Y;

            float anchorWidth = AnchorTo.GetRegion(regionDomain).Width;
            float anchorHeight = AnchorTo.GetRegion(regionDomain).Height;

            bool inner = TypeOfAnchor == AnchorToSideType.Inner;

            if (inner)
            {
                switch (SideOfAnchor)
                {
                    case AnchorSide.Left:
                        if (!updateVariables.leftControl)
                        {
                            updateVariables.leftControl = true;
                            updateVariables.x1 = anchorX + resultValue;
                        }
                        break;
                    case AnchorSide.Right:
                        if (!updateVariables.rightControl)
                        {
                            updateVariables.rightControl = true;
                            updateVariables.x2 = anchorX + anchorWidth - resultValue;
                        }
                        break;
                    case AnchorSide.Top:
                        if (!updateVariables.topControl)
                        {
                            updateVariables.topControl = true;
                            updateVariables.y1 = anchorY + resultValue;
                        }
                        break;
                    case AnchorSide.Bottom:
                        if (!updateVariables.bottomControl)
                        {
                            updateVariables.bottomControl = true;
                            updateVariables.y2 = anchorY + anchorHeight - resultValue;
                        }
                        break;
                    default:
                        break;
                }
            }//inner
            else
            {
                switch (SideOfAnchor)
                {
                    case AnchorSide.Left:
                        if (!updateVariables.rightControl)
                        {
                            updateVariables.rightControl = true;

                            updateVariables.x2 = anchorX - resultValue;
                        }
                        break;
                    case AnchorSide.Right:
                        if (!updateVariables.leftControl)
                        {
                            updateVariables.leftControl = true;

                            updateVariables.x1 = anchorX + anchorWidth + resultValue;
                        }
                        break;
                    case AnchorSide.Top:
                        if (!updateVariables.bottomControl)
                        {
                            updateVariables.bottomControl = true;

                            updateVariables.y2 = anchorY - resultValue;
                        }
                        break;
                    case AnchorSide.Bottom:
                        if (!updateVariables.topControl)
                        {
                            updateVariables.topControl = true;
                            updateVariables.y1 = anchorY + anchorHeight + resultValue;
                        }
                        break;
                    default:
                        break;
                }
            }//else inner
        }
    }
}
