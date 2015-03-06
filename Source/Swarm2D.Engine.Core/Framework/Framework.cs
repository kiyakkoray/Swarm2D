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
using System.Xml;
using Swarm2D.Engine.Core;
using Swarm2D.Library;

namespace Swarm2D.Engine.Core
{
    public abstract class Framework
    {
        public static Framework Current { get; private set; }

        protected Framework()
        {
            Current = this;
        }

        public abstract void Initialize(string resourcesPath, FrameworkDomain[] frameworkDomains);

        public abstract void Start();

        #region Resources

        public abstract string ResourcesName { get; }

        public abstract string ResourcesPath { get; }

        public abstract byte[] LoadBinaryData(string name);

        public abstract byte[] LoadBinaryData(string resourcesName, string name);

        public abstract string LoadTextData(string name);

        public abstract string LoadTextData(string resourcesName, string name);

        public abstract XmlDocument LoadXmlData(string resourcesName, string name);

        public abstract XmlDocument LoadXmlData(string name);

        public abstract void SaveBinaryData(string name, byte[] data);

        public abstract void SaveXmlData(string name, XmlDocument xmlDocument);

        public abstract string[] GetFilesInFolder(String path, ResourceFileType fileType);

        public abstract string GetResourcesNameOf(string fileName);

        #endregion
    }
}
