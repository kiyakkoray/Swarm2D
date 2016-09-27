/******************************************************************************
Copyright (c) 2016 Koray Kiyakoglu

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

using Swarm2D.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Swarm2D.Engine.Core
{
    public sealed class IdTypeMap
    {
        private Dictionary<Type, short> _idsOfTypes;
        private Dictionary<short, Type> _typesOfIds;

        public Type Type { get; private set; }

        public IEnumerable<Type> Types
        {
            get { return _idsOfTypes.Keys; }
        }

        public IdTypeMap(Type type)
        {
            Type = type;

            SearchTypes();
        }

        private void SearchTypes()
        {
            _idsOfTypes = new Dictionary<Type, short>();
            _typesOfIds = new Dictionary<short, Type>();

            Assembly[] assemblies = PlatformHelper.GetGameAssemblies();

            List<Type> messageTypes = new List<Type>();

            Debug.Log("Searching messages...");

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (!PlatformHelper.IsAbstract(type))
                    {
                        if (Type.IsAssignableFrom(type))
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
                short hashCode = IdTypeMap.GetHashCodeOf(name);

                _idsOfTypes.Add(messageType, hashCode);
                _typesOfIds.Add(hashCode, messageType);
            }
        }

        public short GetObjectTypeId(Type type)
        {
            if (!_idsOfTypes.ContainsKey(type))
            {
                SearchTypes();
            }

            return _idsOfTypes[type];
        }

        public Object CreateObjectWithId(short id)
        {
            if (!_typesOfIds.ContainsKey(id))
            {
                SearchTypes();
            }

            return Activator.CreateInstance(_typesOfIds[id]);
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
}
