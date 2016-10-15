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
using Swarm2D.Engine.Logic;
using Swarm2D.Library;
using System.Xml;
using Swarm2D.Engine.Core;

namespace Swarm2D.Engine.View
{
    public class SpriteData
    {
        public Dictionary<string, SpritePart> SpritePartNames { get; private set; }
        public Dictionary<string, Sprite> SpriteNames { get; private set; }
        public Dictionary<string, SpriteCategory> SpriteCategories { get; private set; }

        public string Name { get; private set; }
        public string ResourcesName
        {
            get
            {
                if (_useCustomResources)
                {
                    return _resourcesName;
                }

                return Resources.ResourcesName;
            }
        }

        private string _resourcesName;
        private bool _useCustomResources = false;

        public SpriteData(string name)
        {
            Name = name;

            SpritePartNames = new Dictionary<string, SpritePart>();
            SpriteNames = new Dictionary<string, Sprite>();
            SpriteCategories = new Dictionary<string, SpriteCategory>();
        }

        public SpriteData(string resourcesName, string name)
            : this(name)
        {
            _resourcesName = resourcesName;
            _useCustomResources = true;
        }

        public Sprite GetSprite(string name)
        {
            if (SpriteNames.ContainsKey(name))
            {
                return SpriteNames[name];
            }

            return null;
        }

        private void LoadFromXml()
        {
            XmlDocument spriteData = Resources.LoadXmlData(ResourcesName, Name);

            XmlNode spriteDataNode = spriteData["SpriteData"];

            XmlNode spriteCategoriesNode = spriteDataNode["SpriteCategories"];
            XmlNode spritePartsNode = spriteDataNode["SpriteParts"];
            XmlNode spritesNode = spriteDataNode["Sprites"];

            foreach (XmlNode spriteCategoryNode in spriteCategoriesNode)
            {
                string name = spriteCategoryNode["Name"].InnerText;
                int spriteSheetCount = Convert.ToInt32(spriteCategoryNode["SpriteSheetCount"].InnerText);
                SpriteCategory spriteCategory = new SpriteCategory(name, this, spriteSheetCount);

                SpriteCategories.Add(spriteCategory.Name, spriteCategory);
            }

            foreach (XmlNode spritePartNode in spritePartsNode)
            {
                string name = spritePartNode["Name"].InnerText;
                int width = Convert.ToInt32(spritePartNode["Width"].InnerText);
                int height = Convert.ToInt32(spritePartNode["Height"].InnerText);

                string categoryName = spritePartNode["CategoryName"].InnerText;
                SpriteCategory spriteCategory = SpriteCategories[categoryName];

                SpritePart spritePart = new SpritePart(name, spriteCategory, width, height);

                spritePart.SheetID = Convert.ToInt32(spritePartNode["SheetID"].InnerText);

                spritePart.MinX = Convert.ToInt32(spritePartNode["MinX"].InnerText);
                spritePart.MaxX = Convert.ToInt32(spritePartNode["MaxX"].InnerText);
                spritePart.MinY = Convert.ToInt32(spritePartNode["MinY"].InnerText);
                spritePart.MaxY = Convert.ToInt32(spritePartNode["MaxY"].InnerText);

                spritePart.SheetX = Convert.ToInt32(spritePartNode["SheetX"].InnerText);
                spritePart.SheetY = Convert.ToInt32(spritePartNode["SheetY"].InnerText);
                spritePart.Rotated = Convert.ToBoolean(spritePartNode["Rotated"].InnerText);

                SpritePartNames.Add(spritePart.Name, spritePart);
                spritePart.Initialize();
            }

            foreach (XmlNode spriteNode in spritesNode)
            {
                Sprite sprite = null;

                if (spriteNode.Name == "GenericSprite")
                {
                    string spriteGenericName = spriteNode["Name"].InnerText;

                    string spritePartName = spriteNode["SpritePartName"].InnerText;
                    SpritePart spritePart = SpritePartNames[spritePartName];

                    SpriteGeneric spriteGeneric = new SpriteGeneric(spriteGenericName, spritePart);

                    sprite = spriteGeneric;
                }
                else if (spriteNode.Name == "NineRegionSprite")
                {
                    string spriteNineRegionName = spriteNode["Name"].InnerText;
                    string spritePartName = spriteNode["SpritePartName"].InnerText;
                    int leftWidth = Convert.ToInt32(spriteNode["LeftWidth"].InnerText);
                    int rightWidth = Convert.ToInt32(spriteNode["RightWidth"].InnerText);
                    int topHeight = Convert.ToInt32(spriteNode["TopHeight"].InnerText);
                    int bottomHeight = Convert.ToInt32(spriteNode["BottomHeight"].InnerText);

                    SpriteNineRegion spriteNineRegion = new SpriteNineRegion(spriteNineRegionName, SpritePartNames[spritePartName], leftWidth, rightWidth, topHeight, bottomHeight);

                    sprite = spriteNineRegion;
                }

                if (!SpriteNames.ContainsKey(sprite.Name))
                {
                    SpriteNames.Add(sprite.Name, sprite);
                }
            }
        }

        public void Load()
        {
            LoadFromXml();
        }
    }
}
