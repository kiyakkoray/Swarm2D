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
using System.Windows.Forms;
using System.Xml;
using Swarm2D.Engine.View;
using Swarm2D.Engine.View.GUI;
using Swarm2D.Engine.View.GUI.PositionParameters;
using Swarm2D.Library;
using Swarm2D.Engine.Logic;

namespace Swarm2D.SceneEditor.GUIControllers
{
    public class EditorGUI : UIController
    {
        public SceneEditorDomain EditorDomain { get; private set; }
        public GameLogic GameLogic { get; private set; }
        public GameRenderer GameRenderer { get; private set; }

        private EditPanel _editPanel;

        private UIFrame _editorFrame;

        public UIFrame EditPanelFrame { get; private set; }

        private TopMenu _topMenu;

        private SceneEditor _sceneEditor;

        protected override void OnAdded()
        {
            base.OnAdded();

            EditorDomain = Engine.RootEntity.GetComponent<SceneEditorDomain>();

            GameLogic = EditorDomain.GameLogic;
            GameRenderer = EditorDomain.GameRenderer;

            _editorFrame = GetComponent<UIFrame>();
            EditPanelFrame = _editorFrame.GetChildAtPath("EditPanel");

            UIFrame topMenuFrame = ControllerFrame.GetChildAtPath("TopMenu");
            _topMenu = topMenuFrame.AddComponent<TopMenu>();

            GameRenderer.RenderEnabled = false;
        }

        public void UpdateLoop()
        {
            _topMenu.Update();

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

        public void NewScene()
        {
            _sceneEditor = SceneEditor.NewScene(this);
            OpenEditPanel(_sceneEditor);
        }

        public void OpenScene()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Scene files (*.xml)|*.xml";

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                XmlDocument sceneData = new XmlDocument();

                try
                {
                    sceneData.Load(openFileDialog.FileName);
                }
                catch
                {
                    sceneData = null;
                }

                if (sceneData != null)
                {
                    _sceneEditor = SceneEditor.OpenScene(this, sceneData);

                    OpenEditPanel(_sceneEditor);
                }
            }
        }

        public void SaveScene()
        {
            if (_sceneEditor != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Scene files (*.xml)|*.xml";

                DialogResult result = saveFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    SceneData sceneData = _sceneEditor.Scene.Save();
                    XmlDocument sceneDataAsXml = sceneData.SaveToXML();
                    sceneDataAsXml.Save(saveFileDialog.FileName);
                }
            }
        }
    }
}
