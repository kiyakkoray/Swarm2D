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
using Swarm2D.Engine.Logic;
using Swarm2D.Library;

namespace Swarm2D.Engine.Multiplayer.Scene
{
    public class GameObjectGridCell
    {
        public GameSceneServer Grid { get; private set; }
        public bool Outter { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public List<GameObjectServer> GameObjects { get; private set; }

        public List<GameScenePeer> Peers { get; private set; }

        private List<GameScenePeer> _activePeers;

        private List<GameObjectServer> _newlyAddedGameObjects; //used for synchronizing
        private List<GameObjectServer> _newlyRemovedGameObjects; //used for synchronizing
        private List<GameScenePeer> _newlyAddedPeers;
        private List<GameScenePeer> _newlyRemovedPeers;

        public PeerGroup PeerGroup { get; private set; }

        private bool _hasJob = false;

        private void JobAdded()
        {
            if (!_hasJob)
            {
                Grid.OnGameObjectGridCellHasJob(this);
                _hasJob = true;
            }
        }

        internal GameObjectGridCell(GameSceneServer grid, int x, int y)
        {
            Grid = grid;
            X = x;
            Y = y;

            Outter = false;

            Initialize();
        }

        internal GameObjectGridCell(GameSceneServer grid)
        {
            Grid = grid;
            Outter = true;

            Initialize();
        }

        private void Initialize()
        {
            PeerGroup = new PeerGroup();

            _activePeers = new List<GameScenePeer>();
            GameObjects = new List<GameObjectServer>();
            _newlyAddedGameObjects = new List<GameObjectServer>();
            _newlyRemovedGameObjects = new List<GameObjectServer>();
            Peers = new List<GameScenePeer>();
            _newlyAddedPeers = new List<GameScenePeer>();
            _newlyRemovedPeers = new List<GameScenePeer>();
        }

        internal void AddPeer(GameScenePeer gameScenePeer)
        {
            JobAdded();

            Peers.Add(gameScenePeer);
            _newlyAddedPeers.Add(gameScenePeer);
        }

        internal void RemovePeer(GameScenePeer gameScenePeer) //on moving to another grid cell
        {
            JobAdded();

            Peers.Remove(gameScenePeer);
            _newlyRemovedPeers.Add(gameScenePeer);
        }

        internal void DeletePeer(GameScenePeer gameScenePeer) //on disconnect etc...
        {
            Peers.Remove(gameScenePeer);
            _activePeers.Remove(gameScenePeer);
            PeerGroup.Peers.Remove(gameScenePeer.Peer);
        }

        internal void AddGameObject(GameObjectServer gameObject)
        {
            GameObjects.Add(gameObject);

            if (!gameObject.IsCustomSynchronization)
            {
                JobAdded();

                _newlyAddedGameObjects.Add(gameObject);
            }
        }

        internal void RemoveGameObject(GameObjectServer gameObject) //does not called when object destroyed
        {
            GameObjects.Remove(gameObject);

            if (!gameObject.IsCustomSynchronization)
            {
                JobAdded();

                _newlyRemovedGameObjects.Add(gameObject);
            }
        }

        internal void OnGameObjectDestroyed(GameObjectServer gameObject) //what happens if object is removed before its sync done
        {
            if (!gameObject.IsCustomSynchronization)
            {
                if (_newlyAddedGameObjects.Contains(gameObject))
                {
                    JobAdded();
                    _newlyAddedGameObjects.Remove(gameObject);
                }
            }

            GameObjects.Remove(gameObject);
        }

        internal void Reset()
        {
            GameObjects.Clear();
        }

        internal void DoUnSynchronizationJob()
        {
            foreach (GameObjectServer gameObject in _newlyRemovedGameObjects)
            {
                foreach (GameScenePeer gameScenePeer in _activePeers)
                {
                    Grid.RemoveGameObjectFromPeerOnGrid(gameScenePeer, gameObject);
                }
            }

            _newlyRemovedGameObjects.Clear();

            foreach (GameScenePeer gameScenePeer in _newlyRemovedPeers)
            {
                foreach (GameObjectServer gameObject in GameObjects)
                {
                    if (!gameObject.IsCustomSynchronization)
                    {
                        Grid.RemoveGameObjectFromPeerOnGrid(gameScenePeer, gameObject);
                    }
                }
            }

            foreach (GameScenePeer gameScenePeer in _newlyRemovedPeers)
            {
                _activePeers.Remove(gameScenePeer);
                PeerGroup.Peers.Remove(gameScenePeer.Peer);
            }

            _newlyRemovedPeers.Clear();
        }

        internal void DoSynchronizationJob()
        {
            foreach (GameObjectServer gameObject in _newlyAddedGameObjects)
            {
                foreach (GameScenePeer gameScenePeer in _activePeers)
                {
                    Grid.SynchronizeGameObjectToPeer(gameScenePeer, gameObject);
                }

                gameObject.NotSynchronizedOnNewGridCell = false;
            }

            _newlyAddedGameObjects.Clear();

            foreach (GameScenePeer gameScenePeer in _newlyAddedPeers)
            {
                foreach (GameObjectServer gameObject in GameObjects)
                {
                    if (!gameObject.IsCustomSynchronization)
                    {
                        Grid.SynchronizeGameObjectToPeer(gameScenePeer, gameObject);
                    }
                }
            }

            foreach (GameScenePeer gameScenePeer in _newlyAddedPeers)
            {
                PeerGroup.Peers.Add(gameScenePeer.Peer);
                _activePeers.Add(gameScenePeer);
            }

            _newlyAddedPeers.Clear();

            _hasJob = false;
        }
    }
}