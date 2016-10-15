/******************************************************************************
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
using System.Linq;
using System.Text;

namespace Swarm2D.Engine.View
{
    public class SpriteCategory
    {
        public string Name { get; private set; }

        public SpriteData SpriteData { get; private set; }

        public List<SpritePart> SpriteParts { get; private set; }
        public List<SpritePart> SortedList { get; private set; }
        public List<Texture> SpriteSheets { get; private set; }

        public int SpriteSheetCount { get; set; }

        public bool IsLoaded { get; private set; }

        public SpriteCategory(string name, SpriteData spriteData, int spriteSheetCount)
        {
            Name = name;
            SpriteData = spriteData;
            SpriteSheetCount = spriteSheetCount;

            SpriteSheets = new List<Texture>();
            SpriteParts = new List<SpritePart>();
            SortedList = new List<SpritePart>();
        }

        public void Load()
        {
            if (!IsLoaded)
            {
                string resourcesName = SpriteData.ResourcesName;

                IsLoaded = true;

                for (int i = 1; i <= SpriteSheetCount; i++)
                {
                    Texture spriteSheet = IOSystem.Current.LoadTexture(resourcesName, @"SpriteSheets/" + Name + @"/sheet" + i);
                    SpriteSheets.Add(spriteSheet);
                }
            }
        }

        public void Unload()
        {
            if (IsLoaded)
            {
                SpriteSheets.Clear();
            }
        }

        public void SortList()
        {
            SortedList.Clear();

            foreach (SpritePart spriteInfo in SpriteParts)
            {
                SortedList.Add(spriteInfo);
            }

            for (int i = 0; i < SortedList.Count; i++)
            {
                int found = i;
                for (int j = i + 1; j < SortedList.Count; j++)
                {
                    SpritePart a = SortedList[found];
                    SpritePart b = SortedList[j];

                    if ((b.MaxX - b.MinX) * (b.MaxY - b.MinY) > (a.MaxX - a.MinX) * (a.MaxY - a.MinY))
                    {
                        found = j;
                    }
                }

                SpritePart swapper = SortedList[i];
                SortedList[i] = SortedList[found];
                SortedList[found] = swapper;
            }
        }
    }
}
