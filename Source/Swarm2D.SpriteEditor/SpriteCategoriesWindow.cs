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
    class SpriteCategoriesWindow : EditPanel
    {
        private UILabel _newCategoryButton;
        private UILabel _deleteCategoryButton;
        private UILabel _closeButton;

        private UIListBox _categoriesList;

        private ProjectEditor _projectEditor;

        private SpriteData _spriteData;
        private SpriteDataEditor _spriteDataEditor;

        public override void Initialize(SpriteEditorGUI editorGUI)
        {
            base.Initialize(editorGUI);

            _projectEditor = editorGUI.ProjectEditor;
            _spriteDataEditor = _projectEditor.SpriteDataEditor;
            _spriteData = _projectEditor.SpriteData;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            Entity categoriesListEntity = CreateChildEntity("categoriesListEntity");
            _categoriesList = categoriesListEntity.AddComponent<UIListBox>();
            _categoriesList.Initialize(FastGUI.GenerateStandardParameters(Widget, 10, 10, 300, 400));

            _newCategoryButton = FastGUI.CreateLabel(Widget, "New Category", 320, 10, 300, 40);
            _deleteCategoryButton = FastGUI.CreateLabel(Widget, "Delete Category", 320, 60, 300, 40);
            _closeButton = FastGUI.CreateLabel(Widget, "Close", 320, 110, 300, 40);

            _newCategoryButton.Text = "New Category";
            _deleteCategoryButton.Text = "Delete Category";
            _closeButton.Text = "Close";

            _newCategoryButton.MouseClick += new UIMouseEvent(OnNewCategoryButtonClick);
            _deleteCategoryButton.MouseClick += new UIMouseEvent(OnDeleteCategoryButtonClick);
            _closeButton.MouseClick += new UIMouseEvent(OnCloseButtonClick);

            RefreshCategoriesList();
        }

        private void RefreshCategoriesList()
        {
            _categoriesList.ClearItems();

            foreach (SpriteCategory spriteCategory in _spriteData.SpriteCategories.Values)
            {
                _categoriesList.AddItem(spriteCategory.Name, spriteCategory);
            }

            //for (int i = 0; i < 20; i++)
            //{
            //	_categoriesList.AddItem("oww" + i, null);
            //}
        }

        public override void OnClose()
        {
            base.OnClose();

            Entity.Destroy();
        }

        private void OnNewCategoryButtonClick(UIWidget sender, MouseEventArgs e)
        {

        }

        private void OnDeleteCategoryButtonClick(UIWidget sender, MouseEventArgs e)
        {
            SpriteCategory selectedCategory = _categoriesList.GetSelectedObject() as SpriteCategory;
            _spriteDataEditor.DeleteSpriteCategory(selectedCategory);

            RefreshCategoriesList();
        }

        private void OnCloseButtonClick(UIWidget sender, MouseEventArgs e)
        {
            //_editorGUI.CloseSpriteCategoriesWindow();
        }
    }
}
