using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Swarm2D.Engine.Core
{
    [Serializable]
    public class EngineData
    {
        public EntityData RootEntityData { get; private set; }

        public EngineData(EntityData rootEntityData)
        {
            RootEntityData = rootEntityData;
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

        private List<EntityData> _children;

        public List<EntityData> Children { get { return _children; } }

        public EntityData(string name)
        {
            IsInstantiatedFromPrefab = false;
            Name = name;
            _components = new Dictionary<string, ComponentData>();
            _children = new List<EntityData>();
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

        public EntityData AddEntity(string name)
        {
            EntityData entityData = new EntityData(name);
            _children.Add(entityData);

            return entityData;
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
