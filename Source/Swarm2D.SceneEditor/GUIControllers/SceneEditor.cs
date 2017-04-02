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
using System.Reflection;
using System.Text;
using System.Xml;
using Swarm2D.Engine.Core;
using Swarm2D.Engine.View;
using Swarm2D.Engine.View.GUI;
using Swarm2D.Engine.View.GUI.PositionParameters;
using Swarm2D.Library;
using Swarm2D.Engine.Logic;

namespace Swarm2D.SceneEditor.GUIControllers
{
    public class SceneEditor : EditPanel
    {
        private EditorGUI _editorGUI;

        #region Left Panel

        private UIFrame _leftPanel;

        private UIButton _createNewSceneEntityButton;
        private UIButton _createNewSceneEntityFromPrefabButton;
        private UIButton _sceneEntityProperties;
        private UITreeView _currentSceneObjectsList;

        #endregion

        #region Top Panel

        private UIFrame _editPanelMenu;

        private UIButton _inGameLabel;
        private UIButton _editModeLabel;
        private UIButton _verticalMixedModeLabel;
        private UIButton _horizontalMixedModeLabel;

        private UIButton _startGameButton;
        private UIButton _stopGameButton;
        private UIButton _pauseResumeGameButton;
        private UILabel _currentFrameLabel;

        #endregion

        #region Current Entity Menu

        private UIContextMenu _currentSceneObjectMenu;
        private UIPositionParameter _currentSceneObjectTopParameter;
        private UIPositionParameter _currentSceneObjectLeftParameter;

        #endregion

        #region Current New Entity Menu

        private UIContextMenu _currentNewEntityFromPrefabMenu;

        #endregion

        private UIFrame _entityPropertiesPanel;
        private EntityPropertiesPanel _objectPropertiesPanel;

        private UIContextMenu _selectEntityAroundMenu;
        private AnchorToSide _selectEntityAroundMenuTopParameter;
        private AnchorToSide _selectEntityAroundMenuLeftParameter;

        #region Scene Edit Panel

        private UIFrame _scenePanel;

        private UIFrame _editPanelScreens;
        private UIFrame _editPanelScreen;

        private UIFrame _editorGameScreen;

        private UIFrame _inGamePanelScreen;

        private UIFrame _editPanelScreensVerticalMidLine;
        private UIFrame _editPanelScreensHorizontalMidLine;

        private UIPositionParameter _scenePanelRightAnchorParameter;

        private UIPositionParameter _editPanelScreenBottomParameter;
        private UIPositionParameter _inGamePanelScreenTopParameter;

        private UIPositionParameter _editPanelScreenRightParameter;
        private UIPositionParameter _inGamePanelScreenLeftParameter;

        private Vector2 _editorCameraPosition;
        private Vector2 _editorCameraTransformPreviousPosition;
        private bool _editorScreenLeftMouseDown;
        private bool _editorScreenRightMouseDown;

        #endregion

        #region Scene Edit Panel Buttons

        public SceneEditorActionMode ActionMode { get; set; }

        private List<UIButton> _scenePanelButtons;

        private UIButton _noneModeButton;
        private UIButton _moveModeButton;
        private UIButton _selectModeButton;

        #endregion

        private SceneEditorPanelMode _currentPanelMode = SceneEditorPanelMode.EditMode;

        private GameLogic _gameLogic;
        private GameRenderer _gameRenderer;

        public Entity SelectedEntity { get; private set; }

        public Scene Scene { get; private set; }

        private SceneData _currentSceneData;

        private Dictionary<Type, List<Type>> _sceneEditorPluginTypes;
        private Dictionary<Component, List<ComponentEditor>> _sceneEditorPlugins;

        protected override void OnAdded()
        {
            base.OnAdded();

            _scenePanelButtons = new List<UIButton>();
            _sceneEditorPlugins = new Dictionary<Component, List<ComponentEditor>>();

            CollectSceneEditorPlugins();
        }

        private void CollectSceneEditorPlugins()
        {
            _sceneEditorPluginTypes = new Dictionary<Type, List<Type>>();

            Assembly[] pluginAssemblies = GetPluginAssemblies();

            foreach (var pluginAssembly in pluginAssemblies)
            {
                Type[] pluginTypes = GetPlugins(pluginAssembly);

                foreach (var pluginType in pluginTypes)
                {
                    object[] attributes = pluginType.GetCustomAttributes(typeof(SceneEditorPlugin), true);

                    SceneEditorPlugin sceneEditorPlugin = attributes[0] as SceneEditorPlugin;

                    EnsurePluginTypeDictionary(sceneEditorPlugin.ComponentType);

                    _sceneEditorPluginTypes[sceneEditorPlugin.ComponentType].Add(pluginType);
                }
            }
        }

        private void EnsurePluginTypeDictionary(Type type)
        {
            if (!_sceneEditorPluginTypes.ContainsKey(type))
            {
                _sceneEditorPluginTypes.Add(type, new List<Type>());
            }
        }

