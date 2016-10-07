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

        public Engine(bool pooledMode)
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
            EntityDomain.InitializeNonInitializedEntityComponents();
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
    }

    public class LateUpdateMessage : EntityMessage
    {
        
    }

    public class LastUpdateMessage : EntityMessage
    {

    }
}
