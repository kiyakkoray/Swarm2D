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
using Swarm2D.Library;

namespace Swarm2D.Engine.Logic
{
    public class NavigationPath
    {
        private INavigableNode _startNode;
        private INavigableNode _endNode;

        private List<INavigableNode> _pathNodes;
        private LinkedList<INavigableNode> _openNodes;

        private INavigableWorld _navigableWorld;

        public NavigationPath(INavigableNode startNode, INavigableNode endNode)
        {
            _navigableWorld = startNode.NavigableWorld;

            _startNode = startNode;
            _endNode = endNode;

            _pathNodes = new List<INavigableNode>(512);
            _openNodes = new LinkedList<INavigableNode>();

            GeneratePath();
        }

        private void GeneratePath()
        {
            CloseNode(_startNode);

            while (true)
            {
                INavigableNode currentNode = GetMinimalCostNode();

                if (currentNode == _endNode)
                {
                    //found path
                    CalculatePath();
                    break;
                }
                else if (currentNode == null)
                {
                    // no possible path to there
                    break;
                }
                else
                {
                    CloseNode(currentNode);
                }
            }

            while (_openNodes.Count > 0)
            {
                _openNodes.RemoveLast();
            }

            _navigableWorld.ResetPathCalculations();
        }

        protected void CalculatePath()
        {
            INavigableNode currentNode = _endNode;

            while (currentNode != null)
            {
                PathNodes.Add(currentNode);
                currentNode = currentNode.ParentNode;
            }
        }

        private INavigableNode GetMinimalCostNode()
        {
            INavigableNode selectedNode = null;

            if (_openNodes.Count > 0)
            {
                selectedNode = this._openNodes.First.Value;
            }

            return selectedNode;
        }

        private void CloseNode(INavigableNode closedNode)
        {
            closedNode.PathInfo = NodePathInfo.Closed;

            if (closedNode.ListNode != null)
            {
                _openNodes.Remove(closedNode.ListNode);
                closedNode.ListNode = null;
            }

            foreach (INavigableNode navigableNode in closedNode.Neighbours)
            {
                OpenNode(closedNode, navigableNode);
            }
        }

        private void OpenNode(INavigableNode parentNode, INavigableNode openedNode)
        {
            if (openedNode == null) return;
            //if (!this.pathMap.IsMoveableByObject(OpenedCell.X, OpenedCell.Y)) return;

            float newMovementCost = MovementCost(parentNode, openedNode) + parentNode.MovementCostToHere;
            float newHeuristicCost = HeuristicFunction(openedNode, _endNode);
            
            if (openedNode.PathInfo == NodePathInfo.Opened)
            {
                float newTotalCost = newMovementCost + newHeuristicCost;

                if (newTotalCost < openedNode.GetTotalCost())
                {
                    openedNode.HeuristicCostToEnd = newHeuristicCost;
                    openedNode.MovementCostToHere = newMovementCost;
                    openedNode.ParentNode = parentNode;
                }
            }
            else if (openedNode.PathInfo == NodePathInfo.Undefined)
            {
                openedNode.PathInfo = NodePathInfo.Opened;

                INavigableNode foundNode = null;

                openedNode.HeuristicCostToEnd = newHeuristicCost;
                openedNode.MovementCostToHere = newMovementCost;
                openedNode.ParentNode = parentNode;

                foreach (INavigableNode currentNode in _openNodes)
                {
                    if (currentNode.GetTotalCost() > openedNode.GetTotalCost())
                    {
                        foundNode = currentNode;
                        break;
                    }
                }

                if (foundNode != null)
                {
                    openedNode.ListNode = this._openNodes.AddBefore(foundNode.ListNode, openedNode);
                }
                else
                {
                    openedNode.ListNode = this._openNodes.AddLast(openedNode);
                }
            }
        }

        private static float HeuristicFunction(INavigableNode currentNode, INavigableNode endNode)
        {
            return Vector2.Distance(currentNode.Position, endNode.Position);
        }

        private static float MovementCost(INavigableNode currentNode, INavigableNode neighbourNode)
        {
            return Vector2.Distance(currentNode.Position, neighbourNode.Position);
        }

        public List<INavigableNode> PathNodes
        {
            get
            {
                return this._pathNodes;
            }
        }
    }
}
