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
using System.Xml;
using Swarm2D.Engine.Core;
using Swarm2D.Library;
using Debug = Swarm2D.Library.Debug;

namespace Swarm2D.Engine.Logic
{
    //TODO:finish this class and generalize all entity domains
    public class EntityDomainComponent : Component, IEntityDomain
    {
        private EntityDomain _entityDomain;

        protected override void OnAdded()
        {
            _entityDomain = new EntityDomain(Entity);
        }

        public void SendMessage(DomainMessage message)
        {
            _entityDomain.SendMessage(message);
        }

        void IEntityDomain.OnComponentCreated(Component component)
        {
            _entityDomain.OnComponentCreated(component);
        }

        void IEntityDomain.OnComponentDestroyed(Component component)
        {
            _entityDomain.OnComponentDestroyed(component);
        }

        void IEntityDomain.OnCreateChildEntity(Entity entity)
        {

        }

        void IEntityDomain.OnEntityParentChanged(Entity entity)
        {

        }

        Entity IEntityDomain.InstantiatePrefab(Entity prefab)
        {
            return _entityDomain.InstantiatePrefab(prefab);
        }
    }

    public class Scene : Component, IEntityDomain
    {
        internal int CurrentFrame { get; private set; }

        public LinkedList<SceneEntity> SceneEntities { get; private set; }

        public List<SceneController> SceneControllers { get; private set; }

        private EntityDomain _entityDomain;

        public GameLogic GameLogic { get; internal set; }

        private readonly UpdateMessage _updateMessage = new UpdateMessage();
        private readonly  SceneControllerUpdateMessage _sceneControllerUpdateMessage = new SceneControllerUpdateMessage();

        protected override void OnAdded()
        {
            IsRunning = true;
            SceneEntities = new LinkedList<SceneEntity>();
            SceneControllers = new List<SceneController>();

            _entityDomain = new EntityDomain(Entity);
            Entity.ChildDomain = this;
        }

        void IEntityDomain.OnCreateChildEntity(Entity entity)
        {
            entity.Domain = this;

            SceneEntity sceneEntity = entity.AddComponent<SceneEntity>();

            sceneEntity.Scene = this;
            sceneEntity.NodeOnSceneEntitiesList = SceneEntities.AddLast(sceneEntity);
        }

        internal void OnIdleUpdate()
        {
            _entityDomain.InitializeNonInitializedEntityComponents();

            for (int i = 0; i < SceneControllers.Count; i++)
            {
                SceneControllers[i].OnIdleUpdate();

                _entityDomain.InitializeNonInitializedEntityComponents();
            }

            _entityDomain.InitializeNonInitializedEntityComponents();
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            UpdateMessage updateMessage = (UpdateMessage)message;

            if (IsRunning)
            {
                CurrentFrame++;

                _entityDomain.InitializeNonInitializedEntityComponents();

                _updateMessage.Dt = updateMessage.Dt;
                _sceneControllerUpdateMessage.Dt = updateMessage.Dt; 
                Entity.SendMessage(_sceneControllerUpdateMessage);
                SendMessage(_updateMessage);
            }
            else
            {
                this.OnIdleUpdate();
            }
        }

        public SceneEntity FindEntity(string name)
        {
            foreach (SceneEntity sceneEntity in SceneEntities)
            {
                if (sceneEntity.Entity.Name == name)
                {
                    return sceneEntity;
                }
            }

            return null;
        }

        internal void OnRemoveSceneEntity(SceneEntity entity)
        {
            SceneEntities.Remove(entity.NodeOnSceneEntitiesList);
            entity.NodeOnSceneEntitiesList = null;
        }

        public void Load(SceneData sceneData)
        {
            foreach (var sceneControllerData in sceneData.SceneControllers)
            {
                string componentType = sceneControllerData.Type;

                //TODO: multiple instantiation of same component at same time?

                Component component = Entity.GetComponent(componentType);

                if (component == null)
                {
                    component = Entity.AddComponent(componentType);
                }

                ComponentInfo componentInfo = component.GetComponentInfo();

                foreach (var propertyData in sceneControllerData.Properties)
                {
                    string propertyName = propertyData.Name;
                    string propertyValue = propertyData.Value;

                    ComponentPropertyInfo componentPropertyInfo = componentInfo.ComponentPropertyInfos[propertyName];

                    componentPropertyInfo.SetValueTo(component, propertyValue);
                }
            }

            foreach (var entityData in sceneData.Entities)
            {
                string entityName = entityData.Name;
                bool instantiatedFromPrefab = entityData.IsInstantiatedFromPrefab;

                Entity entity = null;

                if (instantiatedFromPrefab)
                {
                    string prefab = entityData.PrefabName;
                    entity = Engine.InstantiatePrefab(prefab, this);
                }
                else
                {
                    entity = Entity.CreateChildEntity(entityName);
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
            }
        }

        public SceneData Save()
        {
            SceneData sceneData = new SceneData();

            for (int i = 0; i < Entity.Components.Count; i++)
            {
                Component component = Entity.Components[i];
                ComponentInfo componentInfo = component.GetComponentInfo();

                ComponentData componentData = sceneData.AddSceneController(componentInfo);

                foreach (ComponentPropertyInfo componentPropertyInfo in componentInfo.ComponentPropertyInfos.Values)
                {
                    string value = componentPropertyInfo.GetValueAsStringFrom(component);

                    componentData.SetPropertyValue(componentPropertyInfo.Name, value);
                }
            }

            foreach (SceneEntity sceneEntity in SceneEntities)
            {
                Entity entity = sceneEntity.Entity;
                EntityData entityData = sceneData.AddEntity(entity.Name);

                Entity prefab = null;

                if (entity.IsInstantiatedFromPrefab)
                {
                    prefab = Engine.GetPrefab(entity.PrefabName);
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
            }

            return sceneData;
        }

        public T[] FindComponentsOfSceneEntities<T>() where T : SceneEntityComponent
        {
            List<T> result = new List<T>();

            foreach (SceneEntity sceneEntity in SceneEntities)
            {
                T component = sceneEntity.GetComponent<T>();

                if (component != null)
                {
                    result.Add(component);
                }
            }

            return result.ToArray();
        }

        public void SendMessage(DomainMessage message)
        {
            _entityDomain.SendMessage(message);
        }

        void IEntityDomain.OnComponentCreated(Component component)
        {
            _entityDomain.OnComponentCreated(component);
        }

        void IEntityDomain.OnComponentDestroyed(Component component)
        {
            _entityDomain.OnComponentDestroyed(component);
        }

        Entity IEntityDomain.InstantiatePrefab(Entity prefab)
        {
            return _entityDomain.InstantiatePrefab(prefab);
        }

        public bool IsRunning { get; set; }

        void IEntityDomain.OnEntityParentChanged(Entity entity)
        {

        }

        public SceneEntity InstantiatePrefab(string prefabName, Vector2 position, float rotation)
        {
            Entity entity = Engine.InstantiatePrefab(prefabName, this);
            SceneEntity sceneEntity = entity.GetComponent<SceneEntity>();

            Debug.Assert(sceneEntity != null, "sceneEntity != null");

            sceneEntity.LocalPosition = position;
            sceneEntity.LocalRotation = rotation;

            return sceneEntity;
        }
    }

    public class SceneControllerUpdateMessage : EntityMessage
    {
        public float Dt { get; internal set; }
    }
}
