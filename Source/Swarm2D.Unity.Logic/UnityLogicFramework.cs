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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Swarm2D.Engine.Core;
using Swarm2D.Library;
using UnityEngine;
using Debug = Swarm2D.Library.Debug;
using Thread = System.Threading.Thread;

namespace Swarm2D.Unity.Logic
{
    public class UnityLogicFramework : Framework
    {
        private FrameworkDomain[] _frameworkDomains;

        private IUnityBehaviour _unityBehaviour;

        private string _resourcesPath;

        public UnityLogicFramework(IUnityBehaviour unityBehaviour)
        {
            _unityBehaviour = unityBehaviour;

            Debug.Initialize(new UnityDebug(unityBehaviour));

            CultureInfo cultureInfo = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        public override void Initialize(string resourcesPath, FrameworkDomain[] frameworkDomains)
        {
            _resourcesPath = resourcesPath;
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
        }

        public void Destroy()
        {
            for (int i = 0; i < _frameworkDomains.Length; i++)
            {
                FrameworkDomain frameworkDomain = _frameworkDomains[i];
                frameworkDomain.Destroy();
            }
        }

        #region Resources

        public override string ResourcesName { get { return _resourcesPath; } }

        public override string ResourcesPath { get { return _resourcesPath; } }

        public override byte[] LoadBinaryData(string name)
        {
            return LoadBinaryData(_resourcesPath, name);
        }

        public override byte[] LoadBinaryData(string resourcesName, string name)
        {
            Debug.Log("loading binary " + name);
            TextAsset binaryAsset = UnityEngine.Resources.Load(resourcesName + "/" + name) as TextAsset;

            if (binaryAsset != null)
            {
                return binaryAsset.bytes;
            }

            return null;
        }

        public override string LoadTextData(string name)
        {
            return LoadTextData(_resourcesPath, name);
        }

        public override string LoadTextData(string resourcesName, string name)
        {
            Debug.Log("loading text: " + name);

            string textFullPath = resourcesName + "/" + name;
            Debug.Log("text path: " + textFullPath);

            TextAsset textAsset = UnityEngine.Resources.Load(textFullPath) as TextAsset;

            if (textAsset != null)
            {
                return textAsset.text;
            }

            return null;
        }

        public override XmlDocument LoadXmlData(string resourcesName, string name)
        {
            Debug.Log("loading text: " + name);

            string textFullPath = resourcesName + "/" + name;
            Debug.Log("text path: " + textFullPath);

            TextAsset textAsset = UnityEngine.Resources.Load(textFullPath) as TextAsset;

            if (textAsset != null)
            {
                string loadedText = textAsset.text;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(loadedText);
                return xmlDocument;
            }

            return null;
        }

        public override XmlDocument LoadXmlData(string name)
        {
            return LoadXmlData(_resourcesPath, name);
        }

        public override void SaveBinaryData(string name, byte[] data)
        {
            throw new NotImplementedException();
        }

        public override void SaveXmlData(string name, XmlDocument xmlDocument)
        {
            throw new NotImplementedException();
        }

        public override string[] GetFilesInFolder(String path, ResourceFileType fileType)
        {
            throw new NotImplementedException();
            return null;
        }

        public override string GetResourcesNameOf(string fileName)
        {
            return _resourcesPath;
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

        public override long ElapsedTicks { get { return (long)(UnityEngine.Time.time * 1000.0f); } }

        public override long TicksPerSecond { get { return 1000; } }
    }
}
