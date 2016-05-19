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
using System.Reflection;
using System.Text;
using Swarm2D.Library;

namespace Swarm2D.Engine.Core
{
    public abstract class Component
    {
        internal Dictionary<int, List<MessageHandlerDelegate>> GlobalMessageHandlers { get; private set; }
        internal Dictionary<int, List<MessageHandlerDelegate>> DomainMessageHandlers { get; private set; }
        internal Dictionary<int, List<MessageHandlerDelegate>> EntityMessageHandlers { get; private set; }

        internal LinkedListNode<Component> NodeOnAllComponentList { get; set; }

        public Engine Engine
        {
            get { return Entity.Engine; }
        }

        public Entity Entity
        {
            get;
            private set;
        }

        protected Component()
        {
            GlobalMessageHandlers = new Dictionary<int, List<MessageHandlerDelegate>>();
            DomainMessageHandlers = new Dictionary<int, List<MessageHandlerDelegate>>();
            EntityMessageHandlers = new Dictionary<int, List<MessageHandlerDelegate>>();
            IsDestroyed = true;
        }

        internal void Reset(Entity entity)
        {
            Debug.Assert(IsDestroyed, "IsDestroyed");
            
            IsDestroyed = false;
            IsInitialized = false;
            IsStarted = false;

            Entity = entity;
        }

        internal void Initialize()
        {
            if (!IsInitialized)
            {
                OnInitialize();
                IsInitialized = true;
            }
        }

        internal void Added()
        {
            OnAdded();
        }

        protected virtual void OnAdded()
        {

        }

        internal void Start()
        {
            if (!IsStarted)
            {
                OnStart();
                IsStarted = true;
            }
        }

        public T GetComponent<T>() where T : Component
        {
            return Entity.GetComponent<T>();
        }

        public T[] GetComponents<T>() where T : Component
        {
            return Entity.GetComponents<T>();
        }

        public T AddComponent<T>() where T : Component
        {
            return Entity.AddComponent<T>();
        }

        public Component AddComponent(string type)
        {
            return Entity.AddComponent(type);
        }

        protected virtual void OnInitialize()
        {

        }

        protected virtual void OnStart()
        {

        }

        internal void Destroy()
        {
            Debug.Assert(!IsDestroyed, "!IsDestroyed");

            OnDestroy();

            Engine engine = Engine;
            IsDestroyed = true;

            engine.OnComponentDestroyed(this);

            if (Entity.Domain != null)
            {
                Entity.Domain.OnComponentDestroyed(this);
            }

            foreach (KeyValuePair<int, List<MessageHandlerDelegate>> globalMessageHandler in GlobalMessageHandlers)
            {
                List<MessageHandlerDelegate> globalMessageHandlerMethods = globalMessageHandler.Value;

                foreach (MessageHandlerDelegate globalMessageHandlerMethod in globalMessageHandlerMethods)
                {
                    engine.GlobalMessageHandlers[globalMessageHandler.Key].Remove(globalMessageHandlerMethod);
                }
            }

            foreach (KeyValuePair<int, List<MessageHandlerDelegate>> globalMessageHandler in EntityMessageHandlers)
            {
                List<MessageHandlerDelegate> messageHandlerMethods = globalMessageHandler.Value;

                foreach (MessageHandlerDelegate messageHandlerMethod in messageHandlerMethods)
                {
                    Entity.EntityMessageHandlers[globalMessageHandler.Key].Remove(messageHandlerMethod);
                }
            }

            GlobalMessageHandlers.Clear();
            DomainMessageHandlers.Clear();
            EntityMessageHandlers.Clear();

            NodeOnAllComponentList = null;
            Entity = null;

            engine.FreeComponent(this);
        }

        protected virtual void OnDestroy()
        {
           
        }

        public ComponentInfo GetComponentInfo()
        {
            return ComponentInfo.GetComponentInfo(GetType());
        }

        protected internal virtual void OnComponentAdded(Component component)
        {

        }

        protected internal virtual void OnComponentRemoved(Component component)
        {

        }

        public bool IsInitialized { get; private set; }

        public bool IsStarted { get; private set; }

        public bool IsDestroyed { get; private set; }

        public Entity CreateChildEntity(string name)
        {
            return Entity.CreateChildEntity(name);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequiresComponent : Attribute
    {
        public RequiresComponent(Type componentType)
        {

        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PoolableComponent : Attribute
    {
        
    }
}
