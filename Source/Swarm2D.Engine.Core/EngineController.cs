﻿/******************************************************************************
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

namespace Swarm2D.Engine.Core
{
    [Serializable]
    internal class EngineController : Component, IEntityDomain
    {
        internal EntityDomain EntityDomain { get; private set; }

        protected override void OnAdded()
        {
            base.OnAdded();

            EntityDomain = new EntityDomain(Entity);
            Entity.ChildDomain = this;
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            EntityDomain.InitializeNonInitializedEntityComponents();
        }

        void IEntityDomain.OnCreateChildEntity(Entity entity)
        {
            entity.Domain = this;

            EngineEntity engineEntity = entity.AddComponent<EngineEntity>();
        }

        void IEntityDomain.SendMessage(DomainMessage message)
        {
            EntityDomain.SendMessage(message);
        }

        void IEntityDomain.OnComponentCreated(Component component)
        {
            EntityDomain.OnComponentCreated(component);
        }

        void IEntityDomain.OnComponentDestroyed(Component component)
        {
            EntityDomain.OnComponentDestroyed(component);
        }

        Entity IEntityDomain.InstantiatePrefab(Entity prefab)
        {
            return EntityDomain.InstantiatePrefab(prefab);
        }

        void IEntityDomain.OnEntityParentChanged(Entity entity)
        {

        }
    }
}
