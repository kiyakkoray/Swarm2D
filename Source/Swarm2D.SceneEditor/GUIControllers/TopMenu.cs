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
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.Remoting.Contexts;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View.GUI;

namespace Swarm2D.SceneEditor.GUIControllers
{
    class TopMenu : UIController
    {
        private EditorGUI _editorGUI;

        private UIFrame _topMenu;

        private Dictionary<string, UIFrame> _topMenuButtons;

        protected override void OnAdded()
        {
            base.OnAdded();

            _editorGUI = Entity.Parent.GetComponent<EditorGUI>();
            _topMenuButtons = new Dictionary<string, UIFrame>();

            _topMenu = GetComponent<UIFrame>();

            AddTopMenuButton("File");
            AddTopMenuButton("Debug");

            ContextMenuItem newSceneButton = AddTopMenuItem("File", "New");
            ContextMenuItem openSceneButton = AddTopMenuItem("File", "Open");
            ContextMenuItem saveSceneButton = AddTopMenuItem("File", "Save");

            newSceneButton.Click += new ContextMenuItemEvent(OnNewSceneButtonClick);
            openSceneButton.Click += new ContextMenuItemEvent(OnOpenSceneButtonClick);
            saveSceneButton.Click += new ContextMenuItemEvent(OnSaveSceneButtonClick);

            ContextMenuItem treeViewButton = AddTopMenuItem("Debug", "TreeView");

            treeViewButton.Click += new ContextMenuItemEvent(OnTreeViewButtonClick);
        }

        public void Update()
        {

        }

        public UIFrame AddTopMenuButton(string text)
        {
            UIButton topMenuButton = FastGUI.CreateButton(_topMenu.Widget, text, 5.0f + _topMenuButtons.Count * 105.0f, 5.0f, 100.0f, 30.0f);
            topMenuButton.Text = text;
            topMenuButton.Name = text;

            _topMenuButtons.Add(text, topMenuButton);

            {
                List<UIPositionParameter> menuParameters = new List<UIPositionParameter>();

                menuParameters.Add(UIPositionParameter.AnchorToSideParameter(topMenuButton.Widget, AnchorSide.Bottom, AnchorToSideType.Outer, 0));
                menuParameters.Add(UIPositionParameter.AnchorToSideParameter(topMenuButton.Widget, AnchorSide.Left, AnchorToSideType.Inner, 0));

                UIContextMenu menu = ControllerFrame.Manager.CreateContextMenu(menuParameters);
                menu.Name = text;
                //menu->AddItem(SxBase::String(L"hebele"));
                //menu->AddItem(SxBase::String(L"oaa"));
                //menu->AddItem(SxBase::String(L"oaoaoa"));

                menu.Enabled = false;
            }

            topMenuButton.MouseClick += new UIMouseEvent(OnTopMenuButtonClick);

            return topMenuButton;
        }

        public ContextMenuItem AddTopMenuItem(string buttonName, string text)
        {
            UIContextMenu contextMenu = (UIContextMenu)ControllerFrame.Manager.RootObject.GetChildAtPath(buttonName);
            return contextMenu.AddItem(text);
        }

        private void OnTopMenuButtonClick(UIWidget sender, MouseEventArgs e)
        {
            UIFrame contextMenu = ControllerFrame.Manager.RootObject.GetChildAtPath(sender.Name);

            contextMenu.Enabled = !contextMenu.Enabled;
            contextMenu.BringToFront();
        }

        private void OnNewSceneButtonClick(ContextMenuItem item)
        {
            _editorGUI.NewScene();
        }

        private void OnOpenSceneButtonClick(ContextMenuItem item)
        {
            _editorGUI.OpenScene();
        }

        private void OnSaveSceneButtonClick(ContextMenuItem item)
        {
            _editorGUI.SaveScene();
        }

        private void OnTreeViewButtonClick(ContextMenuItem item)
        {
            UIFrame root = ControllerFrame.Manager.RootObject;

            Entity treeViewEntity = root.CreateChildEntity("treeView");
            UITreeView treeView = treeViewEntity.AddComponent<UITreeView>();
            treeView.Initialize(FastGUI.GenerateStandardParameters(root.Widget, 200, 200, 200, 400));

            TreeViewNode node1 = treeView.AddNode("node1", null);
            TreeViewNode node2 = treeView.AddNode("node2", null);
            TreeViewNode node3 = treeView.AddNode("node3", null);

            TreeViewNode node2child1 = treeView.AddNode("node2child1", node2);

            TreeViewNode node3child1 = treeView.AddNode("node3child1", node3);
            TreeViewNode node3child2 = treeView.AddNode("node3child2", node3);

            treeView.AddNode("grandChild", node3child2);
        }
    }
}
