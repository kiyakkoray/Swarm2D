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
        public GameSceneServer GameSceneServer { get; private set; }
        public bool Outter { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public List<GameObjectServer> GameObjects { get; private set; }

        private List<GameObjectServer> _newlyAddedGameObjects; //used for synchronizing
        private List<GameObjectServer> _newlyRemovedGameObjects; //used for synchronizing
        private List<GameScenePeer> _newlyAddedPeers;
        private List<GameScenePeer> _newlyRemovedPeers;

        public PeerGroup PeerGroup { get; private set; }
        private List<GameScenePeer> _activePeers;

        private bool _hasJob = false;

        internal GameObjectGridCell(GameSceneServer gameSceneServer, int x, int y)
        {
            GameSceneServer = gameSceneServer;
            X = x;
            Y = y;

            Outter = false;

            Initialize();
        }

        internal GameObjectGridCell(GameSceneServer gameSceneServer)
        {
            GameSceneServer = gameSceneServer;
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
            _newlyAddedPeers = new List<GameScenePeer>();
            _newlyRemovedPeers = new List<GameScenePeer>();
        }

        private void JobAdded()
        {
            Debug.Assert(GameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");

            if (!_hasJob)
            {
                GameSceneServer.OnGameObjectGridCellHasJob(this);
                _hasJob = true;
            }
        }

        internal void AddPeer(GameScenePeer gameScenePeer)
        {
            Debug.Assert(GameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");

            JobAdded();

            _newlyAddedPeers.Add(gameScenePeer);
        }

        internal void RemovePeer(GameScenePeer gameScenePeer) //on moving to another grid cell
        {
            Debug.Assert(GameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");
            Debug.Assert(_activePeers.Contains(gameScenePeer), "Trying to remove a peer which is not on the _activePeers list.");

            JobAdded();
            _newlyRemovedPeers.Add(gameScenePeer);
        }

        internal void OnPeerDestroyed(GameScenePeer gameScenePeer) //on disconnect etc...
        {
            Debug.Assert(!GameSceneServer.CurrentlySynchronizing, "This method must be called when there are no active synchronization is in progress.");
            Debug.Assert(_activePeers.Contains(gameScenePeer), "Trying to delete a peer which is not on the _activePeers list.");

            _activePeers.Remove(gameScenePeer);
            PeerGroup.Peers.Remove(gameScenePeer.Peer);
        }

        internal void AddGameObject(GameObjectServer gameObject)
        {
            Debug.Assert(GameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");
            Debug.Assert(!GameObjects.Contains(gameObject), "Trying to add gameObject " + gameObject.Entity.Name + " which is already in GameObjects List.");

            GameObjects.Add(gameObject);

            if (!gameObject.IsCustomSynchronization)
            {
                Debug.Assert(!_newlyAddedGameObjects.Contains(gameObject), "Removed gameObject " + gameObject.Entity.Name + " is already exist in _newlyAddedGameObjects List.");

                JobAdded();

                _newlyAddedGameObjects.Add(gameObject);
            }
        }

        internal void RemoveGameObject(GameObjectServer gameObject) //does not called when object destroyed
        {
            Debug.Assert(GameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");
            Debug.Assert(GameObjects.Contains(gameObject), "Removed gameObject " + gameObject.Entity.Name + " does not exist in GameObjects List.");

            GameObjects.Remove(gameObject);

            if (!gameObject.IsCustomSynchronization)
            {
                Debug.Assert(!_newlyRemovedGameObjects.Contains(gameObject), "Removed gameObject " + gameObject.Entity.Name + " is already exist in _newlyRemovedGameObjects List.");

                JobAdded();

                _newlyRemovedGameObjects.Add(gameObject);
            }
        }

        internal void OnGameObjectDestroyed(GameObjectServer gameObject)
        {
            Debug.Assert(!GameSceneServer.CurrentlySynchronizing, "This method must be called when there are no active synchronization is in progress.");
            Debug.Assert(GameObjects.Contains(gameObject), "Destroyed gameObject " + gameObject.Entity.Name + " does not exist in GameObjects List.");
            Debug.Assert(!_newlyAddedGameObjects.Contains(gameObject), "Destroyed gameObject " + gameObject.Entity.Name + " should not be in _newlyAddedGameObjects List.");
            Debug.Assert(!_newlyRemovedGameObjects.Contains(gameObject), "Destroyed gameObject " + gameObject.Entity.Name + " should not be in _newlyRemovedGameObjects List.");

            GameObjects.Remove(gameObject);
        }

        internal void DoUnSynchronizationJob()
        {
            Debug.Assert(GameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");

            foreach (GameObjectServer gameObject in _newlyRemovedGameObjects)
            {
                Debug.Assert(!GameObjects.Contains(gameObject), "Removed gameObject " + gameObject.Entity.Name + " is still in GameObjects list.");

                foreach (GameScenePeer gameScenePeer in _activePeers)
                {
                    GameSceneServer.UnSynchronizeGameObjectFromPeer(gameScenePeer, gameObject);
                }
            }

            _newlyRemovedGameObjects.Clear();

            foreach (GameScenePeer gameScenePeer in _newlyRemovedPeers)
            {
                foreach (GameObjectServer gameObject in GameObjects)
                {
                    if (!gameObject.IsCustomSynchronization)
                    {
                        GameSceneServer.UnSynchronizeGameObjectFromPeer(gameScenePeer, gameObject);
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
            Debug.Assert(GameSceneServer.CurrentlySynchronizing, "This method must be called when an active synchronization is in progress.");

            foreach (GameObjectServer gameObject in _newlyAddedGameObjects)
            {
                Debug.Assert(GameObjects.Contains(gameObject), "Added gameObject " + gameObject.Entity.Name + " does not exist in GameObjects list.");

                foreach (GameScenePeer gameScenePeer in _activePeers)
                {
                    GameSceneServer.SynchronizeGameObjectToPeer(gameScenePeer, gameObject);
                }
            }

            _newlyAddedGameObjects.Clear();

            foreach (GameScenePeer gameScenePeer in _newlyAddedPeers)
            {
                foreach (GameObjectServer gameObject in GameObjects)
                {
                    if (!gameObject.IsCustomSynchronization)
                    {
                        GameSceneServer.SynchronizeGameObjectToPeer(gameScenePeer, gameObject);
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