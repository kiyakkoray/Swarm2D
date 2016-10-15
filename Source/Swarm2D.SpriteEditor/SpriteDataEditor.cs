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
using Swarm2D.Engine.View;
using Swarm2D.Library;
using Swarm2D.Engine.Logic;
using System.Drawing;
using System.IO;
using System.Xml;
using Swarm2D.Engine.Core;

namespace Swarm2D.SpriteEditor
{
    public class SpriteDataEditor
    {
        public SpriteData SpriteData { get; private set; }

        private List<SpriteSheet> _allSpriteSheets;

        public SpriteDataEditor(SpriteData spriteData)
        {
            SpriteData = spriteData;

            _allSpriteSheets = new List<SpriteSheet>();
        }

        public SpriteCategory AddNewSpriteCategory(string categoryName)
        {
            SpriteCategory spriteCategory = new SpriteCategory(categoryName, SpriteData, 0);
            SpriteData.SpriteCategories.Add(categoryName, spriteCategory);

            return spriteCategory;
        }

        public void DeleteSpriteCategory(SpriteCategory spriteCategory)
        {
            SpriteData.SpriteCategories.Remove(spriteCategory.Name);
        }

        public Texture GetSpritePartTexture(string name)
        {
            return null;
        }

        public Bitmap GetSpritePartBitmap(SpritePart spritePart)
        {
            string name = spritePart.Name;
            string categoryName = spritePart.Category.Name;

            string path = Resources.ResourcesPath + @"\SpriteParts\" + categoryName + @"\" + name + ".png";

            if (File.Exists(path))
            {
                return Bitmap.FromFile(path) as Bitmap;
            }

            return null;
        }

        public SpritePart AddSpritePart(SpriteCategory category, string name, bool disableEmptyRegionPrunning)
        {
            string spritePartsPath = @"SpriteParts\" + category.Name + @"\";
            string bitmapPngName = Resources.ResourcesPath + @"\" + spritePartsPath + name + ".png";

            Bitmap bitmap = (Bitmap)Image.FromFile(bitmapPngName);

            SpritePart spritePart = new SpritePart(name, category, bitmap.Width, bitmap.Height);

            spritePart.SheetID = -1;

            if (disableEmptyRegionPrunning)
            {
                spritePart.MinX = 0;
                spritePart.MaxX = bitmap.Width - 1;
                spritePart.MinY = 0;
                spritePart.MaxY = bitmap.Height - 1;
            }
            else
            {
                spritePart.MinX = BitmapOperations.FindMinX(bitmap);
                spritePart.MaxX = BitmapOperations.FindMaxX(bitmap);
                spritePart.MinY = BitmapOperations.FindMinY(bitmap);
                spritePart.MaxY = BitmapOperations.FindMaxY(bitmap);
            }

            SpriteData.SpritePartNames.Add(spritePart.Name, spritePart);

            return spritePart;
        }

        public void GenerateNineRegionSpriteFromSpritePart(SpritePart spritePart, NineRegionSpriteParameters nineRegionSpriteParameters)
        {
            string name = nineRegionSpriteParameters.Name;
            int leftWidth = nineRegionSpriteParameters.LeftWidth;
            int rightWidth = nineRegionSpriteParameters.RightWidth;
            int topHeight = nineRegionSpriteParameters.TopHeight;
            int bottomHeight = nineRegionSpriteParameters.BottomHeight;
         
            SpriteNineRegion sprite = new SpriteNineRegion(name, spritePart, leftWidth, rightWidth, topHeight, bottomHeight);

            SpriteData.SpriteNames.Add(sprite.Name, sprite);
        }

        public void GenerateSpriteFromSpritePart(SpritePart spritePart)
        {
            SpriteGeneric sprite = new SpriteGeneric(spritePart.Name, spritePart);
            SpriteData.SpriteNames.Add(sprite.Name, sprite);
        }

        public bool IsSpritePartImported(string name)
        {
            return SpriteData.SpritePartNames.ContainsKey(name);
        }

