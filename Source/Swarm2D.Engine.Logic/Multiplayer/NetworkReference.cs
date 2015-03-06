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

namespace Swarm2D.Engine.Logic
{
    public struct NetworkReference<T> where T : Component
    {
        private readonly NetworkController _networkController;
        private readonly NetworkID _id;
        private T _target;

        public T Target
        {
            get
            {
                if (_target == null)
                {
                    NetworkView networkView = _networkController.FindNetworkView(_id);

                    if (networkView != null)
                    {
                        _target = networkView.GetComponent<T>();
                    }
                }

                return _target;
            }
        }

        public NetworkReference(NetworkController networkController, NetworkID id)
        {
            _target = null;
            _networkController = networkController;
            _id = id;
        }

        public NetworkReference(T networkObject)
        {
            NetworkView networkView = networkObject.GetComponent<NetworkView>();

            _target = networkObject;
            _networkController = networkView.NetworkController;
            _id = networkView.NetworkID;
        }
    }
}
