using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

namespace Swarm2D.Library
{
    public static class PlatformHelper
    {
        public static void WriteLine(object obj)
        {
#if !WINDOWS_UWP
            Console.WriteLine(obj);
#endif
        }

        public static Assembly[] GetGameAssemblies()
        {
#if !WINDOWS_UWP
            return AppDomain.CurrentDomain.GetAssemblies();
#else
            return new Assembly[0];
#endif
        }

        public static Type GetBaseType(Type type)
        {
#if !WINDOWS_UWP
            return type.BaseType;
#else
            return type.GetTypeInfo().BaseType;
#endif
        }

        public static bool IsAbstract(Type type)
        {
#if !WINDOWS_UWP
            return type.IsAbstract;
#else
            return type.GetTypeInfo().IsAbstract;
#endif
        }

        public static bool IsEnum(Type type)
        {
#if !WINDOWS_UWP
            return type.IsEnum;
#else
            return type.GetTypeInfo().IsEnum;
#endif
        }

        public static bool IsSubclassOf(Type type, Type otherType)
        {
#if !WINDOWS_UWP
            return type.IsSubclassOf(otherType);
#else
            return type.GetTypeInfo().IsSubclassOf(otherType);
#endif
        }

        public static Delegate CreateDelegate(Type delegateType, object target, MethodInfo methodInfo)
        {
#if !WINDOWS_UWP
            return Delegate.CreateDelegate(delegateType, target, methodInfo);
#else
            return methodInfo.CreateDelegate(delegateType, target);
#endif
        }

        public static MethodInfo GetMethod(Type type, string name, BindingFlags bindingAttr, Type[] types)
        {
#if !WINDOWS_UWP
            return type.GetMethod(name, bindingAttr, null, types, null);
#else
            return null;
#endif
        }

        public static object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
#if !WINDOWS_UWP
            return type.GetCustomAttributes(attributeType, inherit);
#else
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).OfType<object>().ToArray();
#endif
        }

        public static XmlNode SelectSingleNode(XmlNode node, string xpath)
        {
#if !WINDOWS_UWP
            return node.SelectSingleNode(xpath);
#else
            return null;
#endif
        }
    }
}
