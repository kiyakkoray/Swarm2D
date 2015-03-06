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
using System.Collections;
using System.Collections.Generic;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View;
using Swarm2D.Unity.Logic;
using UnityEngine;

namespace Swarm2D.UnityFramework
{
    public class UnityBehaviour : MonoBehaviour, IUnityBehaviour
    {
        private UnityLogicFramework _logicFramework;
        private UnityViewFramework _viewFramework;

        private Engine.Core.Engine _engine;

        void Start()
        {
            _logicFramework = new UnityLogicFramework(this);
            _viewFramework = new UnityViewFramework();

            _engine = new Engine.Core.Engine();

            Entity rootEntity = _engine.RootEntity;

            rootEntity.AddComponent<IOSystem>();
            rootEntity.AddComponent<DebugSpriteLoader>();
            //rootEntity.AddComponent<JamController>();
            rootEntity.AddComponent<NetworkController>();
            rootEntity.AddComponent<Swarm2D.Engine.Logic.NetworkView>();
            //rootEntity.AddComponent<JamMainClientGUI>();

            _engine.Start();

            _logicFramework.Initialize("SxTest", new FrameworkDomain[] { _engine });
            _logicFramework.Start();

            Swarm2D.Library.Debug.Log("wuhuu");
        }

        void Update()
        {
            _logicFramework.Update();
        }

        void OnDestroy()
        {
            Debug.Log("Swarm2D. engine destroying");

            _logicFramework.Destroy();
            _engine = null;
            _logicFramework = null;
            _viewFramework = null;
        }

        void OnPostRender()
        {
            //_logicFramework.LateUpdate();
        }
    }

    public class DebugSpriteLoader : EngineComponent
    {
        protected override void OnStart()
        {
            base.OnStart();

            LoadTemporarySprites();
        }

        private void LoadTemporarySprites()
        {
            SpriteData spriteData = new SpriteData("spriteData");
            spriteData.Load();

            SpriteCategory otherSpriteCategory = spriteData.SpriteCategories["Other"];
            otherSpriteCategory.Load();
        }
    }
}