        private Assembly[] GetPluginAssemblies()
        {
            List<Assembly> pluginAssemblies = new List<Assembly>();

            Assembly pluginReferenceAssembly = typeof(SceneEditorPlugin).Assembly;
            AssemblyName pluginAssemblyName = pluginReferenceAssembly.GetName();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();

                foreach (var referencedAssemblyName in referencedAssemblies)
                {
                    if (referencedAssemblyName.FullName == pluginAssemblyName.FullName)
                    {
                        pluginAssemblies.Add(assembly);
                        break;
                    }
                }
            }

            return pluginAssemblies.ToArray();
        }

        private Type[] GetPlugins(Assembly pluginAssembly)
        {
            List<Type> pluginTypes = new List<Type>();

            Type[] allTypes = pluginAssembly.GetTypes();

            foreach (var type in allTypes)
            {
                object[] attributes = type.GetCustomAttributes(typeof(SceneEditorPlugin), true);

                if (attributes != null && attributes.Length > 0)
                {
                    pluginTypes.Add(type);
                }
            }

            return pluginTypes.ToArray();
        }

        private bool DoesSceneControllerHavePlugin(Type type)
        {
            return _sceneEditorPluginTypes.ContainsKey(type) && _sceneEditorPluginTypes[type].Count > 0;
        }

        public override void Initialize(EditorGUI editorGUI)
        {
            base.Initialize(editorGUI);

            _editorGUI = editorGUI;
            _gameLogic = editorGUI.GameLogic;
            _gameRenderer = editorGUI.GameRenderer;
        }

        private static SceneEditor CreateSceneEditor(EditorGUI editorGUI)
        {
            Entity sceneEditorEntity = editorGUI.EditPanelFrame.CreateChildEntity("sceneEditorPanel");
            sceneEditorEntity.AddComponent<UIFrame>();

            SceneEditor sceneEditor = sceneEditorEntity.AddComponent<SceneEditor>();
            sceneEditor.Initialize(editorGUI);

            FastGUI.FillFromXmlFile(@"..\Editor\sceneEditor.xml", sceneEditorEntity.GetComponent<UIWidget>());

            return sceneEditor;
        }

        public static SceneEditor NewScene(EditorGUI editorGUI)
        {
            SceneEditor sceneEditor = CreateSceneEditor(editorGUI);

            sceneEditor.Scene = editorGUI.GameLogic.CreateNewScene();
            sceneEditor.Scene.IsRunning = false;

            return sceneEditor;
        }

        public static SceneEditor OpenScene(EditorGUI editorGUI, XmlDocument sceneDataAsXml)
        {
            SceneEditor sceneEditor = CreateSceneEditor(editorGUI);

            SceneData sceneData = new SceneData();
            sceneData.LoadFromXML(sceneDataAsXml);

            sceneEditor.Scene = editorGUI.GameLogic.CreateNewScene();
            sceneEditor.Scene.IsRunning = false;
            sceneEditor.Scene.Load(sceneData);

            return sceneEditor;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            this.PrepareUI();

            ActionMode = SceneEditorActionMode.None;

            _noneModeButton = AddScenePanelButton("NoneModeButton");
            _noneModeButton.Text = "None";

            _moveModeButton = AddScenePanelButton("MoveModeButton");
            _moveModeButton.Text = "Move";

            _selectModeButton = AddScenePanelButton("SelectModeButton");
            _selectModeButton.Text = "Select";

            _noneModeButton.MouseClick += OnNoneModeButtonClick;
            _moveModeButton.MouseClick += OnMoveModeButtonClick;
            _selectModeButton.MouseClick += OnSelectModeButtonClick;
        }

