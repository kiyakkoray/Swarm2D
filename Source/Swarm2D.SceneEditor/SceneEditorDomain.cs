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

using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View;
using Swarm2D.Engine.View.GUI;
using Swarm2D.SceneEditor.GUIControllers;
using Swarm2D.Library;

namespace Swarm2D.SceneEditor
{
    public class SceneEditorDomain : EngineComponent
    {
        public UIFrame UIRootObject
        {
            get
            {
                return UIManager.RootObject;
            }
        }

        public UIManager UIManager { get; private set; }
        public EditorGUI EditorGUI { get; private set; }
        public GameLogic GameLogic { get; private set; }
        public IOSystem IOSystem { get; private set; }
        public GameRenderer GameRenderer { get; private set; }

        public SpriteData EditorSpriteData { get; private set; }

        protected override void OnInitialize()
        {
            EditorSpriteData = new SpriteData("Editor", "spriteData");

            IOSystem = GetComponent<IOSystem>();

            {
                Entity gameLogicEntity = Entity.CreateChildEntity("GameLogic");
                GameLogic = gameLogicEntity.AddComponent<GameLogic>();
                GameRenderer = gameLogicEntity.AddComponent<GameRenderer>();
                gameLogicEntity.AddComponent<GameUI>();
            }

            EditorInput.Initialize(IOSystem);
            EditorScreen.Initialize(IOSystem);

            Entity uiManagerEntity = Entity.CreateChildEntity("UIManager");
            UIManager = uiManagerEntity.AddComponent<UIManager>();
            UIManager.Initialize(IOSystem);

            EditorSpriteData.Load();

            SpriteCategory otherSpriteCategory = EditorSpriteData.SpriteCategories["Other"];
            otherSpriteCategory.Load();

            UISkin skin = UIManager.Skin;

            skin.ButtonNormalSprite = EditorSpriteData.GetSprite(@"GUI\blackButtonNormal");
            skin.ButtonMouseDownSprite = EditorSpriteData.GetSprite(@"GUI\blackButtonMouseDown");
            skin.ButtonMouseOverSprite = EditorSpriteData.GetSprite(@"GUI\blackButtonMouseOver");

            skin.FrameBoxSprite = skin.ButtonNormalSprite;

            UIWidget editorWidget = FastGUI.CreateFromXmlFile(@"..\Editor\editorFrame.xml", UIManager.RootWidget);
            EditorGUI = editorWidget.AddComponent<EditorGUI>();

            GameLogic.StopGame();
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            EditorGUI.UpdateLoop();
            UIManager.Update();
        }

        [DomainMessageHandler(MessageType = typeof(RenderMessage))]
        public void Render(Message message)
        {
            RenderMessage renderMessage = message as RenderMessage;
            RenderContext renderContext = renderMessage.RenderContext;

            UIManager.Render(renderContext);
        }
    }
}