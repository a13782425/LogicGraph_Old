using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Logic.Editor
{
    public sealed class LGWindow : EditorWindow
    {
        private BaseGraphView _view;

        /// <summary>
        /// 逻辑图唯一Id
        /// </summary>
        private string _graphOnlyId = null;
        public LGInfoCache LGInfoCache => _lgInfoCache;
        private LGInfoCache _lgInfoCache = default;
        public LGEditorCache LGEditorCache => _lgEditorCache;
        private LGEditorCache _lgEditorCache = default;

        //private BaseLogicGraphView _logicGraphView = null;

        private VisualElement _top;
        private VisualElement _center;
        private VisualElement _bottom;

        private ContextualMenuManipulator _centerMenu;
        /// <summary>
        /// 工具栏面板
        /// </summary>
        private ToolbarView _topToolbar;
        private ToolbarView _bottomToolbar;

        [MenuItem("Framework/逻辑图/打开逻辑图", priority = 99)]
        private static void OpenLogic()
        {
            ShowLogic(null);
        }

        /// <summary>
        /// 显示逻辑图面板
        /// </summary>
        /// <param name="p"></param>
        public static void ShowLogic(string onlyId)
        {
            LGWindow panel = GetWindow(onlyId);
            if (panel == null)
            {
                panel = CreateWindow<LGWindow>();
                panel.minSize = LogicUtils.MIN_SIZE;
                LogicProvider.OpenWindow();
            }
            panel._graphOnlyId = onlyId;
            panel.ShowLogic();
            panel.Focus();
        }


        private void ShowLogic()
        {
            if (!string.IsNullOrWhiteSpace(_graphOnlyId))
            {
                LGInfoCache graphInfo = LogicProvider.GetLogicInfo(_graphOnlyId);
                if (graphInfo == null)
                {
                    this.Close();
                }
                else
                {
                    LGEditorCache editorCache = LogicProvider.GetEditorCache(graphInfo);
                    BaseLogicGraph graph = AssetDatabase.LoadAssetAtPath<BaseLogicGraph>(graphInfo.AssetPath);
                    //删除没有的节点
                    graph.Nodes.RemoveAll(n => n == null);
                    graph.Init();
                    _view = Activator.CreateInstance(editorCache.ViewType) as BaseGraphView;
                    _view.Initialize(this, graph, editorCache);
                    _center.RemoveManipulator(_centerMenu);
                    _center.Add(_view);
                    LogicProvider.OpenLogicGraph(graph);
                }
            }


        }
        private static LGWindow GetWindow(string onlyId)
        {
            Object[] panels = Resources.FindObjectsOfTypeAll(typeof(LGWindow));
            LGWindow panel = null;
            foreach (var item in panels)
            {
                if (item is LGWindow p)
                {
                    if (p._graphOnlyId == onlyId)
                    {
                        panel = p;
                        break;
                    }
                }
            }
            return panel;
        }
        #region 内置函数

        private void OnEnable()
        {
            m_createUI();

            if (!string.IsNullOrWhiteSpace(_graphOnlyId))
            {
                ShowLogic();
            }
        }



        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnDestroy();
            }
        }
        #endregion

        #region 私有方法

        private void m_createUI()
        {
            titleContent = new GUIContent("逻辑图");
            _top = new VisualElement();
            _top.name = "top";
            _top.style.height = 21;
            _top.style.flexGrow = 0;
            _center = new VisualElement();
            _center.name = "center";
            _center.style.flexGrow = 1;
            _centerMenu = new ContextualMenuManipulator(m_buildContextualMenu);
            _center.AddManipulator(_centerMenu);
            _bottom = new VisualElement();
            _bottom.name = "bottom";
            _bottom.style.height = 21;
            _bottom.style.flexGrow = 0;

            this.rootVisualElement.Add(_top);
            this.rootVisualElement.Add(_center);
            this.rootVisualElement.Add(_bottom);
            _topToolbar = new ToolbarView();
            _topToolbar.onDrawLeft += _toolbarView_onDrawLeft;
            _top.Add(_topToolbar);
            _bottomToolbar = new ToolbarView();
            _bottomToolbar.onDrawLeft += _toolbarView_onDrawLeft;
            _bottom.Add(_bottomToolbar);
        }

        private void m_buildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            while (evt.menu.MenuItems().Count > 0)
            {
                evt.menu.RemoveItemAt(0);
            }

            evt.menu.AppendAction("创建逻辑图", m_onCreateLogic, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("打开逻辑图", m_onOpenLogic, DropdownMenuAction.AlwaysEnabled);
        }
        private void _toolbarView_onDrawLeft()
        {
            if (GUILayout.Button("菜单", EditorStyles.toolbarButton))
            {
                Debug.LogError("点击了菜单");
            }
        }
        /// <summary>
        /// 创建逻辑图搜索框
        /// </summary>
        /// <param name="obj"></param>
        private void m_onCreateLogic(DropdownMenuAction obj)
        {
            var menuWindowProvider = ScriptableObject.CreateInstance<CreateLGSearchWindow>();
            menuWindowProvider.onSelectHandler += m_onCreateMenuSelectEntry;
            Vector2 screenPos = this.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
        }

        /// <summary>
        /// 打开逻辑图搜索框
        /// </summary>
        /// <param name="obj"></param>
        private void m_onOpenLogic(DropdownMenuAction obj)
        {
            var menuWindowProvider = ScriptableObject.CreateInstance<OpenLGSearchWindow>();
            menuWindowProvider.onSelectHandler += m_onOpenMenuSelectEntry;

            Vector2 screenPos = this.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
        }

        /// <summary>
        /// 创建逻辑图搜索框回调
        /// </summary>
        /// <param name="searchTreeEntry"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool m_onCreateMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            LGEditorCache configData = searchTreeEntry.userData as LGEditorCache;
            string path = EditorUtility.SaveFilePanel("创建逻辑图", Application.dataPath, "LogicGraph", "asset");
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("错误", "路径为空", "确定");
                return false;
            }
            if (File.Exists(path))
            {
                EditorUtility.DisplayDialog("错误", "创建文件已存在", "确定");
                return false;
            }
            string file = Path.GetFileNameWithoutExtension(path);
            BaseLogicGraph graph = ScriptableObject.CreateInstance(configData.GraphClassName) as BaseLogicGraph;
            graph.name = file;

            if (graph.DefaultVars != null)
            {
                foreach (var item in graph.DefaultVars)
                {
                    item.CanRename = false;
                    item.CanDel = false;
                    graph.Variables.Add(item);
                }
            }
            path = path.Replace(Application.dataPath, "Assets");
            graph.Title = file;
            AssetDatabase.CreateAsset(graph, path);
            this._graphOnlyId = graph.OnlyId;
            this.ShowLogic();
            return true;
        }

        /// <summary>
        /// 打开逻辑图搜索框回调
        /// </summary>
        /// <param name="searchTreeEntry"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool m_onOpenMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            LGInfoCache graphCache = searchTreeEntry.userData as LGInfoCache;
            this._graphOnlyId = graphCache.OnlyId;
            this.ShowLogic();
            return true;
        }

        #endregion
    }
}