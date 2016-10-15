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
                _entityDomain.StartNotStartedEntityComponents();

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

        public void LoadFromXML(XmlDocument xmlDocument)
        {
            //SceneEntities.Clear();
            //SceneControllers.Clear();

            XmlElement sceneNode = xmlDocument["Scene"];

            XmlElement sceneControllersElement = sceneNode["SceneControllers"];

            for (int i = 0; i < sceneControllersElement.ChildNodes.Count; i++)
            {
                XmlNode componentNode = sceneControllersElement.ChildNodes[i];

                string componentType = componentNode.Attributes["type"].Value;

                Component component = null;

                if (componentType == "Scene")
                {
                    component = this;
                }
                else
                {
                    component = Entity.AddComponent(componentType);
                }

                ComponentInfo componentInfo = component.GetComponentInfo();

                for (int k = 0; k < componentNode.ChildNodes.Count; k++)
                {
                    XmlNode propertyNode = componentNode.ChildNodes[k];

                    string propertyName = propertyNode.Attributes["name"].Value;
                    string propertyValue = propertyNode.Attributes["value"].Value;

                    ComponentPropertyInfo componentPropertyInfo = componentInfo.ComponentPropertyInfos[propertyName];

                    componentPropertyInfo.SetValueTo(component, propertyValue);
                }
            }

            XmlElement entitiesElement = sceneNode["Entities"];

            for (int i = 0; i < entitiesElement.ChildNodes.Count; i++)
            {
                XmlNode entitiyNode = entitiesElement.ChildNodes[i];

                string entityName = entitiyNode.Attributes["name"].Value;
                bool instantiatedFromPrefab = entitiyNode.Attributes["prefab"] != null;

                Entity entity = null;

                if (instantiatedFromPrefab)
                {
                    string prefab = entitiyNode.Attributes["prefab"].Value;
                    entity = Engine.InstantiatePrefab(prefab, this);
                }
                else
                {
                    entity = Entity.CreateChildEntity(entityName);
                    entity.Name = entityName;
                }

                XmlNode componentsNode = entitiyNode["Components"];

                for (int j = 0; j < componentsNode.ChildNodes.Count; j++)
                {
                    XmlNode componentNode = componentsNode.ChildNodes[j];

                    string componentType = componentNode.Attributes["type"].Value;

                    //TODO: multiple instantiation of same component at same time?

                    Component component = entity.GetComponent(componentType);

                    if (component == null)
                    {
                        component = entity.AddComponent(componentType);
                    }

                    ComponentInfo componentInfo = component.GetComponentInfo();

                    for (int k = 0; k < componentNode.ChildNodes.Count; k++)
                    {
                        XmlNode propertyNode = componentNode.ChildNodes[k];

                        string propertyName = propertyNode.Attributes["name"].Value;
                        string propertyValue = propertyNode.Attributes["value"].Value;

                        ComponentPropertyInfo componentPropertyInfo = componentInfo.ComponentPropertyInfos[propertyName];

                        componentPropertyInfo.SetValueTo(component, propertyValue);
                    }
                }
            }
        }

        public XmlDocument SaveToXML()
        {
            XmlDocument xmlDocument = new XmlDocument();

            XmlElement sceneElement = xmlDocument.CreateElement("Scene");
            xmlDocument.AppendChild(sceneElement);

            XmlElement sceneControllersElement = xmlDocument.CreateElement("SceneControllers");
            sceneElement.AppendChild(sceneControllersElement);

            for (int i = 0; i < Entity.Components.Count; i++)
            {
                Component component = Entity.Components[i];
                ComponentInfo componentInfo = component.GetComponentInfo();

                XmlElement componentElement = xmlDocument.CreateElement("Component");
                sceneControllersElement.AppendChild(componentElement);

                componentElement.SetAttribute("type", componentInfo.Name);

                foreach (ComponentPropertyInfo componentPropertyInfo in componentInfo.ComponentPropertyInfos.Values)
                {
                    string value = componentPropertyInfo.GetValueAsStringFrom(component);

                    XmlElement propertyElement = xmlDocument.CreateElement("Property");
                    componentElement.AppendChild(propertyElement);

                    propertyElement.SetAttribute("name", componentPropertyInfo.Name);
                    propertyElement.SetAttribute("value", value);
                }
            }

            XmlElement entitiesElement = xmlDocument.CreateElement("Entities");
            sceneElement.AppendChild(entitiesElement);

            foreach (SceneEntity sceneEntity in SceneEntities)
            {
                Entity entity = sceneEntity.Entity;

                XmlElement entityElement = xmlDocument.CreateElement("Entity");
                entitiesElement.AppendChild(entityElement);

                entityElement.SetAttribute("name", entity.Name);

                Entity prefab = null;

                if (entity.IsInstantiatedFromPrefab)
                {
                    entityElement.SetAttribute("prefab", entity.PrefabName);
                    prefab = Engine.GetPrefab(entity.PrefabName);
                }

                XmlElement componentsElement = xmlDocument.CreateElement("Components");
                entityElement.AppendChild(componentsElement);

                for (int j = 0; j < entity.Components.Count; j++)
                {
                    Component component = entity.Components[j];
                    Component componentFromPrefab = null;

                    if (prefab != null)
                    {
                        componentFromPrefab = prefab.GetComponent(component.GetType());
                    }

                    ComponentInfo componentInfo = component.GetComponentInfo();

                    XmlElement componentElement = xmlDocument.CreateElement("Component");

                    bool appendComponent = componentFromPrefab == null;

                    componentElement.SetAttribute("type", componentInfo.Name);

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
                            appendComponent = true;

                            XmlElement propertyElement = xmlDocument.CreateElement("Property");
                            componentElement.AppendChild(propertyElement);

                            propertyElement.SetAttribute("name", componentPropertyInfo.Name);
                            propertyElement.SetAttribute("value", valueFromEntity);
                        }
                    }

                    if (appendComponent)
                    {
                        componentsElement.AppendChild(componentElement);
                    }
                }
            }
            return xmlDocument;
            //xmlDocument.Save(@"SxTest\Scenes\export.xml");
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