        private void PrepareUI()
        {
            _gameRenderer.InputEnabled = false;
            _gameRenderer.RenderEnabled = false;

            _editorScreenLeftMouseDown = false;

            _entityPropertiesPanel = ControllerFrame.GetChildAtPath("EntityPropertiesPanel");
            _scenePanel = ControllerFrame.GetChildAtPath("ScenePanel");

            {
                _editPanelMenu = _scenePanel.GetChildAtPath("EditPanelMenu");

                _inGameLabel = (UIButton)_editPanelMenu.GetChildAtPath("InGameLabel");
                _editModeLabel = (UIButton)_editPanelMenu.GetChildAtPath("EditModeLabel");
                _verticalMixedModeLabel = (UIButton)_editPanelMenu.GetChildAtPath("MixedModeLabel");
                _horizontalMixedModeLabel = (UIButton)_editPanelMenu.GetChildAtPath("MixedModeLabel2");
                _currentFrameLabel = (UILabel)_editPanelMenu.GetChildAtPath("CurrentFrameLabel");
                _startGameButton = (UIButton)_editPanelMenu.GetChildAtPath("StartGameButton");
                _stopGameButton = (UIButton)_editPanelMenu.GetChildAtPath("StopGameButton");
                _pauseResumeGameButton = (UIButton)_editPanelMenu.GetChildAtPath("PauseResumeGameButton");

                _inGameLabel.MouseClick += OnGameModeButtonClick;
                _editModeLabel.MouseClick += OnEditModeButtonClick;
                _verticalMixedModeLabel.MouseClick += OnMixedModeButtonClick;
                _horizontalMixedModeLabel.MouseClick += OnMixedMode2ButtonClick;
                _startGameButton.MouseClick += OnStartGameButtonClick;
                _stopGameButton.MouseClick += OnStopGameButtonClick;
                _pauseResumeGameButton.MouseClick += OnPauseResumeButtonClick;
            }

            {
                _editPanelScreens = _scenePanel.GetChildAtPath("EditPanelScreens");

                {
                    _editPanelScreen = _editPanelScreens.GetChildAtPath("EditPanelScreen");

                    _editorGameScreen = _editPanelScreen.GetChildAtPath("EditorGameScreen");

                    _editorGameScreen.AfterRender += AfterEditorGameScreenRender;
                    _editorGameScreen.MouseDown += OnEditorGameScreenMouseDown;
                    _editorGameScreen.MouseUp += OnEditorGameScreenMouseUp;
                    _editorGameScreen.MouseRightDown += OnEditorGameScreenMouseRightDown;
                    _editorGameScreen.MouseRightUp += OnEditorGameScreenMouseRightUp;

                    _editorGameScreen.Enabled = true;

                    _editPanelScreenBottomParameter = _editPanelScreen.Widget.PositionParameters[3];
                    _editPanelScreenRightParameter = _editPanelScreen.Widget.PositionParameters[2];

                }

                {
                    _inGamePanelScreen = _editPanelScreens.GetChildAtPath("InGamePanelScreen");

                    _inGamePanelScreen.AfterRender += AfterInGameScreenRender;

                    _inGamePanelScreen.Enabled = false;
                    _inGamePanelScreen.UseParentShowDebug = false;

                    _inGamePanelScreenTopParameter = _inGamePanelScreen.Widget.PositionParameters[0];
                    _inGamePanelScreenLeftParameter = _inGamePanelScreen.Widget.PositionParameters[1];
                }

                {
                    _editPanelScreensVerticalMidLine = _editPanelScreens.GetChildAtPath("EditPanelScreensVerticalMidLine");
                    _editPanelScreensVerticalMidLine.UseParentShowDebug = false;
                }


                {
                    _editPanelScreensHorizontalMidLine = _editPanelScreens.GetChildAtPath("EditPanelScreensHorizontalMidLine");
                    _editPanelScreensHorizontalMidLine.UseParentShowDebug = false;
                }
            }

            {
                _leftPanel = ControllerFrame.GetChildAtPath("EntitiesPanel");

                _createNewSceneEntityButton = (UIButton)_leftPanel.GetChildAtPath("CreateNewSceneEntityButton");
                _createNewSceneEntityFromPrefabButton = (UIButton)_leftPanel.GetChildAtPath("CreateNewSceneEntityFromPrefabButton");
                _sceneEntityProperties = (UIButton)_leftPanel.GetChildAtPath("SceneEntityProperties");
                _currentSceneObjectsList = (UITreeView)_leftPanel.GetChildAtPath("CurrentSceneObjectsList");

                _createNewSceneEntityButton.MouseClick += OnCreateNewSceneEntityButtonClick;
                _createNewSceneEntityFromPrefabButton.MouseClick += OnCreateNewSceneEntityFromPrefabMenuButtonClick;
                _sceneEntityProperties.MouseClick += OnSceneEntityPropertiesButtonClick;
                _currentSceneObjectsList.ItemMouseRightClick += OnCurrentSceneObjectsItemRightClick;

                CreateCurrentSceneObjectMenu();
                CreateCurrentNewEntityFromPrefabMenu();
            }

            UIFrame entityPropertiesPanelFrame = ControllerFrame.GetChildAtPath("EntityPropertiesPanel");
            _objectPropertiesPanel = entityPropertiesPanelFrame.AddComponent<EntityPropertiesPanel>();

            _scenePanelRightAnchorParameter = _scenePanel.Widget.PositionParameters[1];

            CreateSelectEntityAroundMenu();
        }

        public Vector2 MouseScenePosition
        {
            get
            {
                Vector2 mousePosition = EditorInput.MousePosition;

                Vector2 mouseScenePosition = mousePosition - new Vector2(_editorGameScreen.X, _editorGameScreen.Y)
                    + _editorCameraPosition - new Vector2(_editorGameScreen.Width * 0.5f, _editorGameScreen.Height * 0.5f);

                return mouseScenePosition;
            }
        }

        private void CreateCurrentSceneObjectMenu()
        {
            UIFrame rootFrame = _leftPanel.Manager.RootObject;

            List<UIPositionParameter> menuParameters = new List<UIPositionParameter>();

            _currentSceneObjectTopParameter = UIPositionParameter.AnchorToSideParameter(rootFrame.Widget, AnchorSide.Top, AnchorToSideType.Inner, 50);
            _currentSceneObjectLeftParameter = UIPositionParameter.AnchorToSideParameter(rootFrame.Widget, AnchorSide.Left, AnchorToSideType.Inner, 50);

            menuParameters.Add(_currentSceneObjectTopParameter);
            menuParameters.Add(_currentSceneObjectLeftParameter);

            _currentSceneObjectMenu = _leftPanel.Manager.CreateContextMenu(menuParameters);

            _currentSceneObjectMenu.Name = "SceneObjectMenu";

            ContextMenuItem addChildEntityButton = _currentSceneObjectMenu.AddItem("Add New Child");
            ContextMenuItem entityPropertiesButton = _currentSceneObjectMenu.AddItem("Properties");
            ContextMenuItem deleteEntityButton = _currentSceneObjectMenu.AddItem("Delete Entity");

            _currentSceneObjectMenu.Enabled = false;
            _currentSceneObjectMenu.BringToFront();

            addChildEntityButton.Click += new ContextMenuItemEvent(OnAddChildEntityButtonClick);
            entityPropertiesButton.Click += new ContextMenuItemEvent(OnEntityPropertiesButtonClick);
            deleteEntityButton.Click += new ContextMenuItemEvent(OnDeleteEntityButtonClick);
        }
        
