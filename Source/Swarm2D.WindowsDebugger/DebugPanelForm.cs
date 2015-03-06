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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Swarm2D.Engine.Core;

namespace Swarm2D.WindowsDebugger
{
	public partial class DebugPanelForm : Form
	{
		private DebugPanel _debugPanel;

		public DebugPanelForm(DebugPanel debugPanel)
		{
			_debugPanel = debugPanel;
			InitializeComponent();
		}

		private void OnResetButtonClick(object sender, EventArgs e)
		{
			_treeViewEntities.Nodes.Clear();

			Entity rootEntity = _debugPanel.Engine.RootEntity;

			TreeNode treeNode = new TreeNode(rootEntity.Name);
			treeNode.Tag = rootEntity;

			_treeViewEntities.Nodes.Add(treeNode);
		}

		private void OnEntitiesTreeViewNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			TreeNode treeNode = e.Node;

            treeNode.Nodes.Clear();

			Entity entity = treeNode.Tag as Entity;

			foreach (Entity child in entity.Children)
			{
				TreeNode childNode = new TreeNode(child.Name);
				childNode.Tag = child;

				treeNode.Nodes.Add(childNode);
			}

            OnSelectEntity(entity);
		}

        private void OnEntitiesTreeViewNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode treeNode = e.Node;

            Entity entity = treeNode.Tag as Entity;

            OnSelectEntity(entity);
        }

	    private void OnSelectEntity(Entity entity)
	    {
            _labelSelectedEntity.Text = entity.Name;

            _listBoxComponents.Items.Clear();

            foreach (var component in entity.Components)
            {
                ComponentInfo componentInfo = ComponentInfo.GetComponentInfo(component.GetType());
                _listBoxComponents.Items.Add(componentInfo.Name);
            }
	    }
	}
}
