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
using System.Reflection;
using Swarm2D.Engine.Core;
using Swarm2D.Library;

namespace Swarm2D.Engine.Multiplayer
{
    public class NetworkView : Component
    {
        private NetworkID _networkId;

        public NetworkID NetworkID
        {
            get
            {
                return _networkId;
            }
            set
            {
                NetworkView oldParentView = null;

                if (_networkId != null && _networkId != NetworkID.Root)
                {
                    oldParentView = NetworkController.FindNetworkView(_networkId.ParentObjectId);
                }

                SetNetworkId(oldParentView, value);
            }
        }

        private void SetNetworkId(NetworkView parentNetworkView, NetworkID newNetworkId)
        {
            NetworkID oldNetworkId = _networkId;

            if (parentNetworkView != null)
            {
                parentNetworkView._children.Remove(_networkId);
            }

            _networkId = newNetworkId;

            CheckAndAddNetworkIDToParent();

            if (oldNetworkId != null)
            {
                Dictionary<NetworkID, NetworkView> oldChildrenData = new Dictionary<NetworkID, NetworkView>(_children);

                foreach (var child in oldChildrenData)
                {
                    NetworkID childNetworkId = child.Key;
                    NetworkView childNetworkView = child.Value;

                    NetworkID newChildNetworkId = childNetworkId.CreateNewIdWithNewParentId(_networkId);

                    childNetworkView.SetNetworkId(this, newChildNetworkId);
                }
            }
        }

        private void CheckAndRemoveNetworkIDFromParent()
        {
            if (_networkId != null && _networkId.Type != NetworkID.IDType.Root)
            {
                NetworkView oldParentView = NetworkController.FindNetworkView(_networkId.ParentObjectId);

                if (oldParentView != null)
                {
                    oldParentView._children.Remove(_networkId);
                }
            }
        }

        private void CheckAndAddNetworkIDToParent()
        {
            if (_networkId != null && _networkId.Type != NetworkID.IDType.Root)
            {
                NetworkView newParentView = NetworkController.FindNetworkView(_networkId.ParentObjectId);

                newParentView._children.Add(_networkId, this);
            }
        }

        private void CheckAndRemoveSynchronizeHandlerFromController()
        {
            if (_synchronizeHandler != null)
            {
                NetworkController.SynchronizableNetworkViews.Remove(this);
            }
        }

        private void CheckAndAddSynchronizeHandlerToController()
        {
            if (_synchronizeHandler != null)
            {
                NetworkController.SynchronizableNetworkViews.Add(this);
            }
        }

        private Dictionary<NetworkID, NetworkView> _children;

        private INetworkSynchronizeHandler _synchronizeHandler;

        public INetworkSynchronizeHandler SynchronizeHandler
        {
            get { return _synchronizeHandler; }
            set
            {
                CheckAndRemoveSynchronizeHandlerFromController();

                _synchronizeHandler = value;

                CheckAndAddSynchronizeHandlerToController();
            }
        }

        public PeerGroup SynchronizationPeerGroup { get; set; }

        public NetworkController NetworkController { get; private set; }

        internal NetworkView FindNetworkViewInChildren(NetworkID id)
        {
            int myLevel = NetworkID.Level;
            int targetLevel = id.Level;

            if (targetLevel > myLevel)
            {
                NetworkID childNetworkId = id;

                while (childNetworkId.ParentObjectId != NetworkID)
                {
                    childNetworkId = childNetworkId.ParentObjectId;
                }

                NetworkView foundChild = null;

                if (_children.TryGetValue(childNetworkId, out foundChild))
                {
                    if (childNetworkId.Level != targetLevel)
                    {
                        return foundChild.FindNetworkViewInChildren(id);
                    }
                    else
                    {
                        return foundChild;
                    }
                }
            }

            return null;
        }

        protected override void OnAdded()
        {
            _children = new Dictionary<NetworkID, NetworkView>();

            NetworkController = Engine.RootEntity.GetComponent<NetworkController>();
        }

        public void RPC(string methodName, params object[] parameters)
        {
            NetworkController.DefaultSession.AddRPCEvent(NetworkID, methodName, parameters);
        }

