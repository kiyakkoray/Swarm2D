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
    public class StayInOwner : UIPositionParameter
    {
        public StayInOwner()
        {
            TypeOfParameter = ParameterType.Both;
            PositionOrder = 1;
            ExecuteType = PositionParameterExecuteType.AfterCalculations;
        }

        internal override void DoWork(UIWidget widget, ref PositionUpdateState updateVariables, UIRegionDomain regionDomain)
        {
            float resultValue = widget.GetScaleResult(Value, TypeOfScale);

            UIWidget ownerObject = widget.Owner;

            if (updateVariables.x1 < ownerObject.GetRegion(regionDomain).X)
            {
                updateVariables.x1 = 0;
            }

            if ((updateVariables.x1 + updateVariables.width) > (ownerObject.GetRegion(regionDomain).X + ownerObject.GetRegion(regionDomain).Width))
            {
                updateVariables.x1 = (ownerObject.X + ownerObject.Width) - updateVariables.width;
            }

            if (updateVariables.y1 < ownerObject.GetRegion(regionDomain).Y)
            {
                updateVariables.y1 = 0;
            }

            if ((updateVariables.y1 + updateVariables.height) > (ownerObject.GetRegion(regionDomain).Y + ownerObject.GetRegion(regionDomain).Height))
            {
                updateVariables.y1 = (ownerObject.GetRegion(regionDomain).Y + ownerObject.GetRegion(regionDomain).Height) - updateVariables.height;
            }
        }
    }
}
