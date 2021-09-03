using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Logic.Editor
{
    public sealed class LGWindow : EditorWindow
    {

        private LogicGraphView _view;

        public LGInfoCache LGInfoCache => _lgInfoCache;
        private LGInfoCache _lgInfoCache = default;
        public LGEditorCache LGEditorCache => _lgEditorCache;
        private LGEditorCache _lgEditorCache = default;

        [MenuItem("Framework/逻辑图/打开逻辑图")]
        private static void OpenLogic()
        {
            ShowLGPanel(null);
        }

        /// <summary>
        /// 显示逻辑图面板
        /// </summary>
        /// <param name="p"></param>
        public static void ShowLGPanel(LGInfoCache info)
        {
            Object[] panels = Resources.FindObjectsOfTypeAll(typeof(LGWindow));
            LGWindow panel = null;
            foreach (var item in panels)
            {
                if (item is LGWindow p)
                {
                    if (p._lgInfoCache == info)
                    {
                        panel = p;
                        break;
                    }
                }
            }
            if (panel == null)
            {
                panel = CreateWindow<LGWindow>();
                panel.SetLogic(info);
            }
            panel.Show();
            panel.Focus();
            //panel.SetLogic(info);
        }

        /// <summary>
        /// 设置要显示的逻辑图
        /// </summary>
        /// <param name="info"></param>
        public void SetLogic(LGInfoCache info)
        {
            if (info != null)
            {
                this._lgInfoCache = info;
                this._lgEditorCache = LGCacheOp.GetEditorCache(info);
                BaseLogicGraph graph = LGCacheOp.GetLogicGraph(info);
                this._lgInfoCache.Graph = graph;
                //删除没有的节点
                graph.Nodes.RemoveAll(n => n == null);
                //删除对应节点的配置缓存
                info.Nodes.RemoveAll(n => graph.Nodes.FirstOrDefault(a => a.OnlyId == n.OnlyId) == null);
                foreach (var item in this._lgInfoCache.Graph.Nodes)
                {
                    LNInfoCache nodeCache = this._lgInfoCache.Nodes.FirstOrDefault(a => a.OnlyId == item.OnlyId);
                    if (nodeCache != null)
                    {
                        nodeCache.Node = item;
                    }
                }
                this._view.ShowLogic();
            }
        }

        #region 内置函数

        private void OnEnable()
        {
            titleContent = new GUIContent("逻辑图");
            ShowGui();
        }

        /// <summary>
        /// 当rootVisualElement准备完成后调用
        /// </summary>
        private void ShowGui()
        {
            VisualElement root = rootVisualElement;
            _view = new LogicGraphView(this);
            this.rootVisualElement.Add(_view);
        }
        private void OnDestroy()
        {
            //this._panelData.Save();
        }
        #endregion
    }

}