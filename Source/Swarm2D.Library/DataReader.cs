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

namespace Swarm2D.Library
{
    public interface IDataReader
    {
        int ReadInt32();
        short ReadInt16();
        float ReadFloat();
        bool ReadBool();
        byte ReadByte();
        Vector2 ReadVector2();
        string ReadUnicodeString();
    }

    public class DataReader : IDataReader
    {
        int currentIndex = 0;
        byte[] data;

        public DataReader(byte[] data)
        {
            this.data = data;
        }

        public int ReadInt32()
        {
            int value = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;
            return value;
        }

        public byte ReadByte()
        {
            byte value = data[currentIndex];
            currentIndex += 1;
            return value;
        }

        public uint ReadUInt32()
        {
            uint value = BitConverter.ToUInt32(data, currentIndex);
            currentIndex += 4;
            return value;
        }

        public ushort ReadUInt16()
        {
            ushort value = BitConverter.ToUInt16(data, currentIndex);
            currentIndex += 2;
            return value;
        }

        public short ReadInt16()
        {
            short value = BitConverter.ToInt16(data, currentIndex);
            currentIndex += 2;
            return value;
        }

        public bool ReadBool()
        {
            bool value = BitConverter.ToBoolean(data, currentIndex);
            currentIndex += 1;
            return value;
        }

        public float ReadFloat()
        {
            float value = BitConverter.ToSingle(data, currentIndex);
            currentIndex += 4;
            return value;
        }

        public Vector2 ReadVector2()
        {
            Vector2 result = new Vector2();

            result.X = ReadFloat();
            result.Y = ReadFloat();

            return result;
        }

        public string ReadUTF8String()
        {
            int byteCount = ReadInt32();
        
            byte[] byteArray = new byte[byteCount];
        
            for (int i = 0; i < byteCount; i++)
            {
                byteArray[i] = ReadByte();
            }

            string value = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
        
            return value;
        }

        public string ReadUnicodeString()
        {
            short byteCount = ReadInt16();

            byte[] byteArray = new byte[byteCount];

            for (int i = 0; i < byteCount; i++)
            {
                byteArray[i] = ReadByte();
            }

            string value = Encoding.Unicode.GetString(byteArray, 0, byteArray.Length);

            return value;
        }

        //public string ReadAsciiString(int length)
        //{
        //    string value = Encoding.ASCII.GetString(data, currentIndex, length);
        //    currentIndex += length;
        //    return value;
        //}

        public int CurrentIndex
        {
            get
            {
                return currentIndex;
            }
            set
            {
                currentIndex = value;
            }
        }
    }

    public static class ByteOperations
    {
        public static string ConvertToString(byte[] array)
        {
            string result = array.Length.ToString();

            for (int i = 0; i < array.Length; i++)
            {
                result += ".";
                result += array[i].ToString();
            }

            return result;
        }

        public static byte[] ConvertToBytes(string dataAsString)
        {
            string[] values = dataAsString.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            int size = Convert.ToInt32(values[0]);

            byte[] result = new byte[size];

            for (int i = 1; i < values.Length; i++)
            {
                result[i - 1] = Convert.ToByte(values[i]);
            }

            return result;
        }
    }
}