        private void CreateCurrentNewEntityFromPrefabMenu()
        {
            UIFrame rootFrame = _leftPanel.Manager.RootObject;

            List<UIPositionParameter> menuParameters = new List<UIPositionParameter>();

            var currentNewEntityFromPrefabMenuTopParameter = UIPositionParameter.AnchorToSideParameter(_createNewSceneEntityFromPrefabButton.Widget, AnchorSide.Bottom, AnchorToSideType.Outer, 0);
            var currentNewEntityFromPrefabMenuLeftParameter = UIPositionParameter.AnchorToSideParameter(_createNewSceneEntityFromPrefabButton.Widget, AnchorSide.Left, AnchorToSideType.Inner, 0);

            menuParameters.Add(currentNewEntityFromPrefabMenuTopParameter);
            menuParameters.Add(currentNewEntityFromPrefabMenuLeftParameter);

            _currentNewEntityFromPrefabMenu = _leftPanel.Manager.CreateContextMenu(menuParameters);

            _currentNewEntityFromPrefabMenu.Name = "NewEntityFromPrefabMenu";

            foreach (var prefab in Engine.Prefabs)
            {
                ContextMenuItem prefabButton = _currentNewEntityFromPrefabMenu.AddItem(prefab.Name);
                prefabButton.Click += OnCreateNewSceneEntityFromPrefabButtonClick;
                prefabButton.Data = prefab.Name;
            }

            _currentNewEntityFromPrefabMenu.Enabled = false;
            _currentNewEntityFromPrefabMenu.BringToFront();
        }

        public override void Update()
        {
            if (_inGamePanelScreen.Enabled)
            {
                _gameRenderer.InputEnabled = _inGamePanelScreen.Manager.CurrentFocusObject == _inGamePanelScreen.Widget;
                _gameRenderer.SetRenderToScreenPart((int)_inGamePanelScreen.X, (int)_inGamePanelScreen.Y, (int)_inGamePanelScreen.Width, (int)_inGamePanelScreen.Height);
            }

            _currentFrameLabel.Text = "Current Frame: " + _gameLogic.ExecutedFrame;

            switch (ActionMode)
            {
                case SceneEditorActionMode.None:
                    break;
                case SceneEditorActionMode.Move:
                    UpdateActionMoveMode();
                    break;
                case SceneEditorActionMode.Select:
                    UpdateActionSelectMode();
                    break;
            }

            Vector2 mousePosition = EditorInput.MousePosition;
            _editorCameraTransformPreviousPosition = mousePosition;

            this.RefreshCurrentSceneObjectsListItems();

            if (!(_entityPropertiesPanel.Enabled && SelectedEntity == Scene.Entity))
            {
                SelectedEntity = _currentSceneObjectsList.SelectedNodeData as Entity;
            }

            _objectPropertiesPanel.Update();

            if (_editorScreenLeftMouseDown || _editorScreenRightMouseDown)
            {
                if (_editorGameScreen.Widget !=
                    ControllerFrame.Manager.RootWidget.GetFrontObject(ControllerFrame.Manager.PointerPosition.X, ControllerFrame.Manager.PointerPosition.Y))
                {
                    Entity.SendMessage(new EditorGameScreenMouseUpMessage(mousePosition));
                    _editorScreenLeftMouseDown = false;
                    _editorScreenRightMouseDown = false;
                }
            }

            this.CheckSceneControllerPlugins();
        }

        private void UpdateActionSelectMode()
        {
            Vector2 mousePosition = EditorInput.MousePosition;

            if (_editorScreenLeftMouseDown)
            {
                if (SelectedEntity != null)
                {
                    SceneEntity selectedSceneEntity = SelectedEntity.GetComponent<SceneEntity>();

                    selectedSceneEntity.LocalPosition += mousePosition - _editorCameraTransformPreviousPosition;
                }
            }

            if (_editorScreenRightMouseDown)
            {
                if (SelectedEntity != null)
                {
                    SceneEntity selectedSceneEntity = SelectedEntity.GetComponent<SceneEntity>();

                    Vector2 difference = mousePosition - _editorCameraTransformPreviousPosition;

                    float sign = difference.Angle > 0 ? 1.0f : -1.0f;

                    selectedSceneEntity.LocalRotation += sign * difference.Length;
                }
            }
        }

