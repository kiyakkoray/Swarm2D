using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Swarm2D.Unity.Logic
{
    public static class UnityPlatformHelper
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
