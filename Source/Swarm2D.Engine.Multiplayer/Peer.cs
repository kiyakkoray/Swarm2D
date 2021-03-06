﻿/******************************************************************************
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

namespace Swarm2D.Engine.Multiplayer
{
    [Serializable]
    public class Peer : Component, IMultiplayerNode
    {
        private MultiplayerNode _multiplayerNode;

        public MultiplayerServerSession ServerSession { get; internal set; }

        public Peer()
        {
            _multiplayerNode = new MultiplayerNode();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Session != null)
            {
                Session.Close();
            }
        }

        internal void FinalizeAndSendMessage()
        {
            _multiplayerNode.FinalizeAndSendMessage();
        }

        internal void AddRPCEvent(NetworkID networkId, string methodName, object[] parameters)
        {
            _multiplayerNode.AddRPCEvent(networkId, methodName, parameters);
        }

        void IMultiplayerNode.AddNetworkEntityMessageEvent(NetworkID networkId, NetworkEntityMessage message)
        {
            _multiplayerNode.AddNetworkEntityMessageEvent(networkId, message);
        }

        void IMultiplayerNode.AddNetworkEntityMessageEvent(NetworkID networkId, Entity requester, NetworkEntityMessage message)
        {
            ServerSession.AddNetworkEntityMessageEvent(this, networkId, requester, message);
        }

        public void ResponseNetworkEntityMessageEvent(NetworkEntityMessage requestMessage, ResponseData responseData)
        {
            _multiplayerNode.ResponseNetworkEntityMessageEvent(requestMessage, responseData);
        }

        internal void AddSynchronizeEvent(NetworkID networkId, byte[] data)
        {
            _multiplayerNode.AddSynchronizeEvent(networkId, data);
        }

        internal IServerSideClientSession Session
        {
            get { return _multiplayerNode.Session as IServerSideClientSession; }
            set { _multiplayerNode.Session = value; }
        }

        //internal MultiplayerServerSession MultiplayerSession { get; set; }

        public NetworkID Id { get; internal set; }
    }
}