        public void CalculateSpriteSheets()
        {
            _allSpriteSheets.Clear();

            string spritePath = Resources.ResourcesPath + @"\SpriteSheets";

            //for (int i = 0; i < SpriteData.SpriteCategories.Count; i++)
            foreach (SpriteCategory spriteCategory in SpriteData.SpriteCategories.Values)
            {
                string spriteSheetPath = spritePath + @"\" + spriteCategory.Name + @"\";
                spriteCategory.SortList();

                List<SpriteSheet> currentSpriteSheets = new List<SpriteSheet>();
                SpriteSheet currentSheet = new SpriteSheet(this);

                Debug.Log("Creating new sheet: " + currentSpriteSheets.Count);

                currentSpriteSheets.Add(currentSheet);
                _allSpriteSheets.Add(currentSheet);

                int currentSheetID = 1;

                for (int j = 0; j < spriteCategory.SortedList.Count; j++)
                {
                    SpritePart spriteInfo = spriteCategory.SortedList[j];

                    if (spriteInfo.Width > 2048 /*Parameters.SpriteSheetWidth*/ || spriteInfo.Height > 2048 /*Parameters.SpriteSheetHeight*/)
                    {
                        continue;
                    }

                    Debug.Log("sprite " + spriteInfo.Name + " placing");

                    bool added = false;
                    int id = 1;

                    for (int k = 0; k < currentSpriteSheets.Count; k++)
                    {
                        SpriteSheet spriteSheet = currentSpriteSheets[k];

                        if (spriteSheet.AddSpritePart(spriteInfo))
                        {
                            spriteInfo.SheetID = id;
                            added = true;
                            break;
                        }

                        id++;
                    }

                    if (!added)
                    {
                        currentSheet = new SpriteSheet(this);
                        currentSpriteSheets.Add(currentSheet);
                        _allSpriteSheets.Add(currentSheet);

                        Debug.Log("Creating new sheet: " + currentSpriteSheets.Count);

                        currentSheet.AddSpritePart(spriteInfo);
                        currentSheetID++;
                        spriteInfo.SheetID = currentSheetID;
                    }
                }

                spriteCategory.SpriteSheetCount = currentSheetID;

                int sheetID = 1;

                for (int j = 0; j < currentSpriteSheets.Count(); j++)
                {
                    SpriteSheet spriteSheet = currentSpriteSheets[j];

                    spriteSheet.ID = sheetID;
                    spriteSheet.Path = spriteSheetPath;
                    sheetID++;
                }
            }
        }

        public void SaveSpriteSheetData()
        {
            SaveSpriteSheetDataAsXml();
        }

