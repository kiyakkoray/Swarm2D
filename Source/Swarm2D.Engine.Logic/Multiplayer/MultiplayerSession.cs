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
using Swarm2D.Library;
using Swarm2D.Network;

namespace Swarm2D.Engine.Logic
{
    internal enum NetworkEventType
    {
        RPC,
        Synchronize,
        NetworkEntityMessage,
        NetworkEntityMessageResponse
    }

    public abstract class MultiplayerSession : EngineComponent
    {
        internal abstract INetworkSession Session
        {
            get;
        }

        public bool IsClient
        {
            get;
            protected set;
        }

        public bool IsServer
        {
            get;
            protected set;
        }

        public NetworkController NetworkController { get; private set; }

        private Dictionary<short, Entity> _requestEntities;
        private short _lastRequestEntityId = 0;

        protected override void OnAdded()
        {
            base.OnAdded();

            _requestEntities = new Dictionary<short, Entity>();

            NetworkController = Engine.RootEntity.GetComponent<NetworkController>();
        }

        internal abstract void AddRPCEvent(NetworkID networkId, string methodName, object[] parameters);

        internal abstract void AddNetworkEntityMessageEvent(NetworkID networkId, NetworkEntityMessage message);

        protected void ProcessEvent(IDataReader reader)
        {
            NetworkEventType eventType = (NetworkEventType)reader.ReadByte();

            if (eventType == NetworkEventType.RPC)
            {
                ReadAndProcessRPCEvent(reader);
            }
            else if (eventType == NetworkEventType.Synchronize)
            {
                ReadAndProcessSynchronizeEvent(reader);
            }
            else if (eventType == NetworkEventType.NetworkEntityMessage)
            {
                ReadAndProcessNetworkEntityMessageEvent(reader);
            }
            else if (eventType == NetworkEventType.NetworkEntityMessageResponse)
            {
                ReadAndProcessResponseNetworkEntityMessageEvent(reader);
            }
        }

        private void ReadAndProcessRPCEvent(IDataReader message)
        {
            NetworkID networkId;
            string methodName;
            Type[] types;
            Object[] parameters;

            message.ReadRPCEvent(out networkId, out methodName, out types, out parameters);

            //Debug.Log("RPC EVENT, id: " + networkId.Id + ", name: " + methodName + ", parameter count: " + parameterCount);

            NetworkView networkView = NetworkController.FindNetworkView(networkId);

            if (networkView != null)
            {
                networkView.HandleRPCEvent(methodName, types, parameters);
            }
        }

        private void ReadAndProcessSynchronizeEvent(IDataReader message)
        {
            NetworkID networkId;
            byte[] data;

            message.ReadSynchronizeEvent(out networkId, out data);

            NetworkView networkView = NetworkController.FindNetworkView(networkId);

            if (networkView != null)
            {
                networkView.HandleSynchronizeEvent(data);
            }
        }

        private void ReadAndProcessNetworkEntityMessageEvent(IDataReader message)
        {
            NetworkID networkId;
            NetworkEntityMessage networkEntityMessage;

            message.ReadNetworkEntityMessageEvent(out networkId, out networkEntityMessage);

            OnEntityMessageEventRead(networkEntityMessage);

            //Debug.Log("NetworkEntityMessage EVENT, id: " + networkId.Id + ", message: " + networkEntityMessage);

            NetworkView networkView = NetworkController.FindNetworkView(networkId);

            if (networkView != null)
            {
                networkView.Entity.SendMessage(networkEntityMessage);
            }
        }

        private void ReadAndProcessResponseNetworkEntityMessageEvent(IDataReader message)
        {
            short responseId;
            ResponseData responseData;

            message.ReadResponseNetworkEntityMessageEvent(out responseId, out responseData);

            Entity requestEntity = this._requestEntities[responseId];

            NetworkEntityMessageResponseMessage networkEntityMessageResponseMessage
                = new NetworkEntityMessageResponseMessage();

            networkEntityMessageResponseMessage.Response = responseData;

            requestEntity.SendMessage(networkEntityMessageResponseMessage);

            _requestEntities.Remove(responseId);
        }

        protected virtual void OnEntityMessageEventRead(NetworkEntityMessage networkEntityMessage)
        {

        }

        internal abstract void UpdateRead();
        internal abstract void UpdateWrite();

        public bool IsDefaultSession
        {
            get
            {
                return NetworkController.DefaultSession == this;
            }
        }

        protected void HandleRequestNetworkEntityMessage(NetworkEntityMessage message, Entity requester)
        {
            message.RequestResponseId = _lastRequestEntityId;
            _requestEntities.Add(_lastRequestEntityId, requester);
            _lastRequestEntityId++;
        }

        public INetworkDriver NetworkDriver { get; protected set; }
    }

    public class NetworkEntityMessageResponseMessage : EntityMessage
    {
        public ResponseData Response { get; internal set; }
    }
}
