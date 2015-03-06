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
using System.Text;
using Swarm2D.Library;

namespace Swarm2D.Network.TCPDriver
{
    internal class NetworkMessage : IDataWriter, IDataReader
    {
        private static List<NetworkMessage> _networkMessages;

        static NetworkMessage()
        {
            _networkMessages = new List<NetworkMessage>(128);

            for (int i = 0; i < 128; i++)
            {
                _networkMessages.Add(new NetworkMessage());
            }
        }

        public static NetworkMessage GetNetworkMessage()
        {
            NetworkMessage result = null;

            lock (_networkMessages)
            {
                if (_networkMessages.Count > 0)
                {
                    result = _networkMessages[_networkMessages.Count - 1];
                    _networkMessages.RemoveAt(_networkMessages.Count - 1);
                }
            }

            if (result == null)
            {
                Debug.Log("No networkMessage left on buffer list, creating new one");
                result = new NetworkMessage();
            }

            return result;
        }

        public static void FreeNetworkMessage(NetworkMessage networkMessage)
        {
            networkMessage.Reset();

            lock (_networkMessages)
            {
                _networkMessages.Add(networkMessage);
            }
        }

        public byte[] Buffer { get; private set; }

        public const int MaximumMessageSize = 1280;
        public const int MessageHeaderSize = 2;

        private NetworkMessage()
        {
            Buffer = new byte[MaximumMessageSize + MessageHeaderSize];
        }

        public void BeginWrite()
        {
            Size = 0;
        }

        public void EndWrite()
        {
            Buffer[1] = (byte)(Size >> 8);
            Buffer[0] = (byte)(Size);
        }

        public void BeginRead()
        {
            Cursor = 0;
        }

        public void WriteUnicodeString(string s)
        {
            int count = Encoding.Unicode.GetBytes(s, 0, s.Length, Buffer, Size + MessageHeaderSize + sizeof(short));

            WriteInt16((short)count);

            Size += count;
        }

        public string ReadUnicodeString()
        {
            short size = ReadInt16();

            string result = Encoding.Unicode.GetString(Buffer, Cursor + MessageHeaderSize, size);

            Cursor += size;

            return result;
        }

        public void WriteByte(byte b)
        {
            Buffer[Size + MessageHeaderSize + 0] = b;

            Size += sizeof(byte);
        }

        public byte ReadByte()
        {
            byte b = Buffer[Cursor + MessageHeaderSize];

            Cursor += sizeof(byte);

            return b;
        }

        public void WriteBool(bool b)
        {
            Buffer[Size + MessageHeaderSize + 0] = (byte)(b ? 1 : 0);

            Size += sizeof(byte);
        }

        public bool ReadBool()
        {
            byte b = Buffer[Cursor + MessageHeaderSize];

            Cursor += sizeof(byte);

            return b == 1;
        }

        public void WriteInt16(short s)
        {
            Buffer[Size + MessageHeaderSize + 0] = (byte)s;
            Buffer[Size + MessageHeaderSize + 1] = (byte)(s >> 8);

            Size += sizeof(short);
        }

        public short ReadInt16()
        {
            short s = BitConverter.ToInt16(Buffer, Cursor + MessageHeaderSize);

            Cursor += sizeof(short);

            return s;
        }

        public void WriteInt32(int i)
        {
            Buffer[Size + MessageHeaderSize + 0] = (byte)i;
            Buffer[Size + MessageHeaderSize + 1] = (byte)(i >> 8);
            Buffer[Size + MessageHeaderSize + 2] = (byte)(i >> 16);
            Buffer[Size + MessageHeaderSize + 3] = (byte)(i >> 24);

            Size += sizeof(int);
        }

        public int ReadInt32()
        {
            int i = BitConverter.ToInt32(Buffer, Cursor + MessageHeaderSize);

            Cursor += sizeof(int);

            return i;
        }

        public void WriteFloat(float f)
        {
            //TODO you must fix this!!!!

            byte[] floatArray = BitConverter.GetBytes(f);

            //Buffer[Size + MessageHeaderSize + 0] = (byte)f;
            //Buffer[Size + MessageHeaderSize + 1] = (byte)(f >> 8);
            //Buffer[Size + MessageHeaderSize + 2] = (byte)(f >> 16);
            //Buffer[Size + MessageHeaderSize + 3] = (byte)(f >> 24);

            Buffer[Size + MessageHeaderSize + 0] = floatArray[0];
            Buffer[Size + MessageHeaderSize + 1] = floatArray[1];
            Buffer[Size + MessageHeaderSize + 2] = floatArray[2];
            Buffer[Size + MessageHeaderSize + 3] = floatArray[3];

            Size += sizeof(float);
        }

        public float ReadFloat()
        {
            float f = BitConverter.ToSingle(Buffer, Cursor + MessageHeaderSize);

            Cursor += sizeof(float);

            return f;
        }

        public void WriteVector2(Vector2 vector2)
        {
            WriteFloat(vector2.X);
            WriteFloat(vector2.Y);
        }

        public Vector2 ReadVector2()
        {
            Vector2 result = new Vector2();

            result.X = ReadFloat();
            result.Y = ReadFloat();

            return result;
        }

        public void PrepareMessage(int size) 
        {
            Size = size - MessageHeaderSize;
        }

        public void Reset()
        {
            Cursor = 0;
            Size = 0;
        }

        public void ResetSizeTo(int newSize)
        {
            Size = newSize;
        }

        public int Cursor { get; private set; }

        public int Size { get; private set; }

        public int EmptySize
        {
            get
            {
                return MaximumMessageSize - Size;
            }
        }
    }
}
