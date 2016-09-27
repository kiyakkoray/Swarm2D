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
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.Multiplayer;

namespace Swarm2D.Cluster.Tasks
{
    public class GetChildTask : CoroutineTask
    {
        public ClusterObject Child { get; private set; }

        private ClusterObject _parent;
        private string _name;

        private bool _messageSent = false;
        private bool _isFinished = false;

        private ClusterNode _clusterNode;

        protected override void OnAdded()
        {
            base.OnAdded();

            _clusterNode = Engine.RootEntity.GetComponent<ClusterNode>();
        }

        protected override void DoTask()
        {
            base.DoTask();

            if (!_messageSent)
            {
                if (_parent.IsMine)
                {
                    Entity childEntity = _parent.Entity.FindChild(_name);

                    if (childEntity != null)
                    {
                        Child = childEntity.GetComponent<ClusterObject>();
                    }

                    _isFinished = true;
                }
                else
                {
                    //first look if its exists, if not then query objects owner

                    Entity childEntity = _parent.Entity.FindChild(_name);

                    if (childEntity != null)
                    {
                        Child = childEntity.GetComponent<ClusterObject>();
                        _isFinished = true;
                    }
                    else
                    {
                        _parent.NetworkView.NetworkEntityMessageEvent(_parent.OwnerNode.Session, Entity, new GetChildMessage(_name));
                    }
                }

                _messageSent = true;
            }
        }

        public void Initialize(ClusterObject parent, string name)
        {
            _parent = parent;
            _name = name;
        }

        [EntityMessageHandler(MessageType = typeof(NetworkEntityMessageResponseMessage))]
        private void OnGetChildResponseMessage(Message message)
        {
            var networkEntityMessageResponseMessage = message as NetworkEntityMessageResponseMessage;
            GetChildResponseMessage getChildResponseMessage = networkEntityMessageResponseMessage.Response as GetChildResponseMessage;

            //TODO: it is possible that requested object may created from another task, so check if its exists here

            if (getChildResponseMessage.Id != null)
            {
                Entity childEntity = _parent.Entity.CreateChildEntity(_name);
                Child = childEntity.GetComponent<ClusterObject>();
                Child.OwnerNode = _clusterNode.GetClusterPeer(getChildResponseMessage.ClusterObjectPeerId);

                var networkController = Child.NetworkView.NetworkController;
                Child.NetworkView.NetworkID = getChildResponseMessage.Id;
            }

            _isFinished = true;
        }

        public override bool IsFinished
        {
            get { return _isFinished; }
        }
    }
}
