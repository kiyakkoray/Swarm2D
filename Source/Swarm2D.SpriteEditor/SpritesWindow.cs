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
using Swarm2D.Engine.View;
using Swarm2D.Engine.View.GUI;

using Swarm2D.Engine.Logic;

namespace Swarm2D.SpriteEditor
{
    class SpritesWindow : EditPanel
    {
        private UIListBox _spritesList;
        private UITextureBox _textureBox;

        private UILabel _closeButton;
        private UILabel _removeSpriteButton;

        private ProjectEditor _projectEditor;

        private SpriteData _spriteData;

        public override void Initialize(SpriteEditorGUI editorGUI)
        {
            base.Initialize(editorGUI);

            _projectEditor = editorGUI.ProjectEditor;
            _spriteData = _projectEditor.SpriteData;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            {
                Entity spritesListEntity = CreateChildEntity("spritesListEntity");
                _spritesList = spritesListEntity.AddComponent<UIListBox>();
                _spritesList.Initialize(FastGUI.GenerateStandardParameters(Widget, 10, 10, 500, 350));

                Entity textureBoxEntity = CreateChildEntity("textureBoxEntity");
                _textureBox = textureBoxEntity.AddComponent<UITextureBox>();
                _textureBox.Initialize(FastGUI.GenerateStandardParameters(Widget, 520, 10, 500, 350));

                _spritesList.ItemSelect += new ListBoxEvent(OnSpritesListItemSelect);
            }

            {
                _closeButton = FastGUI.CreateLabel(Widget, "Close", 780, 670, 300, 30);
                _closeButton.Text = "Close";

                _closeButton.MouseClick += new UIMouseEvent(OnCloseButtonClick);
            }

            {
                _removeSpriteButton = FastGUI.CreateLabel(Widget, "RemoveSpriteButton", 270, 500, 240, 30);
                _removeSpriteButton.Text = "Remove Sprite";

                _removeSpriteButton.MouseClick += new UIMouseEvent(OnRemoveSpriteButton);
            }

            RefreshSpritesList();
        }

        private void OnCloseButtonClick(UIWidget sender, MouseEventArgs e)
        {
            //_editorGUI.CloseSpritesWindow();
        }

        private void OnSpritesListItemSelect(UIListBox sender, MouseEventArgs e)
        {
            Sprite sprite = (Sprite)sender.GetSelectedObject();
        }

        private void OnRemoveSpriteButton(UIWidget sender, MouseEventArgs e)
        {
            Sprite sprite = (Sprite)_spritesList.GetSelectedObject();

            if (sprite != null)
            {
                //_projectEditor.Project.SpriteName

                _spriteData.SpriteNames.Remove(sprite.Name);

                RefreshSpritesList();
            }
        }

        private void RefreshSpritesList()
        {
            _spritesList.ClearItems();

            foreach (Sprite sprite in _spriteData.SpriteNames.Values)
            {
                _spritesList.AddItem(sprite.Name, sprite);
            }
        }
    }
}
