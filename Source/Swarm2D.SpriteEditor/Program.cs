/******************************************************************************
Copyright (c) 2016 Koray Kiyakoglu

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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View;
using Swarm2D.Library;
using Swarm2D.SpriteEditor;
using Swarm2D.WindowsFramework;
using Resources = Swarm2D.Engine.Core.Resources;

namespace Swarm2D.SpriteSheetGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string projectName = "SxTest";

            if (args != null && args.Length > 0)
            {
                projectName = args[0];
            }

            WindowsLogicFramework windowsLogicFramework = new WindowsLogicFramework();

            Engine.Core.Engine engine = new Engine.Core.Engine(windowsLogicFramework, false);
            Entity rootEntity = engine.RootEntity;

            rootEntity.AddComponent<SpriteEditorDomain>();

            engine.Start();

            windowsLogicFramework.Initialize(projectName, new FrameworkDomain[] { engine });
            windowsLogicFramework.Start();
        }
    }
}
