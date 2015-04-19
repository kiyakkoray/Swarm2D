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
    public class GameScenePeer : PeerComponent
    {
        private GameSceneServer _gameSceneServer;
        private GameScene _gameScene;
        private NetworkView _gameSceneNetworkView;

        public GameScene GameScene
        {
            get
            {
                return _gameScene;
            }
            internal set
            {
                _gameScene = value;
                _gameSceneServer = _gameScene.GetComponent<GameSceneServer>();
                _gameSceneNetworkView = _gameScene.GetComponent<NetworkView>();
            }
        }

        private GameObject _avatar;

        public GameObject Avatar
        {
            get
            {
                return _avatar;
            }
            set
            {
                Debug.Assert(value == null || !value.IsDestroyed, "Wrong Avatar Object Assigned!");

                _avatar = value;
            }
        }

        internal bool HasSynchronizationJob { get; private set; }
        internal bool HasUnSynchronizationJob { get; private set; }

        private List<GameObjectGridCell> _lastSynchronizedGridCells;
        private GameObjectGridCell _lastCenterGridCell;

        private List<GameObjectServer> _removedGameObjects; //used by grid game object sync
        private List<GameObjectServer> _addedGameObjects; //used by grid game object sync

        private OnSynchronizeGameObjectToPeerMessage _message;

#if DEBUG
        private List<GameObjectServer> _debugSynchronizedGameObjects;
#endif

        protected override void OnAdded()
        {
            base.OnAdded();

            _removedGameObjects = new List<GameObjectServer>();
            _addedGameObjects = new List<GameObjectServer>();
            _lastSynchronizedGridCells = new List<GameObjectGridCell>();

#if DEBUG
            _debugSynchronizedGameObjects = new List<GameObjectServer>();
#endif

            _message = new OnSynchronizeGameObjectToPeerMessage(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Debug.Assert(!_gameSceneServer.CurrentlySynchronizing, "This method must be called when there are no active synchronization is in progress.");

            foreach (var lastSynchronizedGridCell in _lastSynchronizedGridCells)
            {
                lastSynchronizedGridCell.OnPeerDestroyed(this);
            }

            if (Avatar != null)
            {
                Avatar.GetComponent<GameObjectServer>().RemoveTransformDirtyInformation();

                Avatar.Entity.Destroy();
                Avatar = null;
            }
        }

        internal GameScenePeerGridUpdateData UpdateOnGameObjectGrid()
        {
            Debug.Assert(_gameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");
            Debug.Assert(Avatar != null, "Peer's avatar must be set!");
            Debug.Assert(!Avatar.IsDestroyed, "Peer's avatar must not be destroyed!");

            GameObjectServer avatarGameObject = Avatar.GetComponent<GameObjectServer>();

            if (_lastCenterGridCell != avatarGameObject.CurrentGridCell)
            {
                GameScenePeerGridUpdateData updateData = new GameScenePeerGridUpdateData();

                updateData.CurrentGridCells = _gameSceneServer.GetGridCellsAround(avatarGameObject.CurrentGridCell);
                updateData.OldGridCells = _gameSceneServer.GetGridCellsAround(_lastCenterGridCell);

                updateData.CalculateAddedRemovedCells();

                foreach (var removedCell in updateData.RemovedGridCells)
                {
                    _lastSynchronizedGridCells.Remove(removedCell);
                    removedCell.RemovePeer(this);
                }

                foreach (var addedCell in updateData.AddedGridCells)
                {
                    _lastSynchronizedGridCells.Add(addedCell);
                    addedCell.AddPeer(this);
                }

                _lastCenterGridCell = avatarGameObject.CurrentGridCell;

                return updateData;
            }

            return null;
        }

        internal void DoSynchronizationJob()
        {
            Debug.Assert(_gameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");

            foreach (GameObjectServer gameObject in _addedGameObjects)
            {
                AddGameObjectToPeer(gameObject);
            }

            HasSynchronizationJob = false;
            _addedGameObjects.Clear();
        }

        internal void DoUnSynchronizationJob()
        {
            Debug.Assert(_gameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");

            foreach (GameObjectServer gameObject in _removedGameObjects)
            {
                RemoveGameObjectFromPeer(gameObject);
            }

            HasUnSynchronizationJob = false;
            _removedGameObjects.Clear();
        }

        private void AddGameObjectToPeer(GameObjectServer gameObject)
        {
            Debug.Assert(_gameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");

#if DEBUG
            Debug.Assert(!_debugSynchronizedGameObjects.Contains(gameObject), "Game object " + gameObject.Entity.Name + " is already synchronized to client.");
            _debugSynchronizedGameObjects.Add(gameObject);
#endif

            var networkMessage = new SynchronizeGameObjectMessage
            (
                gameObject.PrefabNameForSynchronization,
                gameObject.Entity.Name,
                gameObject.GetComponent<NetworkView>().NetworkID,
                gameObject.SceneEntity.LocalPosition,
                gameObject.SceneEntity.LocalRotation
            );

            _gameSceneNetworkView.NetworkEntityMessageEvent(Peer, networkMessage);
            gameObject.Entity.SendMessage(_message);
        }

        internal void RemoveGameObjectFromPeer(GameObjectServer gameObject)
        {
            Debug.Assert(_gameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");
            Debug.Assert(this.Avatar != gameObject.GameObject, "GameScenePeer has an avatar UnSynchronization job");

#if DEBUG
            Debug.Assert(_debugSynchronizedGameObjects.Contains(gameObject), "Game object " + gameObject.Entity.Name + " does not exist on client.");
            _debugSynchronizedGameObjects.Remove(gameObject);
#endif

#if DEBUG
            _gameSceneNetworkView.RPC(Peer, "RemoveGameObjectWithDebugName", gameObject.GetComponent<NetworkView>().NetworkID, gameObject.Entity.Name);
#else
            _gameSceneNetworkView.RPC(Peer, "RemoveGameObject", gameObject.GetComponent<NetworkView>().NetworkID);
#endif
        }

        internal void AddGameObjectToSynchronizeJob(GameObjectServer gameObject)
        {
            Debug.Assert(_gameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");
            Debug.Assert(!_addedGameObjects.Contains(gameObject), "Added GameObject " + gameObject.Entity.Name + " is already in _addedGameObjects list.");

            HasSynchronizationJob = true;

            if (_removedGameObjects.Contains(gameObject))
            {
                _removedGameObjects.Remove(gameObject);
            }
            else
            {
                _addedGameObjects.Add(gameObject);
            }
        }

        internal void AddGameObjectToUnSynchronizeJob(GameObjectServer gameObject)
        {
            Debug.Assert(_gameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");
            Debug.Assert(!_addedGameObjects.Contains(gameObject), "Removed gameObject " + gameObject.Entity.Name + " is in _addedGameObjects list.");
            Debug.Assert(!_removedGameObjects.Contains(gameObject), "Removed GameObject " + gameObject.Entity.Name + " is already in _removedGameObjects list.");

            HasUnSynchronizationJob = true;
            _removedGameObjects.Add(gameObject);
        }
    }

    internal class GameScenePeerGridUpdateData
    {
        internal GameObjectGridCell[] CurrentGridCells { get; set; }
        internal GameObjectGridCell[] OldGridCells { get; set; }

        internal GameObjectGridCell[] RemovedGridCells { get; private set; }
        internal GameObjectGridCell[] AddedGridCells { get; private set; }

        internal void CalculateAddedRemovedCells()
        {
            RemovedGridCells = OldGridCells.Except(CurrentGridCells).ToArray();
            AddedGridCells = CurrentGridCells.Except(OldGridCells).ToArray();
        }
    }

    internal class SynchronizeGameObjectMessage : NetworkEntityMessage
    {
        internal string PrefabNameForSynchronization { get; private set; }
        public string Name { get; private set; }
        public NetworkID NetwokID { get; private set; }
        public Vector2 LocalPosition { get; private set; }
        public float LocalRotation { get; private set; }

        public SynchronizeGameObjectMessage()
        {

        }

        public SynchronizeGameObjectMessage
            (string prefabNameForSynchronization, string name, NetworkID networkId, Vector2 localPosition, float localRotation)
        {
            PrefabNameForSynchronization = prefabNameForSynchronization;
            Name = name;
            NetwokID = networkId;
            LocalPosition = localPosition;
            LocalRotation = localRotation;
        }

        protected override void OnSerialize(IDataWriter writer)
        {
            writer.WriteUnicodeString(PrefabNameForSynchronization);
            writer.WriteUnicodeString(Name);
            writer.WriteNetworkID(NetwokID);
            writer.WriteVector2(LocalPosition);
            writer.WriteFloat(LocalRotation);
        }

        protected override void OnDeserialize(IDataReader reader)
        {
            PrefabNameForSynchronization = reader.ReadUnicodeString();
            Name = reader.ReadUnicodeString();
            NetwokID = reader.ReadNetworkID();
            LocalPosition = reader.ReadVector2();
            LocalRotation = reader.ReadFloat();
        }
    }
}