        private void UpdateActionMoveMode()
        {
            Vector2 mousePosition = EditorInput.MousePosition;

            if (_editorScreenLeftMouseDown)
            {
                _editorCameraPosition = _editorCameraPosition + _editorCameraTransformPreviousPosition - mousePosition;
            }
        }

        private void CheckSceneControllerPlugins()
        {
            if (SelectedEntity != null)
            {
                //add if not exists
                foreach (var component in SelectedEntity.Components)
                {
                    if (DoesSceneControllerHavePlugin(component.GetType()))
                    {
                        if (!_sceneEditorPlugins.ContainsKey(component))
                        {
                            _sceneEditorPlugins.Add(component, new List<ComponentEditor>());

                            foreach (var sceneEditorPluginType in _sceneEditorPluginTypes[component.GetType()])
                            {
                                Debug.Log("Creating scene controller plugin:" + sceneEditorPluginType.Name);

                                ComponentEditor pluginComponent = (ComponentEditor)Entity.AddComponent(sceneEditorPluginType);
                                pluginComponent.Component = component;
                                _sceneEditorPlugins[component].Add(pluginComponent);
                            }
                        }
                    }
                }
            }

            //remove if deleted
            List<Component> destroyedSceneControllers = new List<Component>();

            foreach (var sceneController in _sceneEditorPlugins.Keys)
            {
                if (sceneController.IsDestroyed || SelectedEntity != sceneController.Entity)
                {
                    destroyedSceneControllers.Add(sceneController);
                }
            }

            foreach (var destroyedSceneController in destroyedSceneControllers)
            {
                List<ComponentEditor> plugins = _sceneEditorPlugins[destroyedSceneController];

                _sceneEditorPlugins.Remove(destroyedSceneController);

                foreach (var component in plugins)
                {
                    Debug.Log("Destroying scene controller plugin:" + component.GetType().Name);

                    component.Entity.DeleteComponent(component);
                }
            }
        }

        private void SelectEntity(Entity entity)
        {
            SelectedEntity = entity;
            _currentSceneObjectsList.SelectNodeWithData(entity);
        }

        public void ShowEntityProperties()
        {
            if (!_entityPropertiesPanel.Enabled)
            {
                _scenePanelRightAnchorParameter.ChangeAnchorToParameter(_objectPropertiesPanel.ControllerFrame.Widget);

                _entityPropertiesPanel.Enabled = true;
                _leftPanel.SetNonUpdatedWithChildrenOnAllDomains();
            }
        }

        public void HideEntityProperties()
        {
            if (_entityPropertiesPanel.Enabled)
            {
                _scenePanelRightAnchorParameter.ChangeAnchorToParameter(_leftPanel.Widget);

                _entityPropertiesPanel.Enabled = false;
                _leftPanel.SetNonUpdatedWithChildrenOnAllDomains();
            }
        }

        private void SetPanelMode(SceneEditorPanelMode mode)
        {
            switch (_currentPanelMode)
            {
                case SceneEditorPanelMode.EditMode:
                    {
                        _editPanelScreen.Enabled = false;
                    }
                    break;
                case SceneEditorPanelMode.InGame:
                    {
                        _gameRenderer.InputEnabled = false;

                        _inGamePanelScreen.Enabled = false;
                    }
                    break;
                case SceneEditorPanelMode.VerticalMixedMode:
                    {
                        _gameRenderer.InputEnabled = false;

                        _inGamePanelScreen.Enabled = false;
                        _editPanelScreen.Enabled = false;

                        _editPanelScreenBottomParameter.ChangeAnchorToParameter(_editPanelScreens.Widget);
                        _inGamePanelScreenTopParameter.ChangeAnchorToParameter(_editPanelScreens.Widget);
                    }
                    break;
                case SceneEditorPanelMode.HorizontalMixedMode:
                    {
                        _gameRenderer.InputEnabled = false;

                        _inGamePanelScreen.Enabled = false;
                        _editPanelScreen.Enabled = false;

                        _editPanelScreenRightParameter.ChangeAnchorToParameter(_editPanelScreens.Widget);
                        _inGamePanelScreenLeftParameter.ChangeAnchorToParameter(_editPanelScreens.Widget);
                    }
                    break;
                default:
                    break;
            }

            switch (mode)
            {
                case SceneEditorPanelMode.EditMode:
                    {
                        _editPanelScreen.Enabled = true;
                    }
                    break;
                case SceneEditorPanelMode.InGame:
                    {
                        _gameRenderer.InputEnabled = true;

                        _inGamePanelScreen.Enabled = true;
                    }
                    break;
                case SceneEditorPanelMode.VerticalMixedMode:
                    {
                        _editPanelScreen.Enabled = true;

                        _gameRenderer.InputEnabled = true;

                        _inGamePanelScreen.Enabled = true;

                        _editPanelScreenBottomParameter.ChangeAnchorToParameter(_editPanelScreensVerticalMidLine.Widget);
                        _inGamePanelScreenTopParameter.ChangeAnchorToParameter(_editPanelScreensVerticalMidLine.Widget);
                    }
                    break;
                case SceneEditorPanelMode.HorizontalMixedMode:
                    {
                        _editPanelScreen.Enabled = true;

                        _gameRenderer.InputEnabled = true;

                        _inGamePanelScreen.Enabled = true;

                        _editPanelScreenRightParameter.ChangeAnchorToParameter(_editPanelScreensHorizontalMidLine.Widget);
                        _inGamePanelScreenLeftParameter.ChangeAnchorToParameter(_editPanelScreensHorizontalMidLine.Widget);
                    }
                    break;
                default:
                    break;
            }

            _editPanelScreens.SetNonUpdatedWithChildrenOnAllDomains();
            _currentPanelMode = mode;
        }

