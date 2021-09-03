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
    public sealed class LogicGraphView : GraphView
    {
        private bool _hasData = false;
        private LGWindow _window;
        public LGInfoCache LGInfoCache => _window.LGInfoCache;
        public LGEditorCache LGEditorCache => _window.LGEditorCache;

        private PinnedParameterView _pinnedView;

        private ToolbarView _toolbarView;

        private EdgeConnectorListener _connectorListener;
        /// <summary>
        /// Connector listener that will create the edges between ports
        /// </summary>
        public EdgeConnectorListener ConnectorListener => _connectorListener;

        public LogicGraphView(LGWindow lgWindow)
        {
            _window = lgWindow;
            _toolbarView = new ToolbarView();
            this.Add(_toolbarView);
            //扩展大小与父对象相同
            this.StretchToParentSize();
        }

        public void ShowLogic()
        {
            this.LGInfoCache.View = this;
            this.LGInfoCache.NodeDic.Clear();
            m_addGridBackGround();
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.RegisterCallback<GeometryChangedEvent>(test);
            graphViewChanged = m_onGraphViewChanged;
            viewTransformChanged = m_onViewTransformChanged;
            m_setToolBar();
            m_showLogic();
            viewTransform.position = LGInfoCache.Position;
            viewTransform.scale = LGInfoCache.Scale;
            _pinnedView = new PinnedParameterView();
            _pinnedView.InitializeGraphView(this);
            this.Add(_pinnedView);
            _hasData = true;
        }

        private void test(GeometryChangedEvent evt)
        {
            float height = evt.newRect.height;
            _pinnedView.SetPosition(new Rect(evt.newRect.width - 120, 0, 100, height));
        }

        #region 重写方法
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!_hasData)
            {
                evt.menu.AppendAction("创建逻辑图", m_onCreateLogic);
                evt.menu.AppendAction("打开逻辑图", m_onOpenLogic);
            }
            else
            {
                evt.menu.AppendAction("创建节点", m_onCreateNode);
                //evt.menu.AppendAction("创建便笺", null);
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("保存", null);
                evt.menu.AppendAction("另存为", null);
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("导出", null);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            if (startPort.direction == Direction.Input)
            {
                return compatiblePorts;
            }
            //LogicNodeBaseView startNodeView = startPort.node as LogicNodeBaseView;
            foreach (var port in ports.ToList())
            {
                if (port.direction == Direction.Output)
                {
                    continue;
                }
                if (startPort.node == port.node)
                {
                    continue;
                }
                if (startPort.connections.FirstOrDefault(a => a.input == port) != null)
                {
                    continue;
                }
                compatiblePorts.Add(port);
            }
            return compatiblePorts;
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
            graph.Nodes.Add(start);
            AssetDatabase.CreateAsset(graph, path);
            LGInfoCache graphCache = new LGInfoCache(graph);
            graphCache.LogicName = file;
            graphCache.AssetPath = path;
            graphCache.Nodes.Add(startCache);
            Instance.LGInfoList.Add(graphCache);
            LGCacheOp.Save();
            _window.SetLogic(graphCache);
            return true;
        }
        private bool m_onOpenMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            LGInfoCache graphCache = searchTreeEntry.userData as LGInfoCache;
            _window.SetLogic(graphCache);
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
        /// 创建节点面板
        /// </summary>
        /// <param name="obj"></param>
        private void m_onCreateNode(DropdownMenuAction obj)
        {
            var menuWindowProvider = ScriptableObject.CreateInstance<CreateLNSearchWindow>();
            menuWindowProvider.Init(LGEditorCache);
            menuWindowProvider.onSelectHandler += m_onCreateNodeSelectEntry;

            Vector2 screenPos = _window.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
        }

        private bool m_onCreateNodeSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            //经过计算得出节点的位置
            var windowMousePosition = this.ChangeCoordinatesTo(this, context.screenMousePosition - _window.position.position);
            var nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
            if (searchTreeEntry.userData is LNEditorCache nodeEditor)
            {
                m_createNodeView(nodeEditor, nodePosition);
            }

            return true;
        }

        private void m_createNodeView(LNEditorCache nodeEditor, Vector2 pos, bool record = true)
        {
            BaseLogicNode logicNode = Activator.CreateInstance(nodeEditor.GetNodeType()) as BaseLogicNode;
            this.LGInfoCache.Graph.Nodes.Add(logicNode);
            LNInfoCache nodeCache = new LNInfoCache(logicNode);
            nodeCache.Pos = pos;
            nodeCache.Title = nodeEditor.NodeName;
            this.LGInfoCache.Nodes.Add(nodeCache);
            LGCacheOp.Save();
            m_showNode(nodeCache);
        }


        /// <summary>
        /// 设置工具条
        /// </summary>
        private void m_setToolBar()
        {
        }
        private void m_showLogic()
        {
            m_initializeEdgeConnectorListener();
            m_initializeNodeViews();
        }

        private void m_initializeEdgeConnectorListener()
        {
            _connectorListener = new EdgeConnectorListener(this);
        }

        private void m_initializeNodeViews()
        {
            List<LNInfoCache> nodes = _window.LGInfoCache.Nodes;
            nodes.ForEach(a => m_showNode(a));
            nodes.ForEach(a => a.View.DrawLink());
        }

        private void m_showNode(LNInfoCache nodeCache)
        {
            LGInfoCache.NodeDic.Add(nodeCache.OnlyId, nodeCache);
            string fullName = nodeCache.Node.GetType().FullName;
            var nodeEditor = LGEditorCache.Nodes.FirstOrDefault(a => a.NodeClassName == fullName);
            nodeCache.View = Activator.CreateInstance(nodeEditor.GetViewType()) as BaseNodeView;
            nodeCache.View.Initialize(this, nodeCache);
            nodeCache.View.ShowUI();
        }
        private GraphViewChange m_onGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.RemoveAll(a => a.userData is StartNodeView);

                foreach (var item in graphViewChange.elementsToRemove)
                {
                    switch (item)
                    {
                        case EdgeView edgeView:
                            var input = edgeView.input as PortView;
                            var output = edgeView.output as PortView;
                            output.Owner.RemoveChild(input.Owner);
                            break;
                        case Node node:
                            var baseNode = node.userData as BaseNodeView;
                            LGInfoCache.Nodes.Remove(baseNode.nodeCache);
                            LGInfoCache.Graph.Nodes.Remove(baseNode.Target);
                            break;
                        default:
                            break;
                    }
                }
            }
            return graphViewChange;
        }

        private void m_onViewTransformChanged(GraphView graphView)
        {
            if (LGInfoCache != null)
            {
                LGInfoCache.Position = viewTransform.position;
                LGInfoCache.Scale = viewTransform.scale;
            }
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
            ContentZoomer contentZoomer = new ContentZoomer();
            contentZoomer.minScale = 0.05f;
            contentZoomer.maxScale = 2f;
            contentZoomer.scaleStep = 0.05f;
            this.AddManipulator(contentZoomer);
            //扩展大小与父对象相同
            this.StretchToParentSize();
        }
    }
}
