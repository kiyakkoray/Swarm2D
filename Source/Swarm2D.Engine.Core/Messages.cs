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
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using Swarm2D.Library;

namespace Swarm2D.Engine.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EntityMessageHandler : Attribute
    {
        public Type MessageType { get; set; }
        public int Order { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class GlobalMessageHandler : Attribute
    {
        public Type MessageType { get; set; }
        public int Order { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DomainMessageHandler : Attribute
    {
        public Type MessageType { get; set; }
        public int Order { get; set; }
    }

    public abstract class Message
    {
        public short Id { get; internal set; }

        private static IdTypeMap _idTypeMap;

        static Message()
        {
            _idTypeMap = new IdTypeMap(typeof(Message));
        }

        internal static int GetMessageId(Type type)
        {
            return _idTypeMap.GetObjectTypeId(type);
        }

        protected Message()
        {
            Id = _idTypeMap.GetObjectTypeId(GetType());
        }

        public static Message CreateMessageWithId(short id)
        {
            return (Message)_idTypeMap.CreateObjectWithId(id);
        }
    }

    public abstract class EntityMessage : Message
    {

    }

    public abstract class GlobalMessage : Message
    {

    }

    public abstract class DomainMessage : Message
    {

    }

    public class UpdateMessage : DomainMessage
    {
        public float Dt { get; set; }
    }

    internal delegate void MessageHandlerDelegate(Message message);
}
