using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Swarm2D.Library;

namespace Swarm2D.Unity.Logic
{
    public static class UnityExtensions
    {
        public static UnityEngine.Vector2 ToUnity3D(this Vector2 vector2)
        {
            return new UnityEngine.Vector2(vector2.X, vector2.Y);
        }
    }
}
