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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View;
using Swarm2D.WindowsFramework.Native.Windows;
using Debug = Swarm2D.Library.Debug;

namespace Swarm2D.WindowsFramework
{
    public class WindowsLogicFramework : Swarm2D.Engine.Core.Framework
    {
        private FrameworkDomain[] _frameworkDomains;

        private Thread[] _frameworkDomainThreads;
        private Stopwatch _timer;

        public bool SingleThreaded { get; set; }

        public WindowsLogicFramework()
        {
            Debug.Initialize(new WindowsDebug());
            _timer = new Stopwatch();
        }

        public override void Initialize(string resourcesPath, FrameworkDomain[] frameworkDomains)
        {
            Resources.Initialize(resourcesPath);
            _frameworkDomains = frameworkDomains;

            if (SingleThreaded)
            {
                _frameworkDomainThreads = new Thread[1];
                CreateThread(0);
            }
            else
            {
                _frameworkDomainThreads = new Thread[frameworkDomains.Length];

                for (int i = 0; i < frameworkDomains.Length; i++)
                {
                    CreateThread(i);
                }
            }
        }

        private void CreateThread(int index)
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            _frameworkDomainThreads[index] = new Thread(MainLoop);
            _frameworkDomainThreads[index].SetApartmentState(ApartmentState.STA);
            _frameworkDomainThreads[index].Name = _frameworkDomains[index].ToString() + " Thread";

            _frameworkDomainThreads[index].CurrentCulture = cultureInfo;
            _frameworkDomainThreads[index].CurrentUICulture = cultureInfo;
        }

        void MessageLoop()
        {

        }

        void MainLoop(object parameter)
        {
            if (SingleThreaded)
            {
                while (true)
                {
                    for (int i = 0; i < _frameworkDomains.Length; i++)
                    {
                        FrameworkDomain frameworkDomain = _frameworkDomains[i];

                        frameworkDomain.Update();
                    }
                }
            }
            else
            {
                FrameworkDomain frameworkDomain = parameter as FrameworkDomain;

                while (true)
                {
                    frameworkDomain.Update();
                }
            }
        }

        public override void Start()
        {
            _timer.Start();

            if (SingleThreaded)
            {
                _frameworkDomainThreads[0].Start();
            }
            else
            {
                for (int i = 0; i < _frameworkDomains.Length; i++)
                {
                    _frameworkDomainThreads[i].Start(_frameworkDomains[i]);
                }
            }

            NativeMessage message = new NativeMessage();

            while (User32.GetMessage(out message, IntPtr.Zero, 0, 0))
            {
                if (message.msg == WindowMessage.Quit)
                {
                    break;
                }

                User32.TranslateMessage(ref message);
                User32.DispatchMessage(ref message);

                MessageLoop();
            }
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
            return Resources.LoadTextData(name);
        }

        public override string LoadTextData(string resourcesName, string name)
        {
            return Resources.LoadTextData(resourcesName, name);
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

        #region Platform Helpers

        public override Assembly[] GetGameAssemblies()
        {
            return WindowsPlatformHelper.GetGameAssemblies();
        }

        public override Type GetBaseType(Type type)
        {
            return WindowsPlatformHelper.GetBaseType(type);
        }

        public override bool IsAbstract(Type type)
        {
            return WindowsPlatformHelper.IsAbstract(type);
        }

        public override bool IsEnum(Type type)
        {
            return WindowsPlatformHelper.IsEnum(type);
        }

        public override bool IsSubclassOf(Type type, Type otherType)
        {
            return WindowsPlatformHelper.IsSubclassOf(type, otherType);
        }

        public override Delegate CreateDelegate(Type delegateType, object target, MethodInfo methodInfo)
        {
            return WindowsPlatformHelper.CreateDelegate(delegateType, target, methodInfo);
        }

        public override MethodInfo GetMethod(Type type, string name, BindingFlags bindingAttr, Type[] types)
        {
            return WindowsPlatformHelper.GetMethod(type, name, bindingAttr, types);
        }

        public override object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
            return WindowsPlatformHelper.GetCustomAttributes(type, attributeType, inherit);
        }

        public override XmlNode SelectSingleNode(XmlNode node, string xpath)
        {
            return WindowsPlatformHelper.SelectSingleNode(node, xpath);
        }

        #endregion

        public override long ElapsedTicks { get { return _timer.ElapsedTicks; } }

        public override long TicksPerSecond { get { return Stopwatch.Frequency; } }
    }
}
