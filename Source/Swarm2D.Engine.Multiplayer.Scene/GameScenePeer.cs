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
        public GameScene GameScene { get; set; }

        public GameObject Avatar { get; set; }

        internal bool HasSynchronizationJob { get; private set; }
        internal bool HasUnSynchronizationJob { get; private set; }

        private List<GameObjectGridCell> _lastSynchronizedGridCells;
        private GameObjectGridCell _lastCenterGridCell;

        private List<GameObjectServer> _destroyedGameObjects; //used by grid game object sync
        private List<GameObjectServer> _createdGameObjects; //used by grid game object sync

        private OnSynchronizeGameObjectToPeerMessage _message;

        protected override void OnAdded()
        {
            base.OnAdded();

            _destroyedGameObjects = new List<GameObjectServer>();
            _createdGameObjects = new List<GameObjectServer>();
            _lastSynchronizedGridCells = new List<GameObjectGridCell>();

            _message = new OnSynchronizeGameObjectToPeerMessage(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var lastSynchronizedGridCell in _lastSynchronizedGridCells)
            {
                lastSynchronizedGridCell.DeletePeer(this);
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
            if (Avatar != null)
            {
                GameObjectServer avatarGameObject = Avatar.GetComponent<GameObjectServer>();

                if (_lastCenterGridCell != avatarGameObject.CurrentGridCell)
                {
                    GameSceneServer gameSceneServer = this.GameScene.GetComponent<GameSceneServer>();

                    GameScenePeerGridUpdateData updateData = new GameScenePeerGridUpdateData();

                    updateData.CurrentGridCells = gameSceneServer.GetGridCellsAround(avatarGameObject.CurrentGridCell);
                    updateData.OldGridCells = gameSceneServer.GetGridCellsAround(_lastCenterGridCell);

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
            }

            return null;
        }

        internal void DoSynchronizationJob()
        {
            NetworkView gameSceneNetworkView = Avatar.Scene.GetComponent<NetworkView>();

            foreach (GameObjectServer gameObject in _createdGameObjects)
            {
                var networkMessage = new SynchronizeGameObjectMessage
                (
                    gameObject.PrefabNameForSynchronization,
                    gameObject.Entity.Name,
                    gameObject.GetComponent<NetworkView>().NetworkID,
                    gameObject.SceneEntity.LocalPosition,
                    gameObject.SceneEntity.LocalRotation
                );

                gameSceneNetworkView.NetworkEntityMessageEvent(Peer, networkMessage);
                gameObject.Entity.SendMessage(_message);
            }

            HasSynchronizationJob = false;
            _createdGameObjects.Clear();
        }

        internal void DoUnSynchronizationJob()
        {
            foreach (GameObjectServer gameObject in _destroyedGameObjects)
            {
                GameScene.GetComponent<GameSceneServer>().RemoveGameObjectFromPeer(this, gameObject);
            }

            HasUnSynchronizationJob = false;
            _destroyedGameObjects.Clear();
        }

        internal void AddGameObjectToSynchronizeJob(GameObjectServer gameObject)
        {
            HasSynchronizationJob = true;

            if (_destroyedGameObjects.Contains(gameObject))
            {
                _destroyedGameObjects.Remove(gameObject);
            }
            else
            {
                _createdGameObjects.Add(gameObject);
            }
        }

        internal void AddGameObjectToUnSynchronizeJob(GameObjectServer gameObject)
        {
            HasUnSynchronizationJob = true;
            _destroyedGameObjects.Add(gameObject);
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
