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
using Swarm2D.Library;

namespace Swarm2D.SpriteEditor
{
    public class SpriteEditorDomain : EngineComponent
    {
        public UIFrame UIRootObject
        {
            get
            {
                return UIManager.RootObject;
            }
        }

        public UIManager UIManager { get; private set; }
        public SpriteEditorGUI EditorGUI { get; private set; }
        public IOSystem IOSystem { get; private set; }
        public ProjectEditor ProjectEditor { get; private set; }

        public SpriteData EditorSpriteData { get; private set; }
        public SpriteData SpriteData { get; private set; }

        protected override void OnStart()
        {
            SpriteData = new SpriteData("spriteData");
            SpriteData.Load();

            ProjectEditor = new ProjectEditor(SpriteData);

            EditorSpriteData = new SpriteData("Editor", "spriteData");

            IOSystem = Entity.GetComponent<IOSystem>();

            //EditorInput.Initialize(IOSystem);
            //EditorScreen.Initialize(IOSystem);

            //_gameDomain->SetUpdateScreenPosition(false);

            //LoadTemporarySprites();

            Entity uiManagerEntity = Entity.CreateChildEntity("UIManager");
            UIManager = uiManagerEntity.AddComponent<UIManager>();
            UIManager.Initialize(IOSystem);

            //UIManager.ShowAllBoxes = true;

            EditorGUI = new SpriteEditorGUI(this);
            //GameLogic.StopGame();

            ProjectEditor.Initialize(this);

            EditorSpriteData.Load();

            SpriteCategory otherSpriteCategory = EditorSpriteData.SpriteCategories["Other"];
            otherSpriteCategory.Load();

            UISkin skin = UIManager.Skin;

            skin.ButtonNormalSprite = EditorSpriteData.GetSprite(@"GUI\blackButtonNormal");
            skin.ButtonMouseDownSprite = EditorSpriteData.GetSprite(@"GUI\blackButtonMouseDown");
            skin.ButtonMouseOverSprite = EditorSpriteData.GetSprite(@"GUI\blackButtonMouseOver");

            skin.FrameBoxSprite = skin.ButtonNormalSprite;
        }

        [DomainMessageHandler(MessageType = typeof(UpdateMessage))]
        private void OnUpdate(Message message)
        {
            //if (GetUpdateScreenPosition())
            //{
            //	Width(GetGraphicsForm()->Width());
            //	Height(GetGraphicsForm()->Height());
            //}

            //if (IOSystem != null)
            //{
            //	IOSystem.UpdateInput();
            //}

            EditorGUI.UpdateLoop();
            UIManager.Update();
            ProjectEditor.Update();

            //if (GetUpdateScreenPosition())
            //{
            //	_gameDomain->Width(_editorGUI->GetInGameScreen()->Width());
            //	_gameDomain->Height(_editorGUI->GetInGameScreen()->Height());
            //	_gameDomain->Position(_editorGUI->GetInGameScreen()->ScreenX(), _editorGUI->GetInGameScreen()->ScreenY());
            //}
        }

        [DomainMessageHandler(MessageType = typeof(RenderMessage))]
        public void Render(Message message)
        {
            RenderMessage renderMessage = message as RenderMessage;
            IOSystem ioSystem = renderMessage.IOSystem;

            UIManager.Render(ioSystem);
        }
    }
}