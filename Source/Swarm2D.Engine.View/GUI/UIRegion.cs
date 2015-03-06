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

namespace Swarm2D.Engine.View.GUI
{
    public struct PositionUpdateState
    {
        public float x1;
        public float y1;
        public float x2;
        public float y2;
        public float width;
        public float height;

        public bool leftControl;
        public bool rightControl;
        public bool topControl;
        public bool bottomControl;

        public bool widthControl;
        public bool heightControl;

        public void Reset()
        {
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 0;
            width = 0;
            height = 0;

            leftControl = false;
            rightControl = false;
            topControl = false;
            bottomControl = false;

            widthControl = false;
            heightControl = false;
        }
    }

    public class UIRegion
    {
        private float _x;
        private float _y;
        private float _width;
        private float _height;

        public float X
        {
            get
            {
                if (!Updated)
                {
                    SetNonUpdatedWithChildren();
                    UpdatePositionParameters();
                }

                return _x;
            }
        }

        public float Y
        {
            get
            {
                if (!Updated)
                {
                    SetNonUpdatedWithChildren();
                    UpdatePositionParameters();
                }

                return _y;
            }
        }

        public float Width
        {
            get
            {
                if (!Updated)
                {
                    SetNonUpdatedWithChildren();
                    UpdatePositionParameters();
                }
                return _width;
            }
        }

        public float Height
        {
            get
            {
                if (!Updated)
                {
                    SetNonUpdatedWithChildren();
                    UpdatePositionParameters();
                }
                return _height;
            }
        }

        public float RelativeMouseX { get; private set; }
        public float RelativeMouseY { get; private set; }

        public UIWidget Frame { get; private set; }
        public bool Updated { get; private set; }
        public bool CurrentlyUpdating { get; private set; }
        public UIRegionDomain Domain { get; set; }

        private bool updateStarted;

        private bool nonUpdateStarted;

        private UIManager _uiManager;

        public UIRegion(UIWidget frame)
        {
            _uiManager = frame.Manager;
            updateStarted = false;
            nonUpdateStarted = false;

            Frame = frame;
            CurrentlyUpdating = false;
            Updated = false;
        }

        public void UpdatePositionParameters()
        {
            if (Updated)
            {
                return;
            }

            if (updateStarted)
            {
                return;
            }

            updateStarted = true;

            if (CurrentlyUpdating)
            {
                Debug.Log("circular reference on position parameters\n");
                //throw new UICircularPositionReferenceException();
                //exception
            }

            for (int i = 0; i < Frame.DependedPositionParameters.Count(); i++)
            {
                UIWidget dependedPositionInfo = Frame.DependedPositionParameters[i].AnchorTo;

                if (!dependedPositionInfo.GetRegion(Domain).Updated)
                {
                    dependedPositionInfo.CurrentRegion.UpdatePositionParameters();
                }
            }

            //if (!Updated) //todo: bu ne ?
            //{
            //	UpdatePositionParameters();
            //}

            CurrentlyUpdating = true;

            PositionUpdateState updateVariables = new PositionUpdateState();
            updateVariables.Reset();

            for (int i = 0; i < Frame.FirstPositionParameters.Count; i++)
            {
                UIPositionParameter positionParameter = Frame.FirstPositionParameters[i];

                positionParameter.DoWork(Frame, ref updateVariables, Domain);
            }

            if (updateVariables.leftControl && updateVariables.rightControl)
            {
                updateVariables.width = updateVariables.x2 - updateVariables.x1;
            }
            else if (updateVariables.leftControl)
            {

            }
            else if (updateVariables.rightControl)
            {
                updateVariables.x1 = updateVariables.x2 - updateVariables.width;
            }
            else
            {

            }

            if (updateVariables.topControl && updateVariables.bottomControl)
            {
                updateVariables.height = updateVariables.y2 - updateVariables.y1;
            }
            else if (updateVariables.topControl)
            {
            }
            else if (updateVariables.bottomControl)
            {
                updateVariables.y1 = updateVariables.y2 - updateVariables.height;
            }
            else
            {
            }

            for (int i = 0; i < Frame.SecondPositionParameters.Count; i++)
            {
                UIPositionParameter positionParameter = Frame.SecondPositionParameters[i];

                positionParameter.DoWork(Frame, ref updateVariables, Domain);
            }

            _width = updateVariables.width;
            _height = updateVariables.height;

            _x = updateVariables.x1;
            _y = updateVariables.y1;

            CurrentlyUpdating = false;
            updateStarted = false;
            Updated = true;

            for (int i = 0; i < Frame.ChildPositionParameters.Count; i++)
            {
                UIWidget childPositionInfo = Frame.ChildPositionParameters[i].Owner;
                childPositionInfo.GetRegion(Domain).UpdatePositionParameters();
            }
        }

        public void SetNonUpdatedWithChildren()
        {
            SetNonUpdatedWithChildren(this);
        }

        private static void SetNonUpdatedWithChildren(UIRegion region)
        {
            if (region.nonUpdateStarted)
            {
                return;
            }

            region.Updated = false;
            region.nonUpdateStarted = true;

            for (int i = 0; i < region.Frame.ChildPositionParameters.Count; i++)
            {
                UIWidget childInfo = region.Frame.ChildPositionParameters[i].Owner;
                SetNonUpdatedWithChildren(childInfo.CurrentRegion);
            }

            region.nonUpdateStarted = false;
        }
    }
}