        private List<Entity> GetEntitiesAround(Vector2 scenePosition)
        {
            List<Entity> result = new List<Entity>();

            PhysicsWorld physicsWorld = Scene.GetComponent<PhysicsWorld>();
            SceneRenderer sceneRenderer = Scene.GetComponent<SceneRenderer>();

            if (physicsWorld != null)
            {
                List<PhysicsObject> physicsObjects = new List<PhysicsObject>();

                physicsWorld.GetPhysicsObjectsIn(scenePosition, 2.0f, physicsObjects);

                for (int i = 0; i < physicsObjects.Count; i++)
                {
                    if (!result.Contains(physicsObjects[i].Entity))
                    {
                        result.Add(physicsObjects[i].Entity);
                    }
                }
            }

            if (sceneRenderer != null)
            {
                List<Renderer> renderers = new List<Renderer>();

                sceneRenderer.GetRendererIn(scenePosition, renderers);

                for (int i = 0; i < renderers.Count; i++)
                {
                    if (!result.Contains(renderers[i].Entity))
                    {
                        result.Add(renderers[i].Entity);
                    }
                }
            }

            return result;
        }

        private void CreateSelectEntityAroundMenu()
        {
            UIFrame rootFrame = ControllerFrame.Manager.RootObject;

            List<UIPositionParameter> menuParameters = new List<UIPositionParameter>();

            _selectEntityAroundMenuTopParameter = UIPositionParameter.AnchorToSideParameter(rootFrame.Widget, AnchorSide.Top, AnchorToSideType.Inner, 50);
            _selectEntityAroundMenuLeftParameter = UIPositionParameter.AnchorToSideParameter(rootFrame.Widget, AnchorSide.Left, AnchorToSideType.Inner, 50);

            menuParameters.Add(_selectEntityAroundMenuTopParameter);
            menuParameters.Add(_selectEntityAroundMenuLeftParameter);

            _selectEntityAroundMenu = ControllerFrame.Manager.CreateContextMenu(menuParameters);

            _selectEntityAroundMenu.Name = "SelectEntityAroundMenu";

            //foreach (ComponentInfo componentInfo in Engine.Engine.ComponentInfos)
            //{
            //	UIFrame componentButton = _selectEntityAroundMenu.AddItem(componentInfo.Name);
            //
            //	componentButton.DataObject = componentInfo;
            //
            //	componentButton.MouseClick += new UIMouseEvent(OnAddComponentButtonClick);
            //}

            _selectEntityAroundMenu.Enabled = false;
            _selectEntityAroundMenu.BringToFront();
        }

        private void RefreshCurrentSceneObjectsListItems()
        {
            if (Scene != null)
            {
                Tree<Entity> tree = new Tree<Entity>();
                tree.Root = new TreeNode<Entity>();

                int addCount = 0;

                while (addCount != Scene.SceneEntities.Count)
                {
                    foreach (SceneEntity sceneEntity in Scene.SceneEntities)
                    {
                        if (tree.FindTreeNodeWithData(sceneEntity.Entity) == null)
                        {
                            TreeNode<Entity> childNode = new TreeNode<Entity>();
                            childNode.Data = sceneEntity.Entity;

                            Entity parentEntity = sceneEntity.Parent != null ? sceneEntity.Parent.Entity : null;
                            TreeNode<Entity> parentNode = tree.FindTreeNodeWithData(parentEntity);

                            if (parentNode != null)
                            {
                                addCount++;
                                parentNode.Children.Add(childNode);
                            }
                        }
                    }
                }

                _currentSceneObjectsList.SynchronizeWithTree(tree);

                //foreach (Entity entity in Scene.Entities)
                //{
                //	_currentSceneObjectsList.AddNode(entity);
                //}

                //_currentSceneObjectsList.SynchronizeWithList(Scene.Entities);
            }
        }

        public UIButton AddScenePanelButton(string name)
        {
            Entity buttonEntity = _editPanelScreen.CreateChildEntity(name);
            UIButton button = buttonEntity.AddComponent<UIButton>();

            List<UIPositionParameter> positionParameters = new List<UIPositionParameter>();

            positionParameters.Add(UIPositionParameter.AnchorToSideParameter(_editPanelScreen.Widget, AnchorSide.Top, AnchorToSideType.Inner, 5));
            positionParameters.Add(UIPositionParameter.AnchorToSideParameter(_editPanelScreen.Widget, AnchorSide.Left, AnchorToSideType.Inner, 5 + 85 * _scenePanelButtons.Count));
            positionParameters.Add(UIPositionParameter.SetWidth(80));
            positionParameters.Add(UIPositionParameter.SetHeight(40));

            button.Initialize(positionParameters);

            _scenePanelButtons.Add(button);

            return button;
        }

