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
using System.Windows.Forms;
using System.Xml;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.View;
using Swarm2D.Game;
using Swarm2D.Engine.Logic;
using Swarm2D.SceneEditor;
using Swarm2D.Library;
using Swarm2D.WindowsDebugger;
using Swarm2D.WindowsFramework;

namespace Swarm2D.Starter
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Debug.Log("Welcome to Sx Testbed");
            Debug.Log("1. Testbed");
            Debug.Log("2. Scene Editor");

            int selection = 0;

            try
            {
                selection = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {

            }

            WindowsLogicFramework windowsLogicFramework = new WindowsLogicFramework();
            WindowsViewFramework windowsViewFramework = new WindowsViewFramework();

            Engine.Core.Engine engine = new Engine.Core.Engine(false);

            string name = "";

            Entity rootEntity = engine.RootEntity;

            if (selection == 1)
            {
                name = "SxTest";

                rootEntity.AddComponent<IOSystem>();
                rootEntity.AddComponent<DebugSpriteLoader>();
                rootEntity.AddComponent<NetworkController>();
                rootEntity.AddComponent<Testbed>();
                rootEntity.AddComponent<DebugPanel>();
            }
            else if (selection == 2)
            {
                name = SelectProject();

                rootEntity.AddComponent<IOSystem>();
                rootEntity.AddComponent<DebugSpriteLoader>();
                rootEntity.AddComponent<SceneEditorDomain>(); 
                rootEntity.AddComponent<DebugPanel>();
            }

            engine.Start();

            windowsLogicFramework.Initialize(name, new FrameworkDomain[] { engine });
            windowsLogicFramework.Start();
        }

        private static string SelectProject()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Project files (*.bytes)|*.bytes";

            DialogResult result = openFileDialog.ShowDialog();

            string projectName = "";

            if (result == DialogResult.OK)
            {
                string fullPath = Path.GetFullPath(openFileDialog.FileName);
                string mainPath = Directory.GetParent(fullPath).FullName;

                DirectoryInfo directoryInfo = new DirectoryInfo(mainPath);
                projectName = directoryInfo.Name;
            }

            return projectName;
        }
    }

    public class DebugSpriteLoader : EngineComponent
    {
        protected override void OnStart()
        {
            base.OnStart();

            LoadTemporarySprites();
        }

        private void LoadTemporarySprites()
        {
            SpriteData spriteData = new SpriteData("spriteData");
            spriteData.Load();

            SpriteCategory otherSpriteCategory = spriteData.SpriteCategories["Other"];
            otherSpriteCategory.Load();
        }
    }
}