        public void RPC(TargetSession targetSession, string methodName, params object[] parameters)
        {
            switch (targetSession)
            {
                case TargetSession.DefaultSession:
                    NetworkController.DefaultSession.AddRPCEvent(NetworkID, methodName, parameters);
                    break;
                case TargetSession.AllSessions:
                    break;
            }
        }

        public void RPC(MultiplayerSession targetSession, string methodName, params object[] paramters)
        {
            targetSession.AddRPCEvent(NetworkID, methodName, paramters);
        }

        public void RPC(Peer targetPeer, string methodName, params object[] parameters)
        {
            targetPeer.AddRPCEvent(NetworkID, methodName, parameters);
        }

        public void RPC(PeerGroup targetPeerGroup, string methodName, params object[] parameters)
        {
            if (targetPeerGroup != null)
            {
                foreach (Peer peer in targetPeerGroup.Peers)
                {
                    peer.AddRPCEvent(NetworkID, methodName, parameters);
                }
            }
        }

        public void NetworkEntityMessageEvent(NetworkEntityMessage message)
        {
            NetworkController.DefaultSession.AddNetworkEntityMessageEvent(NetworkID, message);
        }

        public void NetworkEntityMessageEvent(TargetSession targetSession, NetworkEntityMessage message)
        {
            switch (targetSession)
            {
                case TargetSession.DefaultSession:
                    NetworkController.DefaultSession.AddNetworkEntityMessageEvent(NetworkID, message);
                    break;
                case TargetSession.AllSessions:
                    break;
            }
        }

        public void NetworkEntityMessageEvent(IMultiplayerNode target, NetworkEntityMessage message)
        {
            target.AddNetworkEntityMessageEvent(NetworkID, message);
        }

        public void NetworkEntityMessageEvent(IMultiplayerNode multiplayerNode, Entity requester, NetworkEntityMessage message)
        {
            message.Type = NetworkEntityMessageType.Request;
            multiplayerNode.AddNetworkEntityMessageEvent(NetworkID, requester, message);
        }

        public void NetworkEntityMessageEvent(PeerGroup targetPeerGroup, NetworkEntityMessage message)
        {
            foreach (Peer peer in targetPeerGroup.Peers)
            {
                IMultiplayerNode peerNode = peer;
                peerNode.AddNetworkEntityMessageEvent(NetworkID, message);
            }
        }

        internal void HandleRPCEvent(string methodName, Type[] types, object[] parameters)
        {
            MethodInfo methodInfo = null;
            Component foundComponent = null;

            foreach (Component component in Entity.Components)
            {
                methodInfo = component.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);

                if (methodInfo != null)
                {
                    foundComponent = component;
                    break;
                }
            }

            if (methodInfo != null)
            {
                //Debug.Log("Invoking RPC Method " + methodName);
                try
                {
                    methodInfo.Invoke(foundComponent, parameters);
                }
                catch (Exception e)
                {
                    Debug.Log("exception on HandleRPCEvent: " + e);
                }

            }
            else
            {
                Debug.Log("RPC Method " + methodName + " not found");
            }
        }

        internal void HandleSynchronizeEvent(byte[] data)
        {
            if (SynchronizeHandler != null)
            {
                DataReader reader = new DataReader(data);

                SynchronizeHandler.OnDeserializeFromNetwork(reader);
            }
        }

        public void DestroyEntity()
        {
            if (IsMine)
            {
                RPC("OnDestoryEntityFromNetwork");
            }

            Entity.Destroy();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            CheckAndRemoveNetworkIDFromParent();
            CheckAndRemoveSynchronizeHandlerFromController();
        }

        [RPCMethod]
        private void OnDestoryEntityFromNetwork()
        {
            Entity.Destroy();
        }

        public bool IsMine
        {
            get
            {
                return NetworkID.IsOwnedBy(NetworkController.PeerId);
            }
        }
    }

    public enum TargetSession
    {
        DefaultSession,
        AllSessions
    }

    public interface INetworkSynchronizeHandler
    {
        void OnSerializeToNetwork(DataWriter dataWriter);

        void OnDeserializeFromNetwork(DataReader reader);
    }
}