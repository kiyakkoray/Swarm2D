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
using System.Xml;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.Engine.View
{
    public class Font : Resource
    {
        public Font(string name)
            : base(name)
        {
            Characters = new Dictionary<int, FontCharacter>();

            XmlDocument fontXml = Resources.LoadXmlData(@"Fonts/" + name);
            XmlElement fontNode = fontXml["font"];

            XmlElement commonNode = fontNode["common"];
            LineHeight = Convert.ToInt32(commonNode.Attributes["lineHeight"].Value);
            Base = Convert.ToInt32(commonNode.Attributes["base"].Value);

            XmlElement charsNode = fontNode["chars"];
            CharacterCount = Convert.ToInt32(charsNode.Attributes["count"].Value);

            for (int i = 0; i < CharacterCount; i++)
            {
                XmlNode charNode = charsNode.ChildNodes[i];

                FontCharacter character;

                character.ID = Convert.ToInt32(charNode.Attributes["id"].Value);
                character.X = Convert.ToInt32(charNode.Attributes["x"].Value);
                character.Y = Convert.ToInt32(charNode.Attributes["y"].Value);
                character.Width = Convert.ToInt32(charNode.Attributes["width"].Value);
                character.Height = Convert.ToInt32(charNode.Attributes["height"].Value);
                character.XOffset = Convert.ToInt32(charNode.Attributes["xoffset"].Value);
                character.YOffset = Convert.ToInt32(charNode.Attributes["yoffset"].Value);
                character.XAdvance = Convert.ToInt32(charNode.Attributes["xadvance"].Value);

                Characters.Add(character.ID, character);
            }

            FontTexture = IOSystem.Current.LoadTexture(@"Fonts/" + name);
        }

        public int LineHeight { get; private set; }
        public int Base { get; private set; }

        public int CharacterCount { get; private set; }

        public Texture FontTexture { get; private set; }

        public Dictionary<int, FontCharacter> Characters { get; private set; }
    }

    public struct FontCharacter
    {
        public int ID;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int XOffset;
        public int YOffset;
        public int XAdvance;
    }
}
