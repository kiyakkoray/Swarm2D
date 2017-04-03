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
using System.Linq;
using System.Threading;
using Debug = Swarm2D.Library.Debug;

namespace Swarm2D.Engine.Core
{
    public sealed class Engine : FrameworkDomain
    {
        public bool PooledMode { get; private set; }

        public long CurrentFrame { get; private set; }

        private bool _domainsInitialized = false;

        private Entity _rootEntity;

        private bool _currentFrameHadJob = false;

        private bool _started = false;

        public int UpdatePerSecond { get; private set; }
        public int UpdateMessageTimePerSecond { get; private set; }
        public int LateUpdateMessageTimePerSecond { get; private set; }
        public int LastUpdateMessageTimePerSecond { get; private set; }

        private long _lastCountedElapsedTicksForUpdatePerSecond;
        private long _lastCountedElapsedTicksForLateUpdatePerSecond;
        private long _lastCountedElapsedTicksForLastUpdatePerSecond;

        private int _countedUpdateLastSecond;
        private Stopwatch _timer;

        public Entity RootEntity
        {
            get
            {
                return _rootEntity;
            }
        }

        private EntityDomain EntityDomain
        {
            get { return _engineController.EntityDomain; }
        }

        internal Dictionary<int, List<MessageHandlerDelegate>> GlobalMessageHandlers { get; private set; }

        private Dictionary<Type, LinkedList<Component>> _components; //all created components
        private EngineController _engineController;

        private Stack<Entity> _freeEntities;
        private Dictionary<Type, Stack<Component>> _freeComponents;

        public Engine(Framework framework, bool pooledMode)
            : base(framework)
        {
            PooledMode = pooledMode;

            _components = new Dictionary<Type, LinkedList<Component>>();
            GlobalMessageHandlers = new Dictionary<int, List<MessageHandlerDelegate>>();

            if (PooledMode)
            {
                _freeEntities = new Stack<Entity>(16384);

                for (int i = 0; i < 16384; i++)
                {
                    _freeEntities.Push(new Entity(this));
                }

                _freeComponents = new Dictionary<Type, Stack<Component>>();
            }

            InitializePrefabManager();
            InitializeGlobalVariables();

            CreateRootEntity();

            _timer = new Stopwatch();
            _timer.Start();
        }

        private void CreateRootEntity()
        {
            _rootEntity = new Entity(this);
            _rootEntity.Reset("Engine");

            _engineController = _rootEntity.AddComponent<EngineController>();
            _rootEntity.Domain = _engineController;
            _rootEntity.Domain.OnComponentCreated(_engineController);
        }

        public override void Update()
        {
            _currentFrameHadJob = false;

            if (!_domainsInitialized)
            {
                _domainsInitialized = true;
                _currentFrameHadJob = true;
            }

            long elapsedTickBeforeUpdate = _timer.ElapsedTicks;

            SendMessage(new UpdateMessage());

            long elapsedTickBeforeLateUpdate = _timer.ElapsedTicks;

            _rootEntity.SendMessage(new LateUpdateMessage());

            long elapsedTickAfterLateUpdate = _timer.ElapsedTicks;

            _rootEntity.SendMessage(new LastUpdateMessage());

            long elapsedTickAfterLastUpdate = _timer.ElapsedTicks;

            _lastCountedElapsedTicksForUpdatePerSecond += elapsedTickBeforeLateUpdate - elapsedTickBeforeUpdate;
            _lastCountedElapsedTicksForLateUpdatePerSecond += elapsedTickAfterLateUpdate - elapsedTickBeforeLateUpdate;
            _lastCountedElapsedTicksForLastUpdatePerSecond += elapsedTickAfterLastUpdate - elapsedTickAfterLateUpdate;

            if (!_currentFrameHadJob)
            {
                Framework.Current.Sleep(1);
            }

            _countedUpdateLastSecond++;
            CurrentFrame++;

            if (_timer.ElapsedMilliseconds >= 1000)
            {
                _timer.Reset();
                _timer.Start();

                UpdatePerSecond = _countedUpdateLastSecond;
                _countedUpdateLastSecond = 0;

                UpdateMessageTimePerSecond = (int)((1000 * _lastCountedElapsedTicksForUpdatePerSecond) / Stopwatch.Frequency);
                LateUpdateMessageTimePerSecond = (int)((1000 * _lastCountedElapsedTicksForLateUpdatePerSecond) / Stopwatch.Frequency);
                LastUpdateMessageTimePerSecond = (int)((1000 * _lastCountedElapsedTicksForLastUpdatePerSecond) / Stopwatch.Frequency);

                _lastCountedElapsedTicksForUpdatePerSecond = 0;
                _lastCountedElapsedTicksForLateUpdatePerSecond = 0;
                _lastCountedElapsedTicksForLastUpdatePerSecond = 0;
            }
        }

