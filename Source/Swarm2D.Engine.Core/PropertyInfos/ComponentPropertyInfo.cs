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
using System.Reflection;
using Swarm2D.Library;

namespace Swarm2D.Engine.Core
{
    public class ComponentPropertyInfo
    {
        public string Name { get; internal set; }
        public PropertyInfo PropertyInfo { get; internal set; }

        public ComponentPropertyType PropertyType { get; internal set; }

        public void SetValueTo(Component component, string value)
        {
            MethodInfo setMethod = PropertyInfo.GetSetMethod();

            switch (PropertyType)
            {
                case ComponentPropertyType.Boolean:
                    {
                        bool b = Convert.ToBoolean(value);
                        setMethod.Invoke(component, new object[] { b });
                    }
                    break;
                case ComponentPropertyType.Float:
                    {
                        float f = Convert.ToSingle(value);
                        setMethod.Invoke(component, new object[] { f });
                    }
                    break;
                case ComponentPropertyType.Int:
                    {
                        int i = Convert.ToInt32(value);
                        setMethod.Invoke(component, new object[] { i });
                    }
                    break;
                case ComponentPropertyType.Vector2:
                    {
                        string[] floats = value.Split(new char[] { ';' });

                        float x = Convert.ToSingle(floats[0]);
                        float y = Convert.ToSingle(floats[1]);

                        Vector2 vector2 = new Vector2(x, y);

                        setMethod.Invoke(component, new object[] { vector2 });
                    }
                    break;
                case ComponentPropertyType.Resource:
                    {
                        Resource resource = null;

                        if (value != "@NULL")
                        {
                            resource = Resource.GetResource(typeof(Resource), value);
                        }

                        if (resource != null)
                        {
                            setMethod.Invoke(component, new object[] { resource });
                        }
                    }
                    break;
                case ComponentPropertyType.Enumerator:
                    {
                        object enumValue = Enum.Parse(PropertyInfo.PropertyType, value);
                        setMethod.Invoke(component, new object[] { enumValue });
                    }
                    break;
                case ComponentPropertyType.Object:
                    {
                        ConstructorInfo constructorInfo = PropertyInfo.PropertyType.GetConstructor(new Type[] { });

                        ISerializableObject serializableObject = constructorInfo.Invoke(new object[] { }) as ISerializableObject;

                        DataReader dataReader = new DataReader(ByteOperations.ConvertToBytes(value));
                        serializableObject.OnDeserialize(dataReader);

                        setMethod.Invoke(component, new object[] { serializableObject });
                    }
                    break;
                default:
                    break;
            }
        }

        public void SetValueTo(Component component, object value)
        {
            MethodInfo setMethod = PropertyInfo.GetSetMethod();

            switch (PropertyType)
            {
                case ComponentPropertyType.Boolean:
                    {
                        setMethod.Invoke(component, new object[] { value });
                    }
                    break;
                case ComponentPropertyType.Float:
                    {
                        setMethod.Invoke(component, new object[] { value });
                    }
                    break;
                case ComponentPropertyType.Int:
                    {
                        setMethod.Invoke(component, new object[] { value });
                    }
                    break;
                case ComponentPropertyType.Vector2:
                    {
                        setMethod.Invoke(component, new object[] { value });
                    }
                    break;
                case ComponentPropertyType.Resource:
                    {
                        setMethod.Invoke(component, new object[] { value });
                    }
                    break;
                case ComponentPropertyType.Enumerator:
                    {
                        setMethod.Invoke(component, new object[] { value });
                    }
                    break;
                case ComponentPropertyType.Object:
                    {
                        setMethod.Invoke(component, new object[] { value });
                    }
                    break;
                default:
                    break;
            }
        }

        public string GetValueAsStringFrom(Component component)
        {
            string result = "";
            MethodInfo getMethod = PropertyInfo.GetGetMethod();

            switch (PropertyType)
            {
                case ComponentPropertyType.Boolean:
                    {
                        bool value = (bool)getMethod.Invoke(component, new object[] { });
                        result = value.ToString();
                    }
                    break;
                case ComponentPropertyType.Float:
                    {
                        float value = (float)getMethod.Invoke(component, new object[] { });
                        result = value.ToString();
                    }
                    break;
                case ComponentPropertyType.Int:
                    {
                        int value = (int)getMethod.Invoke(component, new object[] { });
                        result = value.ToString();
                    }
                    break;
                case ComponentPropertyType.Vector2:
                    {
                        Vector2 value = (Vector2)getMethod.Invoke(component, new object[] { });
                        result = value.X + ";" + value.Y;
                    }
                    break;
                case ComponentPropertyType.Resource:
                    {
                        Resource resource = getMethod.Invoke(component, new object[] { }) as Resource;
                        if (resource != null)
                        {
                            result = resource.Name;
                        }
                        else
                        {
                            result = "@NULL";
                        }
                    }
                    break;
                case ComponentPropertyType.Enumerator:
                    {
                        object enumValue = getMethod.Invoke(component, new object[] { });
                        string enumValueAsString = Enum.GetName(PropertyInfo.PropertyType, enumValue);
                        result = enumValueAsString;
                    }
                    break;
                case ComponentPropertyType.Object:
                    {
                        object objectValue = getMethod.Invoke(component, new object[] { });

                        ISerializableObject serializableObject = objectValue as ISerializableObject;

                        DataWriter writer = new DataWriter();
                        serializableObject.OnSerialize(writer);

                        byte[] dataAsBytes = writer.GetData();

                        result = ByteOperations.ConvertToString(dataAsBytes);
                    }
                    break;
                default:
                    break;
            }

            return result;
        }

        public object GetValueAsObjectFrom(Component component)
        {
            MethodInfo getMethod = PropertyInfo.GetGetMethod();

            object result = getMethod.Invoke(component, new object[] { });

            return result;
        }
    }

    public enum ComponentPropertyType
    {
        Boolean,
        Float,
        Int,
        Vector2,
        Resource,
        Enumerator,
        Object,
    }
}
