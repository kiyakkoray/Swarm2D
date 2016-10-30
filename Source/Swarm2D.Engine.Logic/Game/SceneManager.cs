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
    public class SceneManager : Component, IEntityDomain
    {
        public GameLogic GameLogic { get; private set; }

        public List<Scene> LoadedScenes { get; private set; }

        private EntityDomain _entityDomain;
        private GameSystem _gameSystem;

        private readonly UpdateMessage _updateMessage = new UpdateMessage();

        [EntityMessageHandler(MessageType = typeof(OnGameFrameUpdate))]
        private void HanldeOnGameFrameUpdateMessage(Message message)
        {
            _entityDomain.InitializeNonInitializedEntityComponents();
            _entityDomain.StartNotStartedEntityComponents();

            _updateMessage.Dt = GameLogic.FixedDeltaTime;

            _entityDomain.SendMessage(_updateMessage);
        }

        [EntityMessageHandler(MessageType = typeof(OnIdleGameFrameUpdate))]
        private void HanldeOnIdleGameFrameUpdateMessage(Message message)
        {
            _entityDomain.InitializeNonInitializedEntityComponents();

             for (int i = 0; i < LoadedScenes.Count; i++)
             {
                Scene scene = LoadedScenes[i];
                scene.OnIdleUpdate();
            }
        }

        [EntityMessageHandler(MessageType = typeof(NonStartedGameFrameUpdate))]
        private void OnNonStartedGameFrameUpdate(Message message)
        {
            _entityDomain.InitializeNonInitializedEntityComponents();
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            LoadedScenes = new List<Scene>();
            _gameSystem = GetComponent<GameSystem>();
            _entityDomain = new EntityDomain(Entity);
            Entity.ChildDomain = this;
            GameLogic = _gameSystem.GameLogic;
        }

        public void DeleteScene(Scene scene)
        {
            LoadedScenes.Remove(scene);

            scene.Entity.Destroy();
        }

        void IEntityDomain.OnCreateChildEntity(Entity entity)
        {
            entity.Domain = this;

            Scene scene = entity.AddComponent<Scene>();
            scene.GameLogic = GameLogic;

            LoadedScenes.Add(scene);
        }

        public void SendMessage(DomainMessage message)
        {
            _entityDomain.SendMessage(message);
        }

        void IEntityDomain.OnComponentCreated(Component component)
        {
            _entityDomain.OnComponentCreated(component);
        }

        void IEntityDomain.OnComponentDestroyed(Component component)
        {
            _entityDomain.OnComponentDestroyed(component);
        }

        Entity IEntityDomain.InstantiatePrefab(Entity prefab)
        {
            return _entityDomain.InstantiatePrefab(prefab);
        }

        void IEntityDomain.OnEntityParentChanged(Entity entity)
        {

        }
    }
}
