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

using Swarm2D.Engine.Core;
using Swarm2D.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using Debug = Swarm2D.Library.Debug;

namespace Swarm2D.Test
{
    public abstract class TestRole : Framework, IDebug
    {
        public abstract void DoTest();

        void IDebug.Log(object log)
        {
            Console.WriteLine(log);
        }

        void IDebug.Assert(bool condition, string message)
        {
            StackTrace stackTrace = new StackTrace();
            Console.WriteLine("Assertion Failed!\n" + stackTrace + "\n" + message);
        }

        private FrameworkDomain[] _frameworkDomains;

        private int _currentFrame = 0;

        protected TestRole()
        {
            Debug.Initialize(this);
        }

        public override void Initialize(string resources, FrameworkDomain[] frameworkDomains)
        {
            _frameworkDomains = frameworkDomains;
        }

        public override void Start()
        {

        }

        public void Update()
        {
            for (int i = 0; i < _frameworkDomains.Length; i++)
            {
                FrameworkDomain frameworkDomain = _frameworkDomains[i];

                frameworkDomain.Update();
            }

            _currentFrame++;
        }

        #region Resources

        public override string ResourcesName { get { return ""; } }

        public override string ResourcesPath { get { return ""; } }

        public override byte[] LoadBinaryData(string name)
        {
            return null;
        }

        public override byte[] LoadBinaryData(string resourcesName, string name)
        {
            return null;
        }

        public override string LoadTextData(string name)
        {
            return "";
        }

        public override string LoadTextData(string resourcesName, string name)
        {
            return "";
        }

        public override XmlDocument LoadXmlData(string resourcesName, string name)
        {
            return null;
        }

        public override XmlDocument LoadXmlData(string name)
        {
            return null;
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
            return "";
        }

        #endregion

        #region Platform Helpers

        public override IThread CreateThread(Engine.Core.ThreadStart threadStart)
        {
            return new LogicFramework.Thread(threadStart);
        }

        public override void Sleep(int miliSeconds)
        {
            Thread.Sleep(1);
        }

        public override Assembly[] GetGameAssemblies()
        {
            return LogicFramework.PlatformHelper.GetGameAssemblies();
        }

        public override Type GetBaseType(Type type)
        {
            return LogicFramework.PlatformHelper.GetBaseType(type);
        }

        public override bool IsAbstract(Type type)
        {
            return LogicFramework.PlatformHelper.IsAbstract(type);
        }

        public override bool IsEnum(Type type)
        {
            return LogicFramework.PlatformHelper.IsEnum(type);
        }

        public override bool IsSubclassOf(Type type, Type otherType)
        {
            return LogicFramework.PlatformHelper.IsSubclassOf(type, otherType);
        }

        public override Delegate CreateDelegate(Type delegateType, object target, MethodInfo methodInfo)
        {
            return LogicFramework.PlatformHelper.CreateDelegate(delegateType, target, methodInfo);
        }

        public override MethodInfo GetMethod(Type type, string name, BindingFlags bindingAttr, Type[] types)
        {
            return LogicFramework.PlatformHelper.GetMethod(type, name, bindingAttr, types);
        }

        public override object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
            return LogicFramework.PlatformHelper.GetCustomAttributes(type, attributeType, inherit);
        }

        public override XmlNode SelectSingleNode(XmlNode node, string xpath)
        {
            return LogicFramework.PlatformHelper.SelectSingleNode(node, xpath);
        }

        #endregion

        public override long ElapsedTicks { get { return _currentFrame; } }

        public override long TicksPerSecond { get { return 50; } }
    }
}
