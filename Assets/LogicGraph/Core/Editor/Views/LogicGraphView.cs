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
        private bool _showPinned = false;
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
            this.RegisterCallback<GeometryChangedEvent>(m_onGeometryChanged);
            this.RegisterCallback<KeyDownEvent>(m_onKeyDownEvent);
            graphViewChanged = m_onGraphViewChanged;
            viewTransformChanged = m_onViewTransformChanged;
            m_setToolBar();
            m_showLogic();
            viewTransform.position = LGInfoCache.Position;
            viewTransform.scale = LGInfoCache.Scale;
            _pinnedView = new PinnedParameterView();
            _pinnedView.InitializeGraphView(this);
            this.Add(_pinnedView);
            _pinnedView.Hide();
            //m_setPinnedPanel(this.layout);
            _hasData = true;
        }

        /// <summary>
        /// 添加元素到参数界面
        /// </summary>
        /// <param name="ui"></param>
        public void AddParamElement(VisualElement element)
        {
            if (_showPinned)
            {
                _pinnedView.AddUI(element);
            }
        }
        public void RepaintParamPanel()
        {
            if (_showPinned)
            {
                _pinnedView.Repaint();
            }
        }
        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            if (LGInfoCache.Graph != null)
            {
                EditorUtility.SetDirty(LGInfoCache.Graph);
            }
            LGCacheOp.Save();
        }
        #region 重写方法
        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            if (selection.Count == 1)
            {
                if (selectable is Node node)
                {
                    BaseNodeView nodeView = node.userData as BaseNodeView;
                    if (nodeView.ShowParamPanel)
                    {
                        _pinnedView.Show(this.layout);
                        _showPinned = true;
                        nodeView.ShowParamUI();
                    }
                }
            }
            if (selection.Count > 1 && _showPinned)
            {
                _pinnedView.Hide();
                _showPinned = false;
            }
        }
        public override void ClearSelection()
        {
            base.ClearSelection();
            if (_showPinned)
            {
                _pinnedView.Hide();
                _showPinned = false;
            }
        }

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
            if (startPort is PortView portView)
            {
                //LogicNodeBaseView startNodeView = startPort.node as LogicNodeBaseView;
                foreach (var port in ports.ToList())
                {
                    if (port.direction == Direction.Output)
                    {
                        continue;
                    }
                    if (portView.node == port.node)
                    {
                        continue;
                    }
                    if (portView.connections.FirstOrDefault(a => a.input == port) != null)
                    {
                        continue;
                    }
                    if (port is PortView tarPort)
                    {
                        if (tarPort.CanLink(portView))
                        {
                            compatiblePorts.Add(port);
                        }
                    }

                }
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
            var start = m_createStartNode();
            graph.StartNode = start;
            graph.Nodes.Add(start);
            LGInfoCache graphCache = new LGInfoCache(graph);
            graphCache.LogicName = file;
            graphCache.AssetPath = path;
            Instance.LGInfoList.Add(graphCache);
            AssetDatabase.CreateAsset(graph, path);
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

        private StartNode m_createStartNode()
        {
            StartNode start = new StartNode();
            start.Pos = Vector2.zero;
            start.Title = "开始";
            return start;
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

            logicNode.Pos = pos;
            logicNode.Title = nodeEditor.NodeName;
            this.Save();
            m_showNode(logicNode);
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
            List<BaseLogicNode> nodes = _window.LGInfoCache.Graph.Nodes;
            nodes.ForEach(a => m_showNode(a));
            _window.LGInfoCache.NodeDic.Values.ToList().ForEach(a => a.DrawLink());
        }

        private void m_showNode(BaseLogicNode node)
        {
            string fullName = node.GetType().FullName;
            LNEditorCache nodeEditor = LGEditorCache.Nodes.FirstOrDefault(a => a.NodeClassName == fullName);
            BaseNodeView nodeView = Activator.CreateInstance(nodeEditor.GetViewType()) as BaseNodeView;
            LGInfoCache.NodeDic.Add(node.OnlyId, nodeView);
            nodeView.Initialize(this, node);
            nodeView.ShowUI();
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

        private void m_onGeometryChanged(GeometryChangedEvent evt)
        {
            if (_showPinned)
            {
                _pinnedView.Show(evt.newRect);
            }
        }
        private void m_onKeyDownEvent(KeyDownEvent evt)
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                //保存
                this.Save();
                evt.StopImmediatePropagation();
                return;
            }
            if (evt.ctrlKey && evt.keyCode == KeyCode.D)
            {
                Vector2 screenPos = _window.GetScreenPosition(evt.originalMousePosition);
                //经过计算得出节点的位置
                var windowMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, screenPos - _window.position.position);
                var nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
                //复制
                foreach (ISelectable item in selection)
                {
                    switch (item)
                    {
                        case Node node:
                            m_duplicateNodeView(node.userData as BaseNodeView, nodePosition);
                            break;
                        //case LogicGroupView groupView:
                        //    DuplicateGroupView(groupView, nodePosition);
                        //    break;
                        default:
                            break;
                    }
                }
            }
        }

        private void m_duplicateNodeView(BaseNodeView baseNodeView, Vector2 nodePosition)
        {
            string nodeFullName = baseNodeView.Target.GetType().FullName;
            var nodeEditor = LGEditorCache.Nodes.FirstOrDefault(a => a.NodeClassName == nodeFullName);
            m_createNodeView(nodeEditor, nodePosition, false);

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
