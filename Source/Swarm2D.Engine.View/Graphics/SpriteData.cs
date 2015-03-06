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

        private string _resourcesName;

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

        private void LoadFromBinary()
        {
            byte[] spriteData = Resources.LoadBinaryData(Name);
            DataReader reader = new DataReader(spriteData);

            int spriteCategoryCount = reader.ReadInt32();

            for (int i = 0; i < spriteCategoryCount; i++)
            {
                SpriteCategory spriteCategory = new SpriteCategory(this);

                spriteCategory.Name = reader.ReadUnicodeString();
                spriteCategory.SheetCount = reader.ReadInt32();

                SpriteCategories.Add(spriteCategory.Name, spriteCategory);
            }

            int spritePartCount = reader.ReadInt32();

            for (int j = 0; j < spritePartCount; j++)
            {
                SpritePart spritePart = new SpritePart();

                spritePart.SheetID = reader.ReadInt32();
                spritePart.Name = reader.ReadUnicodeString();

                spritePart.Width = reader.ReadInt32();
                spritePart.Height = reader.ReadInt32();

                spritePart.MinX = reader.ReadInt32();
                spritePart.MaxX = reader.ReadInt32();
                spritePart.MinY = reader.ReadInt32();
                spritePart.MaxY = reader.ReadInt32();

                spritePart.SheetX = reader.ReadInt32();
                spritePart.SheetY = reader.ReadInt32();
                spritePart.Rotated = reader.ReadBool();

                int spriteCategoryCountofSprite = reader.ReadInt32();

                for (int k = 0; k < spriteCategoryCountofSprite; k++)
                {
                    string spriteCategoryName = reader.ReadUnicodeString();
                    SpriteCategory spriteCategory = SpriteCategories[spriteCategoryName];
                    spritePart.Categories.Add(spriteCategory);
                    spriteCategory.SpriteParts.Add(spritePart);
                }

                SpritePartNames.Add(spritePart.Name, spritePart);
                spritePart.Initialize();
            }

            int spriteCount = reader.ReadInt32();

            for (int i = 0; i < spriteCount; i++)
            {
                string spriteName = reader.ReadUnicodeString();

                SpriteGeneric sprite = new SpriteGeneric(spriteName);

                sprite.Width = reader.ReadInt32();
                sprite.Height = reader.ReadInt32();

                string spritePartName = reader.ReadUnicodeString();
                sprite.SpritePart = SpritePartNames[spritePartName];

                //int spritePartInfoCount = reader.ReadInt32();
                //
                //for (int j = 0; j < spritePartInfoCount; j++)
                //{
                //	SpritePartInfo* spritePartInfo = new SpritePartInfo();
                //
                //	spritePartInfo.X = reader.ReadFloat();
                //	spritePartInfo.Y = reader.ReadFloat();
                //
                //	if (_spritePartNames.ContainsKey(spritePartName))
                //	{
                //		spritePartInfo.SpritePart = _spritePartNames.GetValue(spritePartName);
                //	}
                //
                //	//sprite.SpriteParts.Add(spritePartInfo);
                //	
                //}

                if (!SpriteNames.ContainsKey(sprite.Name))
                {
                    SpriteNames.Add(sprite.Name, sprite);
                }
            }

            /*foreach (SpritePart spritePart in SpritePartNames.Values)
            {
                Sprite sprite = new Sprite();

                sprite.Name = spritePart.Name;
                sprite.Width = spritePart.Width;
                sprite.Height = spritePart.Height;

                SpritePartInfo spritePartInfo = new SpritePartInfo();

                spritePartInfo.SpritePart = spritePart;
                spritePartInfo.X = spritePart.Width / 2;
                spritePartInfo.Y = spritePart.Height / 2;

                sprite.SpriteParts.Add(spritePartInfo);
                SpriteNames.Add(spritePart.Name, sprite);
            }*/
        }

        private void LoadFromXml()
        {
            XmlDocument spriteData = Resources.LoadXmlData(ResourcesName, Name);

            XmlNode spriteDataNode = spriteData["SpriteData"];

            XmlNode spriteCategoriesNode = spriteDataNode["SpriteCategories"];

            foreach (XmlNode spriteCategoryNode in spriteCategoriesNode)
            {
                SpriteCategory spriteCategory = new SpriteCategory(this);

                spriteCategory.Name = spriteCategoryNode["Name"].InnerText;
                spriteCategory.SheetCount = Convert.ToInt32(spriteCategoryNode["SpriteSheetCount"].InnerText);

                SpriteCategories.Add(spriteCategory.Name, spriteCategory);
            }

            XmlNode spritePartsNode = spriteDataNode["SpriteParts"];

            foreach (XmlNode spritePartNode in spritePartsNode)
            {
                SpritePart spritePart = new SpritePart();

                spritePart.SheetID = Convert.ToInt32(spritePartNode["SheetID"].InnerText);
                spritePart.Name = spritePartNode["Name"].InnerText;

                spritePart.Width = Convert.ToInt32(spritePartNode["Width"].InnerText);
                spritePart.Height = Convert.ToInt32(spritePartNode["Height"].InnerText);

                spritePart.MinX = Convert.ToInt32(spritePartNode["MinX"].InnerText);
                spritePart.MaxX = Convert.ToInt32(spritePartNode["MaxX"].InnerText);
                spritePart.MinY = Convert.ToInt32(spritePartNode["MinY"].InnerText);
                spritePart.MaxY = Convert.ToInt32(spritePartNode["MaxY"].InnerText);

                spritePart.SheetX = Convert.ToInt32(spritePartNode["SheetX"].InnerText);
                spritePart.SheetY = Convert.ToInt32(spritePartNode["SheetY"].InnerText);
                spritePart.Rotated = Convert.ToBoolean(spritePartNode["Rotated"].InnerText);

                XmlNode spritePartCategoriesNode = spritePartNode["SpritePartCategories"];

                foreach (XmlNode spritePartCategoryNode in spritePartCategoriesNode)
                {
                    string spriteCategoryName = spritePartCategoryNode.InnerText;
                    SpriteCategory spriteCategory = SpriteCategories[spriteCategoryName];
                    spritePart.Categories.Add(spriteCategory);
                    spriteCategory.SpriteParts.Add(spritePart);
                }

                SpritePartNames.Add(spritePart.Name, spritePart);
                spritePart.Initialize();
            }

            XmlNode spritesNode = spriteDataNode["Sprites"];

            foreach (XmlNode spriteNode in spritesNode)
            {
                Sprite sprite = null;

                if (spriteNode.Name == "GenericSprite")
                {
                    string spriteGenericName = spriteNode["Name"].InnerText;

                    SpriteGeneric spriteGeneric = new SpriteGeneric(spriteGenericName);

                    spriteGeneric.Width = Convert.ToInt32(spriteNode["Width"].InnerText);
                    spriteGeneric.Height = Convert.ToInt32(spriteNode["Height"].InnerText);

                    string spritePartName = spriteNode["SpritePartName"].InnerText;
                    spriteGeneric.SpritePart = SpritePartNames[spritePartName];

                    sprite = spriteGeneric;
                }
                else if (spriteNode.Name == "NineRegionSprite")
                {
                    string spriteNineRegionName = spriteNode["Name"].InnerText;

                    SpriteNineRegion spriteNineRegion = new SpriteNineRegion(spriteNineRegionName);

                    spriteNineRegion.Width = Convert.ToInt32(spriteNode["Width"].InnerText);
                    spriteNineRegion.Height = Convert.ToInt32(spriteNode["Height"].InnerText);

                    spriteNineRegion.TopLeftSprite = SpritePartNames[spriteNode["TopLeftSpritePartName"].InnerText];
                    spriteNineRegion.TopSprite = SpritePartNames[spriteNode["TopSpritePartName"].InnerText];
                    spriteNineRegion.TopRightSprite = SpritePartNames[spriteNode["TopRightSpritePartName"].InnerText];

                    spriteNineRegion.LeftSprite = SpritePartNames[spriteNode["LeftSpritePartName"].InnerText];
                    spriteNineRegion.RightSprite = SpritePartNames[spriteNode["RightSpritePartName"].InnerText];

                    spriteNineRegion.BottomLeftSprite = SpritePartNames[spriteNode["BottomLeftSpritePartName"].InnerText];
                    spriteNineRegion.BottomSprite = SpritePartNames[spriteNode["BottomSpritePartName"].InnerText];
                    spriteNineRegion.BottomRightSprite = SpritePartNames[spriteNode["BottomRightSpritePartName"].InnerText];

                    spriteNineRegion.CenterSprite = SpritePartNames[spriteNode["CenterSprite"].InnerText];

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
            //LoadFromBinary();
            LoadFromXml();
        }
    }
}
