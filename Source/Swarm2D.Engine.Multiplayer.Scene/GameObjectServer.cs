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
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.Engine.Multiplayer.Scene
{
    public class GameObjectServer : SceneEntityComponent
    {
        private bool _prefabNameForSynchronizationSet = false;
        private string _prefabNameForSynchronization;

        public string PrefabNameForSynchronization
        {
            get
            {
                if (!_prefabNameForSynchronizationSet)
                {
                    return Entity.PrefabName;
                }

                return _prefabNameForSynchronization;
            }
            set
            {
                _prefabNameForSynchronizationSet = true;
                _prefabNameForSynchronization = value;
            }
        }

        private GameScene _gameScene;
        private GameSceneServer _gameSceneServer;

        public GameObject GameObject { get; private set; }

        public bool IsTransformDirty { get; private set; }

        private bool _synchronizedAtLeastOneTime = false;

        private GameObjectGridCell _currentGridCell;

        public GameObjectGridCell CurrentGridCell
        {
            get
            {
                return _currentGridCell;
            }
            private set
            {
                Debug.Assert(value != null, "New grid cell must not be null!");

                _currentGridCell = value;
                _synchronizedAtLeastOneTime = true;
            }
        }

        public LinkedListNode<GameObjectServer> NodeOnDirtyTransformList { get; private set; }

        public bool IsCustomSynchronization { get; set; }

        public PeerGroup PeerGroup
        {
            get
            {
                if (!_synchronizedAtLeastOneTime)
                {
                    return null;
                }

                return CurrentGridCell.PeerGroup;
            }
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            IsTransformDirty = false;
            IsCustomSynchronization = false;

            _gameScene = Scene.GetComponent<GameScene>();

            _gameSceneServer = _gameScene.GetComponent<GameSceneServer>();
            GameObject = GetComponent<GameObject>();

            Debug.Assert(!_gameSceneServer.CurrentlySynchronizing, "This method must be called when there are no active synchronization is in progress.");
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Debug.Assert(!_gameSceneServer.CurrentlySynchronizing, "This method must be called when there are no active synchronization is in progress.");

            MakeTransformDirty();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Debug.Assert(!_gameSceneServer.CurrentlySynchronizing, "This method must be called when there are no active synchronization is in progress.");

            if (_synchronizedAtLeastOneTime)
            {
                foreach (Peer peer in PeerGroup.Peers)
                {
                    GameScenePeer gameScenePeer = _gameSceneServer.GetGameScenePeerOfPeer(peer);
                    gameScenePeer.RemoveGameObjectFromPeer(this);
                }

                CurrentGridCell.OnGameObjectDestroyed(this);
            }

            this.RemoveTransformDirtyInformation();
        }

        [EntityMessageHandler(MessageType = typeof(SceneEntityTransformMatrixChangeMesssage))]
        private void OnEntityTransformMatrixChange(Message message)
        {
            Debug.Assert(!_gameSceneServer.CurrentlySynchronizing, "This method must be called when there are no active synchronization is in progress.");

            MakeTransformDirty();
        }

        internal void UpdateOnGrid()
        {
            Debug.Assert(_gameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");

            int newGridX = (int)(SceneEntity.LocalPosition.X / _gameSceneServer.Length);
            int newGridY = (int)(SceneEntity.LocalPosition.Y / _gameSceneServer.Length);

            GameObjectGridCell newGridCell = _gameSceneServer[newGridX, newGridY];

            if (newGridCell != CurrentGridCell)
            {
                if (CurrentGridCell != null)
                {
                    CurrentGridCell.RemoveGameObject(this);
                }

                newGridCell.AddGameObject(this);

                CurrentGridCell = newGridCell;
            }

            RemoveTransformDirtyInformation();
        }

        private void MakeTransformDirty()
        {
            Debug.Assert(!_gameSceneServer.CurrentlySynchronizing, "This method must be called when there are no active synchronization is in progress.");

            if (!IsTransformDirty && _gameScene != null)
            {
                IsTransformDirty = true;
                NodeOnDirtyTransformList = _gameSceneServer.OnGameObjectTransformDirty(this);
            }
        }

        internal void RemoveTransformDirtyInformation()
        {
            if (IsTransformDirty)
            {
                _gameSceneServer.OnGameObjectTransformClean(this);
                NodeOnDirtyTransformList = null;
                IsTransformDirty = false;
            }
        }
    }
}
