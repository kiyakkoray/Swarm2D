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

namespace Swarm2D.Engine.Logic
{
    public class NetworkEntityMessageTask : CoroutineTask
    {
        public NetworkEntityMessage RequestMessage { get; private set; }
        public IMultiplayerNode TargetNode { get; private set; }
        public NetworkView TargetNetworkView { get; private set; }

        public ResponseData ResponseMessage { get; private set; }

        private bool _messageSent = false;

        private bool _isFinished = false;

        protected internal override void DoTask()
        {
            base.DoTask();

            if (!_messageSent)
            {
                TargetNetworkView.NetworkEntityMessageEvent(TargetNode, Entity, RequestMessage);

                _messageSent = true;
            }
        }

        public void Initialize(NetworkEntityMessage requestMessage, IMultiplayerNode targetNode, NetworkView targetNetworkView)
        {
            RequestMessage = requestMessage;
            TargetNode = targetNode;
            TargetNetworkView = targetNetworkView;
        }

        [EntityMessageHandler(MessageType = typeof(NetworkEntityMessageResponseMessage))]
        private void OnNetworkEntityMessageResponseMessage(Message message)
        {
            NetworkEntityMessageResponseMessage networkEntityMessageResponseMessage =
                message as NetworkEntityMessageResponseMessage;

            ResponseMessage = networkEntityMessageResponseMessage.Response;

            _isFinished = true;
        }

        public override bool IsFinished
        {
            get { return _isFinished; }
        }
    }
}