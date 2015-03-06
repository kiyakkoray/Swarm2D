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
using System.Reflection.Emit;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View.GUI.PositionParameters;
using Swarm2D.Library;

namespace Swarm2D.Engine.View.GUI
{
    public class UITreeView : UIScrollViewer
    {
        public event TreeViewEvent ItemSelect;
        public event TreeViewEvent ItemMouseRightClick;

        public TreeViewNode RootNode;

        internal const int ItemHeight = 20;

        private float _totalHeight = 0.0f;

        private TreeViewNode _selectedNode;

        public override void Initialize(List<UIPositionParameter> positionParameters)
        {
            base.Initialize(positionParameters);

            RootNode = new TreeViewNode();
            RootNode.ChildrenFrame = HolderFrame;
            RootNode.IsOpen = true;
            RootNode.TreeView = this;
            RootNode.RootNode = RootNode;
        }

        public TreeViewNode AddNode(Object data, TreeViewNode parent = null)
        {
            if (parent == null)
            {
                parent = RootNode;
            }

            TreeViewNode node = parent.AddChildNode(data);

            RepositionItems();
            SetNonUpdatedWithChildrenOnAllDomains();

            return node;
        }

        private void RepositionItems()
        {
            _totalHeight = RootNode.RepositionItems();
        }

        public TreeViewNode SelectedNode
        {
            get
            {
                return _selectedNode;
            }
            set
            {
                if (_selectedNode != null)
                {
                    _selectedNode.Label.ShowBox = false;
                }

                if (RootNode != value && RootNode.ContainsRecursively(value))
                {
                    _selectedNode = value;
                    _selectedNode.Label.ShowBox = true;
                }
                else
                {
                    _selectedNode = null;
                }
            }
        }

        public object SelectedNodeData
        {
            get
            {
                return SelectedNode != null ? SelectedNode.Data : null;
            }
        }

        public void ClearItems()
        {
            _selectedNode = null;
            RootNode.Clear();

            RepositionItems();
        }

        protected override int CalculateScrollViewerHeight()
        {
            return (int)_totalHeight;
        }

        internal void OnItemButtonClick(UIWidget sender, MouseEventArgs e)
        {
            TreeViewNode node = sender.DataObject as TreeViewNode;

            node.IsOpen = !node.IsOpen;

            RepositionItems();
            SetNonUpdatedWithChildrenOnAllDomains();
        }

        internal void OnItemClick(UIWidget sender, MouseEventArgs e)
        {
            SelectedNode = sender.DataObject as TreeViewNode;

            if (ItemSelect != null)
            {
                ItemSelect(this, e);
            }
        }

        internal void OnItemRightClick(UIWidget sender, MouseEventArgs e)
        {
            if (ItemMouseRightClick != null)
            {
                ItemMouseRightClick(this, e);
            }
        }

        public TreeViewNode FindTreeViewNodeWithData(object data)
        {
            return RootNode.FindTreeViewNodeWithData(data);
        }

        public void SelectNodeWithData(Object data)
        {
            TreeViewNode node = FindTreeViewNodeWithData(data);

            SelectedNode = node;
        }

        public void SynchronizeWithTree<T>(Tree<T> tree)
        {
            if (!RootNode.HoldSameDataWith(tree.Root))
            {
                object oldSelectedData = SelectedNode != null ? SelectedNode.Data : null;

                ClearItems();

                AddTree(tree);

                TreeViewNode foundNode = FindTreeViewNodeWithData(oldSelectedData);

                if (foundNode != null)
                {
                    SelectedNode = foundNode;

                    TreeViewNode parentNode = foundNode.Parent;

                    while (parentNode != null)
                    {
                        parentNode.IsOpen = true;
                        parentNode = parentNode.Parent;
                    }
                }

                RepositionItems();
                SetNonUpdatedWithChildrenOnAllDomains();
            }
        }

        public void AddTree<T>(Tree<T> tree)
        {
            AddTreeNode(RootNode, tree.Root);
        }

        private void AddTreeNode<T>(TreeViewNode treeViewNode, TreeNode<T> treeNode)
        {
            foreach (TreeNode<T> childTreeNode in treeNode.Children)
            {
                TreeViewNode childTreeViewNode = AddNode(childTreeNode.Data, treeViewNode);
                AddTreeNode(childTreeViewNode, childTreeNode);
            }
        }
    }

    public class TreeViewNode
    {
        public List<TreeViewNode> Children { get; private set; }
        public UITreeView TreeView { get; internal set; }

        public string Name { get; set; }
        public object Data { get; set; }

        internal TreeViewNode RootNode { get; set; }
        internal UILabel Label { get; set; }
        internal UIButton Button { get; set; }
        internal UIFrame ChildrenFrame { get; set; }

        internal TreeViewNode Parent { get; set; }

        internal SetHeight HeightParameter { get; set; }
        internal List<AnchorToSide> YParameters { get; private set; }

        internal float Height { get; set; }

        internal bool IsOpen { get; set; }

        internal TreeViewNode()
        {
            YParameters = new List<AnchorToSide>();
            Children = new List<TreeViewNode>();
        }

        public bool ContainsRecursively(TreeViewNode node)
        {
            if (node == this)
            {
                return true;
            }

            foreach (TreeViewNode treeViewNode in Children)
            {
                bool containsInChild = treeViewNode.ContainsRecursively(node);

                if (containsInChild)
                {
                    return true;
                }
            }

            return false;
        }

