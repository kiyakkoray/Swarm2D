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
using Swarm2D.Engine.View.GUI;
using Swarm2D.Engine.View.GUI.PositionParameters;
using Swarm2D.Library;
using Swarm2D.Engine.Logic;

namespace Swarm2D.SpriteEditor
{
    public class SpriteEditorGUI : UIController
    {
        public SpriteEditorDomain EditorDomain { get; private set; }

        private ProjectEditor _projectEditor;

        private EditPanel _editPanel;

        private UIFrame _editorFrame;

        public UIFrame EditPanelFrame { get; private set; }

        public ProjectEditor ProjectEditor { get { return _projectEditor; } }

        public SpriteEditorGUI(SpriteEditorDomain editorApplicationDomain)
        {
            EditorDomain = editorApplicationDomain;
            _projectEditor = EditorDomain.ProjectEditor;

            _editorFrame = FastGUI.CreateFromXmlFile(@"..\Editor\spriteEditorFrame.xml", editorApplicationDomain.UIRootObject.Widget).GetComponent<UIFrame>();
            EditPanelFrame = _editorFrame.GetChildAtPath("EditPanel");

            UIFrame leftMenu = _editorFrame.GetChildAtPath("LeftMenu");

            UIFrame categoriesButton = leftMenu.GetChildAtPath("CategoriesButton");
            UIFrame spritePartsButton = leftMenu.GetChildAtPath("PartsButton");
            UIFrame spritesButton = leftMenu.GetChildAtPath("SpritesButton");
            UIFrame importSpritePartsButton = leftMenu.GetChildAtPath("ImportSpritePartsButton");
            UIFrame generateSpriteSheetsButton = leftMenu.GetChildAtPath("GenerateSpriteSheetsButton");

            categoriesButton.MouseClick += new UIMouseEvent(OnCategoriesButtonClick);
            spritePartsButton.MouseClick += new UIMouseEvent(OnSpritePartsButtonClick);
            spritesButton.MouseClick += new UIMouseEvent(OnSpritesButtonClick);
            importSpritePartsButton.MouseClick += new UIMouseEvent(OnImportSpritePartsButtonClick);
            generateSpriteSheetsButton.MouseClick += new UIMouseEvent(OnGenerateSpriteSheetsButtonClick);

            //_topPanel = new TopPanel(this);
            //_topMenu = new TopMenu(this);

            //{
            //	UIMouseEvent eventHandler(this, (UIMouseEvent.MemberPointer)&EditorGUI.OnCloseObjectPropertiesButtonClick);
            //	_closeObjectPropertiesPanel.MouseClick.AddHandler(eventHandler);
            //}
        }

        public void UpdateLoop()
        {
            //_topMenu.Update();

            if (_editPanel != null)
            {
                _editPanel.Update();
            }
        }

        public void OpenEditPanel(EditPanel editPanel)
        {
            if (_editPanel != null)
            {
                _editPanel.OnClose();
                _editPanel = null;
            }

            _editPanel = editPanel;
            _editPanel.OnOpen();
        }

        private void OnCategoriesButtonClick(UIWidget sender, MouseEventArgs e)
        {
            Entity editPanelEntity = EditPanelFrame.CreateChildEntity("EditPanel");
            EditPanel editPanel = editPanelEntity.AddComponent<SpriteCategoriesWindow>();
            editPanel.Initialize(this);

            OpenEditPanel(editPanel);
        }

        private void OnSpritePartsButtonClick(UIWidget sender, MouseEventArgs e)
        {
            Entity editPanelEntity = EditPanelFrame.CreateChildEntity("EditPanel");
            EditPanel editPanel = editPanelEntity.AddComponent<SpritePartsWindow>();
            editPanel.Initialize(this);

            OpenEditPanel(editPanel);
        }

        private void OnSpritesButtonClick(UIWidget sender, MouseEventArgs e)
        {
            Entity editPanelEntity = EditPanelFrame.CreateChildEntity("EditPanel");
            EditPanel editPanel = editPanelEntity.AddComponent<SpritesWindow>();
            editPanel.Initialize(this);

            OpenEditPanel(editPanel);
        }

        private void OnImportSpritePartsButtonClick(UIWidget sender, MouseEventArgs e)
        {
            Entity editPanelEntity = EditPanelFrame.CreateChildEntity("EditPanel");
            EditPanel editPanel = editPanelEntity.AddComponent<ImportSpritePartsWindow>();
            editPanel.Initialize(this);

            OpenEditPanel(editPanel);
        }

        private void OnGenerateSpriteSheetsButtonClick(UIWidget sender, MouseEventArgs e)
        {
            _projectEditor.SpriteDataEditor.GenerateSpriteSheets();
        }
    }
}
