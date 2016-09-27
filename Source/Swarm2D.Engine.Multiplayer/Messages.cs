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
using System.Reflection;
using System.Security.Policy;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Library;

namespace Swarm2D.Engine.Multiplayer
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class NetworkMessageHandler : Attribute
    {
        public Type MessageType { get; set; }
        public int Order { get; set; }
    }

    public abstract class NetworkEntityMessage : EntityMessage
    {
        internal void Serialize(IDataWriter writer)
        {
            this.OnSerialize(writer);
        }

        internal void Deserialize(IDataReader reader)
        {
            this.OnDeserialize(reader);
        }

        protected abstract void OnSerialize(IDataWriter writer);
        protected abstract void OnDeserialize(IDataReader reader);

        public NetworkEntityMessageType Type { get; internal set; }
        internal short RequestResponseId { get; set; }

        public IMultiplayerNode Node { get; internal set; }
    }

    public enum NetworkEntityMessageType
    {
        Plain,
        Request,
    }

    public abstract class ClientEntityMessage : NetworkEntityMessage
    {
        public Peer Peer { get; internal set; }

        protected ClientEntityMessage()
        {
            //Debug.Log("Creating a client entity message: " + GetType().Name);
        }
    }

    public class ClientConnectMessage : GlobalMessage
    {
        public Peer Peer { get; internal set; }
        public MultiplayerSession Session { get; set; }
    }

    public class ClientDisconnectMessage : GlobalMessage
    {
        public Peer Peer { get; set; }
        public MultiplayerSession Session { get; set; }
    }
}
