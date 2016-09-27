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
using System.Text;
using Swarm2D.Library;

namespace Swarm2D.Engine.Core
{
    public abstract class Resource
    {
        private static List<Resource> _resources = new List<Resource>();

        private static Dictionary<Type, Dictionary<string, Resource>> _resourcesWithTypes = new Dictionary<Type, Dictionary<string, Resource>>();

        public string Name { get; private set; }

        protected Resource(string name)
        {
            Name = name;

            AddResource(this);
        }

        private static void AddResource(Type type, Resource resource)
        {
            EnsureType(type);

            if (_resourcesWithTypes[type].ContainsKey(resource.Name))
            {
                _resourcesWithTypes[type][resource.Name] = resource;
            }
            else
            {
                _resourcesWithTypes[type].Add(resource.Name, resource);
            }

            if (typeof(Resource) != type && typeof(Resource).IsAssignableFrom(PlatformHelper.GetBaseType(type)))
            {
                AddResource(PlatformHelper.GetBaseType(type), resource);
            }
        }

        private static void AddResource(Resource resource)
        {
            _resources.Add(resource);

            AddResource(resource.GetType(), resource);
        }

        public static T GetResource<T>(string name) where T : Resource
        {
            EnsureType(typeof(T));

            Dictionary<string, Resource> resourceTypes = _resourcesWithTypes[typeof(T)];

            if (resourceTypes.ContainsKey(name))
            {
                return resourceTypes[name] as T;
            }

            return null;
        }

        public static Resource GetResource(Type type, string name)
        {
            EnsureType(type);

            Dictionary<string, Resource> resourceTypes = _resourcesWithTypes[type];

            if (resourceTypes.ContainsKey(name))
            {
                return resourceTypes[name];
            }

            return null;
        }

        private static void EnsureType(Type type)
        {
            if (!_resourcesWithTypes.ContainsKey(type))
            {
                _resourcesWithTypes.Add(type, new Dictionary<string, Resource>());
            }
        }

        public static string GenerateName<T>() where T : Resource
        {
            EnsureType(typeof(T));

            Dictionary<string, Resource> resourceTypes = _resourcesWithTypes[typeof(T)];

            return typeof(T).FullName + resourceTypes.Count;
        }
    }
}
