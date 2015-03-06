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
    public interface IDataWriter
    {
        void WriteInt32(int value);
        void WriteInt16(short value);
        void WriteFloat(float value);
        void WriteBool(bool value);
        void WriteByte(byte value);
        void WriteVector2(Vector2 vector2);
        void WriteUnicodeString(string value);
    }

    public class DataWriter : IDataWriter
    {
        List<Byte[]> _data;
        int totalSize = 0;

        public DataWriter()
        {
            _data = new List<byte[]>();
        }

        public void WriteInt32(int value)
        {
            byte[] valueArray = BitConverter.GetBytes(value);
            totalSize += valueArray.Length;
            _data.Add(valueArray);
        }

        public void WriteInt16(short value)
        {
            byte[] valueArray = BitConverter.GetBytes(value);
            totalSize += valueArray.Length;
            _data.Add(valueArray);
        }

        public void WriteUInt16(ushort value)
        {
            byte[] valueArray = BitConverter.GetBytes(value);
            totalSize += valueArray.Length;
            _data.Add(valueArray);
        }

        public void WriteFloat(float value)
        {
            byte[] valueArray = BitConverter.GetBytes(value);
            totalSize += valueArray.Length;
            _data.Add(valueArray);
        }

        public void WriteBool(bool value)
        {
            byte[] valueArray = BitConverter.GetBytes(value);
            totalSize += valueArray.Length;
            _data.Add(valueArray);
        }

        public void WriteByte(byte value)
        {
            totalSize += 1;
            _data.Add(new byte[] { value });
        }

        public void WriteVector2(Vector2 vector2)
        {
            WriteFloat(vector2.X);
            WriteFloat(vector2.Y);
        }

        public void WriteUnicodeString(string value)
        {
            if (value == null)
            {
                value = "";
            }

            byte[] valueArray = Encoding.Unicode.GetBytes(value);

            byte[] lengthArray = BitConverter.GetBytes((short)valueArray.Length);

            totalSize += lengthArray.Length;
            _data.Add(lengthArray);

            totalSize += valueArray.Length;
            _data.Add(valueArray);
        }

        public Byte[] GetData()
        {
            Byte[] data = new byte[totalSize];

            int k = 0;

            for (int i = 0; i < _data.Count; i++)
            {
                byte[] currentData = _data[i];

                for (int j = 0; j < currentData.Length; j++)
                {
                    data[k] = currentData[j];
                    k++;
                }
            }

            return data;
        }
    }
}
