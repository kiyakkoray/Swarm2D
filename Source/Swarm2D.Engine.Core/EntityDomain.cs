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
using System.Text;
using Swarm2D.Library;

namespace Swarm2D.Engine.Core
{
    public class EntityDomain
    {
        private List<Component> _nonInitializedComponents;

        private Dictionary<int, List<MessageHandlerDelegate>> _domainMessageHandlerDelegates;
        private int _lastClonedPrefabId = 1;

        private Entity _entity;

        public EntityDomain(Entity entity)
        {
            _entity = entity;
            _domainMessageHandlerDelegates = new Dictionary<int, List<MessageHandlerDelegate>>();

            _nonInitializedComponents = new List<Component>();
        }

        public void SendMessage(DomainMessage message)
        {
            if (_domainMessageHandlerDelegates.ContainsKey(message.Id))
            {
                List<MessageHandlerDelegate> messageHandlers = _domainMessageHandlerDelegates[message.Id];

                for (int index = 0; index < messageHandlers.Count; index++)
                {
                    InitializeNonInitializedEntityComponents();

                    MessageHandlerDelegate messageHandlerDelegate = messageHandlers[index];

                    try
                    {
                        messageHandlerDelegate.Invoke(message);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Exception on domain message handler " + message + " " + messageHandlerDelegate.Target);
                        Debug.Log(e.Message);
                        Debug.Log("Stack:" + e.StackTrace);
                    }

                    InitializeNonInitializedEntityComponents();
                }
            }
        }

        public void InitializeNonInitializedEntityComponents()
        {
            while (_nonInitializedComponents.Count > 0)
            {
                Component[] nonInitializedComponents = _nonInitializedComponents.ToArray();

                _nonInitializedComponents.Clear();

                for (int i = 0; i < nonInitializedComponents.Length; i++)
                {
                    Component component = nonInitializedComponents[i];

                    Debug.Assert(!component.Entity.IsDestroyed, "a destroyed entity tried to initialize");

                    try
                    {
                        component.Initialize();
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Exception on component initialize, type of component " + component.GetType().Name);
                        Debug.Log(ex.Message);
                    }
                }
            }
        }

        public void OnComponentCreated(Component component)
        {
            foreach (KeyValuePair<int, List<MessageHandlerDelegate>> messageHandlers in component.DomainMessageHandlers)
            {
                int currentMessageId = messageHandlers.Key;

                if (!_domainMessageHandlerDelegates.ContainsKey(currentMessageId))
                {
                    _domainMessageHandlerDelegates.Add(currentMessageId, new List<MessageHandlerDelegate>());
                }

                foreach (MessageHandlerDelegate messageHandlerDelegate in messageHandlers.Value)
                {
                    _domainMessageHandlerDelegates[currentMessageId].Add(messageHandlerDelegate);
                }
            }

            _nonInitializedComponents.Add(component);
        }

        public void OnComponentDestroyed(Component component)
        {
            foreach (KeyValuePair<int, List<MessageHandlerDelegate>> messageHandlers in component.DomainMessageHandlers)
            {
                int currentMessageId = messageHandlers.Key;

                foreach (MessageHandlerDelegate messageHandlerDelegate in messageHandlers.Value)
                {
                    _domainMessageHandlerDelegates[currentMessageId].Remove(messageHandlerDelegate);
                }
            }

            if (_nonInitializedComponents.Contains(component))
            {
                _nonInitializedComponents.Remove(component);
            }
        }

        public Entity InstantiatePrefab(Entity prefab)
        {
            Entity clonedPrefab = null;

            //TODO: hmm
            //if (prefab.GetComponent<SceneEntity>() != null)
            {
                clonedPrefab = _entity.CreateChildEntity(prefab.Name + "(Clone)-" + _lastClonedPrefabId);
                clonedPrefab.SetAsInstantiatedFromPrefab(prefab.Name);
                _lastClonedPrefabId++;

                foreach (Component componentPrefab in prefab.Components)
                {
                    Component clonedComponent = clonedPrefab.GetComponent(componentPrefab.GetType());

                    if (clonedComponent == null)
                    {
                        clonedComponent = clonedPrefab.AddComponent(componentPrefab.GetType());
                    }
                    else
                    {

                    }

                    ComponentInfo componentInfo = componentPrefab.GetComponentInfo();

                    foreach (ComponentPropertyInfo propertyInfo in componentInfo.ComponentPropertyInfos.Values)
                    {
                        object value = propertyInfo.GetValueAsObjectFrom(componentPrefab);
                        propertyInfo.SetValueTo(clonedComponent, value);
                    }
                }
            }

            return clonedPrefab;
        }
    }

}
