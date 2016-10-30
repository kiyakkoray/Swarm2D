/******************************************************************************
Copyright (c) 2016 Koray Kiyakoglu

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
using System.Text;
using System.Xml;
using Swarm2D.Engine.Core;

namespace Swarm2D.Engine.Logic
{
    [Serializable]
    public class SceneData
    {
        private EntityData _sceneEntity;
        private List<EntityData> _entities;

        public IEnumerable<EntityData> Entities
        {
            get { return _entities; }
        }

        public IEnumerable<ComponentData> SceneControllers
        {
            get
            {
                return _sceneEntity.Components;
            }
        }

        public SceneData()
        {
            _entities = new List<EntityData>();
            _sceneEntity = new EntityData("Scene");
        }

        public EntityData AddEntity(string name)
        {
            EntityData entityData = new EntityData(name);
            _entities.Add(entityData);

            return entityData;
        }

        public ComponentData AddSceneController<T>() where T : Component
        {
            return _sceneEntity.AddComponent<T>();
        }

        public ComponentData AddSceneController(ComponentInfo componentInfo)
        {
            return _sceneEntity.AddComponent(componentInfo);
        }

        public ComponentData AddSceneController(string componentType)
        {
            return _sceneEntity.AddComponent(componentType);
        }

        public void LoadFromXML(XmlDocument xmlDocument)
        {
            XmlElement sceneNode = xmlDocument["Scene"];

            XmlElement sceneControllersElement = sceneNode["SceneControllers"];

            for (int i = 0; i < sceneControllersElement.ChildNodes.Count; i++)
            {
                XmlNode componentNode = sceneControllersElement.ChildNodes[i];

                string componentType = componentNode.Attributes["type"].Value;

                ComponentData component = AddSceneController(componentType);

                for (int k = 0; k < componentNode.ChildNodes.Count; k++)
                {
                    XmlNode propertyNode = componentNode.ChildNodes[k];

                    string propertyName = propertyNode.Attributes["name"].Value;
                    string propertyValue = propertyNode.Attributes["value"].Value;

                    component.SetPropertyValue(propertyName, propertyValue);
                }
            }

            XmlElement entitiesElement = sceneNode["Entities"];

            for (int i = 0; i < entitiesElement.ChildNodes.Count; i++)
            {
                XmlNode entitiyNode = entitiesElement.ChildNodes[i];

                string entityName = entitiyNode.Attributes["name"].Value;
                bool instantiatedFromPrefab = entitiyNode.Attributes["prefab"] != null;

                EntityData entity = AddEntity(entityName);

                if (instantiatedFromPrefab)
                {
                    string prefab = entitiyNode.Attributes["prefab"].Value;
                    entity.PrefabName = prefab;
                }

                XmlNode componentsNode = entitiyNode["Components"];

                for (int j = 0; j < componentsNode.ChildNodes.Count; j++)
                {
                    XmlNode componentNode = componentsNode.ChildNodes[j];

                    string componentType = componentNode.Attributes["type"].Value;

                    //TODO: multiple instantiation of same component at same time?

                    ComponentData component = entity.GetComponent(componentType);

                    if (component == null)
                    {
                        component = entity.AddComponent(componentType);
                    }

                    for (int k = 0; k < componentNode.ChildNodes.Count; k++)
                    {
                        XmlNode propertyNode = componentNode.ChildNodes[k];

                        string propertyName = propertyNode.Attributes["name"].Value;
                        string propertyValue = propertyNode.Attributes["value"].Value;

                        component.SetPropertyValue(propertyName, propertyValue);
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

            foreach (var component in _sceneEntity.Components)
            {
                ComponentInfo componentInfo = component.GetComponentInfo();

                XmlElement componentElement = xmlDocument.CreateElement("Component");
                sceneControllersElement.AppendChild(componentElement);

                componentElement.SetAttribute("type", componentInfo.Name);

                foreach (var componentProperty in component.Properties)
                {
                    string name = componentProperty.Name;
                    string value = componentProperty.Value;

                    XmlElement propertyElement = xmlDocument.CreateElement("Property");
                    componentElement.AppendChild(propertyElement);

                    propertyElement.SetAttribute("name", name);
                    propertyElement.SetAttribute("value", value);
                }
            }

            XmlElement entitiesElement = xmlDocument.CreateElement("Entities");
            sceneElement.AppendChild(entitiesElement);

            foreach (var entity in _entities)
            {
                XmlElement entityElement = xmlDocument.CreateElement("Entity");
                entitiesElement.AppendChild(entityElement);

                entityElement.SetAttribute("name", entity.Name);

                if (entity.IsInstantiatedFromPrefab)
                {
                    entityElement.SetAttribute("prefab", entity.PrefabName);
                }

                XmlElement componentsElement = xmlDocument.CreateElement("Components");
                entityElement.AppendChild(componentsElement);

                foreach (var component in entity.Components)
                {
                    ComponentInfo componentInfo = component.GetComponentInfo();

                    XmlElement componentElement = xmlDocument.CreateElement("Component");
                    componentsElement.AppendChild(componentElement);

                    componentElement.SetAttribute("type", componentInfo.Name);

                    foreach (var componentProperty in component.Properties)
                    {
                        string name = componentProperty.Name;
                        string value = componentProperty.Value;

                        XmlElement propertyElement = xmlDocument.CreateElement("Property");
                        componentElement.AppendChild(propertyElement);

                        propertyElement.SetAttribute("name", name);
                        propertyElement.SetAttribute("value", value);
                    }
                }
            }

            return xmlDocument;
        }
    }

    [Serializable]
    public class EntityData
    {
        public bool IsInstantiatedFromPrefab { get; private set; }

        public string PrefabName
        {
            get
            {
                return _prefabName;
            }
            set
            {
                _prefabName = value;
                IsInstantiatedFromPrefab = true;
            }
        }

        public string Name { get; private set; }

        private string _prefabName;

        private Dictionary<string, ComponentData> _components;

        public IEnumerable<ComponentData> Components
        {
            get
            {
                return _components.Values;
            }
        }

        public EntityData(string name)
        {
            IsInstantiatedFromPrefab = false;
            Name = name;
            _components = new Dictionary<string, ComponentData>();
        }

        public ComponentData GetComponent(string type)
        {
            if (_components.ContainsKey(type))
            {
                return _components[type];
            }

            return null;
        }

        public ComponentData AddComponent<T>() where T : Component
        {
            ComponentInfo componentInfo = ComponentInfo.GetComponentInfo(typeof(T));

            ComponentData componentData = new ComponentData(componentInfo);
            _components.Add(componentInfo.Name, componentData);

            return componentData;
        }

        public ComponentData AddComponent(ComponentInfo componentInfo)
        {
            ComponentData componentData = new ComponentData(componentInfo);
            _components.Add(componentInfo.Name, componentData);

            return componentData;
        }

        public ComponentData AddComponent(string componentType)
        {
            return AddComponent(ComponentInfo.GetComponentInfo(componentType));
        }
    }

    [Serializable]
    public class ComponentData
    {
        public string Type { get; private set; }
        private Dictionary<string, PropertyData> _propertyValues;

        public IEnumerable<PropertyData> Properties
        {
            get { return _propertyValues.Values; }
        }

        public ComponentData(ComponentInfo componentInfo)
        {
            Type = componentInfo.Name;
            _propertyValues = new Dictionary<string, PropertyData>();
        }

        public ComponentInfo GetComponentInfo()
        {
            return ComponentInfo.GetComponentInfo(Type);
        }

        public void SetPropertyValue(string name, string value)
        {
            if (!_propertyValues.ContainsKey(name))
            {
                _propertyValues.Add(name, new PropertyData(name, value));
            }
            else
            {
                _propertyValues[name] = new PropertyData(name, value);
            }
        }
    }

    [Serializable]
    public class PropertyData
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public ComponentPropertyType Type { get; private set; }

        public PropertyData(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
