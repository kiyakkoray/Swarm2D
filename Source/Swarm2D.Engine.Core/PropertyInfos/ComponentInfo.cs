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
using System.Runtime.Remoting;
using System.Text;
using System.Reflection;
using Swarm2D.Library;

namespace Swarm2D.Engine.Core
{
    public class ComponentInfo
    {
        public static List<ComponentInfo> ComponentInfos { get; private set; }
        private static Dictionary<Type, ComponentInfo> _componentInfosWithTypes;

        private static readonly object[] ConstructorParameters = new object[0];

        private ConstructorInfo _componentConstructor;

        public Dictionary<string, ComponentPropertyInfo> ComponentPropertyInfos { get; private set; }
        internal Dictionary<Type, MethodInfo> GlobalMessageHandlers { get; private set; }
        internal Dictionary<Type, MethodInfo> DomainMessageHandlers { get; private set; }
        internal Dictionary<Type, MethodInfo> EntityMessageHandlers { get; private set; }

        public string Name { get; private set; }

        public Type ComponentType { get; private set; }

        public bool Poolable { get; private set; }

        static ComponentInfo()
        {
            ComponentInfos = new List<ComponentInfo>();
            _componentInfosWithTypes = new Dictionary<Type, ComponentInfo>();
            CollectComponentInformations();
        }

        private static ComponentInfo GetComponentInfoWithoutCollectingAgain(Type componentType)
        {
            ComponentInfo componentInfo = null;
            _componentInfosWithTypes.TryGetValue(componentType, out componentInfo);

            return componentInfo;
        }

        public static ComponentInfo GetComponentInfo(Type component)
        {
            ComponentInfo componentInfo = GetComponentInfoWithoutCollectingAgain(component);

            if (componentInfo == null)
            {
                CollectComponentInformations();
                componentInfo = GetComponentInfoWithoutCollectingAgain(component);
            }

            return componentInfo;
        }

        public static ComponentInfo GetComponentInfo(string name)
        {
            foreach (ComponentInfo componentInfo in ComponentInfos)
            {
                if (componentInfo.ComponentType.Name == name)
                {
                    return componentInfo;
                }
            }

            CollectComponentInformations();

            foreach (ComponentInfo componentInfo in ComponentInfos)
            {
                if (componentInfo.ComponentType.Name == name)
                {
                    return componentInfo;
                }
            }

            return null;
        }

        internal ComponentInfo()
        {
            ComponentPropertyInfos = new Dictionary<string, ComponentPropertyInfo>();
            GlobalMessageHandlers = new Dictionary<Type, MethodInfo>();
            DomainMessageHandlers = new Dictionary<Type, MethodInfo>();
            EntityMessageHandlers = new Dictionary<Type, MethodInfo>();
        }

