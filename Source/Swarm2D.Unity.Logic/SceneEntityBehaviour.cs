using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using UnityEngine;

namespace Swarm2D.Unity.Logic
{
    public class SceneEntityBehaviour : SceneEntityComponent
    {
        public SceneEntityBehaviourListener Listener { get; set; }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            if (Listener != null)
            {
                Listener.OnSceneEntityUpdate();
            }
        }
    }
}
