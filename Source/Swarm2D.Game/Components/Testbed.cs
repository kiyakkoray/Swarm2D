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
using System.Reflection.Emit;
using System.Text;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View;
using Swarm2D.Engine.View.GUI;
using Swarm2D.Library;

namespace Swarm2D.Game
{
    public class Testbed : EngineComponent
    {
        private GameLogic _gameLogic;
        private GameUI _gameUI;

        #region left frame variables

        private UIFrame _leftFrame;
        private UILabel _infoLabel;

        private UIButton _testScene1Button;
        private UIButton _testScene2Button;
        private UIButton _testScene3Button;
        private UIButton _testScene4Button;

        private UILabel _gravityStateLabel;
        private UIButton _changeGravityButton;

        private UILabel _simulationStateLabel;
        private UIButton _changeSimulationButton;

        private UILabel _simulationNextStepLabel;
        private UIButton _doNextSimulationStepButton;

        #endregion

        private enum WorldState
        {
            Starting,
            Ok
        }

        private WorldState _state;

        private Scene _scene;
        private PhysicsWorld _physicsWorld;

        protected override void OnInitialize()
        {
            base.OnAdded();

            _gameLogic =  Engine.CreateGame();
            _gameLogic.AddComponent<GameRenderer>();
            _gameUI = _gameLogic.AddComponent<GameUI>();
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            switch (_state)
            {
                case WorldState.Starting:
                    {
                        SpriteData editorSpriteData = new SpriteData("Editor", "spriteData");
                        editorSpriteData.Load();

                        SpriteCategory otherSpriteCategory = editorSpriteData.SpriteCategories["Other"];
                        otherSpriteCategory.Load();

                        _gameLogic.StartGame();

                        UISkin skin = _gameUI.UIManager.Skin;

                        skin.ButtonNormalSprite = editorSpriteData.GetSprite(@"GUI\blackButtonNormal");
                        skin.ButtonMouseDownSprite = editorSpriteData.GetSprite(@"GUI\blackButtonMouseDown");
                        skin.ButtonMouseOverSprite = editorSpriteData.GetSprite(@"GUI\blackButtonMouseOver");

                        skin.FrameBoxSprite = skin.ButtonNormalSprite;

                        CreateLeftFrame();

                        _state = WorldState.Ok;
                    }
                    break;
                case WorldState.Ok:
                    break;
            }

            if (GameInput.GetKeyDown(KeyCode.KeyG))
            {
                ChangeGravity();
            }

            if (GameInput.GetKeyDown(KeyCode.KeyQ))
            {
                ChangeSimulation();
            }

            if (GameInput.GetKeyDown(KeyCode.KeyE))
            {
                DoNextSimulationStep();
            }

            if (_physicsWorld != null && !_physicsWorld.SimulationEnabled)
            {
                _simulationNextStepLabel.Text = "Next step: " + _physicsWorld.CurrentDebugState;
            }
        }