        public void RemoveScenePanelButton(UIButton button)
        {
            _scenePanelButtons.Remove(button);

            button.Entity.Destroy();

            for (int i = 0; i < _scenePanelButtons.Count; i++)
            {
                UIButton currentButton = _scenePanelButtons[i];

                AnchorToSide anchorToSide = currentButton.Widget.PositionParameters[1] as AnchorToSide;
                anchorToSide.Value = 5 + 85 * i;

                currentButton.SetNonUpdatedWithChildrenOnAllDomains();
            }
        }

        #region UI Events

        private void OnGameModeButtonClick(UIWidget sender, MouseEventArgs e)
        {
            SetPanelMode(SceneEditorPanelMode.InGame);
        }

        private void OnEditModeButtonClick(UIWidget sender, MouseEventArgs e)
        {
            SetPanelMode(SceneEditorPanelMode.EditMode);
        }

        private void OnMixedModeButtonClick(UIWidget sender, MouseEventArgs e)
        {
            SetPanelMode(SceneEditorPanelMode.VerticalMixedMode);
        }

        private void OnMixedMode2ButtonClick(UIWidget sender, MouseEventArgs e)
        {
            SetPanelMode(SceneEditorPanelMode.HorizontalMixedMode);
        }

        private void OnNoneModeButtonClick(UIWidget sender, MouseEventArgs e)
        {
            ActionMode = SceneEditorActionMode.None;
        }

        private void OnMoveModeButtonClick(UIWidget sender, MouseEventArgs e)
        {
            ActionMode = SceneEditorActionMode.Move;
        }

        private void OnSelectModeButtonClick(UIWidget sender, MouseEventArgs e)
        {
            ActionMode = SceneEditorActionMode.Select;
        }

        private void AfterInGameScreenRender(UIFrame sender, RenderContext renderContext)
        {
            RenderContext gameRenderContext = renderContext.AddChildRenderContext(0);
            gameRenderContext.SetScissor((int)sender.X, (int)sender.Y, (int)sender.Width, (int)sender.Height);

            _gameRenderer.Render(gameRenderContext);
        }

        private void AfterEditorGameScreenRender(UIFrame sender, RenderContext renderContext)
        {
            if (Scene != null)
            {
                SceneRenderer sceneRenderer = Scene.GetComponent<SceneRenderer>();

                if (sceneRenderer != null)
                {
                    Vector2 uiPosition = new Vector2(sender.X, sender.Y);

                    RenderContext editorRenderContext = renderContext.AddChildRenderContext(0);

                    editorRenderContext.SetScissor((int)sender.X, (int)sender.Y, (int)sender.Width, (int)sender.Height);
                    editorRenderContext.ViewMatrix = Matrix4x4.Position2D(uiPosition + _editorCameraPosition * -1.0f + new Vector2(sender.Width * 0.5f, sender.Height * 0.5f));

                    Box renderBox = new Box();

                    renderBox.Size = new Vector2(sender.Width, sender.Height);
                    renderBox.Position = _editorCameraPosition - new Vector2(sender.Width * 0.5f, sender.Height * 0.5f);

                    sceneRenderer.Render(editorRenderContext, renderBox);
                }
            }
        }

        private void OnSelectEntityAroundMenuItemClick(ContextMenuItem item)
        {
            SelectEntity(item.Data as Entity);
            _selectEntityAroundMenu.Enabled = false;
        }

        private void OnEditorGameScreenMouseDown(UIWidget sender, MouseEventArgs e)
        {
            _editorScreenLeftMouseDown = true;
            _editorCameraTransformPreviousPosition = EditorInput.MousePosition;

            Entity.SendMessage(new EditorGameScreenMouseDownMessage(MouseScenePosition));

            if (ActionMode == SceneEditorActionMode.Select)
            {
                List<Entity> entities = GetEntitiesAround(MouseScenePosition);

                if (EditorInput.GetKey(KeyCode.KeyShift))
                {
                    if (entities.Count > 0)
                    {
                        _selectEntityAroundMenuLeftParameter.Value = e.X;
                        _selectEntityAroundMenuTopParameter.Value = e.Y;

                        _selectEntityAroundMenu.Enabled = true;
                        _selectEntityAroundMenu.BringToFront();

                        _selectEntityAroundMenu.SetNonUpdatedWithChildrenOnAllDomains();

                        _selectEntityAroundMenu.ClearItems();

                        for (int i = 0; i < entities.Count; i++)
                        {
                            ContextMenuItem item = _selectEntityAroundMenu.AddItem(entities[i].Name);
                            item.Data = entities[i];
                            item.Click += new ContextMenuItemEvent(OnSelectEntityAroundMenuItemClick);
                        }
                    }
                }
                else
                {
                    if (entities.Count > 0)
                    {
                        SelectEntity(entities[0]);
                    }
                    else
                    {
                        SelectEntity(null);
                    }
                }
            }
        }

