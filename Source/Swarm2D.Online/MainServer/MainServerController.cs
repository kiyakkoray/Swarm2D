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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Swarm2D.Cluster;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Library;
using Debug = Swarm2D.Library.Debug;

namespace Swarm2D.Online.MainServer
{
    public class MainServerController : EngineComponent
    {
        private NetworkController _networkController;
        private NetworkView _networkView;

        private bool _initialized = false;

        private ClusterNode _clusterNode;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _networkController = GetComponent<NetworkController>();
            _networkView = GetComponent<NetworkView>();

            _clusterNode = GetComponent<ClusterNode>();
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            if (!_initialized)
            {
                _initialized = true;
                _clusterNode.HostCluster("127.0.0.1", Parameters.MainServerPortForCluster);
            }
        }

        [GlobalMessageHandler(MessageType = typeof(ClientConnectMessage))]
        private void OnClientConnect(Message message)
        {
            Debug.Log("a client conencted to main server");
            ClientConnectMessage clientConnectMessage = message as ClientConnectMessage;
        }

        [GlobalMessageHandler(MessageType = typeof(ClientDisconnectMessage))]
        private void OnClientDisconnect(Message message)
        {
        }
    }

}