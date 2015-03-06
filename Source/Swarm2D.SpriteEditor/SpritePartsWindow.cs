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
using Swarm2D.Library;
using Swarm2D.Engine.Logic;

namespace Swarm2D.SpriteEditor
{
    class SpritePartsWindow : EditPanel
    {
        private UIListBox _spritePartsList;
        private UITextureBox _textureBox;

        private UILabel _spritePartsCategoriesLabel;
        private UIListBox _spritePartsCategoriesList;

        private UILabel _addSpriteCategoryButton;
        private UILabel _removeSpriteCategoryButton;
        private UILabel _removeSpriteButton;

        private UILabel _categoriesListLabel;
        private UIListBox _categoriesList;

        private UILabel _showSelectedCategoryButton;
        private UILabel _showAllCategoriesButton;
        private UILabel _closeButton;

        private ProjectEditor _projectEditor;

        private Texture _currentSpritePartTexture;

        private bool _showAllCategories;
        private SpriteCategory _selectedCategoryToShow;

        private SpriteData _spriteData;
        private SpriteDataEditor _spriteDataEditor;

        public override void Initialize(SpriteEditorGUI editorGUI)
        {
            base.Initialize(editorGUI);

            _projectEditor = editorGUI.ProjectEditor;

            _spriteData = _projectEditor.SpriteDataEditor.SpriteData;
            _spriteDataEditor = _projectEditor.SpriteDataEditor;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            _currentSpritePartTexture = null;
            _showAllCategories = true;
            _selectedCategoryToShow = null;

            {
                Entity spritePartsListEntity = CreateChildEntity("spritePartsList");
                _spritePartsList = spritePartsListEntity.AddComponent<UIListBox>();
                _spritePartsList.Initialize(FastGUI.GenerateStandardParameters(Widget, 10, 10, 500, 350));

                Entity textureBoxEntity = CreateChildEntity("textureBoxEntity");
                _textureBox = textureBoxEntity.AddComponent<UITextureBox>();
                _textureBox.Initialize(FastGUI.GenerateStandardParameters(Widget, 520, 10, 500, 350));

                _spritePartsList.ItemSelect += new ListBoxEvent(OnSpritePartsListItemSelect);

                RefreshSpritePartsList();
            }

            {
                _spritePartsCategoriesLabel = FastGUI.CreateLabel(Widget, "SpritePartCategoriesLabel", 10, 380, 250, 30);
                _spritePartsCategoriesLabel.Text = "Sprite Parts Categories:";

                Entity spritePartsCategoriesListEntity = CreateChildEntity("spritePartsCategoriesListEntity");
                _spritePartsCategoriesList = spritePartsCategoriesListEntity.AddComponent<UIListBox>();
                _spritePartsCategoriesList.Initialize(FastGUI.GenerateStandardParameters(Widget, 10, 420, 250, 290));
            }

            {
                _addSpriteCategoryButton = FastGUI.CreateLabel(Widget, "AddSpriteCategoryButton", 270, 420, 240, 30);
                _addSpriteCategoryButton.Text = "Add Sprite Category";

                _removeSpriteCategoryButton = FastGUI.CreateLabel(Widget, "RemoveSpriteCategoryButton", 270, 460, 240, 30);
                _removeSpriteCategoryButton.Text = "Remove Sprite Category";

                _removeSpriteButton = FastGUI.CreateLabel(Widget, "RemoveSpriteButton", 270, 500, 240, 30);
                _removeSpriteButton.Text = "Remove Sprite";

                _addSpriteCategoryButton.MouseClick += new UIMouseEvent(OnAddSpriteCategoryButtonClick);
                _removeSpriteCategoryButton.MouseClick += new UIMouseEvent(OnRemoveSpriteCategoryButtonClick);
                _removeSpriteButton.MouseClick += new UIMouseEvent(OnRemoveSpriteButton);
            }

            {
                _categoriesListLabel = FastGUI.CreateLabel(Widget, "CategoriesListLabel", 520, 380, 250, 30);
                _categoriesListLabel.Text = "Sprite Categories:";

                Entity categoriesListEntity = CreateChildEntity("categoriesListEntity");
                _categoriesList = categoriesListEntity.AddComponent<UIListBox>();
                _categoriesList.Initialize(FastGUI.GenerateStandardParameters(Widget, 520, 420, 250, 290));

                Dictionary<string, SpriteCategory> spriteCategories = _spriteData.SpriteCategories;

                foreach (SpriteCategory spriteCategory in spriteCategories.Values)
                {
                    _categoriesList.AddItem(spriteCategory.Name, spriteCategory);
                }

            }

            {
                _showSelectedCategoryButton = FastGUI.CreateLabel(Widget, "ShowSelectedCategoryButton", 780, 420, 300, 30);
                _showSelectedCategoryButton.Text = "Show Selected Category";

                _showAllCategoriesButton = FastGUI.CreateLabel(Widget, "ShowAllCategoriesButton", 780, 460, 300, 30);
                _showAllCategoriesButton.Text = "Show All Categories";

                _closeButton = FastGUI.CreateLabel(Widget, "Close", 780, 670, 300, 30);
                _closeButton.Text = "Close";

                _showSelectedCategoryButton.MouseClick += new UIMouseEvent(OnShowSelectedCategoryButtonClick);
                _showAllCategoriesButton.MouseClick += new UIMouseEvent(OnShowAllCategoriesButtonClick);
                _closeButton.MouseClick += new UIMouseEvent(OnCloseButtonClick);
            }
        }

