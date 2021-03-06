﻿/******************************************************************************
Copyright (c) 2016 Koray Kiyakoglu

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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.View;
using Swarm2D.Library;

namespace Swarm2D.SpriteEditor
{
    class SpriteEditorDomain : EngineComponent
    {
        private SpriteDataEditor _spriteDataEditor;
        private SpriteData _spriteData;

        private string[] _spritePartNames;

        private bool _generated = false;

        protected override void OnAdded()
        {
            base.OnAdded();
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            try
            {
                if (!_generated)
                {
                    Dictionary<SpritePart, NineRegionSpriteParameters> spritePartsWithNineRegionSpriteParameters = new Dictionary<SpritePart, NineRegionSpriteParameters>();

                    _generated = true;

                    _spriteData = new SpriteData("spriteData");
                    _spriteDataEditor = new SpriteDataEditor(_spriteData);

                    _spritePartNames = GetSpritePartsList();

                    string[] categories = GetCategories(_spritePartNames);

                    foreach (var categoryName in categories)
                    {
                        string[] currentSpriteParts = GetSpritePartOfCategories(_spritePartNames, categoryName);

                        SpriteCategory category = _spriteDataEditor.AddNewSpriteCategory(categoryName);

                        foreach (var spritePartName in currentSpriteParts)
                        {
                            NineRegionSpriteParameters nineRegionSpriteParameters = HaveCustomParameters(category, spritePartName);
                            bool nineRegionParameter = nineRegionSpriteParameters != null;

                            SpritePart spritePart = _spriteDataEditor.AddSpritePart(category, spritePartName, nineRegionParameter);
                            _spriteDataEditor.GenerateSpriteFromSpritePart(spritePart);

                            if (nineRegionParameter)
                            {
                                spritePartsWithNineRegionSpriteParameters.Add(spritePart, nineRegionSpriteParameters);
                            }
                        }
                    }

                    _spriteDataEditor.CalculateSpriteSheets();

                    foreach (var spritePartsWithNineRegionSpriteParameter in spritePartsWithNineRegionSpriteParameters)
                    {
                        SpritePart spritePart = spritePartsWithNineRegionSpriteParameter.Key;
                        NineRegionSpriteParameters nineRegionSpriteParameters = spritePartsWithNineRegionSpriteParameter.Value;

                        _spriteDataEditor.GenerateNineRegionSpriteFromSpritePart(spritePart, nineRegionSpriteParameters);
                    }

                    _spriteDataEditor.SaveSpriteSheetData();
                    _spriteDataEditor.SaveSpriteSheets();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception: " + e);

                Environment.Exit(-1);
            }
        }

        private string[] GetFiles(string path, string postfix)
        {
            string fullPath = Resources.ResourcesPath + @"\" + path + @"\";

            string[] files = Directory.GetFiles(fullPath, "*." + postfix, SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                string result = files[i];

                result = result.Substring(fullPath.Length);
                result = result.Substring(0, result.Length - postfix.Length - 1);

                files[i] = result;
            }

            return files;
        }

        private NineRegionSpriteParameters HaveCustomParameters(SpriteCategory category, string spritePartName)
        {
            try
            {
                string spritePartsPath = @"SpriteParts\" + category.Name + @"\";
                string spritePartXmlName = Resources.ResourcesPath + @"\" + spritePartsPath + spritePartName + ".xml";

                if (File.Exists(spritePartXmlName))
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(spritePartXmlName);

                    XmlNode nineRegionSpriteNode = document.SelectSingleNode("NineRegionSprite");

                    if (nineRegionSpriteNode != null)
                    {
                        string name = nineRegionSpriteNode.SelectSingleNode("Name").InnerText;
                        int leftWidth = Convert.ToInt32(nineRegionSpriteNode.SelectSingleNode("LeftWidth").InnerText);
                        int rightWidth = Convert.ToInt32(nineRegionSpriteNode.SelectSingleNode("RightWidth").InnerText);
                        int topHeight = Convert.ToInt32(nineRegionSpriteNode.SelectSingleNode("TopHeight").InnerText);
                        int bottomHeight = Convert.ToInt32(nineRegionSpriteNode.SelectSingleNode("BottomHeight").InnerText);

                        NineRegionSpriteParameters nineRegionSpriteParameters = new NineRegionSpriteParameters(name, leftWidth, rightWidth, topHeight, bottomHeight);
                        return nineRegionSpriteParameters;
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private string[] GetSpritePartsList()
        {
            string[] spritePngPartsList = GetFiles("SpriteParts", "png");

            List<string> spritePartsList = new List<string>();
            spritePartsList.AddRange(spritePngPartsList);

            return spritePartsList.ToArray();
        }

        private string GetCategoryOf(string spritePartName)
        {
            int index = spritePartName.IndexOf(@"\");

            string category = "Default";

            if (index > 0)
            {
                category = spritePartName.Substring(0, index);
            }

            return category;
        }

        private string GetPureNameOf(string spritePartName)
        {
            string result = spritePartName;

            int index = spritePartName.IndexOf("\\");

            if (index > 0)
            {
                result = spritePartName.Substring(index + 1);
            }

            return result;
        }

        private string[] GetCategories(string[] spriteParts)
        {
            List<string> categories = new List<string>();

            foreach (var spritePart in spriteParts)
            {
                string categorty = GetCategoryOf(spritePart);

                if (!categories.Contains(categorty))
                {
                    categories.Add(categorty);
                }

            }

            return categories.ToArray();
        }

        private string[] GetSpritePartOfCategories(string[] allSpriteParts, string category)
        {
            List<string> result = new List<string>();

            foreach (var spritePart in allSpriteParts)
            {
                string currentCategory = GetCategoryOf(spritePart);

                if (currentCategory == category)
                {
                    string pureName = GetPureNameOf(spritePart);

                    result.Add(pureName);
                }
            }

            return result.ToArray();
        }
    }
}
