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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Library;
using System.IO;
using Swarm2D.Network;
using Debug = Swarm2D.Library.Debug;

namespace Swarm2D.Engine.Logic
{
    static class NetworkMessageDataHelper
    {
        private static byte GetIdOfType(Type type)
        {
            byte result = 0;

            if (type == typeof(byte))
            {
                result = 1;
            }
            else if (type == typeof(short))
            {
                result = 2;
            }
            else if (type == typeof(int))
            {
                result = 3;
            }
            else if (type == typeof(string))
            {
                result = 4;
            }
            else if (type == typeof(float))
            {
                result = 5;
            }
            else if (type == typeof(Vector2))
            {
                result = 6;
            }
            else if (type == typeof(NetworkID))
            {
                result = 7;
            }
            else if (type == typeof(bool))
            {
                result = 8;
            }

            return result;
        }

        private static Type GetTypeOfId(byte typeId)
        {
            Type result = null;

            if (typeId == 1)
            {
                result = typeof(byte);
            }
            else if (typeId == 2)
            {
                result = typeof(short);
            }
            else if (typeId == 3)
            {
                result = typeof(int);
            }
            else if (typeId == 4)
            {
                result = typeof(string);
            }
            else if (typeId == 5)
            {
                result = typeof(float);
            }
            else if (typeId == 6)
            {
                result = typeof(Vector2);
            }
            else if (typeId == 7)
            {
                result = typeof(NetworkID);
            }
            else if (typeId == 8)
            {
                result = typeof(bool);
            }

            return result;
        }

        private static void WriteWithType(this IDataWriter writer, object parameter)
        {
            Type typeOfObject = parameter.GetType();

            writer.WriteByte(GetIdOfType(typeOfObject));

            if (typeOfObject == typeof(byte))
            {
                writer.WriteByte((byte)parameter);
            }
            else if (typeOfObject == typeof(short))
            {
                writer.WriteInt16((short)parameter);
            }
            else if (typeOfObject == typeof(int))
            {
                writer.WriteInt32((int)parameter);
            }
            else if (typeOfObject == typeof(string))
            {
                writer.WriteUnicodeString((string)parameter);
            }
            else if (typeOfObject == typeof(float))
            {
                writer.WriteFloat((float)parameter);
            }
            else if (typeOfObject == typeof(Vector2))
            {
                writer.WriteVector2((Vector2)parameter);
            }
            else if (typeOfObject == typeof(NetworkID))
            {
                writer.WriteNetworkID((NetworkID)parameter);
            }
            else if (typeOfObject == typeof(bool))
            {
                writer.WriteBool((bool)parameter);
            }
        }

        private static object ReadWithType(this IDataReader reader)
        {
            object result = null;

            byte typeId = reader.ReadByte();
            Type typeOfObject = GetTypeOfId(typeId);

            if (typeOfObject == typeof(byte))
            {
                result = reader.ReadByte();
            }
            else if (typeOfObject == typeof(short))
            {
                result = reader.ReadInt16();
            }
            else if (typeOfObject == typeof(int))
            {
                result = reader.ReadInt32();
            }
            else if (typeOfObject == typeof(string))
            {
                result = reader.ReadUnicodeString();
            }
            else if (typeOfObject == typeof(float))
            {
                result = reader.ReadFloat();
            }
            else if (typeOfObject == typeof(Vector2))
            {
                result = reader.ReadVector2();
            }
            else if (typeOfObject == typeof(NetworkID))
            {
                result = reader.ReadNetworkID();
            }
            else if (typeOfObject == typeof(bool))
            {
                result = reader.ReadBool();
            }

            return result;
        }

        public static void WriteRPCEvent(this IDataWriter writer, NetworkID networkId, string methodName, object[] parameters)
        {
            writer.WriteByte((byte)NetworkEventType.RPC);

            writer.WriteNetworkID(networkId);
            //message.WriteInt(message.GetHashCode());
            writer.WriteUnicodeString(methodName);

            writer.WriteByte((byte)parameters.Length);

            for (int i = 0; i < parameters.Length; i++)
            {
                object parameter = parameters[i];

                writer.WriteWithType(parameter);
            }
        }

