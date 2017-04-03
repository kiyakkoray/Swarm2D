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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

namespace Swarm2D.Engine.Core
{
    public static class PlatformHelper
    {
        public static Assembly[] GetGameAssemblies()
        {
            return Framework.Current.GetGameAssemblies();
        }

        public static Type GetBaseType(Type type)
        {
            return Framework.Current.GetBaseType(type);
        }

        public static bool IsAbstract(Type type)
        {
            return Framework.Current.IsAbstract(type);
        }

        public static bool IsEnum(Type type)
        {
            return Framework.Current.IsEnum(type);
        }

        public static bool IsSubclassOf(Type type, Type otherType)
        {
            return Framework.Current.IsSubclassOf(type, otherType);
        }

        public static Delegate CreateDelegate(Type delegateType, object target, MethodInfo methodInfo)
        {
            return Framework.Current.CreateDelegate(delegateType, target, methodInfo);
        }

        public static MethodInfo GetMethod(Type type, string name, BindingFlags bindingAttr, Type[] types)
        {
            return Framework.Current.GetMethod(type, name, bindingAttr, types);
        }

        public static object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
            return Framework.Current.GetCustomAttributes(type, attributeType, inherit);
        }

        public static XmlNode SelectSingleNode(XmlNode node, string xpath)
        {
            return Framework.Current.SelectSingleNode(node, xpath);
        }
    }
}
