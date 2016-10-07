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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Swarm2D.Engine.Core;
using Swarm2D.Library;

namespace Swarm2D.UniversalWindowsPlatform.Framework
{
    public class LogicFramework : Swarm2D.Engine.Core.Framework
    {
        private Stopwatch _timer;

        private string _resourcesPath;

        private List<Assembly> _assemblies;
        private FrameworkDomain[] _frameworkDomains;

        public LogicFramework()
        {
            _assemblies = new List<Assembly>();
            _timer = new Stopwatch();
        }

        public override void Initialize(string resourcesPath, FrameworkDomain[] frameworkDomains)
        {
            _resourcesPath = resourcesPath;
            _frameworkDomains = frameworkDomains;
        }

        public override void Start()
        {
            _timer.Start();
        }

        public void Update()
        {
            for (int i = 0; i < _frameworkDomains.Length; i++)
            {
                FrameworkDomain frameworkDomain = _frameworkDomains[i];

                frameworkDomain.Update();
            }
        }

        public void AddGameAssembly(Assembly gameAssembly)
        {
            _assemblies.Add(gameAssembly);
        }

        #region Resources

        public override string ResourcesName { get { return _resourcesPath; } }

        public override string ResourcesPath { get { return _resourcesPath; } }

        public override byte[] LoadBinaryData(string name)
        {
            return LoadBinaryData(ResourcesName, name);
        }

        public override byte[] LoadBinaryData(string resourcesName, string name)
        {
            byte[] fileContent = File.ReadAllBytes(resourcesName + @"\" + name + ".bytes");
            return fileContent;
        }

        public override string LoadTextData(string name)
        {
            return LoadTextData(ResourcesName, name);
        }

        public override string LoadTextData(string resourcesName, string name)
        {
            return "";
        }

        public override XmlDocument LoadXmlData(string resourcesName, string name)
        {
            const string fileExtension = "xml";
            byte[] fileContent = File.ReadAllBytes(resourcesName + @"\" + name + "." + fileExtension);

            XmlDocument document = new XmlDocument();
            document.Load(new MemoryStream(fileContent));

            return document;
        }

        public override XmlDocument LoadXmlData(string name)
        {
            return LoadXmlData(ResourcesName, name);
        }

        public override void SaveBinaryData(string name, byte[] data)
        {
            
        }

        public override void SaveXmlData(string name, XmlDocument xmlDocument)
        {
            
        }

        public override string[] GetFilesInFolder(String path, ResourceFileType fileType)
        {
            return null;
        }

        public override string GetResourcesNameOf(string fileName)
        {
            return null;
        }

        #endregion

        #region Platform Helpers

        public override void Sleep(int miliSeconds)
        {
            Task.Delay(miliSeconds).Wait();
        }

        public override IThread CreateThread(ThreadStart threadStart)
        {
            return new Thread(threadStart);
        }

        public override Assembly[] GetGameAssemblies()
        {
            return _assemblies.ToArray();
        }

        public override Type GetBaseType(Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public override bool IsAbstract(Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        public override bool IsEnum(Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public override bool IsSubclassOf(Type type, Type otherType)
        {
            return type.GetTypeInfo().IsSubclassOf(otherType);
        }

        public override Delegate CreateDelegate(Type delegateType, object target, MethodInfo methodInfo)
        {
            return methodInfo.CreateDelegate(delegateType, target);
        }

        public override MethodInfo GetMethod(Type type, string name, BindingFlags bindingAttr, Type[] types)
        {
            return null;
        }

        public override object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).OfType<object>().ToArray();
        }

        public override XmlNode SelectSingleNode(XmlNode node, string xpath)
        {
            return null;
        }

        #endregion

        public override long ElapsedTicks { get { return _timer.ElapsedTicks; } }

        public override long TicksPerSecond { get { return Stopwatch.Frequency; } }
    }
}
