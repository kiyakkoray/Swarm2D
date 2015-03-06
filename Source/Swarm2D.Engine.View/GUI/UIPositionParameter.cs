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
using Swarm2D.Library;
using Swarm2D.Engine.View.GUI.PositionParameters;

namespace Swarm2D.Engine.View.GUI
{
    public enum AnchorSide
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public enum AnchorToSideType
    {
        Inner,
        Outer
    }

    public enum AnchorToCenterType
    {
        Horizontal,
        Vertical
    }

    public delegate Vector2 CoordinateDelegate(object Parameter);
    public delegate float HeightDelegate(object Parameter);

    public enum ParameterType
    {
        Horizontal,
        Vertical,
        Both
    }

    public enum ScaleType
    {
        None,
        RootHorizontalScale,
        RootVerticalScale,
        OwnerWidthScale,
        OwnerHeightScale
    }

    internal enum PositionParameterExecuteType
    {
        BeforeCalculations,
        AfterCalculations
    }

    public abstract class UIPositionParameter
    {
        internal ParameterType TypeOfParameter;

        //CoordinateDelegate coordinateDelegate = null;
        //HeightDelegate heightDelegate = null;
        internal Object Parameter;

        public UIWidget AnchorTo { get; protected set; }
        public float Value { get; set; }
        public ScaleType TypeOfScale { get; set; }

        /* Side Anchor Parameters */
        public AnchorSide SideOfAnchor { get; set; }
        public AnchorToSideType TypeOfAnchor { get; set; }

        /* Center Anchor Parameters */
        internal AnchorToCenterType TypeOfCenterAnchor { get; set; }

        internal PositionParameterExecuteType ExecuteType { get; set; }
        internal int PositionOrder { get; set; }
        public UIWidget Owner { get; internal set; }

        protected UIPositionParameter()
        {
            TypeOfParameter = ParameterType.Both;
            Parameter = null;
            Value = 0;
            TypeOfScale = ScaleType.None;
            ExecuteType = PositionParameterExecuteType.BeforeCalculations;
            PositionOrder = 0;
            Owner = null;
            AnchorTo = null;
        }

        internal abstract void DoWork(UIWidget widget, ref PositionUpdateState updateVariables, UIRegionDomain regionDomain);

        public static AnchorToSide AnchorToSideParameter(UIWidget anchorTo, AnchorSide sideOfAnchor, AnchorToSideType typeOfAnchor, float offset = 0, ScaleType typeOfScale = ScaleType.None)
        {
            return new AnchorToSide(anchorTo, sideOfAnchor, typeOfAnchor, offset, typeOfScale);
        }

        //public static UIPositionParameter RelativeToObjectOwner()
        //{
        //	return new RelativeToObjectOwner();
        //}

        public static AnchorToCenter AnchorToCenterParameter(UIWidget anchorTo, AnchorToCenterType typeOfAlignment, float offset = 0, ScaleType typeOfScale = ScaleType.None)
        {
            return new AnchorToCenter(anchorTo, typeOfAlignment, offset, typeOfScale);
        }

        public static UIPositionParameter FitTo(UIWidget fitTo)
        {
            return new FitTo(fitTo);
        }

        //public static UIPositionParameter FitToAndKeepAspect(UIFrame FitTo, float aspectRatio)
        //{
        //	return new FitToAndKeepAspect(FitTo, aspectRatio);
        //}

        public static UIPositionParameter FitToDomain()
        {
            return new FitToDomain();
        }

        public static SetWidth SetWidth(float width, ScaleType typeOfScale = ScaleType.None)
        {
            return new SetWidth(width, typeOfScale);
        }

        public static SetHeight SetHeight(float height, ScaleType typeOfScale = ScaleType.None)
        {
            return new SetHeight(height, typeOfScale);
        }

        //public static UIPositionParameter SameWidthAs(UIFrame Relative)
        //{
        //	return new SameWidthAs(Relative);
        //}
        //
        //public static UIPositionParameter SameHeightAs(UIFrame Relative)
        //{
        //	return new SameHeightAs(Relative);
        //}
        //
        //public static UIPositionParameter CoordinatesFromDelegate(CoordinateDelegate coordinateDelegate, object parameter)
        //{
        //	return new CoordinatesFromDelegate(coordinateDelegate, parameter);
        //}
        //
        //public static UIPositionParameter HeightFromDelegate(HeightDelegate heightDelegate, object parameter)
        //{
        //	return new HeightFromDelegate(heightDelegate, parameter);
        //}
        //
        //public static UIPositionParameter DependTo(UIFrame uiObject)
        //{
        //	return new DependTo(uiObject);
        //}
        //
        //public static UIPositionParameter RelativeToCursorX(float Offset = 0, ScaleType TypeOfScale = ScaleType.None)
        //{
        //	return new RelativeToCursorX(Offset, TypeOfScale);
        //}
        //
        //public static UIPositionParameter RelativeToCursorY(float Offset = 0, ScaleType TypeOfScale = ScaleType.None)
        //{
        //	return new RelativeToCursorY(Offset, TypeOfScale);
        //}

        public static StayInOwner StayInOwner()
        {
            return new StayInOwner();
        }

        public void ChangeAnchorToParameter(UIWidget newAnchorTo)
        {
            AnchorTo.ChildPositionParameters.Remove(this);
            newAnchorTo.ChildPositionParameters.Add(this);

            AnchorTo = newAnchorTo;
        }
    }
}