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
using Swarm2D.Engine.Core;
using Swarm2D.Library;

namespace Swarm2D.Engine.Logic
{
    public class CoroutineManager : Component, IEntityDomain
    {
        private EntityDomain _entityDomain;

        protected override void OnAdded()
        {
            base.OnAdded();

            _entityDomain = new EntityDomain(Entity);
            Entity.ChildDomain = this;
        }

        public void SendMessage(DomainMessage message)
        {
            _entityDomain.SendMessage(message);
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            if (Entity.Children.Count > 0)
            {
                Entity[] coroutineEntities = Entity.Children.ToArray();

                foreach (var coroutineEntity in coroutineEntities)
                {
                    Coroutine currentCoroutine = coroutineEntity.GetComponent<Coroutine>();

                    currentCoroutine.DoJob();

                    if (currentCoroutine.IsFinished)
                    {
                        coroutineEntity.Destroy();
                    }
                }
            }
        }

        public Coroutine StartCoroutine(Component coroutineOwner, CoroutineMethod coroutineMethod, object parameter = null)
        {
            Entity coroutineEntity = Entity.CreateChildEntity("Coroutine");

            Coroutine coroutine = coroutineEntity.GetComponent<Coroutine>();
            coroutine.Owner = coroutineOwner;
            coroutine.Parameter = parameter;
            coroutine.CoroutineMethod = coroutineMethod;

            return coroutine;
        }

        void IEntityDomain.OnCreateChildEntity(Entity entity)
        {
            entity.Domain = this;

            Coroutine coroutine = entity.AddComponent<Coroutine>();
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

        void IEntityDomain.OnEntityParentChanged(Entity entity)
        {

        }
    }
}