        public TreeViewNode FindTreeViewNodeWithData(object data)
        {
            if (data == Data)
            {
                return this;
            }

            foreach (TreeViewNode treeViewNode in Children)
            {
                TreeViewNode foundChildNode = treeViewNode.FindTreeViewNodeWithData(data);

                if (foundChildNode != null && foundChildNode.Data == data)
                {
                    return foundChildNode;
                }
            }

            return null;
        }

        internal void CreateUI()
        {
            UIFrame parentFrame = Parent.ChildrenFrame;
            float itemYParameter = 5 + Parent.Children.IndexOf(this) * 22;

            {
                List<UIPositionParameter> itemFrameParameters = FastGUI.GenerateStandardParameters(parentFrame.Widget, 5,
                    itemYParameter, UITreeView.ItemHeight, UITreeView.ItemHeight);

                YParameters.Add(itemFrameParameters[0] as AnchorToSide);

                Entity buttonEntity = parentFrame.CreateChildEntity("buttonEntity");
                Button = buttonEntity.AddComponent<UIButton>();
                Button.Initialize(itemFrameParameters);
                Button.Widget.MouseClick += new UIMouseEvent(TreeView.OnItemButtonClick);
                Button.Enabled = false;
                //node.Button.Text = "+";
            }

            {
                List<UIPositionParameter> itemFrameParameters = FastGUI.GenerateStandardParameters(parentFrame.Widget, 25,
                    itemYParameter, TreeView.Width - 60, UITreeView.ItemHeight);

                Entity labelEntity = parentFrame.CreateChildEntity("labelEntity");
                Label = labelEntity.AddComponent<UILabel>();
                Label.Initialize(itemFrameParameters);
                YParameters.Add(itemFrameParameters[0] as AnchorToSide);

                Label.Widget.MouseClick += new UIMouseEvent(TreeView.OnItemClick);
                Label.Widget.MouseRightClick += new UIMouseEvent(TreeView.OnItemRightClick);
            }

            {
                List<UIPositionParameter> itemFrameParameters = new List<UIPositionParameter>();
                itemFrameParameters.Add(UIPositionParameter.AnchorToSideParameter(Label.Widget, AnchorSide.Bottom, AnchorToSideType.Outer));
                itemFrameParameters.Add(UIPositionParameter.AnchorToSideParameter(Label.Widget, AnchorSide.Left, AnchorToSideType.Inner));

                itemFrameParameters.Add(UIPositionParameter.SetWidth(Label.Width));

                HeightParameter = UIPositionParameter.SetHeight(UITreeView.ItemHeight);
                itemFrameParameters.Add(HeightParameter);

                Entity childrenFrameEntity = parentFrame.CreateChildEntity("childrenFrameEntity");
                ChildrenFrame = childrenFrameEntity.AddComponent<UIFrame>();
                ChildrenFrame.Initialize(itemFrameParameters);
                ChildrenFrame.ShowBox = false;
            }

            RefreshData();
        }

        private void RefreshData()
        {
            Button.DataObject = this;
            Label.DataObject = this;
            Label.Name = Name;
            Label.Text = Name;
        }

        internal void Clear()
        {
            foreach (TreeViewNode treeViewNode in Children)
            {
                treeViewNode.Clear();
            }

            foreach (TreeViewNode treeViewNode in Children)
            {
                treeViewNode.Label.Entity.Destroy();
                treeViewNode.Button.Entity.Destroy();
                treeViewNode.ChildrenFrame.Entity.Destroy();
            }

            Children.Clear();
        }

        public TreeViewNode AddChildNode(Object data)
        {
            return AddChildNode(data, Children.Count);
        }

        public TreeViewNode AddChildNode(Object data, int index)
        {
            TreeViewNode node = new TreeViewNode();

            node.Name = data.ToString();
            node.Data = data;
            node.Parent = this;
            node.TreeView = TreeView;
            node.RootNode = RootNode;

            Children.Insert(index, node);
            node.CreateUI();

            return node;
        }

        internal float RepositionItems()
        {
            float currentHeight = 0;

            RepositionItems(ref currentHeight);

            return currentHeight;
        }

        private void RepositionItems(ref float currentHeight)
        {
            float childrenHeight = 0.0f;

            if (IsOpen)
            {
                for (int index = 0; index < Children.Count; index++)
                {
                    TreeViewNode childNode = Children[index];
                    childrenHeight += 5.0f;

                    foreach (AnchorToSide anchorToSide in childNode.YParameters)
                    {
                        anchorToSide.Value = childrenHeight;
                    }

                    childNode.RepositionItems(ref childrenHeight);

                    if (index + 1 == Children.Count)
                    {
                        childrenHeight += 5.0f;
                    }
                }
            }

            Height = childrenHeight;

            bool hasChildren = Children.Count != 0;

            if (this != RootNode)
            {
                HeightParameter.Value = Height;
                Button.Enabled = hasChildren;
                ChildrenFrame.Enabled = hasChildren && IsOpen;

                //if (hasChildren && !node.IsOpen)
                //{
                //	node.Button.Text = "+";
                //}
                //else if (hasChildren && node.IsOpen)
                //{
                //	node.Button.Text = "-";
                //}
            }

            currentHeight += childrenHeight + UITreeView.ItemHeight;
        }

        public bool HoldSameDataWith<T>(TreeNode<T> node)
        {
            if (Data == node.Data)
            {
                if (Children.Count == node.Children.Count)
                {
                    for (int i = 0; i < Children.Count; i++)
                    {
                        bool result = Children[i].HoldSameDataWith(node.Children[i]);

                        if (!result)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }

    public delegate void TreeViewEvent(UITreeView treeView, MouseEventArgs e);
}
