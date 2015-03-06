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

namespace Swarm2D.Library
{
    public class Tree<T>
    {
        public TreeNode<T> Root { get; set; }

        public bool ContainsRecursively(TreeNode<T> node)
        {
            return Root.ContainsRecursively(node);
        }

        public TreeNode<T> FindTreeNodeWithData(object data)
        {
            return Root.FindTreeNodeWithData(data);
        }
    }

    public class TreeNode<T>
    {
        public List<TreeNode<T>> Children { get; private set; }

        public object Data { get; set; }

        public TreeNode()
        {
            Children = new List<TreeNode<T>>();
        }

        public bool ContainsRecursively(TreeNode<T> node)
        {
            if (node == this)
            {
                return true;
            }

            foreach (TreeNode<T> treeNode in Children)
            {
                bool containsInChild = treeNode.ContainsRecursively(node);

                if (containsInChild)
                {
                    return true;
                }
            }

            return false;
        }

        public TreeNode<T> FindTreeNodeWithData(object data)
        {
            if (data == Data)
            {
                return this;
            }

            foreach (TreeNode<T> treeNode in Children)
            {
                TreeNode<T> foundNode = treeNode.FindTreeNodeWithData(data);

                if (foundNode != null && foundNode.Data == data)
                {
                    return foundNode;
                }
            }

            return null;
        }
    }
}
