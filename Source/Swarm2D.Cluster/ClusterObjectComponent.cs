using Swarm2D.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.Multiplayer;

namespace Swarm2D.Cluster
{
    public abstract class ClusterObjectComponent : Component
    {
        public ClusterObject ClusterObject { get; private set; }

        public NetworkController NetworkController { get; private set; }
        public CoroutineManager CoroutineManager { get; private set; }
        public ClusterNode ClusterNode { get; private set; }
        public ClusterObject RootClusterObject { get; private set; }

        protected override void OnAdded()
        {
            base.OnAdded();

            ClusterObject = GetComponent<ClusterObject>();

            NetworkController = Engine.RootEntity.GetComponent<NetworkController>();
            CoroutineManager = Engine.FindComponent<CoroutineManager>();
            ClusterNode = Engine.RootEntity.GetComponent<ClusterNode>();
            RootClusterObject = ClusterNode.RootClusterObject;
        }
    }
}