        private void SaveSpriteSheetDataAsXml()
        {
            XmlDocument doc = new XmlDocument();

            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode spriteDataNode = doc.CreateElement("SpriteData");
            doc.AppendChild(spriteDataNode);

            XmlNode spriteCategoriesNode = doc.CreateElement("SpriteCategories");
            spriteDataNode.AppendChild(spriteCategoriesNode);

            foreach (SpriteCategory spriteCategory in SpriteData.SpriteCategories.Values)
            {
                XmlNode spriteCategoryNode = doc.CreateElement("SpriteCategory");
                spriteCategoriesNode.AppendChild(spriteCategoryNode);

                XmlNode nameNode = doc.CreateElement("Name");
                nameNode.InnerText = spriteCategory.Name;
                spriteCategoryNode.AppendChild(nameNode);

                XmlNode countNode = doc.CreateElement("SpriteSheetCount");
                countNode.InnerText = spriteCategory.SpriteSheetCount.ToString();
                spriteCategoryNode.AppendChild(countNode);
            }

            XmlNode spritePartsNode = doc.CreateElement("SpriteParts");
            spriteDataNode.AppendChild(spritePartsNode);

            foreach (SpritePart spritePart in SpriteData.SpritePartNames.Values)
            {
                XmlNode spritePartNode = doc.CreateElement("SpritePart");
                spritePartsNode.AppendChild(spritePartNode);

                XmlNode sheetIDNode = doc.CreateElement("SheetID");
                sheetIDNode.InnerText = spritePart.SheetID.ToString();
                spritePartNode.AppendChild(sheetIDNode);

                XmlNode nameNode = doc.CreateElement("Name");
                nameNode.InnerText = spritePart.Name;
                spritePartNode.AppendChild(nameNode);

                XmlNode widthNode = doc.CreateElement("Width");
                widthNode.InnerText = spritePart.Width.ToString();
                spritePartNode.AppendChild(widthNode);

                XmlNode heightNode = doc.CreateElement("Height");
                heightNode.InnerText = spritePart.Height.ToString();
                spritePartNode.AppendChild(heightNode);

                XmlNode minXNode = doc.CreateElement("MinX");
                minXNode.InnerText = spritePart.MinX.ToString();
                spritePartNode.AppendChild(minXNode);

                XmlNode maxXNode = doc.CreateElement("MaxX");
                maxXNode.InnerText = spritePart.MaxX.ToString();
                spritePartNode.AppendChild(maxXNode);

                XmlNode minYNode = doc.CreateElement("MinY");
                minYNode.InnerText = spritePart.MinY.ToString();
                spritePartNode.AppendChild(minYNode);

                XmlNode maxYNode = doc.CreateElement("MaxY");
                maxYNode.InnerText = spritePart.MaxY.ToString();
                spritePartNode.AppendChild(maxYNode);

                XmlNode sheetXNode = doc.CreateElement("SheetX");
                sheetXNode.InnerText = spritePart.SheetX.ToString();
                spritePartNode.AppendChild(sheetXNode);

                XmlNode sheetYNode = doc.CreateElement("SheetY");
                sheetYNode.InnerText = spritePart.SheetY.ToString();
                spritePartNode.AppendChild(sheetYNode);

                XmlNode rotatedNode = doc.CreateElement("Rotated");
                rotatedNode.InnerText = spritePart.Rotated.ToString();
                spritePartNode.AppendChild(rotatedNode);

                XmlNode categoryNameNode = doc.CreateElement("CategoryName");
                categoryNameNode.InnerText = spritePart.Category.Name;
                spritePartNode.AppendChild(categoryNameNode);
            }

            XmlNode spritesNode = doc.CreateElement("Sprites");
            spriteDataNode.AppendChild(spritesNode);

            foreach (Sprite sprite in SpriteData.SpriteNames.Values)
            {
                if (sprite is SpriteGeneric)
                {
                    XmlNode genericSpriteNode = doc.CreateElement("GenericSprite");
                    spritesNode.AppendChild(genericSpriteNode);

                    XmlNode nameNode = doc.CreateElement("Name");
                    nameNode.InnerText = sprite.Name;
                    genericSpriteNode.AppendChild(nameNode);

                    XmlNode spritePartName = doc.CreateElement("SpritePartName");
                    spritePartName.InnerText = ((SpriteGeneric)sprite).SpritePart.Name;
                    genericSpriteNode.AppendChild(spritePartName);
                }
                else if (sprite is SpriteNineRegion)
                {
                    XmlNode nineRegionSpriteNode = doc.CreateElement("NineRegionSprite");
                    spritesNode.AppendChild(nineRegionSpriteNode);

                    XmlNode nameNode = doc.CreateElement("Name");
                    nameNode.InnerText = sprite.Name;
                    nineRegionSpriteNode.AppendChild(nameNode);

                    {
                        XmlNode spritePartName = doc.CreateElement("SpritePartName");
                        spritePartName.InnerText = ((SpriteNineRegion)sprite).BaseSprite.Name;
                        nineRegionSpriteNode.AppendChild(spritePartName);
                    }

                    {
                        XmlNode valueNode = doc.CreateElement("LeftWidth");
                        valueNode.InnerText = ((SpriteNineRegion)sprite).LeftWidth.ToString();
                        nineRegionSpriteNode.AppendChild(valueNode);
                    }

                    {
                        XmlNode valueNode = doc.CreateElement("RightWidth");
                        valueNode.InnerText = ((SpriteNineRegion)sprite).RightWidth.ToString();
                        nineRegionSpriteNode.AppendChild(valueNode);
                    }

                    {
                        XmlNode valueNode = doc.CreateElement("TopHeight");
                        valueNode.InnerText = ((SpriteNineRegion)sprite).TopHeight.ToString();
                        nineRegionSpriteNode.AppendChild(valueNode);
                    }

                    {
                        XmlNode valueNode = doc.CreateElement("BottomHeight");
                        valueNode.InnerText = ((SpriteNineRegion)sprite).BottomHeight.ToString();
                        nineRegionSpriteNode.AppendChild(valueNode);
                    }
                }
            }

            Resources.SaveXmlData(SpriteData.Name, doc);
        }

        public static void EmptyDirectory(DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories())
            {
                subDirectory.Delete(true);
            }
        }

        public void SaveSpriteSheets()
        {
            string spritePath = Resources.ResourcesPath + @"\SpriteSheets\";

            if (Directory.Exists(spritePath))
            {
                DirectoryInfo info = new DirectoryInfo(spritePath);
                EmptyDirectory(info);
            }
            else
            {
                Directory.CreateDirectory(spritePath);
            }

            Debug.Log("Creating sprite sheet directories");

            for (int i = 0; i < _allSpriteSheets.Count; i++)
            {
                SpriteSheet spriteSheet = _allSpriteSheets[i];

                if (!Directory.Exists(spriteSheet.Path))
                {
                    Directory.CreateDirectory(spriteSheet.Path);
                }
            }

            for (int i = 0; i < _allSpriteSheets.Count; i++)
            {
                SpriteSheet spriteSheet = _allSpriteSheets[i];

                Debug.Log("saving sprite sheet " + spriteSheet.ID);

                spriteSheet.SaveSheet();
            }
        }
    }
}