        private void OnCloseButtonClick(UIWidget sender, MouseEventArgs e)
        {
            //_editorGUI.CloseSpritePartsWindow();
        }

        private void OnSpritePartsListItemSelect(UIListBox sender, MouseEventArgs e)
        {
            SpritePart spritePart = (SpritePart)sender.GetSelectedObject();

            if (_currentSpritePartTexture != null)
            {
                _currentSpritePartTexture.Delete();
                _currentSpritePartTexture = null;
            }
            _currentSpritePartTexture = _spriteDataEditor.GetSpritePartTexture(spritePart.Name);

            _textureBox.Texture = _currentSpritePartTexture;

            _spritePartsCategoriesList.ClearItems();

            foreach (SpriteCategory spriteCategory in spritePart.Categories)
            {
                _spritePartsCategoriesList.AddItem(spriteCategory.Name, spriteCategory);
            }
        }

        private void OnAddSpriteCategoryButtonClick(UIWidget sender, MouseEventArgs e)
        {
            SpriteCategory spriteCategory = (SpriteCategory)_categoriesList.GetSelectedObject();

            if (spriteCategory != null)
            {
                SpritePart spritePart = (SpritePart)_spritePartsList.GetSelectedObject();

                if (spritePart != null)
                {
                    if (!spritePart.Categories.Contains(spriteCategory))
                    {
                        spritePart.Categories.Add(spriteCategory);
                        spriteCategory.SpriteParts.Add(spritePart);
                        _spritePartsCategoriesList.AddItem(spriteCategory.Name, spriteCategory);
                    }
                }
            }
        }

        private void OnRemoveSpriteCategoryButtonClick(UIWidget sender, MouseEventArgs e)
        {
            SpritePart spritePart = (SpritePart)_spritePartsList.GetSelectedObject();

            if (spritePart != null)
            {
                SpriteCategory spriteCategory = (SpriteCategory)_spritePartsCategoriesList.GetSelectedObject();

                if (spriteCategory != null)
                {
                    spritePart.Categories.Remove(spriteCategory);
                    spriteCategory.SpriteParts.Remove(spritePart);

                    _spritePartsCategoriesList.ClearItems();

                    foreach (SpriteCategory otherSpriteCategory in spritePart.Categories)
                    {
                        _spritePartsCategoriesList.AddItem(otherSpriteCategory.Name, otherSpriteCategory);
                    }
                }
            }
        }

        private void OnRemoveSpriteButton(UIWidget sender, MouseEventArgs e)
        {
            SpritePart spritePart = (SpritePart)_spritePartsList.GetSelectedObject();

            if (spritePart != null)
            {
                _spriteData.SpritePartNames.Remove(spritePart.Name);

                foreach (SpriteCategory spriteCategory in spritePart.Categories)
                {
                    spriteCategory.SpriteParts.Remove(spritePart);
                }

                _textureBox.Texture = null;

                RefreshSpritePartsList();

                _spritePartsCategoriesList.ClearItems();
            }

        }

        private void OnShowAllCategoriesButtonClick(UIWidget sender, MouseEventArgs e)
        {
            _selectedCategoryToShow = null;
            _showAllCategories = true;
            RefreshSpritePartsList();
        }

        private void OnShowSelectedCategoryButtonClick(UIWidget sender, MouseEventArgs e)
        {
            SpriteCategory spriteCategory = (SpriteCategory)_categoriesList.GetSelectedObject();

            if (spriteCategory != null)
            {
                _selectedCategoryToShow = spriteCategory;
                _showAllCategories = false;
                RefreshSpritePartsList();
            }
        }

        private void RefreshSpritePartsList()
        {
            _spritePartsList.ClearItems();

            if (_showAllCategories)
            {
                foreach (SpritePart spritePart in _spriteData.SpritePartNames.Values)
                {
                    _spritePartsList.AddItem(spritePart.Name, spritePart);
                }
            }
            else
            {
                foreach (SpritePart spritePart in _selectedCategoryToShow.SpriteParts)
                {
                    _spritePartsList.AddItem(spritePart.Name, spritePart);
                }
            }
        }
    }
}
