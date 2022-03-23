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
        public string GraphOnlyId
        {
            get => _graphOnlyId; 
            set
            {
                _graphOnlyId = value;
                ShowLogic();
            }
        }
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
        public Action onDrawTopLeft;
        public Action onDrawTopRight;
        private ToolbarView _bottomToolbar;
        public Action onDrawBottomLeft;
        public Action onDrawBottomRight;

        private GraphListPanel _graphPanel;

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
            _topToolbar.onDrawLeft += m_onDrawTopLeft;
            _topToolbar.onDrawRight += m_onDrawTopRight;
            _top.Add(_topToolbar);
            _bottomToolbar = new ToolbarView();
            _bottomToolbar.onDrawLeft += m_onDrawBottomLeft;
            _bottomToolbar.onDrawRight += m_onDrawBottomRight;
            _bottom.Add(_bottomToolbar);
            _graphPanel = new GraphListPanel(this);
            this.rootVisualElement.Add(_graphPanel);
            _graphPanel.Hide();

        }

        private void m_buildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            while (evt.menu.MenuItems().Count > 0)
            {
                evt.menu.RemoveItemAt(0);
            }
        }
        private void m_onDrawTopLeft()
        {
            if (GUILayout.Button("菜单", EditorStyles.toolbarButton))
            {
                _graphPanel.Show();
            }
            onDrawTopLeft?.Invoke();
        }
        private void m_onDrawTopRight() => onDrawTopRight?.Invoke();
        private void m_onDrawBottomLeft() => onDrawBottomLeft?.Invoke();
        private void m_onDrawBottomRight() => onDrawBottomRight?.Invoke();

        #endregion
    }
}