        public static void ReadRPCEvent(this IDataReader reader, out NetworkID networkId, out string methodName, out Type[] types, out object[] parameters)
        {
            networkId = reader.ReadNetworkID();

            //int methodNameHash = message.ReadInt();
            methodName = reader.ReadUnicodeString();
            byte parameterCount = reader.ReadByte();

            List<object> parametersAsList = new List<object>(16);
            List<Type> typesAsList = new List<Type>(16);

            for (int i = 0; i < parameterCount; i++)
            {
                object parameter = reader.ReadWithType();

                typesAsList.Add(parameter.GetType());
                parametersAsList.Add(parameter);
            }

            types = typesAsList.ToArray();
            parameters = parametersAsList.ToArray();
        }

        public static void WriteNetworkEntityMessageEvent(this IDataWriter writer, NetworkID networkId, NetworkEntityMessage networkEntityMessage)
        {
            writer.WriteByte((byte)NetworkEventType.NetworkEntityMessage);

            writer.WriteNetworkID(networkId);
            writer.WriteInt16(networkEntityMessage.Id);

            writer.WriteByte((byte)networkEntityMessage.Type);

            if (networkEntityMessage.Type != NetworkEntityMessageType.Plain)
            {
                writer.WriteInt16(networkEntityMessage.RequestResponseId);
            }

            DataWriter dataWriter = new DataWriter();

            networkEntityMessage.Serialize(dataWriter);
            Byte[] data = dataWriter.GetData();

            writer.WriteInt16((short)data.Length);

            for (int i = 0; i < data.Length; i++)
            {
                writer.WriteByte(data[i]);
            }
        }

        public static void ReadNetworkEntityMessageEvent(this IDataReader reader, out NetworkID networkId,
            out NetworkEntityMessage networkEntityMessage)
        {
            networkId = reader.ReadNetworkID();
            short messageId = reader.ReadInt16();

            NetworkEntityMessageType networkEntityMessageType = (NetworkEntityMessageType)reader.ReadByte();
            short requestResponseId = -1;

            if (networkEntityMessageType != NetworkEntityMessageType.Plain)
            {
                requestResponseId = reader.ReadInt16();
            }

            short messageLength = reader.ReadInt16();

            byte[] data = new byte[messageLength];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = reader.ReadByte();
            }

            DataReader messageReader = new DataReader(data);

            networkEntityMessage = Message.CreateMessageWithId(messageId) as NetworkEntityMessage;
            networkEntityMessage.Type = networkEntityMessageType;
            networkEntityMessage.RequestResponseId = requestResponseId;

            networkEntityMessage.Deserialize(messageReader);
        }

        public static void WriteResponseNetworkEntityMessageEvent(this IDataWriter writer, short responseId,
            ResponseData responseData)
        {
            writer.WriteByte((byte)NetworkEventType.NetworkEntityMessageResponse);

            writer.WriteInt16(responseData.Id);
            writer.WriteInt16(responseId);

            DataWriter dataWriter = new DataWriter();

            responseData.Serialize(dataWriter);
            Byte[] data = dataWriter.GetData();

            writer.WriteInt16((short)data.Length);

            for (int i = 0; i < data.Length; i++)
            {
                writer.WriteByte(data[i]);
            }
        }

        public static void ReadResponseNetworkEntityMessageEvent(this IDataReader reader, out short responseId, out ResponseData responseData)
        {
            short responseDataId = reader.ReadInt16();
            responseId = reader.ReadInt16();

            short messageLength = reader.ReadInt16();

            byte[] data = new byte[messageLength];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = reader.ReadByte();
            }

            DataReader messageReader = new DataReader(data);

            responseData = ResponseData.CreateResponseDataWithId(responseDataId);
            responseData.Deserialize(messageReader);
        }

        public static void WriteSynchronizeEvent(this IDataWriter writer, NetworkID networkId, byte[] data)
        {
            writer.WriteByte((byte)NetworkEventType.Synchronize);
            writer.WriteNetworkID(networkId);

            writer.WriteInt16((short)data.Length);

            for (int i = 0; i < data.Length; i++)
            {
                writer.WriteByte(data[i]);
            }
        }

        public static void ReadSynchronizeEvent(this IDataReader reader, out NetworkID networkId, out byte[] data)
        {
            networkId = reader.ReadNetworkID();

            short dataLength = reader.ReadInt16();

            data = new byte[dataLength];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = reader.ReadByte();
            }
        }
    }
}
