using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swarm2D.SpriteEditor
{
    public class NineRegionSpriteParameters
    {
        public string Name { get; private set; }
        public int LeftWidth { get; private set; }
        public int RightWidth { get; private set; }
        public int TopHeight { get; private set; }
        public int BottomHeight { get; private set; }

        public NineRegionSpriteParameters(string name, int leftWidth, int rightWidth, int topHeight, int bottomHeight)
        {
            Name = name;
            LeftWidth = leftWidth;
            RightWidth = rightWidth;
            TopHeight = topHeight;
            BottomHeight = bottomHeight;
        }
    }
}
