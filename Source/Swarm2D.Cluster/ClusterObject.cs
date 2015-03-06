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
using Swarm2D.Library;

namespace Swarm2D.Cluster
{
    public class ClusterObject : Component
    {
        public ClusterPeer OwnerNode { get; internal set; }
        public NetworkView NetworkView { get; private set; }

        public IMultiplayerNode Locker { get; private set; }
        public bool Locked { get; private set; }

        public bool IsMine { get { return OwnerNode.IsMine; } }

        private Queue<LockRequest> _lockRequests;

        private Dictionary<string, ClusterObject> _children;
        private NetworkController _networkController;

        private ClusterNode _clusterNode;

        protected override void OnAdded()
        {
            base.OnAdded();

            _clusterNode = Engine.RootEntity.GetComponent<ClusterNode>();

            _lockRequests = new Queue<LockRequest>();
            _children = new Dictionary<string, ClusterObject>();

            Locked = false;
            Locker = null;

            NetworkView = GetComponent<NetworkView>();
            _networkController = NetworkView.NetworkController;
        }

        [EntityMessageHandler(MessageType = typeof(LockRequestMessage))]
        private void OnLockRequestMessage(Message message)
        {
            LockRequestMessage lockObjectMessage = message as LockRequestMessage;

            if (!Locked)
            {
                Debug.Log("Object Locked!");
                Locked = true;
                Locker = lockObjectMessage.Node;

                Locker.ResponseNetworkEntityMessageEvent(lockObjectMessage, new ResponseData());
            }
            else
            {
                Debug.Log("Object already locked, waiting previous locker to finish its operation...");
                _lockRequests.Enqueue(new LockRequest(lockObjectMessage));
            }
        }

        [EntityMessageHandler(MessageType = typeof(UnlockRequestMessage))]
        private void OnUnlockRequestMessage(Message message)
        {
            UnlockRequestMessage unlockObjectMessage = message as UnlockRequestMessage;

            if (Locked && Locker == unlockObjectMessage.Node)
            {
                Debug.Log("Object Unlocked!");

                IMultiplayerNode oldLocker = Locker;
                Locked = false;
                Locker = null;

                oldLocker.ResponseNetworkEntityMessageEvent(unlockObjectMessage, new ResponseData());

                if (_lockRequests.Count > 0)
                {
                    Debug.Log("Object Locked!");

                    LockRequest newLockRequest = _lockRequests.Dequeue();

                    Locked = true;
                    Locker = newLockRequest.Requester;

                    Locker.ResponseNetworkEntityMessageEvent(newLockRequest.RequestMessage, new ResponseData());
                }
            }
        }

        [EntityMessageHandler(MessageType = typeof(GetChildMessage))]
        private void OnGetChildMessage(Message message)
        {
            GetChildMessage getChildMessage = message as GetChildMessage;

            Entity child = Entity.FindChild(getChildMessage.Name);

            if (child != null)
            {
                ClusterObject childClusterObject = child.GetComponent<ClusterObject>();
                NetworkView childNetworkView = child.GetComponent<NetworkView>();

                getChildMessage.Node.ResponseNetworkEntityMessageEvent(getChildMessage, new GetChildResponseMessage(childNetworkView.NetworkID, childClusterObject.OwnerNode.Id));
            }
            else
            {
                getChildMessage.Node.ResponseNetworkEntityMessageEvent(getChildMessage, new GetChildResponseMessage(null, null));
            }
        }

        [EntityMessageHandler(MessageType = typeof(CreateChildMessage))]
        private void OnCreateChildMessage(Message message)
        {
            CreateChildMessage createChildMessage = message as CreateChildMessage;

            //ClusterPeer requesterPeer = createChildMessage.ClusterPeer;
            ClusterPeer requesterPeer = _clusterNode.GetClusterPeer(createChildMessage.Node);

            Debug.Log("A child named " + createChildMessage.Name + " requested for creation on parent named" + Entity.Name);

            Entity createdChild = Entity.CreateChildEntity(createChildMessage.Name);

            ClusterObject createdClusterObject = createdChild.GetComponent<ClusterObject>();
            createdClusterObject.OwnerNode = requesterPeer;

            NetworkView createdNetworkView = createdChild.GetComponent<NetworkView>();
            createdNetworkView.NetworkID = _networkController.GenerateNewNetworkId(NetworkView.NetworkID);

            createChildMessage.Node.ResponseNetworkEntityMessageEvent(createChildMessage, new CreateChildResponseMessage(createdNetworkView.NetworkID));
        }
    }

    class LockRequest
    {
        internal IMultiplayerNode Requester { get; private set; }
        internal LockRequestMessage RequestMessage { get; private set; }

        internal LockRequest(LockRequestMessage requestMessage)
        {
            RequestMessage = requestMessage;
            Requester = requestMessage.Node;
        }
    }

    public class LockRequestMessage : ClusterObjectMessage
    {
        public LockRequestMessage()
        {
        }

        protected override void OnSerialize(IDataWriter writer)
        {
        }

        protected override void OnDeserialize(IDataReader reader)
        {
        }
    }

    public class UnlockRequestMessage : ClusterObjectMessage
    {
        public UnlockRequestMessage()
        {
        }

        protected override void OnSerialize(IDataWriter writer)
        {
        }

        protected override void OnDeserialize(IDataReader reader)
        {
        }
    }

    public class GetChildMessage : ClusterObjectMessage
    {
        public string Name { get; private set; }

        public GetChildMessage()
        {
        }

        public GetChildMessage(string name)
        {
            Name = name;
        }

        protected override void OnSerialize(IDataWriter writer)
        {
            writer.WriteUnicodeString(Name);
        }

        protected override void OnDeserialize(IDataReader reader)
        {
            Name = reader.ReadUnicodeString();
        }
    }

    public class GetChildResponseMessage : ResponseData
    {
        public NetworkID Id { get; private set; }
        public NetworkID ClusterObjectPeerId { get; private set; }

        public GetChildResponseMessage()
        {
        }

        public GetChildResponseMessage(NetworkID id, NetworkID clusterObjectPeerId)
        {
            Id = id;
            ClusterObjectPeerId = clusterObjectPeerId;
        }

        protected override void Serialize(IDataWriter writer)
        {
            if (Id != null)
            {
                writer.WriteBool(true);
                writer.WriteNetworkID(Id);
                writer.WriteNetworkID(ClusterObjectPeerId);
            }
            else
            {
                writer.WriteBool(false);
            }
        }

        protected override void Deserialize(IDataReader reader)
        {
            bool exists = reader.ReadBool();

            if (exists)
            {
                Id = reader.ReadNetworkID();
                ClusterObjectPeerId = reader.ReadNetworkID();
            }
        }
    }

    public class CreateChildMessage : ClusterObjectMessage
    {
        public string Name { get; private set; }

        public CreateChildMessage()
        {
        }

        public CreateChildMessage(string name)
        {
            Name = name;
        }

        protected override void OnSerialize(IDataWriter writer)
        {
            writer.WriteUnicodeString(Name);
        }

        protected override void OnDeserialize(IDataReader reader)
        {
            Name = reader.ReadUnicodeString();
        }
    }

    public class CreateChildResponseMessage : ResponseData
    {
        public NetworkID ChildId { get; private set; }

        public CreateChildResponseMessage()
        {
        }

        public CreateChildResponseMessage(NetworkID childId)
        {
            ChildId = childId;
        }

        protected override void Serialize(IDataWriter writer)
        {
            writer.WriteNetworkID(ChildId);
        }

        protected override void Deserialize(IDataReader reader)
        {
            ChildId = reader.ReadNetworkID();
        }
    }
}
