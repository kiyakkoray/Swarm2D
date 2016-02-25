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
using System.Reflection;
using Swarm2D.Engine.Core;
using Swarm2D.Library;

namespace Swarm2D.Engine.Logic
{
    public class ResponseData
    {
        internal short Id { get; set; }

        private static Dictionary<Type, short> _idsOfResponseDatas;
        private static Dictionary<short, Type> _typesOfResponseDatas;

        static ResponseData()
        {
            _idsOfResponseDatas = new Dictionary<Type, short>();
            _typesOfResponseDatas = new Dictionary<short, Type>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> responseDataTypes = new List<Type>();

            Debug.Log("Searching response datas...");

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (typeof(ResponseData).IsAssignableFrom(type))
                    {
                        Debug.Log("found response data: " + type.Name);

                        responseDataTypes.Add(type);
                    }
                }
            }

            Debug.Log("Response data searching is over..");

            foreach (Type responseDataType in responseDataTypes)
            {
                string name = responseDataType.FullName;
                short hashCode = IdTypeMap.GetHashCodeOf(name);

                _idsOfResponseDatas.Add(responseDataType, hashCode);
                _typesOfResponseDatas.Add(hashCode, responseDataType);
            }
        }

        internal static int GetResponseDataId(Type type)
        {
            return _idsOfResponseDatas[type];
        }

        public ResponseData()
        {
            Id = _idsOfResponseDatas[GetType()];
        }

        public static ResponseData CreateResponseDataWithId(short id)
        {
            return Activator.CreateInstance(_typesOfResponseDatas[id]) as ResponseData;
        }

        protected internal virtual void Serialize(IDataWriter writer)
        {

        }

        protected internal virtual void Deserialize(IDataReader reader)
        {

        }
    }
}
