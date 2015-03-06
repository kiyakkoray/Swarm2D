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
using System.IO;
using System.Xml;

namespace Swarm2D.Engine.Core
{
    public class Resources
    {
        public static string ResourcesName
        {
            get
            {
                return Framework.Current.ResourcesName;
            }
        }

        public static string ResourcesPath
        {
            get
            {
                return Framework.Current.ResourcesPath;
            }
        }

        public static byte[] LoadBinaryData(string name)
        {
            return Framework.Current.LoadBinaryData(name);
        }

        public static byte[] LoadBinaryData(string resourcesName, string name)
        {
            return Framework.Current.LoadBinaryData(resourcesName, name);
        }

        public static string LoadTextData(string name)
        {
            return Framework.Current.LoadTextData(name);
        }

        public static string LoadTextData(string resourcesName, string name)
        {
            return Framework.Current.LoadTextData(resourcesName, name);
        }

        public static XmlDocument LoadXmlData(string resourcesName, string name)
        {
            return Framework.Current.LoadXmlData(resourcesName, name);
        }

        public static XmlDocument LoadXmlData(string name)
        {
            return Framework.Current.LoadXmlData(name);
        }

        public static void SaveBinaryData(string name, byte[] data)
        {
            Framework.Current.SaveBinaryData(name, data);
        }

        public static void SaveXmlData(string name, XmlDocument xmlDocument)
        {
            Framework.Current.SaveXmlData(name, xmlDocument);
        }

        public static string[] GetFilesInFolder(String path, ResourceFileType fileType)
        {
            return Framework.Current.GetFilesInFolder(path, fileType);
        }

        //public static Texture LoadTexture(string name)
        //{
        //	return Graphics.LoadTexture(name);
        //}

        public static string GetResourcesNameOf(string fileName)
        {
            return Framework.Current.GetResourcesNameOf(fileName);
        }

        //public static Texture LoadTexture(string resourcesName, string name)
        //{
        //	return Graphics.LoadTexture(resourcesName, name);
        //}
    }
}
