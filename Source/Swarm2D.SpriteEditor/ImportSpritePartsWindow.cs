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
using Swarm2D.Engine.View;
using Swarm2D.Engine.View.GUI;
using Swarm2D.Library;
using System.IO;

namespace Swarm2D.SpriteEditor
{
    class ImportSpritePartsWindow : EditPanel
    {
        private UIListBox _spriteList;
        private UITextureBox _textureBox;
        private UILabel _addSelectedButton;

        private UILabel _closeButton;

        private ProjectEditor _projectEditor;

        private Texture _currentSpritePartTexture;

        private SpriteDataEditor _spriteDataEditor;

        public override void Initialize(SpriteEditorGUI editorGUI)
        {
            base.Initialize(editorGUI);

            _projectEditor = editorGUI.ProjectEditor;
            _spriteDataEditor = _projectEditor.SpriteDataEditor;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            _currentSpritePartTexture = null;

            Entity spritesListEntity = CreateChildEntity("spriteListEntity");
            _spriteList = spritesListEntity.AddComponent<UIListBox>();
            _spriteList.Initialize(FastGUI.GenerateStandardParameters(Widget, 10, 10, 500, 350));

            Entity textureBoxEntity = CreateChildEntity("textureBoxEntity");
            _textureBox = textureBoxEntity.AddComponent<UITextureBox>();
            _textureBox.Initialize(FastGUI.GenerateStandardParameters(Widget, 520, 10, 500, 350));

            _spriteList.ItemSelect += new ListBoxEvent(OnSpriteListItemSelect);

            {
                _addSelectedButton = FastGUI.CreateLabel(Widget, "AddSelectedButton", 780, 620, 300, 30);
                _addSelectedButton.Text = "Add Selected Sprite";

                _addSelectedButton.MouseClick += new UIMouseEvent(OnAddSelectedButtonClick);
            }

            {
                _closeButton = FastGUI.CreateLabel(Widget, "Close", 780, 670, 300, 30);
                _closeButton.Text = "Close";

                _closeButton.MouseClick += new UIMouseEvent(OnCloseButtonClick);
            }

            RefreshSpriteList();
        }

        private void OnCloseButtonClick(UIWidget sender, MouseEventArgs e)
        {
            //_editorGUI.CloseImportSpritesWindow();
        }

        private void OnSpriteListItemSelect(UIListBox sender, MouseEventArgs e)
        {
            int selectedIndex = sender.GetSelectedIndex();

            if (_currentSpritePartTexture != null)
            {
                _currentSpritePartTexture.Delete();
                _currentSpritePartTexture = null;
            }
            _currentSpritePartTexture = _spriteDataEditor.GetSpritePartTexture(sender.ListBoxItemNames[selectedIndex]);

            _textureBox.Texture = _currentSpritePartTexture;
        }

        private void OnAddSelectedButtonClick(UIWidget sender, MouseEventArgs e)
        {
            int selectedIndex = _spriteList.GetSelectedIndex();
            string spritePartName = _spriteList.ListBoxItemNames[selectedIndex];

            _spriteDataEditor.AddSpritePart(spritePartName);
            _spriteDataEditor.GenerateSpriteFromSpritePart(spritePartName);

            RefreshSpriteList();
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

        private void RefreshSpriteList()
        {
            string[] spriteBmpPartsList = GetFiles("SpriteParts", "bmp");
            string[] spritePngPartsList = GetFiles("SpriteParts", "png");

            List<string> spritePartsList = new List<string>();
            spritePartsList.AddRange(spriteBmpPartsList);
            spritePartsList.AddRange(spritePngPartsList);

            List<string> notImportedSpritePartslist = new List<string>();

            _spriteList.ClearItems();

            for (int i = 0; i < spritePartsList.Count; i++)
            {
                string spriteName = spritePartsList[i];

                if (!_spriteDataEditor.IsSpritePartImported(spritePartsList[i]))
                {
                    notImportedSpritePartslist.Add(spriteName);
                    _spriteList.AddItem(spriteName, spriteName);
                }
            }
        }
    }
}