        private static void CollectComponentInformations()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(Component)) && !type.IsAbstract)
                    {
                        if (GetComponentInfoWithoutCollectingAgain(type) == null)
                        {
                            ComponentInfo componentInfo = new ComponentInfo();
                            componentInfo.FillInformationsFromComponent(type);
                            ComponentInfos.Add(componentInfo);
                            _componentInfosWithTypes.Add(type, componentInfo);
                        }
                    }
                }
            }

            ComponentInfos = ComponentInfos.OrderBy(componentInfo => componentInfo.Name).ToList();
        }

        internal void FillInformationsFromComponent(Type type)
        {
            if (type.IsSubclassOf(typeof(Component)) && !type.IsAbstract)
            {
                ComponentType = type;
                Debug.Log("Component Info Collecting: " + type.ToString());

                Name = type.Name;

                _componentConstructor = type.GetConstructor(new Type[] { });

                {
                    object[] poolableAttributes = type.GetCustomAttributes(typeof (PoolableComponent), true);
                    
                    if (poolableAttributes != null && poolableAttributes.Length > 0)
                    {
                        Poolable = true;
                    }
                }

                PropertyInfo[] propertyInfos = type.GetProperties();

                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    object[] attributes = propertyInfo.GetCustomAttributes(typeof(ComponentProperty), true);

                    if (attributes.Length == 1)
                    {
                        Debug.Log("Property Found: " + propertyInfo.Name);
                        AddProperty(propertyInfo.Name, propertyInfo);
                    }
                }

                MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                foreach (MethodInfo methodInfo in methodInfos)
                {
                    object[] entityMessageAttributes = methodInfo.GetCustomAttributes(typeof(EntityMessageHandler), true);

                    if (entityMessageAttributes.Length == 1)
                    {
                        EntityMessageHandler entityMessageHandler = entityMessageAttributes[0] as EntityMessageHandler;

                        Debug.Log("Entity Message Handler Found: " + methodInfo.Name);
                        AddEntityMessage(entityMessageHandler.MessageType, methodInfo);
                    }

                    object[] domainMessageAttributes = methodInfo.GetCustomAttributes(typeof(DomainMessageHandler), true);

                    if (domainMessageAttributes.Length == 1)
                    {
                        DomainMessageHandler domainMessageHandler = domainMessageAttributes[0] as DomainMessageHandler;
                        Debug.Log("Domain Message Handler Found: " + methodInfo.Name);

                        AddDomainMessage(domainMessageHandler.MessageType, methodInfo);
                    }

                    object[] globalMessageAttributes = methodInfo.GetCustomAttributes(typeof(GlobalMessageHandler), true);

                    if (globalMessageAttributes.Length == 1)
                    {
                        GlobalMessageHandler globalMessageHandler = globalMessageAttributes[0] as GlobalMessageHandler;
                        Debug.Log("Global Message Handler Found: " + methodInfo.Name);

                        AddGlobalMessage(globalMessageHandler.MessageType, methodInfo);
                    }
                }
            }
        }

        internal Component CreateComponent(Entity entity)
        {
            Engine engine = entity.Engine;
            bool pooledMode = engine.PooledMode;

            Component component = null;

            if (Poolable && pooledMode)
            {
                component = engine.CreateComponentIfExists(ComponentType);
            }

            if (component == null)
            {
                component = (Component) _componentConstructor.Invoke(ConstructorParameters);
            }

            component.Reset(entity);

            foreach (KeyValuePair<Type, MethodInfo> globalMessageHandler in GlobalMessageHandlers)
            {
                Type globalMessageType = globalMessageHandler.Key;
                MethodInfo globalMessageHandlerMethod = globalMessageHandler.Value;

                MessageHandlerDelegate messageHandler = (MessageHandlerDelegate)Delegate.CreateDelegate(typeof(MessageHandlerDelegate), component, globalMessageHandlerMethod);

                int globalMessageId = Message.GetMessageId(globalMessageType);

                {
                    if (!component.GlobalMessageHandlers.ContainsKey(globalMessageId))
                    {
                        component.GlobalMessageHandlers.Add(globalMessageId, new List<MessageHandlerDelegate>());
                    }

                    component.GlobalMessageHandlers[globalMessageId].Add(messageHandler);
                }

                {
                    if (!engine.GlobalMessageHandlers.ContainsKey(globalMessageId))
                    {
                        engine.GlobalMessageHandlers.Add(globalMessageId, new List<MessageHandlerDelegate>());
                    }

                    engine.GlobalMessageHandlers[globalMessageId].Add(messageHandler);
                }
            }

            foreach (KeyValuePair<Type, MethodInfo> domainMessageHandler in DomainMessageHandlers)
            {
                Type domainMessageType = domainMessageHandler.Key;
                MethodInfo domainMessageHandlerMethod = domainMessageHandler.Value;

                MessageHandlerDelegate messageHandler = (MessageHandlerDelegate)Delegate.CreateDelegate(typeof(MessageHandlerDelegate), component, domainMessageHandlerMethod);

                int entityMessageId = Message.GetMessageId(domainMessageType);

                {
                    if (!component.DomainMessageHandlers.ContainsKey(entityMessageId))
                    {
                        component.DomainMessageHandlers.Add(entityMessageId, new List<MessageHandlerDelegate>());
                    }

                    component.DomainMessageHandlers[entityMessageId].Add(messageHandler);
                }
            }

            foreach (KeyValuePair<Type, MethodInfo> entityMessageHandler in EntityMessageHandlers)
            {
                Type entityMessageType = entityMessageHandler.Key;
                MethodInfo entityMessageHandlerMethod = entityMessageHandler.Value;

                MessageHandlerDelegate messageHandler = (MessageHandlerDelegate)Delegate.CreateDelegate(typeof(MessageHandlerDelegate), component, entityMessageHandlerMethod);

                int entityMessageId = Message.GetMessageId(entityMessageType);

                {
                    if (!component.EntityMessageHandlers.ContainsKey(entityMessageId))
                    {
                        component.EntityMessageHandlers.Add(entityMessageId, new List<MessageHandlerDelegate>());
                    }

                    component.EntityMessageHandlers[entityMessageId].Add(messageHandler);
                }

                {
                    if (!entity.EntityMessageHandlers.ContainsKey(entityMessageId))
                    {
                        entity.EntityMessageHandlers.Add(entityMessageId, new List<MessageHandlerDelegate>());
                    }

                    entity.EntityMessageHandlers[entityMessageId].Add(messageHandler);
                }
            }

            engine.OnComponentCreated(component);

            if (entity.Domain != null)
            {
                entity.Domain.OnComponentCreated(component);
            }

            return component;
        }

        private void AddProperty(string name, PropertyInfo propertyInfo)
        {
            ComponentPropertyInfo componentPropertyInfo = new ComponentPropertyInfo();

            componentPropertyInfo.PropertyInfo = propertyInfo;
            componentPropertyInfo.Name = name;

            if (typeof(Resource).IsAssignableFrom(propertyInfo.PropertyType))
            {
                componentPropertyInfo.PropertyType = ComponentPropertyType.Resource;
            }
            else if (propertyInfo.PropertyType == typeof(Vector2))
            {
                componentPropertyInfo.PropertyType = ComponentPropertyType.Vector2;
            }
            else if (propertyInfo.PropertyType == typeof(float))
            {
                componentPropertyInfo.PropertyType = ComponentPropertyType.Float;
            }
            else if (propertyInfo.PropertyType == typeof(int))
            {
                componentPropertyInfo.PropertyType = ComponentPropertyType.Int;
            }
            else if (propertyInfo.PropertyType.IsEnum)
            {
                componentPropertyInfo.PropertyType = ComponentPropertyType.Enumerator;
            }
            else if (typeof(ISerializableObject).IsAssignableFrom(propertyInfo.PropertyType))
            {
                componentPropertyInfo.PropertyType = ComponentPropertyType.Object;
            }

            ComponentPropertyInfos.Add(name, componentPropertyInfo);
        }

        private void AddEntityMessage(Type entityMessageType, MethodInfo methodInfo)
        {
            EntityMessageHandlers.Add(entityMessageType, methodInfo);
        }

        private void AddDomainMessage(Type entityMessageType, MethodInfo methodInfo)
        {
            DomainMessageHandlers.Add(entityMessageType, methodInfo);
        }

        private void AddGlobalMessage(Type globalMessageType, MethodInfo methodInfo)
        {
            GlobalMessageHandlers.Add(globalMessageType, methodInfo);
        }
    }
}
