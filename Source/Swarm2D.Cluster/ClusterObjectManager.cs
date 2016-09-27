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
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.Multiplayer;

namespace Swarm2D.Cluster
{
    public class ClusterObjectManager : Component, IEntityDomain
    {
        private EntityDomain _entityDomain;
        private NetworkController _networkController;

        protected override void OnAdded()
        {
            _networkController = Engine.FindComponent<NetworkController>();
            _entityDomain = new EntityDomain(Entity);
            Entity.ChildDomain = this;
        }

        void IEntityDomain.OnCreateChildEntity(Entity entity)
        {
            entity.Domain = this;

            NetworkView networkView = entity.AddComponent<NetworkView>();
            ClusterObject clusterObject = entity.AddComponent<ClusterObject>();
        }

        internal void CreateRootObject(ClusterPeer rootPeer, NetworkID networkId)
        {
            Entity rootClusterObjectEntity = Entity.CreateChildEntity("Root");
            RootClusterObject = rootClusterObjectEntity.GetComponent<ClusterObject>();
            RootClusterObject.OwnerNode = rootPeer;

            RootClusterObject.GetComponent<NetworkView>().NetworkID = networkId;
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

        public void SendMessage(DomainMessage message)
        {
            _entityDomain.SendMessage(message);
        }

        public ClusterObject RootClusterObject { get; private set; }
    }
}
