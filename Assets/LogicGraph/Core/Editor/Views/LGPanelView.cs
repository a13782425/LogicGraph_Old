using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static Logic.Editor.LGCacheData;
namespace Logic.Editor
{
    public sealed class LGPanelView : GraphView
    {
        private LGWindow _window;
        private ToolbarView _toolbarView;
        public LGPanelView(LGWindow lgWindow)
        {
            _window = lgWindow;
            _toolbarView = new ToolbarView();
            this.Add(_toolbarView);
            m_addGridBackGround();
        }

        public void ShowLogic()
        {
            this._window.LGInfoCache.View = this;
            m_setToolBar();
        }

        #region 重写方法

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_window.LGInfoCache == null)
            {
                evt.menu.AppendAction("创建逻辑图", m_onCreateLogic);
                evt.menu.AppendAction("打开逻辑图", m_onOpenLogic);
            }
            else
            {
                evt.menu.AppendAction("创建节点", null);
                evt.menu.AppendAction("创建便笺", null);
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("保存", null);
                evt.menu.AppendAction("另存为", null);
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("导出", null);
            }
        }

        #endregion

        #region 子类重写



        #endregion


        #region 私有方法
        /// <summary>
        /// 创建逻辑图
        /// </summary>
        /// <param name="obj"></param>
        private void m_onCreateLogic(DropdownMenuAction obj)
        {
            var menuWindowProvider = ScriptableObject.CreateInstance<CreateLGSearchWindow>();
            menuWindowProvider.onSelectHandler += m_onCreateMenuSelectEntry;
            Vector2 screenPos = _window.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
        }

        /// <summary>
        /// 打开逻辑图
        /// </summary>
        /// <param name="obj"></param>
        private void m_onOpenLogic(DropdownMenuAction obj)
        {
            var menuWindowProvider = ScriptableObject.CreateInstance<OpenLGSearchWindow>();
            menuWindowProvider.onSelectHandler += m_onOpenMenuSelectEntry;

            Vector2 screenPos = _window.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
        }

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
            path = path.Replace(Application.dataPath, "Assets");
            var (start, startCache) = m_createStartNode();
            graph.StartNode = start;
            graph.LogicNodeList.Add(start);
            AssetDatabase.CreateAsset(graph, path);
            LGInfoCache graphCache = new LGInfoCache(graph);
            graphCache.Title = file;
            graphCache.AssetPath = path;
            graphCache.Nodes.Add(startCache);
            Instance.LGInfoList.Add(graphCache);
            LGCacheOp.Save();
            _window.SetLogic(graphCache);
            return true;
        }
        private bool m_onOpenMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            return true;
        }

        private (StartNode, LNInfoCache) m_createStartNode()
        {
            StartNode start = new StartNode();
            LNInfoCache cache = new LNInfoCache(start);
            cache.Pos = Vector2.zero;
            cache.Title = "开始";
            return (start, cache);
        }

        /// <summary>
        /// 设置工具条
        /// </summary>
        private void m_setToolBar()
        {
        }

        #endregion



        private class LGPanelViewGrid : GridBackground { }
        /// <summary>
        /// 添加背景网格
        /// </summary>
        private void m_addGridBackGround()
        {
            //添加网格背景
            GridBackground gridBackground = new LGPanelViewGrid();
            gridBackground.name = "GridBackground";
            Insert(0, gridBackground);
            //设置背景缩放范围
            this.SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);

            //扩展大小与父对象相同
            this.StretchToParentSize();
        }
    }
}
