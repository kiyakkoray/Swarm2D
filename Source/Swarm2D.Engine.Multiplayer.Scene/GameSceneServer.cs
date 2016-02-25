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
    public class GameSceneServer : SceneController
    {
        private float _length;
        private float _inverseLength;

        public float Length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
                _inverseLength = 1.0f / _length;
            }
        }

        public float InverseLength
        {
            get
            {
                return _inverseLength;
            }
        }

        public int Size = 64;

        private GameObjectGridCell[,] _grid;

        private GameObjectGridCell _outterGridCell;

        private List<GameObjectGridCell> _allCells;
        private List<GameObjectGridCell> _cellsWithJob;

        private NetworkView _networkView;
        private NetworkController _networkController;

        private List<GameScenePeer> _peersWithUnSynchronizationJob;
        private List<GameScenePeer> _peersWithSynchronizationJob;
        private List<GameScenePeer> _players;

        private LinkedList<GameObjectServer> _gameObjectsWithDirtyTransform;

        public GameScene GameScene { get; private set; }

        private long _synchronizationTick = 0;

        internal bool CurrentlySynchronizing { get; private set; }

        public IEnumerable<GameScenePeer> Players
        {
            get
            {
                return _players.AsReadOnly();
            }
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Length = 256.0f;

            _networkView = GetComponent<NetworkView>();
            GameScene = GetComponent<GameScene>();

            _networkController = _networkView.NetworkController;

            _players = new List<GameScenePeer>();
            _peersWithUnSynchronizationJob = new List<GameScenePeer>();
            _peersWithSynchronizationJob = new List<GameScenePeer>();
            _cellsWithJob = new List<GameObjectGridCell>();
            _grid = new GameObjectGridCell[Size, Size];
            _allCells = new List<GameObjectGridCell>();
            _outterGridCell = new GameObjectGridCell(this);
            _gameObjectsWithDirtyTransform = new LinkedList<GameObjectServer>();

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    GameObjectGridCell gameObjectGridCell = new GameObjectGridCell(this, i, j);

                    _grid[i, j] = gameObjectGridCell;
                    _allCells.Add(gameObjectGridCell);
                }
            }
        }

        [EntityMessageHandler(MessageType = typeof(SceneControllerUpdateMessage))]
        private void OnUpdate(Message message)
        {
            if (DoesSceneNeedsSynchronization())
            {
                DoSynchronizeJob();
            }
        }

        private bool DoesSceneNeedsSynchronization()
        {
            if (_networkController.SessionUpdateCheckType == NetworkUpdateCheckType.Time)
            {
                _synchronizationTick++;

                return _synchronizationTick % 24 == 0;
            }

            if (_networkController.SessionUpdateCheckType == NetworkUpdateCheckType.Frame)
            {
                if (GameLogic.ExecutedFrame - _synchronizationTick >= _networkController.SessionUpdatePeriod)
                {
                    _synchronizationTick = GameLogic.ExecutedFrame;

                    return true;
                }

                return false;
            }

            return false;
        }

        private void DoSynchronizeJob()
        {
            CurrentlySynchronizing = true;

            while (_gameObjectsWithDirtyTransform.Count > 0)
            {
                GameObjectServer gameObject = _gameObjectsWithDirtyTransform.First.Value;

                gameObject.UpdateOnGrid();
            }

            foreach (GameScenePeer peer in _players)
            {
                GameScenePeerGridUpdateData updateData = peer.UpdateOnGameObjectGrid();

                if (updateData != null)
                {
                    GameObject avatar = peer.Avatar;
                    GameObjectServer avatarGameObject = avatar.GetComponent<GameObjectServer>();

                    Entity.SendMessage(new OnSynchronizeGameSceneToPeerMessage(peer, avatarGameObject, updateData.RemovedGridCells, updateData.AddedGridCells));
                }
            }

            foreach (var gameObjectGridCell in _cellsWithJob)
            {
                gameObjectGridCell.DoUnSynchronizationJob();
            }

            foreach (var gameObjectGridCell in _cellsWithJob)
            {
                gameObjectGridCell.DoSynchronizationJob();
            }

            _cellsWithJob.Clear();

            foreach (GameScenePeer gameScenePeer in _peersWithUnSynchronizationJob)
            {
                gameScenePeer.DoUnSynchronizationJob();
            }

            _peersWithUnSynchronizationJob.Clear();

            foreach (GameScenePeer gameScenePeer in _peersWithSynchronizationJob)
            {
                gameScenePeer.DoSynchronizationJob();
            }

            _peersWithSynchronizationJob.Clear();

            CurrentlySynchronizing = false;
        }

        public void EnterPeerToScene(Peer peer)
        {
            Debug.Assert(!CurrentlySynchronizing, "This method must be called when there are no active synchronization is in progress.");

            GameScenePeer gameScenePeer = peer.AddComponent<GameScenePeer>();
            gameScenePeer.GameScene = GameScene;

            _players.Add(gameScenePeer);

            Entity.SendMessage(new PeerEnteredToSceneMessage(peer));
        }
        public void RemovePeerFromScene(GameScenePeer gameScenePeer)
        {
            Debug.Assert(!CurrentlySynchronizing, "This method must be called when there are no active synchronization is in progress.");

            Entity.SendMessage(new PeerRemovedFromSceneMessage(gameScenePeer));

            gameScenePeer.Entity.DeleteComponent(gameScenePeer);

            _players.Remove(gameScenePeer);

            if (_peersWithSynchronizationJob.Contains(gameScenePeer))
            {
                _peersWithSynchronizationJob.Remove(gameScenePeer);
            }

            if (_peersWithUnSynchronizationJob.Contains(gameScenePeer))
            {
                _peersWithUnSynchronizationJob.Remove(gameScenePeer);
            }
        }

        internal void OnGameObjectGridCellHasJob(GameObjectGridCell gameObjectGridCell)
        {
            Debug.Assert(CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");

            _cellsWithJob.Add(gameObjectGridCell);
        }

        internal void UnSynchronizeGameObjectFromPeer(GameScenePeer peer, GameObjectServer gameObject)
        {
            Debug.Assert(CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");

            if (!peer.HasUnSynchronizationJob)
            {
                Debug.Assert(peer.Avatar != null, "peer's avatar is null!");

                _peersWithUnSynchronizationJob.Add(peer);
            }

            peer.AddGameObjectToUnSynchronizeJob(gameObject);
        }

        internal void SynchronizeGameObjectToPeer(GameScenePeer peer, GameObjectServer gameObject)
        {
            Debug.Assert(CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");

            if (!peer.HasSynchronizationJob)
            {
                Debug.Assert(peer.Avatar != null, "peer's avatar is null!");

                _peersWithSynchronizationJob.Add(peer);
            }

            peer.AddGameObjectToSynchronizeJob(gameObject);
        }

        internal void OnGameObjectTransformClean(GameObjectServer gameObject)
        {
            Debug.Assert(gameObject.NodeOnDirtyTransformList != null, "GameObject's NodeOnDirtyTransformList must be valid");
            Debug.Assert(gameObject.IsTransformDirty, "GameObject's IsTransformDirty must be true");

            _gameObjectsWithDirtyTransform.Remove(gameObject.NodeOnDirtyTransformList);
        }

        internal LinkedListNode<GameObjectServer> OnGameObjectTransformDirty(GameObjectServer gameObject)
        {
            Debug.Assert(!CurrentlySynchronizing, "This method must be called when there are no active synchronization is in progress.");
            Debug.Assert(!gameObject.IsTransformDirty, "GameObject's IsTransformDirty must be false");
            Debug.Assert(gameObject.NodeOnDirtyTransformList == null, "GameObject's NodeOnDirtyTransformList must be null");

            return _gameObjectsWithDirtyTransform.AddLast(gameObject);
        }

        internal GameObjectGridCell this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= Size || y < 0 || y >= Size)
                {
                    return _outterGridCell;
                }

                return _grid[x, y];
            }
        }

        public GameScenePeer GetGameScenePeerOfPeer(Peer peer)
        {
            GameScenePeer[] gameScenePeers = peer.GetComponents<GameScenePeer>();

            foreach (var gameScenePeer in gameScenePeers)
            {
                if (gameScenePeer.GameScene == GameScene)
                {
                    return gameScenePeer;
                }
            }

            return null;
        }

        internal GameObjectGridCell[] GetGridCellsAround(GameObjectGridCell gridCell)
        {
            if (gridCell != null)
            {
                if (gridCell.Outter)
                {
                    return new GameObjectGridCell[] { gridCell };
                }
                else
                {
                    List<GameObjectGridCell> result = new List<GameObjectGridCell>();

                    int startX = gridCell.X - 1;
                    int startY = gridCell.Y - 1;

                    for (int i = 0; i <= 2; i++)
                    {
                        for (int j = 0; j <= 2; j++)
                        {
                            GameObjectGridCell grid = this[i + startX, j + startY];

                            if (!result.Contains(grid))
                            {
                                result.Add(grid);
                            }
                        }
                    }

                    return result.ToArray();
                }
            }

            return new GameObjectGridCell[] { };
        }
    }

    public class PeerEnteredToSceneMessage : EntityMessage
    {
        public Peer Peer { get; private set; }

        internal PeerEnteredToSceneMessage(Peer peer)
        {
            Peer = peer;
        }
    }

    public class PeerRemovedFromSceneMessage : EntityMessage
    {
        public GameScenePeer GameScenePeer { get; private set; }

        internal PeerRemovedFromSceneMessage(GameScenePeer gameScenePeer)
        {
            GameScenePeer = gameScenePeer;
        }
    }

    public class OnSynchronizeGameSceneToPeerMessage : EntityMessage
    {
        public GameObjectServer Avatar { get; private set; }
        public GameScenePeer Peer { get; private set; }

        public GameObjectGridCell[] RemovedCells { get; private set; }
        public GameObjectGridCell[] AddedCells { get; private set; }

        public OnSynchronizeGameSceneToPeerMessage(GameScenePeer peer, GameObjectServer avatar, GameObjectGridCell[] removedCells, GameObjectGridCell[] addedCells)
        {
            Peer = peer;
            Avatar = avatar;

            RemovedCells = removedCells;
            AddedCells = addedCells;
        }
    }
}