        public override void Destroy()
        {
            RootEntity.Destroy();
        }

        public void Start()
        {
            if (!_started)
            {
                EngineComponent[] engineComponents = RootEntity.GetComponents<EngineComponent>();

                foreach (var engineComponent in engineComponents)
                {
                    engineComponent.OnEngineStart();
                }

                _started = true;
            }
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
                    messageHandlerDelegate.Invoke(message);
                }
            }
        }

        public void SendMessage(DomainMessage message)
        {
            ((IEntityDomain)_engineController).SendMessage(message);
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

        public IEnumerable<Entity> Prefabs
        {
            get { return _prefabs.Values; }
        }

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
            Entity prefab = new Entity(this);
            prefab.ResetAsPrefab(name);

            _prefabs.Add(name, prefab);

            return prefab;
        }

        public Entity InstantiatePrefab(string name, IEntityDomain domain)
        {
            Entity prefab = _prefabs[name];

            Entity clonedPrefab = domain.InstantiatePrefab(prefab);
            Debug.Assert(clonedPrefab.IsInstantiatedFromPrefab, "clonedPrefab.IsInstantiatedFromPrefab");

            return clonedPrefab;
        }

        #endregion

        #region Global Variables

        private Dictionary<string, List<string>> _globalLists;

        private void InitializeGlobalVariables()
        {
            _globalLists = new Dictionary<string, List<string>>();
        }

        public void CreateGlobalList(string name)
        {
            _globalLists.Add(name, new List<string>());
        }

        public void AddGlobalListValue(string name, string value)
        {
            _globalLists[name].Add(value);
        }

        public IEnumerable<string> GetGlobalListValues(string name)
        {
            return _globalLists[name];
        }

        #endregion

        internal Entity CreateEntity()
        {
            if (PooledMode)
            {
                if (_freeEntities.Count > 0)
                {
                    return _freeEntities.Pop();
                }
            }

            return new Entity(this);
        }

        internal void FreeEntity(Entity entity)
        {
            if (PooledMode)
            {
                Debug.Assert(entity.Engine == this, "entity.Engine == this");
                _freeEntities.Push(entity);
            }
        }

        internal Component CreateComponentIfExists(Type type)
        {
            if (PooledMode)
            {
                Stack<Component> _freComponentsOfType;

                if (_freeComponents.TryGetValue(type, out _freComponentsOfType))
                {
                    if (_freComponentsOfType.Count > 0)
                    {
                        return _freComponentsOfType.Pop();
                    }
                }
            }

            return null;
        }

        internal void FreeComponent(Component component)
        {
            if (PooledMode)
            {
                Stack<Component> _freComponentsOfType;
                Type type = component.GetType();

                if (!_freeComponents.TryGetValue(type, out _freComponentsOfType))
                {
                    _freComponentsOfType = new Stack<Component>();
                    _freeComponents.Add(type, _freComponentsOfType);
                }

                _freComponentsOfType.Push(component);
            }
        }

        public EngineData Save()
        {
            EntityData rootEntityData = SaveEntity(_rootEntity, null);
            EngineData engineData = new EngineData(rootEntityData);

            return engineData;
        }

        private EntityData SaveEntity(Entity entity, EntityData parentEntityData)
        {
            EntityData entityData = null;

            if (parentEntityData != null)
            {
                entityData = parentEntityData.AddEntity(entity.Name);
            }
            else
            {
                entityData = new EntityData(entity.Name);
            }

            Entity prefab = null;

            if (entity.IsInstantiatedFromPrefab)
            {
                prefab = GetPrefab(entity.PrefabName);
                entityData.PrefabName = entity.PrefabName;
            }

            for (int j = 0; j < entity.Components.Count; j++)
            {
                Component component = entity.Components[j];
                Component componentFromPrefab = null;

                ComponentData componentData = null;

                if (prefab != null)
                {
                    componentFromPrefab = prefab.GetComponent(component.GetType());
                }

                ComponentInfo componentInfo = component.GetComponentInfo();

                if (componentFromPrefab == null)
                {
                    componentData = entityData.AddComponent(componentInfo);
                }

                foreach (ComponentPropertyInfo componentPropertyInfo in componentInfo.ComponentPropertyInfos.Values)
                {
                    string valueFromEntity = componentPropertyInfo.GetValueAsStringFrom(component);

                    bool appendProperty = true;

                    if (componentFromPrefab != null)
                    {
                        string valueFromPrefab = componentPropertyInfo.GetValueAsStringFrom(componentFromPrefab);

                        if (valueFromPrefab == valueFromEntity)
                        {
                            appendProperty = false;
                        }
                    }

                    if (appendProperty)
                    {
                        if (componentData == null)
                        {
                            componentData = entityData.AddComponent(componentInfo);
                        }

                        componentData.SetPropertyValue(componentPropertyInfo.Name, valueFromEntity);
                    }
                }
            }

            foreach (Entity childEntity in entity.Children)
            {
                SaveEntity(childEntity, entityData);
            }

            return entityData;
        }

        public void StartWithLoading(EngineData engineData)
        {
            if (!_started)
            {
                _started = true;

                EntityData parentEntityData = engineData.RootEntityData;

                _rootEntity = LoadEntity(null, parentEntityData);
            }
        }

        private Entity LoadEntity(Entity parentEntity, EntityData entityData)
        {
            string entityName = entityData.Name;
            bool instantiatedFromPrefab = entityData.IsInstantiatedFromPrefab;

            Entity entity = null;

            if (instantiatedFromPrefab)
            {
                string prefab = entityData.PrefabName;
                entity = InstantiatePrefab(prefab, null); //TODO: 
            }
            else if(parentEntity != null)
            {
                entity = parentEntity.CreateChildEntity(entityName);
                entity.Name = entityName;
            }
            else
            {
                entity = new Entity(this);
                entity.Name = entityName;
            }

            foreach (var componentData in entityData.Components)
            {
                string componentType = componentData.Type;

                //TODO: multiple instantiation of same component at same time?

                Component component = entity.GetComponent(componentType);

                if (component == null)
                {
                    component = entity.AddComponent(componentType);
                }

                ComponentInfo componentInfo = component.GetComponentInfo();

                foreach (var componentDataProperty in componentData.Properties)
                {
                    string propertyName = componentDataProperty.Name;
                    string propertyValue = componentDataProperty.Value;

                    ComponentPropertyInfo componentPropertyInfo = componentInfo.ComponentPropertyInfos[propertyName];

                    componentPropertyInfo.SetValueTo(component, propertyValue);
                }
            }

            foreach (var childEntityData in entityData.Children)
            {
                LoadEntity(entity, childEntityData);
            }

            return entity;
        }
    }

    public class LateUpdateMessage : EntityMessage
    {
        
    }

    public class LastUpdateMessage : EntityMessage
    {

    }
}
