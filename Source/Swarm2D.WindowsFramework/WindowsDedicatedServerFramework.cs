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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View;
using Swarm2D.Library;
using Swarm2D.WindowsFramework.Native.Windows;
using Framework = Swarm2D.Engine.Core.Framework;

namespace Swarm2D.WindowsFramework
{
    public class WindowsDedicatedServerFramework : Framework
    {
        private Thread _mainThread;

        private FrameworkDomain[] _frameworkDomains;

        public WindowsDedicatedServerFramework()
        {
            Debug.Initialize(new WindowsDebug());
        }

        public override void Initialize(string resourcesPath, FrameworkDomain[] frameworkDomains)
        {
            Resources.Initialize(resourcesPath);
            _frameworkDomains = frameworkDomains;
        }

        void MessageLoop()
        {

        }

        void MainLoop()
        {
            while (true)
            {
                Update();
            }
        }

        void Update()
        {
            foreach (FrameworkDomain frameworkDomain in _frameworkDomains)
            {
                frameworkDomain.Update();
            }
        }

        public override void Start()
        {
            _mainThread = new Thread(MainLoop);

            _mainThread.SetApartmentState(ApartmentState.STA);
            _mainThread.Start();

            _mainThread.Join();
        }

        #region Resources

        public override string ResourcesName { get { return Resources.ResourcesName; } }

        public override string ResourcesPath { get { return Resources.ResourcesPath; } }

        public override byte[] LoadBinaryData(string name)
        {
            return Resources.LoadBinaryData(name);
        }

        public override byte[] LoadBinaryData(string resourcesName, string name)
        {
            return Resources.LoadBinaryData(resourcesName, name);
        }

        public override string LoadTextData(string name)
        {
            throw new NotImplementedException();
        }

        public override string LoadTextData(string resourcesName, string name)
        {
            throw new NotImplementedException();
        }

        public override XmlDocument LoadXmlData(string resourcesName, string name)
        {
            return Resources.LoadXmlData(resourcesName, name);
        }

        public override XmlDocument LoadXmlData(string name)
        {
            return Resources.LoadXmlData(name);
        }

        public override void SaveBinaryData(string name, byte[] data)
        {
            Resources.SaveBinaryData(name, data);
        }

        public override void SaveXmlData(string name, XmlDocument xmlDocument)
        {
            Resources.SaveXmlData(name, xmlDocument);
        }

        public override string[] GetFilesInFolder(String path, ResourceFileType fileType)
        {
            return Resources.GetFilesInFolder(path, fileType);
        }

        public override string GetResourcesNameOf(string fileName)
        {
            return Resources.GetResourcesNameOf(fileName);
        }

        #endregion
    }
}
