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

        private static Dictionary<Type, short> _idsOfMessages;
        private static Dictionary<short, Type> _typesOfMessages;

        static Message()
        {
            SearchMessages();
        }

        private static void SearchMessages()
        {
            _idsOfMessages = new Dictionary<Type, short>();
            _typesOfMessages = new Dictionary<short, Type>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> messageTypes = new List<Type>();

            Debug.Log("Searching messages...");

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type != typeof(Message) && type != typeof(EntityMessage) && type != typeof(GlobalMessage))
                    {
                        if (typeof(Message).IsAssignableFrom(type))
                        {
                            Debug.Log("found message: " + type.Name);

                            messageTypes.Add(type);
                        }
                    }
                }
            }

            Debug.Log("Message searching is over..");

            messageTypes = messageTypes.OrderBy(type => type.Name).ToList();

            foreach (Type messageType in messageTypes)
            {
                string name = messageType.FullName;
                short hashCode = Message.GetHashCodeOf(name);

                _idsOfMessages.Add(messageType, hashCode);
                _typesOfMessages.Add(hashCode, messageType);
            }
        }

        internal static int GetMessageId(Type type)
        {
            if (!_idsOfMessages.ContainsKey(type))
            {
                SearchMessages();
            }

            return _idsOfMessages[type];
        }

        protected Message()
        {
            if (!_idsOfMessages.ContainsKey(GetType()))
            {
                SearchMessages();
            }

            Id = _idsOfMessages[GetType()];
        }

        public static Message CreateMessageWithId(short id)
        {
            if (!_typesOfMessages.ContainsKey(id))
            {
                SearchMessages();
            }

            return Activator.CreateInstance(_typesOfMessages[id]) as Message;
        }

        public static short GetHashCodeOf(string text)
        {
            short result = 0;

            for (int i = 0; i < text.Length; i++)
            {
                short c = (short)text[i];
                short adder = 0;

                if (i % 2 == 0)
                {
                    adder = c;
                }
                else
                {
                    adder = c;

                    adder = (short)(adder << 8);
                }

                result = (short)(result ^ adder);
            }

            result += (short)text.Length;

            return result;
            
            //int hashCode = text.GetHashCode();
            //
            //short firstPart = (short)hashCode;
            //short secondPart = (short)(hashCode >> 16);
            //short result = (short)(firstPart ^ secondPart);
            //
            //return result;

            //SHA1 hash = SHA1.Create();
            //
            //byte[] byteResult = hash.ComputeHash(Encoding.ASCII.GetBytes(text));
            //
            //short result = 0;
            //
            //for (int i = 0; i < byteResult.Length / 2; i++)
            //{
            //    result = (short)(result ^ BitConverter.ToInt16(byteResult, i * 2));
            //}
            //
            //return result;
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

    public delegate void MessageHandlerDelegate(Message message);
}
