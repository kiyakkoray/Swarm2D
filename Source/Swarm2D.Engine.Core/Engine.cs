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
using Swarm2D.Library;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Swarm2D.Engine.Core
{
    public sealed class Engine : FrameworkDomain
    {
        public long CurrentFrame { get; private set; }

        private bool _domainsInitialized = false;

        private Entity _rootEntity;

        private bool _currentFrameHadJob = false;

        public Entity RootEntity
        {
            get
            {
                return _rootEntity;
            }
        }

        internal Dictionary<int, List<MessageHandlerDelegate>> GlobalMessageHandlers { get; private set; }

        private Dictionary<Type, LinkedList<Component>> _components; //all created components

        public Engine()
        {
            _components = new Dictionary<Type, LinkedList<Component>>();
            GlobalMessageHandlers = new Dictionary<int, List<MessageHandlerDelegate>>();

            InitializePrefabManager();

            CreateRootEntity();
        }

        private void CreateRootEntity()
        {
            _rootEntity = new Entity("Engine");
            _rootEntity.Engine = this;

            EngineController engineController = _rootEntity.AddComponent<EngineController>();
            _rootEntity.Domain = engineController;
        }

        private void InitializeEngineComponents()
        {
            foreach (Component component in _rootEntity.Components)
            {
                EngineComponent engineComponent = component as EngineComponent;

                if (engineComponent != null)
                {
                    engineComponent.Initialize();
                }
            }
        }

        public override void Update()
        {
            _currentFrameHadJob = false;

            if (!_domainsInitialized)
            {
                _domainsInitialized = true;
                _currentFrameHadJob = true;
            }

            InitializeEngineComponents();

            Component[] engineComponents = _rootEntity.Components.ToArray();

            foreach (Component engineComponent in engineComponents)
            {
                engineComponent.Start();
            }

            RootEntity.GetComponent<EngineController>().SendMessage(new UpdateMessage());

            _rootEntity.SendMessage(new LateUpdateMessage());

            if (!_currentFrameHadJob)
            {
                Thread.Sleep(1);
            }

            CurrentFrame++;
        }

        public override void Destroy()
        {
            RootEntity.Destroy();
        }

        public void Start()
        {
            InitializeEngineComponents();
        }

        public void DoneJob()
        {
            _currentFrameHadJob = true;
        }

        public void InvokeMessage(GlobalMessage message)
        {
            if (GlobalMessageHandlers.ContainsKey(message.Id))
            {
                List<MessageHandlerDelegate> delegates = GlobalMessageHandlers[message.Id];

                foreach (MessageHandlerDelegate messageHandlerDelegate in delegates)
                {
                    messageHandlerDelegate(message);
                }
            }
        }

        internal void OnComponentCreated(Component component)
        {
            if (!_components.ContainsKey(component.GetType()))
            {
                _components.Add(component.GetType(), new LinkedList<Component>());
            }

            component.NodeOnAllComponentList = _components[component.GetType()].AddLast(component);
        }

        internal void OnComponentDestroyed(Component component)
        {
            _components[component.GetType()].Remove(component.NodeOnAllComponentList);
        }

        public T FindComponent<T>() where T : Component
        {
            if (!_components.ContainsKey(typeof(T)))
            {
                _components.Add(typeof(T), new LinkedList<Component>());

                return null;
            }

            LinkedList<Component> components = _components[typeof(T)];

            if (components.Count > 0)
            {
                return components.First.Value as T;
            }

            return null;
        }

        public Component[] FindComponents<T>() where T : Component
        {
            if (!_components.ContainsKey(typeof(T)))
            {
                _components.Add(typeof(T), new LinkedList<Component>());

                return new Component[] { };
            }

            LinkedList<Component> components = _components[typeof(T)];

            if (components.Count > 0)
            {
                return components.ToArray();
            }

            return null;
        }

        #region Prefab section

        private Dictionary<string, Entity> _prefabs;

        private void InitializePrefabManager()
        {
            _prefabs = new Dictionary<string, Entity>();
        }

        public Entity GetPrefab(string name)
        {
            if (_prefabs.ContainsKey(name))
            {
                return _prefabs[name];
            }

            return null;
        }

        public Entity CreatePrefab(string name)
        {
            Entity prefab = new Entity(name);
            prefab.Engine = this;
            prefab.IsPrefab = true;

            _prefabs.Add(name, prefab);

            return prefab;
        }

        public Entity InstantiatePrefab(string name, IEntityDomain domain)
        {
            Entity prefab = _prefabs[name];

            Entity clonedPrefab = domain.InstantiatePrefab(prefab);

            clonedPrefab.IsInstantiatedFromPrefab = true;
            clonedPrefab.PrefabName = name;

            return clonedPrefab;
        }

        #endregion
    }

    public class LateUpdateMessage : EntityMessage
    {
        
    }
}
