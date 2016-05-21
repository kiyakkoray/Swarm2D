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
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.Engine.View
{
    public abstract class Sprite : Resource
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public Sprite(string name)
            : base(name)
        {

        }

        internal abstract void DrawToScreen(float mapX, float mapY, bool horizontalCenter, bool verticalCenter, float scale, bool flipped, float rotation, float width, float height);
    }

    public class SpriteGeneric : Sprite
    {
        public SpritePart SpritePart { get; set; }

        float[] vertices = new float[12];
        float[] uvs = new float[12];

        public SpriteGeneric(string name)
            : base(name)
        {

        }

        internal override void DrawToScreen(float mapX, float mapY, bool horizontalCenter, bool verticalCenter, float scale, bool flipped, float rotation, float width, float height)
        {
            Graphics2D.DrawSpritePart(SpritePart, mapX, mapY, vertices, uvs, 0, 0, rotation, horizontalCenter, verticalCenter, scale, flipped, width, height);

            Graphics.DrawArrays(0, 0, SpritePart.Texture, vertices, uvs);
        }
    }

    class SpriteNineRegionValue
    {
        public int Width;
        public int Height;
        public float[] Uvs;
        public float[] Vertices;

        public SpriteNineRegionValue()
        {
            Uvs = new float[72];
            Vertices = new float[72];
        }
    };

    public class SpriteNineRegion : Sprite
    {
        public SpritePart TopLeftSprite { get; set; }
        public SpritePart TopRightSprite { get; set; }

        public SpritePart BottomLeftSprite { get; set; }
        public SpritePart BottomRightSprite { get; set; }

        public SpritePart LeftSprite { get; set; }
        public SpritePart RightSprite { get; set; }

        public SpritePart TopSprite { get; set; }
        public SpritePart BottomSprite { get; set; }

        public SpritePart CenterSprite { get; set; }

        Dictionary<int, SpriteNineRegionValue> PrecalculatedValues { get; set; }

        public SpriteNineRegion(string name)
            : base(name)
        {
            PrecalculatedValues = new Dictionary<int, SpriteNineRegionValue>();
        }

        internal override void DrawToScreen(float mapX, float mapY, bool horizontalCenter, bool verticalCenter, float scale, bool flipped, float rotation, float customWidth, float customHeight)
        {
            //TODO: do not generate this every frame
            //TODO: clear non used pre calculated values
            //TODO: is this really increasing performance?

            int widthHeight = (int)customWidth;
            widthHeight = widthHeight << 16;
            widthHeight += (int)customHeight;

            SpriteNineRegionValue currentValue = null;

            if (!PrecalculatedValues.ContainsKey(widthHeight))
            {
                currentValue = new SpriteNineRegionValue();

                PrecalculatedValues.Add(widthHeight, currentValue);

                currentValue.Width = (int)customWidth;
                currentValue.Height = (int)customHeight;

                int left = LeftSprite.Width;
                int right = RightSprite.Width;
                int top = TopSprite.Height;
                int bottom = BottomSprite.Height;

                float centerWidth = customWidth - left - right;
                float centerHeight = customHeight - top - bottom;

                Graphics2D.DrawSpritePart(CenterSprite, left, top, currentValue.Vertices, currentValue.Uvs, 0, 0,
                    0.0f, false, false, 1.0f, false, customWidth - left - right, customHeight - top - bottom);

                Graphics2D.DrawSpritePart(TopLeftSprite, 0, 0, currentValue.Vertices, currentValue.Uvs, 8, 8);
                Graphics2D.DrawSpritePart(TopRightSprite, customWidth - TopRightSprite.Width, 0, currentValue.Vertices, currentValue.Uvs, 16, 16);
                Graphics2D.DrawSpritePart(BottomLeftSprite, 0, customHeight - BottomLeftSprite.Height, currentValue.Vertices, currentValue.Uvs, 24, 24);
                Graphics2D.DrawSpritePart(BottomRightSprite, customWidth - BottomRightSprite.Width, customHeight - BottomRightSprite.Height, currentValue.Vertices, currentValue.Uvs, 32, 32);

                Graphics2D.DrawSpritePart(TopSprite, TopLeftSprite.Width, 0, currentValue.Vertices, currentValue.Uvs, 40, 40,
                    0.0f, false, false, 1.0f, false, customWidth - TopLeftSprite.Width - TopRightSprite.Width, TopSprite.Height);

                Graphics2D.DrawSpritePart(BottomSprite, BottomLeftSprite.Width, customHeight - bottom, currentValue.Vertices, currentValue.Uvs, 48, 48,
                    0.0f, false, false, 1.0f, false, customWidth - BottomLeftSprite.Width - BottomRightSprite.Width, BottomSprite.Height);

                Graphics2D.DrawSpritePart(LeftSprite, 0, TopLeftSprite.Height, currentValue.Vertices, currentValue.Uvs, 56, 56,
                    0.0f, false, false, 1.0f, false, LeftSprite.Width, customHeight - TopLeftSprite.Height - BottomLeftSprite.Height);

                Graphics2D.DrawSpritePart(RightSprite, customWidth - right, TopRightSprite.Height, currentValue.Vertices, currentValue.Uvs, 64, 64,
                    0.0f, false, false, 1.0f, false, LeftSprite.Width, customHeight - TopRightSprite.Height - BottomRightSprite.Height);
            }
            else
            {
                currentValue = PrecalculatedValues[widthHeight];
            }

            Graphics.DrawArrays(mapX, mapY, CenterSprite.Texture, currentValue.Vertices, currentValue.Uvs);
        }
    }

    public class SpriteCombined : Sprite
    {
        List<SpritePartInfo> SpriteParts { get; set; }

        public SpriteCombined(string name)
            : base(name)
        {
            SpriteParts = new List<SpritePartInfo>();
        }

        internal override void DrawToScreen(float mapX, float mapY, bool horizontalCenter, bool verticalCenter, float scale, bool flipped, float rotation, float width, float height)
        {

        }
    }

    public class SpritePartInfo
    {
        public float X;
        public float Y;
        public SpritePart SpritePart;
    }

    public class SpritePart
    {
        public string Name;

        public int SheetID;

        public int MinX;
        public int MaxX;
        public int MinY;
        public int MaxY;

        public int Width;
        public int Height;

        public int SheetX;
        public int SheetY;
        public bool Rotated;

        public float MinU;
        public float MinV;
        public float MaxU;
        public float MaxV;

        public int RelativeWidth;
        public int RelativeHeight;

        public SpritePart(SpriteCategory category)
        {
            _category = category;
            _category.SpriteParts.Add(this);
        }

        public void Initialize()
        {
            RelativeWidth = MaxX - MinX + 1;
            RelativeHeight = MaxY - MinY + 1;

            //float division = 1.0f / (float)Parameters.SpriteSheetWidth;
            const double division = 1.0 / 2048.0;
            const double adder = 0;

            double minU, maxU, minV, maxV;

            if (Rotated)
            {
                minU = ((double)SheetX + adder) * division;
                maxU = ((double)(SheetX + RelativeHeight + adder)) * division;
                //MaxV = /*1.0f - */((float) SheetY) * division;
                //MinV = /*1.0f - */((float)(SheetY + RelativeWidth)) * division;

                minV = /*1.0f - */((double)SheetY + adder) * division;
                maxV = /*1.0f - */((double)(SheetY + RelativeWidth + adder)) * division;
            }
            else
            {
                minU = ((double)SheetX + adder) * division;
                maxU = ((double)(SheetX + RelativeWidth + adder)) * division;
                //MaxV = /*1.0f - */((float)SheetY) * division;
                //MinV = /*1.0f -*/ ((float)(SheetY + RelativeHeight)) * division;

                minV = /*1.0f - */((double)SheetY + adder) * division;
                maxV = /*1.0f -*/ ((double)(SheetY + RelativeHeight + adder)) * division;
            }

            MinU = (float)minU;
            MaxU = (float)maxU;
            MinV = (float)minV;
            MaxV = (float)maxV;
        }

        public Texture Texture
        {
            get
            {
                if (_category != null)
                {
                    if (_category.IsLoaded)
                    {
                        if (_category.SpriteSheets != null)
                        {
                            if (_category.SpriteSheets.Count >= SheetID)
                            {
                                return _category.SpriteSheets[SheetID - 1]; 
                            }
                        }
                    }
                }

                return null;
            }
        }

        public SpriteCategory Category
        {
            get
            {
                return _category;
            }
        }

        private SpriteCategory _category;
    }

    public class SpriteCategory
    {
        public SpriteData SpriteData { get; private set; }

        public List<SpritePart> SortedList { get; private set; }

        public SpriteCategory(SpriteData spriteData)
        {
            SpriteData = spriteData;

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
  
                for (int i = 1; i <= SheetCount; i++)
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

        public List<Texture> SpriteSheets;

        public int SpriteSheetCount;

        public string Name;

        public int SheetCount;

        public List<SpritePart> SpriteParts;

        public bool IsLoaded;
    }
}
