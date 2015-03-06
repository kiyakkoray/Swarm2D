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
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View;

namespace Swarm2D.WindowsFramework
{
    public class Resources
    {
        public static string ResourcesPath
        {
            get
            {
                return MainPath + @"\" + ResourcesName;
            }
        }

        public static string ResourcesName { get; private set; }

        public static string MainPath { get; private set; }

        public static void Initialize(string resourcesPath)
        {
            string fullPath = Path.GetFullPath(resourcesPath);

            Directory.SetCurrentDirectory(fullPath);

            MainPath = Directory.GetParent(fullPath).FullName;

            DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);
            ResourcesName = directoryInfo.Name;
        }

        public static byte[] LoadBinaryData(string name)
        {
            return LoadBinaryData(ResourcesName, name);
        }

        public static byte[] LoadBinaryData(string resourcesName, string name)
        {
            string fileExtension = ExtensionOfFileType(ResourceFileType.Binary);

            byte[] data = File.ReadAllBytes(MainPath + @"\" + resourcesName + @"\" + name + "." + fileExtension);
            return data;
        }

        public static string LoadTextData(string name)
        {
            return LoadTextData(ResourcesName, name);
        }

        public static string LoadTextData(string resourcesName, string name)
        {
            string fileExtension = ExtensionOfFileType(ResourceFileType.Text);

            string text = File.ReadAllText(MainPath + @"\" + resourcesName + @"\" + name + "." + fileExtension);
            return text;
        }

        public static XmlDocument LoadXmlData(string resourcesName, string name)
        {
            string fileExtension = ExtensionOfFileType(ResourceFileType.Xml);

            XmlDocument document = new XmlDocument();
            document.Load(MainPath + @"\" + resourcesName + @"\" + name + "." + fileExtension);

            return document;
        }

        public static XmlDocument LoadXmlData(string name)
        {
            return LoadXmlData(ResourcesName, name);
        }

        public static void SaveBinaryData(string name, byte[] data)
        {
            string fileExtension = ExtensionOfFileType(ResourceFileType.Binary);

            File.WriteAllBytes(MainPath + @"\" + ResourcesName + @"\" + name + "." + fileExtension, data);
        }

        public static void SaveXmlData(string name, XmlDocument xmlDocument)
        {
            string fileExtension = ExtensionOfFileType(ResourceFileType.Xml);

            xmlDocument.Save(MainPath + @"\" + ResourcesName + @"\" + name + "." + fileExtension);
        }

        private static string ExtensionOfFileType(ResourceFileType fileType)
        {
            string result = "";

            switch (fileType)
            {
                case ResourceFileType.Xml:
                    result = "xml";
                    break;
                case ResourceFileType.Binary:
                    result = "bytes";
                    break;
                case ResourceFileType.Text:
                    result = "txt";
                    break;
                default:
                    break;
            }

            return result;
        }

        public static string[] GetFilesInFolder(String path, ResourceFileType fileType)
        {
            string fullPath = Path.GetFullPath(ResourcesPath + @"\" + path);

            string fileExtension = ExtensionOfFileType(fileType);

            string[] files = Directory.EnumerateFiles(ResourcesPath + @"\" + path, "*." + fileExtension, System.IO.SearchOption.AllDirectories).ToArray();

            for (int i = 0; i < files.Length; i++)
            {
                string filePath = files[i];

                string sceneFullName = filePath.Substring(fullPath.Length, filePath.Length - fullPath.Length - 1 - fileExtension.Length);
                files[i] = sceneFullName;
            }

            return files;
        }

        public static Texture LoadTexture(string name)
        {
            return LoadTexture(ResourcesName, name);
        }

        public static string GetResourcesNameOf(string fileName)
        {
            string path = Path.GetFullPath(ResourcesPath + @"\" + fileName);

            path = path.Substring(MainPath.Length + 1);

            path = path.Split(new char[] { '\\' })[0];

            return path;
        }

        public static Texture LoadTexture(string resourcesName, string name)
        {
            return GraphicsContext.Active.LoadTexture(resourcesName, name);
        }
    }
}
