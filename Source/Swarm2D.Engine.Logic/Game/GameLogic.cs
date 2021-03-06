﻿/******************************************************************************
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
using Swarm2D.Engine.Core;
using Swarm2D.Library;
using System.Diagnostics;

namespace Swarm2D.Engine.Logic
{
    public class GameLogic : Component, IEntityDomain
    {
        public bool IsGameStarted { get; private set; }
        public bool IsRunning { get; private set; }

        public int ExecutedFrame { get; private set; }

        public SceneManager SceneManager { get; private set; }

        public List<Scene> LoadedScenes
        {
            get { return SceneManager.LoadedScenes; }
        }

        public int FrameRate { get; set; }

        private long _startTick = 0;
        private long _delayTick = 0;
        private long _lastPauseTick = 0;

        private bool _firstFrame = true;

        public float FixedDeltaTime { get; internal set; }
        public float FixedTime { get; internal set; }

        private EntityDomain _entityDomain;

        private readonly OnGameFrameUpdate _onGameFrameUpdate = new OnGameFrameUpdate();
        private readonly OnIdleGameFrameUpdate _onIdleGameFrameUpdate = new OnIdleGameFrameUpdate();
        private readonly NonStartedGameFrameUpdate _nonStartedGameFrameUpdate = new NonStartedGameFrameUpdate();

        protected override void OnAdded()
        {
            base.OnAdded();

            FrameRate = 60;

            _entityDomain = new EntityDomain(Entity);
            Entity.ChildDomain = this;

            Entity sceneManagerEntity = Entity.CreateChildEntity("SceneManager");
            SceneManager = sceneManagerEntity.AddComponent<SceneManager>();
        }

        public void StartGame()
        {
            if (!IsGameStarted)
            {
                _startTick = Time.ElapsedTicks;
                IsRunning = true;
                IsGameStarted = true;

                Entity.SendMessage(new OnStartGameMessage());
            }
        }

        public void StopGame()
        {
            if (IsGameStarted)
            {
                Entity.SendMessage(new OnStopGameMessage());

                ExecutedFrame = 0;
                _delayTick = 0;
                IsRunning = false;
                IsGameStarted = false;

                for (int i = 0; i < LoadedScenes.Count; i++)
                {
                    Scene scene = LoadedScenes[i];
                    //scene.Reset();
                }

                LoadedScenes.Clear();
            }
        }

        public void PauseGame()
        {
            if (IsGameStarted && IsRunning)
            {
                IsRunning = false;
                _lastPauseTick = Time.ElapsedTicks;
            }
        }

        public void ResumeGame()
        {
            if (IsGameStarted && !IsRunning)
            {
                IsRunning = true;

                long currentTick = Time.ElapsedTicks;
                _delayTick += currentTick - _lastPauseTick;
            }
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            _entityDomain.InitializeNonInitializedEntityComponents();

            if (IsGameStarted)
            {
                if (IsRunning)
                {
                    float fixedDt = 1.0f/(float) FrameRate;

                    FixedDeltaTime = fixedDt;

                    long currentTick = Time.ElapsedTicks;
                    long passedTick = currentTick - _startTick - _delayTick;
                    int currentFrame = (int) ((float) (FrameRate*passedTick)/(float) Time.TicksPerSecond);

                    int currentUpdateFrameCount = 0;

                    while (currentFrame > ExecutedFrame && !_firstFrame && currentUpdateFrameCount < 10)
                    {
                        ExecutedFrame++;

                        Engine.DoneJob();

                        Engine.RootEntity.SendMessage(_onGameFrameUpdate);
                        Entity.SendMessage(_onGameFrameUpdate);
                        SceneManager.Entity.SendMessage(_onGameFrameUpdate);

                        FixedTime = ExecutedFrame*fixedDt;

                        currentUpdateFrameCount++;
                    }

                    if (_firstFrame)
                    {
                        _firstFrame = false;
                    }
                }
                else
                {
                    Entity.SendMessage(_onIdleGameFrameUpdate);
                    SceneManager.Entity.SendMessage(_onIdleGameFrameUpdate);

                    _firstFrame = false;
                }
            }
            else
            {
                Entity.SendMessage(_nonStartedGameFrameUpdate);
                SceneManager.Entity.SendMessage(_nonStartedGameFrameUpdate);

                _firstFrame = false;
            }
        }

        public Scene CreateNewScene()
        {
            Entity sceneEntity = SceneManager.CreateChildEntity("Scene");
            return sceneEntity.GetComponent<Scene>();
        }

        void IEntityDomain.OnCreateChildEntity(Entity entity)
        {
            entity.Domain = this;

            GameSystem gameSystem = entity.AddComponent<GameSystem>();
            gameSystem.GameLogic = this;
        }

        public void DeleteScene(Scene scene)
        {
            SceneManager.DeleteScene(scene);
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

    public static class GameLogicExtensions
    {
        public static GameLogic CreateGame(this Core.Engine engine)
        {
            Entity gameLogicEntity = engine.RootEntity.CreateChildEntity("GameLogic");
            GameLogic gameLogic = gameLogicEntity.AddComponent<GameLogic>();

            engine.InvokeMessage(new GameCreatedMessage(gameLogic));

            return gameLogic;
        }
    }

    public class GameCreatedMessage : GlobalMessage
    {
        public GameLogic GameLogic { get; private set; }

        public GameCreatedMessage(GameLogic gameLogic)
        {
            GameLogic = gameLogic;
        }
    }

    public class OnIdleGameFrameUpdate : EntityMessage
    {

    }

    public class NonStartedGameFrameUpdate : EntityMessage
    {
        
    }

    public class OnStartGameMessage : EntityMessage
    {

    }

    public class OnStopGameMessage : EntityMessage
    {

    }

    public class OnGameFrameUpdate : EntityMessage
    {

    }
}