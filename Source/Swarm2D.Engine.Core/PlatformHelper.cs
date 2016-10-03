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
