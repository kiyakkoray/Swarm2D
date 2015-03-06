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
using Swarm2D.Engine.View.GUI;
using Swarm2D.Library;

namespace Swarm2D.Engine.View
{
    public class GameUI : Component
    {
        public UIManager UIManager { get; private set; }

        private GameRenderer _gameRenderer;
        private GameLogic _gameLogic;

        protected override void OnAdded()
        {
            _gameLogic = Entity.GetComponent<GameLogic>();
            _gameRenderer = Entity.GetComponent<GameRenderer>();

            if (_gameLogic.IsGameStarted)
            {
                CreateUI();
            }
        }

        [EntityMessageHandler(MessageType = typeof(OnStartGameMessage))]
        private void HandleOnStartGameMessage(Message message)
        {
            CreateUI();
        }

        private void CreateUI()
        {
            Entity uiManagerEntity = Entity.CreateChildEntity("UIManager");
            UIManager = uiManagerEntity.AddComponent<UIManager>();
            UIManager.Initialize(Entity.GetComponent<GameRenderer>());
            UIManager.ShowAllBoxes = false;
        }

        [EntityMessageHandler(MessageType = typeof(OnStopGameMessage))]
        private void HandleOnStopGameMessage(Message message)
        {
            UIManager.Entity.Destroy();
            UIManager = null;
        }

        [EntityMessageHandler(MessageType = typeof(OnGameFrameUpdate))]
        private void HanldeOnGameFrameUpdateMessage(Message message)
        {
            UIManager.Update();
        }

        [DomainMessageHandler(MessageType = typeof(RenderMessage))]
        public void Render(Message message)
        {
            RenderMessage renderMessage = message as RenderMessage;
            IOSystem ioSystem = renderMessage.IOSystem;

            if (_gameLogic.IsRunning)
            {
                ioSystem.AddGraphicsCommand(new CommandSetViewMatrix(Matrix4x4.Position2D(_gameRenderer.RenderTargetPosition)));
                //graphicsContext.ViewMatrix = Matrix4x4.Position2D(_gameRenderer.RenderTargetPosition);
                UIManager.Render(ioSystem);
            }
        }
    }
}