        private void OnEditorGameScreenMouseUp(UIWidget sender, MouseEventArgs e)
        {
            _editorScreenLeftMouseDown = false;
            Entity.SendMessage(new EditorGameScreenMouseUpMessage(MouseScenePosition));
        }

        private void OnEditorGameScreenMouseRightDown(UIWidget sender, MouseEventArgs e)
        {
            _editorScreenRightMouseDown = true;
        }

        private void OnEditorGameScreenMouseRightUp(UIWidget sender, MouseEventArgs e)
        {
            _editorScreenRightMouseDown = false;
        }

        private void OnStartGameButtonClick(UIWidget sender, MouseEventArgs e)
        {
            if (!_gameLogic.IsGameStarted)
            {
                Debug.Log("game started");

                _currentSceneData = Scene.Save();

                _gameLogic.StartGame();
            }
        }

        private void OnStopGameButtonClick(UIWidget sender, MouseEventArgs e)
        {
            if (_gameLogic.IsRunning)
            {
                Debug.Log("game stopped");
                _gameLogic.StopGame();

                Scene = _gameLogic.CreateNewScene();
                Scene.IsRunning = false;
                Scene.Load(_currentSceneData);

                _currentSceneData = null;
            }
        }

        private void OnPauseResumeButtonClick(UIWidget sender, MouseEventArgs e)
        {
            if (_gameLogic.IsRunning)
            {
                Debug.Log("game paused");
                _gameLogic.PauseGame();
            }
            else
            {
                Debug.Log("game resumed");
                _gameLogic.ResumeGame();
            }
        }

        private void OnCreateNewSceneEntityButtonClick(UIWidget sender, MouseEventArgs e)
        {
            if (Scene != null)
            {
                Scene.CreateChildEntity("newSceneEntity");
            }
        }

        private void OnCreateNewSceneEntityFromPrefabMenuButtonClick(UIWidget sender, MouseEventArgs e)
        {
            if (Scene != null)
            {
                _currentNewEntityFromPrefabMenu.Enabled = true;
                _currentNewEntityFromPrefabMenu.BringToFront();
                _currentNewEntityFromPrefabMenu.SetNonUpdatedWithChildrenOnAllDomains();
            }
        }

        private void OnCreateNewSceneEntityFromPrefabButtonClick(ContextMenuItem item)
        {
            if (Scene != null)
            {
                string prefabName = (string)item.Data;

                Engine.InstantiatePrefab(prefabName, Scene);
            }
        }

        private void OnSceneEntityPropertiesButtonClick(UIWidget sender, MouseEventArgs e)
        {
            if (Scene != null)
            {
                SelectedEntity = Scene.Entity;
                ShowEntityProperties();
            }
        }

        private void OnCurrentSceneObjectsItemRightClick(UITreeView sender, MouseEventArgs e)
        {
            _currentSceneObjectLeftParameter.Value = e.X;
            _currentSceneObjectTopParameter.Value = e.Y;

            _currentSceneObjectMenu.Enabled = true;
            _currentSceneObjectMenu.BringToFront();
            _currentSceneObjectMenu.SetNonUpdatedWithChildrenOnAllDomains();
        }

        private void OnAddChildEntityButtonClick(ContextMenuItem item)
        {
            if (Scene != null)
            {
                SceneEntity parentSceneEntity = (_currentSceneObjectsList.SelectedNodeData as Entity).GetComponent<SceneEntity>();
                SceneEntity childEntity = parentSceneEntity.CreateChildEntity("newSceneEntity").GetComponent<SceneEntity>();
            }
        }

        private void OnEntityPropertiesButtonClick(ContextMenuItem item)
        {
            ShowEntityProperties();
        }

        private void OnDeleteEntityButtonClick(ContextMenuItem item)
        {
            Entity selectedSceneObject = _currentSceneObjectsList.SelectedNodeData as Entity;
            selectedSceneObject.Destroy();

            _currentSceneObjectsList.SelectedNode = null;

            if (SelectedEntity == selectedSceneObject)
            {
                HideEntityProperties();
                SelectedEntity = null;
            }

            //defaultScene.RemoveEntityRecursively(selectedSceneObject);
        }

        #endregion
    }

    public enum SceneEditorActionMode
    {
        None,
        Move,
        Select
    }

    enum SceneEditorPanelMode
    {
        EditMode,
        InGame,
        VerticalMixedMode,
        HorizontalMixedMode,
    }

    public class SceneEditorPlugin : Attribute
    {
        public Type ComponentType { get; set; }
    }

    public abstract class ComponentEditor : UIController
    {
        public Component Component { get; internal set; }
    }

    public class EditorGameScreenMouseDownMessage : EntityMessage
    {
        public Vector2 MouseScenePosition { get; private set; }

        public EditorGameScreenMouseDownMessage(Vector2 mouseScenePosition)
        {
            MouseScenePosition = mouseScenePosition;
        }
    }

    public class EditorGameScreenMouseUpMessage : EntityMessage
    {
        public Vector2 MouseScenePosition { get; private set; }

        public EditorGameScreenMouseUpMessage(Vector2 mouseScenePosition)
        {
            MouseScenePosition = mouseScenePosition;
        }
    }
}
