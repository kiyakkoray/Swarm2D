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
using System.Xml;

namespace Swarm2D.LogicFramework
{
    public static class PlatformHelper
    {
        public static Assembly[] GetGameAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public static Type GetBaseType(Type type)
        {
            return type.BaseType;
        }

        public static bool IsAbstract(Type type)
        {
            return type.IsAbstract;
        }

        public static bool IsEnum(Type type)
        {
            return type.IsEnum;
        }

        public static bool IsSubclassOf(Type type, Type otherType)
        {
            return type.IsSubclassOf(otherType);
        }

        public static Delegate CreateDelegate(Type delegateType, object target, MethodInfo methodInfo)
        {
            return Delegate.CreateDelegate(delegateType, target, methodInfo);
        }

        public static MethodInfo GetMethod(Type type, string name, BindingFlags bindingAttr, Type[] types)
        {
            return type.GetMethod(name, bindingAttr, null, types, null);
        }

        public static object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
            return type.GetCustomAttributes(attributeType, inherit);
        }

        public static XmlNode SelectSingleNode(XmlNode node, string xpath)
        {
            return node.SelectSingleNode(xpath);
        }
    }
}
