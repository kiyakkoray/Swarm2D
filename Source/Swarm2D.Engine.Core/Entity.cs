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
    public sealed class Entity
    {
        public string Name { get; set; }

        public List<Component> Components { get; private set; }

        private Entity _parent;

        public Entity Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                Debug.Assert(value != null, "Assigned parent is null!");

                if (_parent != value)
                {
                    bool firstAssignment = _parent == null;

                    if (!firstAssignment)
                    {
                        _parent.Children.Remove(NodeOnEntitiesList);
                        NodeOnEntitiesList = null;
                    }

                    _parent = value;
                    NodeOnEntitiesList = _parent.Children.AddLast(this);

                    if (!firstAssignment)
                    {
                        Domain.OnEntityParentChanged(this);
                    }
                }
            }
        }

        public LinkedList<Entity> Children { get; private set; }
        internal LinkedListNode<Entity> NodeOnEntitiesList { get; set; }

        internal Dictionary<int, List<MessageHandlerDelegate>> EntityMessageHandlers { get; private set; }

        public bool IsPrefab { get; internal set; }
        public bool IsInstantiatedFromPrefab { get; internal set; }
        public string PrefabName { get; internal set; }

        internal Entity(string name)
        {
            Components = new List<Component>();
            Children = new LinkedList<Entity>();
            EntityMessageHandlers = new Dictionary<int, List<MessageHandlerDelegate>>();

            Name = name;

            IsPrefab = false;
            IsInstantiatedFromPrefab = false;
            PrefabName = "";
        }

        private Component AddComponentWithInfo(ComponentInfo componentInfo)
        {
            Component component = componentInfo.CreateComponent(this);

            Components.Add(component);

            if (!IsPrefab)
            {
                component.Added();
            }

            for (int i = 0; i < Components.Count; i++)
            {
                Component existingComponent = Components[i];

                if (existingComponent != component)
                {
                    if (!IsPrefab)
                    {
                        existingComponent.OnComponentAdded(component);
                    }
                }
            }

            return component;
        }

        public Component AddComponent(string type)
        {
            ComponentInfo componentInfo = ComponentInfo.GetComponentInfo(type);

            if (componentInfo != null)
            {
                return AddComponentWithInfo(componentInfo);
            }

            return null;
        }

        public Component AddComponent(Type type)
        {
            ComponentInfo componentInfo = ComponentInfo.GetComponentInfo(type);

            if (componentInfo != null)
            {
                return AddComponentWithInfo(componentInfo);
            }

            return null;
        }

        public T AddComponent<T>() where T : Component
        {
            T component = AddComponentWithInfo(ComponentInfo.GetComponentInfo(typeof(T))) as T;

            return component;
        }

        public Component GetComponent(Type type)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Component component = Components[i];

                if (type.IsInstanceOfType(component))
                {
                    return component;
                }
            }

            return null;
        }

        public Component GetComponent(string type)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Component component = Components[i];

                if (type == component.GetType().Name)
                {
                    return component;
                }
            }

            return null;
        }

        public T GetComponent<T>() where T : Component
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Component component = Components[i];

                if (component is T)
                {
                    return component as T;
                }
            }

            return null;
        }

        public T[] GetComponents<T>() where T : Component
        {
            List<T> result = new List<T>();

            for (int i = 0; i < Components.Count; i++)
            {
                Component component = Components[i];

                if (component is T)
                {
                    result.Add(component as T);
                }
            }

            return result.ToArray();
        }

        public void DeleteComponent(Component component)
        {
            component.Destroy();
            Components.Remove(component);
        }

        public void SendMessage(string message, object data)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Component component = Components[i];

                MethodInfo messageMethod = component.GetType().GetMethod
                    (message, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { data.GetType() }, null);

                if (messageMethod != null)
                {
                    messageMethod.Invoke(component, new object[] { data });
                }
            }
        }

        public void Destroy()
        {
            if (IsDestroyed)
            {
                Debug.Log(Name + " already destroyed");
            }
            else
            {
                IsDestroyed = true;

                Entity[] entities = Children.ToArray();

                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];

                    entity.Destroy();
                }

                for (int i = 0; i < Components.Count; i++)
                {
                    Component component = Components[i];

                    component.Destroy();
                }

                if (Parent != null)
                {
                    Parent.Children.Remove(NodeOnEntitiesList);
                    NodeOnEntitiesList = null;
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public void SendMessage(EntityMessage message)
        {
            if (EntityMessageHandlers.ContainsKey(message.Id))
            {
                List<MessageHandlerDelegate> messageHandlers = EntityMessageHandlers[message.Id];

                for (int i = 0; i < messageHandlers.Count; i++)
                {
                    MessageHandlerDelegate messageHandlerDelegate = messageHandlers[i];
                    try
                    {
                        messageHandlerDelegate.Invoke(message);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Exception on component message " + message);
                        Debug.Log("exception:" + ex.Message);
                        Debug.Log("printing exception over...");
                    }
                }
            }
        }

        public Entity CreateChildEntity(string name)
        {
            Entity entity = new Entity(name);
            entity.Engine = Engine;
            entity.Parent = this;

            if (ChildDomain != null)
            {
                ChildDomain.OnCreateChildEntity(entity);
            }
            else
            {
                Domain.OnCreateChildEntity(entity);
            }

            return entity;
        }

        public Entity FindChild(string name)
        {
            foreach (var entity in Children)
            {
                if (entity.Name == name)
                {
                    return entity;
                }
            }

            return null;
        }

        public Engine Engine { get; internal set; }

        public IEntityDomain Domain { get; set; }

        public IEntityDomain ChildDomain { get; set; }

        public bool IsDestroyed { get; private set; }
    }

    public delegate void EntityComponentMessage(Component component);

    public interface IEntityDomain
    {
        void SendMessage(DomainMessage message);
        void OnComponentCreated(Component component);
        void OnComponentDestroyed(Component component);
        Entity InstantiatePrefab(Entity prefab);
        void OnCreateChildEntity(Entity entity);
        void OnEntityParentChanged(Entity entity);
    }
}
