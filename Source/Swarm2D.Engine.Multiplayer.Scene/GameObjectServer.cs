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

        public GameObjectGridCell CurrentGridCell { get; private set; }

        public LinkedListNode<GameObjectServer> NodeOnDirtyTransformList { get; private set; }

        public bool IsCustomSynchronization { get; set; }

        public bool NotSynchronizedOnNewGridCell { get; internal set; }

        public PeerGroup PeerGroup
        {
            get
            {
                if (CurrentGridCell == null)
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
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            MakeTransformDirty();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!NotSynchronizedOnNewGridCell)
            {
                foreach (Peer peer in PeerGroup.Peers)
                {
                    _gameSceneServer.RemoveGameObjectFromPeer(_gameSceneServer.GetGameScenePeerOfPeer(peer), this);
                }
            }

            this.RemoveTransformDirtyInformation();
            _gameSceneServer.OnGameObjectDestroyed(this);
        }

        [EntityMessageHandler(MessageType = typeof(SceneEntityTransformMatrixChangeMesssage))]
        private void OnEntityTransformMatrixChange(Message message)
        {
            MakeTransformDirty();
        }

        internal void UpdateOnGrid()
        {
            int newGridX = (int)(SceneEntity.LocalPosition.X / _gameSceneServer.Length);
            int newGridY = (int)(SceneEntity.LocalPosition.Y / _gameSceneServer.Length);

            GameObjectGridCell newGridCell = _gameSceneServer[newGridX, newGridY];

            if (newGridCell != CurrentGridCell)
            {
                _gameSceneServer.UpdateGameObjectGridCell(CurrentGridCell, newGridCell, this);

                CurrentGridCell = newGridCell;
                NotSynchronizedOnNewGridCell = true;
            }

            RemoveTransformDirtyInformation();
        }

        internal void MakeTransformDirty()
        {
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
