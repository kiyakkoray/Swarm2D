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
using Swarm2D.Engine.Core;
using Swarm2D.Library;

namespace Swarm2D.Engine.Logic
{
    public struct NetworkReference<T> where T : Component
    {
        private readonly NetworkController _networkController;
        private readonly NetworkID _id;
        private WeakReference _reference;
        private bool _invalid;

        public bool IsValid
        {
            get { return !_invalid; }
        }

        public NetworkID Id
        {
            get { return _id; }
        }

        public T Target
        {
            get
            {
                if (_invalid)
                {
                    return null;
                }

                RefreshTarget();

                if (_reference.Target != null)
                {
                    T target = (T)_reference.Target;

                    if (target.Entity.IsDestroyed)
                    {
                        _reference.Target = null;

                        RefreshTarget();
                    }
                }

                return (T)_reference.Target;
            }
        }

        private void RefreshTarget()
        {
            if (_reference.Target == null)
            {
                NetworkView networkView = _networkController.FindNetworkView(_id);

                if (networkView != null)
                {
                    _reference.Target = networkView.GetComponent<T>();
                }
            }
        }

        public NetworkReference(NetworkController networkController, NetworkID id)
        {
            if (id == null)
            {
                _invalid = true;

                _reference = null;
                _networkController = null;
                _id = null;
            }
            else
            {
                _reference = new WeakReference(null);
                _networkController = networkController;
                _id = id;
                _invalid = false;
            }
        }

        public NetworkReference(T networkObject)
        {
            if (networkObject == null)
            {
                _invalid = true;

                _reference = null;
                _networkController = null;
                _id = null;
            }
            else
            {
                _invalid = false;

                NetworkView networkView = networkObject.GetComponent<NetworkView>();

                _reference = new WeakReference(networkObject);
                _networkController = networkView.NetworkController;
                _id = networkView.NetworkID;
            }
        }

        public static implicit operator T(NetworkReference<T> networkReference)
        {
            if (networkReference._invalid)
            {
                return null;
            }

            return networkReference.Target;
        }

        public static NetworkReference<T> Invalid
        {
            get
            {
                return new NetworkReference<T>(null);
            }
        }
    }
}
