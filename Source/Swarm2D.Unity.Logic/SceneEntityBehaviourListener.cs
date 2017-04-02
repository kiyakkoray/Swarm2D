using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Swarm2D.Unity.Logic
{
    public class SceneEntityBehaviourListener : MonoBehaviour
    {
        internal void OnSceneEntityUpdate()
        {
            SendMessage("SceneEntityUpdate", SendMessageOptions.DontRequireReceiver);
        }
    }
}