        private void CreateLeftFrame()
        {
            {
                _leftFrame = FastGUI.CreateFrame(_gameUI.UIManager.RootWidget, "leftFrame", 10, 10, 250, 660);
                _leftFrame.ShowBox = true;
            }

            float currentY = 10.0f;

            {
                _infoLabel = FastGUI.CreateLabel(_leftFrame.Widget, "infoLabel", 10, currentY, 230, 80);
                _infoLabel.Text = "Welcome to Testbed\n\n\n\nSelect scene:";

                currentY += 80.0f + 10.0f;
            }

            {
                _testScene1Button = FastGUI.CreateButton(_leftFrame.Widget, "testScene1Button", 10, currentY, 230, 30);
                _testScene1Button.PassEventsToChildren = false;

                UILabel buttonLabel = FastGUI.CreateLabel(_testScene1Button.Widget, "testScene1ButtonLabel", 0, 0, 230, 30);
                buttonLabel.Text = "Random shapes scene";
                _testScene1Button.Widget.MouseClick += new UIMouseEvent(OnTestScene1ButtonClick);

                currentY += 30.0f + 10.0f;
            }

            {
                _testScene2Button = FastGUI.CreateButton(_leftFrame.Widget, "testScene2Button", 10, currentY, 230, 30);
                _testScene2Button.PassEventsToChildren = false;

                UILabel buttonLabel = FastGUI.CreateLabel(_testScene2Button.Widget, "testScene2ButtonLabel", 0, 0, 230, 30);
                buttonLabel.Text = "Crates scene";
                _testScene2Button.Widget.MouseClick += new UIMouseEvent(OnTestScene2ButtonClick);

                currentY += 30.0f + 10.0f;
            }

            {
                _testScene3Button = FastGUI.CreateButton(_leftFrame.Widget, "testScene3Button", 10, currentY, 230, 30);
                _testScene3Button.PassEventsToChildren = false;

                UILabel buttonLabel = FastGUI.CreateLabel(_testScene3Button.Widget, "testScene3ButtonLabel", 0, 0, 230, 30);
                buttonLabel.Text = "Another scene";
                _testScene3Button.Widget.MouseClick += new UIMouseEvent(OnTestScene3ButtonClick);

                currentY += 30.0f + 10.0f;
            }

            {
                _testScene4Button = FastGUI.CreateButton(_leftFrame.Widget, "testScene4Button", 10, currentY, 230, 30);
                _testScene4Button.PassEventsToChildren = false;

                UILabel buttonLabel = FastGUI.CreateLabel(_testScene4Button.Widget, "testScene4ButtonLabel", 0, 0, 230, 30);
                buttonLabel.Text = "Free fall scene";
                _testScene4Button.Widget.MouseClick += new UIMouseEvent(OnTestScene4ButtonClick);

                currentY += 30.0f + 10.0f;
            }

            {
                currentY += 30.0f;
            }

            {
                _gravityStateLabel = FastGUI.CreateLabel(_leftFrame.Widget, "gravityState", 10, currentY, 230, 30);
                _gravityStateLabel.Text = "Gravity: off";

                currentY += 30.0f + 10.0f;
            }

            {
                _changeGravityButton = FastGUI.CreateButton(_leftFrame.Widget, "changeGravityButton", 10, currentY, 230, 30);
                _changeGravityButton.PassEventsToChildren = false;

                UILabel buttonLabel = FastGUI.CreateLabel(_changeGravityButton.Widget, "changeGravityButtonLabel", 0, 0, 230, 30);
                buttonLabel.Text = "Turn gravity on/off";
                _changeGravityButton.Widget.MouseClick += new UIMouseEvent(OnChangeGravityButtonClick);

                currentY += 30.0f + 10.0f;
            }

            {
                currentY += 30.0f;
            }

            {
                _simulationStateLabel = FastGUI.CreateLabel(_leftFrame.Widget, "simulationState", 10, currentY, 230, 30);
                _simulationStateLabel.Text = "Simulation: on";

                currentY += 30.0f + 10.0f;
            }

            {
                _changeSimulationButton = FastGUI.CreateButton(_leftFrame.Widget, "changeSimulationButton", 10, currentY, 230, 30);
                _changeSimulationButton.PassEventsToChildren = false;

                UILabel buttonLabel = FastGUI.CreateLabel(_changeSimulationButton.Widget, "changeSimulationButtonLabel", 0, 0, 230, 30);
                buttonLabel.Text = "Turn simulation on/off";
                _changeSimulationButton.Widget.MouseClick += new UIMouseEvent(OnChangeSimulationButtonClick);

                currentY += 30.0f + 10.0f;
            }

            {
                _simulationNextStepLabel = FastGUI.CreateLabel(_leftFrame.Widget, "simulationStep", 10, currentY, 230, 30);
                _simulationNextStepLabel.Enabled = false;

                currentY += 30.0f + 10.0f;
            }

            {
                _doNextSimulationStepButton = FastGUI.CreateButton(_leftFrame.Widget, "changeSimulationButton", 10, currentY, 230, 30);
                _doNextSimulationStepButton.PassEventsToChildren = false;
                _doNextSimulationStepButton.Enabled = false;

                UILabel buttonLabel = FastGUI.CreateLabel(_doNextSimulationStepButton.Widget, "changeSimulationButtonLabel", 0, 0, 230, 30);
                buttonLabel.Text = "Do next simulation step";
                _doNextSimulationStepButton.Widget.MouseClick += new UIMouseEvent(OnDoNextSimulationButtonClick);

                currentY += 30.0f + 10.0f;
            }

            {
                UILabel lastInfoLabel = FastGUI.CreateLabel(_leftFrame.Widget, "lastInfoLabel", 10, currentY, 230, 30);
                lastInfoLabel.Text = "Use W,A,S,D to move";

                currentY += 30.0f + 10.0f;
            }

        }

        private void CreateNewScene()
        {
            if (_scene != null) //TODO: do not forget to remove from loaded scenes list
            {
                _scene = null;
            }

            _scene = _gameLogic.CreateNewScene();
            _physicsWorld = _scene.AddComponent<PhysicsWorld>();
            _scene.AddComponent<SceneRenderer>();
        }

        void OnTestScene1ButtonClick(UIWidget frame, MouseEventArgs e)
        {
            CreateNewScene();

            SceneExtensions.Reset1(_scene);
        }

        void OnTestScene2ButtonClick(UIWidget frame, MouseEventArgs e)
        {
            CreateNewScene();

            SceneExtensions.Reset2(_scene);
        }

        void OnTestScene3ButtonClick(UIWidget frame, MouseEventArgs e)
        {
            CreateNewScene();

            SceneExtensions.Reset3(_scene);
        }

        void OnTestScene4ButtonClick(UIWidget frame, MouseEventArgs e)
        {
            CreateNewScene();

            SceneExtensions.Reset4(_scene);
        }

        void OnChangeGravityButtonClick(UIWidget frame, MouseEventArgs e)
        {
            ChangeGravity();
        }

        void OnChangeSimulationButtonClick(UIWidget frame, MouseEventArgs e)
        {
            ChangeSimulation();
        }

        void OnDoNextSimulationButtonClick(UIWidget frame, MouseEventArgs e)
        {
            DoNextSimulationStep();
        }

        private void ChangeGravity()
        {
            if (Mathf.IsZero(_physicsWorld.Gravity))
            {
                _gravityStateLabel.Text = "Gravity: on";
                _physicsWorld.Gravity = new Vector2(0.0f, 9.81f * PhysicsWorld.MeterToPixel);
            }
            else
            {
                _gravityStateLabel.Text = "Gravity: off";
                _physicsWorld.Gravity = Vector2.Zero;
            }
        }

        private void ChangeSimulation()
        {
            _physicsWorld.SimulationEnabled = !_physicsWorld.SimulationEnabled;

            if (_physicsWorld.SimulationEnabled)
            {
                _simulationStateLabel.Text = "Simulation: on";

                _simulationNextStepLabel.Enabled = false;
                _doNextSimulationStepButton.Enabled = false;
            }
            else
            {
                _simulationStateLabel.Text = "Simulation: off";

                _simulationNextStepLabel.Enabled = true;
                _doNextSimulationStepButton.Enabled = true;
            }

            Debug.Log("Simulation state: " + _physicsWorld.SimulationEnabled);
        }

        private void DoNextSimulationStep()
        {
            _physicsWorld.DoNextSimulationStep = true;
        }
    }
}
