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

        private LNParameterView _lnParamView;

        private LGParameterView _lgParamView;

        private ToolbarView _toolbarView;

        private EdgeConnectorListener _connectorListener;
        /// <summary>
        /// Connector listener that will create the edges between ports
        /// </summary>
        public EdgeConnectorListener ConnectorListener => _connectorListener;

        /// <summary>
        /// 刷新界面数据
        /// </summary>
        public event Action onUpdateLGParam;
        /// <summary>
        /// 重命名界面数据
        /// </summary>
        public event Action onRenameLGParam;
        public LogicGraphView(LGWindow lgWindow)
        {
            _window = lgWindow;
            _toolbarView = new ToolbarView();
            _toolbarView.onDrawLeft += toolbarView_onDrawLeft;
            _toolbarView.onDrawRight += toolbarView_onDrawRight;
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
            this.RegisterCallback<DragPerformEvent>(m_onDragPerformEvent);
            this.RegisterCallback<DragUpdatedEvent>(m_onDragUpdatedEvent);
            graphViewChanged = m_onGraphViewChanged;
            viewTransformChanged = m_onViewTransformChanged;
            viewTransform.position = LGInfoCache.Pos;
            viewTransform.scale = LGInfoCache.Scale;
            m_showLogic();
            _lnParamView = new LNParameterView();
            _lnParamView.InitializeGraphView(this);
            this.Add(_lnParamView);
            _lnParamView.Hide();


            _lgParamView = new LGParameterView();
            _lgParamView.InitializeGraphView(this);
            this.Add(_lgParamView);
            if (LGInfoCache.ParamCache.IsShow)
                _lgParamView.Show();
            else
                _lgParamView.Hide();
            //BlackboardField field = new BlackboardField();
            //field.AddManipulator(new Dragger());
            //field.text = "cccc";
            //this.Add(field);
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
                _lnParamView.AddUI(element);
            }
        }
        public void RepaintParamPanel()
        {
            if (_showPinned)
            {
                _lnParamView.Repaint();
            }
        }
        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            if (LGInfoCache != null && LGInfoCache.Graph != null)
            {
                EditorUtility.SetDirty(LGInfoCache.Graph);
            }
            LGCacheOp.Save();
        }

        /// <summary>
        /// 添加一个逻辑图参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramType"></param>
        public void AddLGParam(string paramName, Type paramType)
        {
            var param = Activator.CreateInstance(paramType) as BaseParameter;
            param.Name = paramName;
            LGInfoCache.Graph.Params.Add(param);
            this.onUpdateLGParam?.Invoke();
        }

        /// <summary>
        /// 添加一个逻辑图参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramType"></param>
        public void DelLGParam(BaseParameter param)
        {
            LGInfoCache.Graph.Params.Remove(param);
            this.onUpdateLGParam?.Invoke();
        }
        /// <summary>
        /// 重命名逻辑图参数
        /// </summary>
        /// <param name="param"></param>
        /// <param name="newValue"></param>
        public void RenameLGParam(BaseParameter param, string newValue)
        {
            param.Name = newValue;
            this.onRenameLGParam?.Invoke();
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
                        _lnParamView.Show(this.layout);
                        _showPinned = true;
                        nodeView.ShowParamUI();
                    }
                }
            }
            if (selection.Count > 1 && _showPinned)
            {
                _lnParamView.Hide();
                _showPinned = false;
            }
        }
        public override void ClearSelection()
        {
            base.ClearSelection();
            if (_showPinned)
            {
                _lnParamView.Hide();
                _showPinned = false;
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!_hasData)
            {
                evt.menu.AppendAction("创建逻辑图", m_onCreateLogic, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendAction("打开逻辑图", m_onOpenLogic, DropdownMenuAction.AlwaysEnabled);
            }
            else
            {
                evt.menu.AppendAction("创建节点", m_onCreateNode, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendAction("创建分组", m_onCreateGroup, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendAction("创建默认节点", m_onCreateDefaultNode, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("保存", m_onSaveCallback, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendSeparator();
                foreach (var item in LGEditorCache.Formats)
                {
                    evt.menu.AppendAction("导出: " + item.FormatName, m_onFormatCallback, DropdownMenuAction.AlwaysEnabled, item);
                }
                //evt.menu.AppendAction("导出", null);
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
                    var tarPort = port as PortView;
                    if (tarPort == null)
                    {
                        continue;
                    }
                    switch (portView.Owner)
                    {
                        case ParameterNodeView paramView:
                            if (!tarPort.IsDefault && tarPort.CanLink(portView))
                            {
                                compatiblePorts.Add(port);
                            }
                            break;
                        case BaseNodeView nodeView:
                            if (tarPort.CanLink(portView))
                            {
                                compatiblePorts.Add(port);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return compatiblePorts;
        }

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
            LGInfoCache graphCache = new LGInfoCache(graph);
            graphCache.ParamCache = new LGParamCache();
            graphCache.ParamCache.Pos = new Vector2(0, 20);
            graphCache.ParamCache.Size = new Vector2(180, 320);
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
            if (graphCache.ParamCache == null)
            {
                graphCache.ParamCache = new LGParamCache();
                graphCache.ParamCache.Pos = new Vector2(0, 20);
                graphCache.ParamCache.Size = new Vector2(180, 320);
            }
            else if (graphCache.ParamCache.Size.magnitude < 350)
                graphCache.ParamCache.Size = new Vector2(180, 320);
            _window.SetLogic(graphCache);
            return true;
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
        private void m_onCreateGroup(DropdownMenuAction obj)
        {
            //经过计算得出节点的位置

            Vector2 screenPos = _window.GetScreenPosition(obj.eventInfo.mousePosition);
            //经过计算得出节点的位置
            var windowMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, screenPos - _window.position.position);
            var nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
            var logicGroup = new BaseLogicGroup();
            this.LGInfoCache.Graph.Groups.Add(logicGroup);
            m_showGroup(logicGroup, nodePosition);

        }

        private void m_createNodeView(LNEditorCache nodeEditor, Vector2 pos, bool record = true)
        {
            BaseLogicNode logicNode = Activator.CreateInstance(nodeEditor.GetNodeType()) as BaseLogicNode;
            this.LGInfoCache.Graph.Nodes.Add(logicNode);
            if (this.LGEditorCache.DefaultNodes.Contains(nodeEditor))
            {
                this.LGInfoCache.Graph.StartNodes.Add(logicNode);
            }
            logicNode.Pos = pos;
            logicNode.Title = nodeEditor.NodeName;
            logicNode.Initialize(this.LGInfoCache.Graph);
            this.Save();
            m_showNode(logicNode, record);
        }

        private void m_onCreateDefaultNode(DropdownMenuAction obj)
        {
            if (LGEditorCache.DefaultNodes.Count == 0)
            {
                _window.ShowNotification(new GUIContent("当前逻辑图没有默认节点,请联系程序员设置"));
                return;
            }
            int createNum = 0;
            Vector2 screenPos = _window.GetScreenPosition(obj.eventInfo.mousePosition);
            //经过计算得出节点的位置
            var windowMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, screenPos - _window.position.position);
            var nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
            foreach (var item in LGEditorCache.DefaultNodes)
            {
                if (LGInfoCache.Graph.StartNodes.FirstOrDefault(a => a.GetType() == item.GetNodeType()) != null)
                {
                    continue;
                }
                m_createNodeView(item, nodePosition, false);
                createNum++;
            }
            if (createNum == 0)
            {
                _window.ShowNotification(new GUIContent("当前逻辑图已创建所有默认节点"));
            }
        }
        private void m_onSaveCallback(DropdownMenuAction obj)
        {
            Save();
        }
        private void m_onFormatCallback(DropdownMenuAction obj)
        {
            LFEditorCache formatConfig = obj.userData as LFEditorCache;
            if (formatConfig != null)
            {
                string filePath = EditorUtility.SaveFilePanel("导出", Application.dataPath, "undefined", formatConfig.Extension);
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    _window.ShowNotification(new GUIContent("请选择导出路径"));
                    return;
                }
                var logicFormat = Activator.CreateInstance(formatConfig.GetFormatType()) as ILogicFormat;
                bool res = logicFormat.ToFormat(LGInfoCache, filePath);
                if (res)
                {
                    _window.ShowNotification(new GUIContent($"导出: {formatConfig.FormatName} 成功"));
                }
                else
                {
                    _window.ShowNotification(new GUIContent($"导出: {formatConfig.FormatName} 失败"));
                }
            }
        }
        private void m_showLogic()
        {
            m_initializeEdgeConnectorListener();
            m_initializeNodeViews();
            m_initializeGroupViews();
        }

        private void m_initializeEdgeConnectorListener()
        {
            _connectorListener = new EdgeConnectorListener(this);
        }

        private void m_initializeNodeViews()
        {
            List<BaseLogicNode> nodes = _window.LGInfoCache.Graph.Nodes;
            nodes.ForEach(a => m_showNode(a, false));
            _window.LGInfoCache.NodeDic.Values.ToList().ForEach(a => a.DrawLink());
        }
        private void m_initializeGroupViews()
        {
            List<BaseLogicGroup> groups = _window.LGInfoCache.Graph.Groups;
            groups.ForEach(a => m_showGroup(a, a.Pos));
        }
        private void m_showNode(BaseLogicNode node, bool record = true)
        {
            string fullName = node.GetType().FullName;
            BaseNodeView nodeView = null;
            LNEditorCache nodeEditor = null;
            if (node is ParameterNode)
            {
                nodeView = new ParameterNodeView();
            }
            else
            {
                nodeEditor = LGEditorCache.Nodes.FirstOrDefault(a => a.NodeClassName == fullName);
                nodeView = Activator.CreateInstance(nodeEditor.GetViewType()) as BaseNodeView;
            }
            LGInfoCache.NodeDic.Add(node.OnlyId, nodeView);
            nodeView.Initialize(this, node);
            nodeView.ShowUI();
            if (record && nodeEditor != null)
            {
                nodeEditor.AddUseCount();
                LGEditorCache.Nodes.ForEach(a =>
                  {
                      if (a != nodeEditor)
                      {
                          a.SubUseCount();
                      }
                  });
            }
        }

        private void m_showGroup(BaseLogicGroup group, Vector2 pos)
        {
            group.Pos = pos;
            GroupView groupView = new GroupView();
            groupView.Initialize(this, group);
            this.AddElement(groupView);
        }

        private GraphViewChange m_onGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var item in graphViewChange.elementsToRemove)
                {
                    switch (item)
                    {
                        case EdgeView edgeView:
                            var input = edgeView.input as PortView;
                            var output = edgeView.output as PortView;
                            output.Owner.RemoveChild(input.Owner.Target);
                            break;
                        case Node node:
                            var baseNode = node.userData as BaseNodeView;
                            LGInfoCache.Graph.Nodes.Remove(baseNode.Target);
                            break;
                        case GroupView groupView:
                            LGInfoCache.Graph.Groups.Remove(groupView.group);
                            break;
                        case LGParameterFieldView blackboardField:
                            DelLGParam(blackboardField.param);
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
                LGInfoCache.Pos = viewTransform.position;
                LGInfoCache.Scale = viewTransform.scale;
            }
        }

        private void m_onGeometryChanged(GeometryChangedEvent evt)
        {
            if (_showPinned)
            {
                _lnParamView.Show(evt.newRect);
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
        private void m_onDragPerformEvent(DragPerformEvent evt)
        {
            var mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            if (dragData != null)
            {
                var exposedParameterFieldViews = dragData.OfType<LGParameterFieldView>();
                if (exposedParameterFieldViews.Any())
                {
                    foreach (var paramFieldView in exposedParameterFieldViews)
                    {
                        ParameterNode node = new ParameterNode();
                        node.paramId = paramFieldView.param.OnlyId;
                        var nodeView = new ParameterNodeView();
                        LGInfoCache.Graph.Nodes.Add(node);
                        node.Pos = mousePos;
                        node.Title = paramFieldView.param.Name;
                        node.Initialize(LGInfoCache.Graph);
                        LGInfoCache.NodeDic.Add(node.OnlyId, nodeView);
                        nodeView.Initialize(this, node);
                        nodeView.ShowUI();
                    }
                }
            }
        }
        private void m_onDragUpdatedEvent(DragUpdatedEvent evt)
        {
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            bool dragging = false;
            if (dragData != null)
                dragging = dragData.OfType<LGParameterFieldView>().Any();
            if (dragging)
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }

        private void m_duplicateNodeView(BaseNodeView baseNodeView, Vector2 nodePosition)
        {
            if (baseNodeView is ParameterNodeView)
            {
                _window.ShowNotification(new GUIContent($"参数节点不能复制,请在参数界面拖拽"));
                return;
            }
            if (LGEditorCache.DefaultNodes.Exists(a => a.NodeClassName == baseNodeView.Target.GetType().FullName))
            {
                _window.ShowNotification(new GUIContent($"默认节点不能复制"));
                return;
            }
            string nodeFullName = baseNodeView.Target.GetType().FullName;
            var nodeEditor = LGEditorCache.Nodes.FirstOrDefault(a => a.NodeClassName == nodeFullName);
            m_createNodeView(nodeEditor, nodePosition, false);
        }

        /// <summary>
        /// 绘制左边工具条
        /// </summary>
        private void toolbarView_onDrawLeft()
        {
            if (_hasData)
            {
                GUILayout.Label("逻辑图:");
                var logicName = EditorGUILayout.TextField(LGInfoCache.LogicName, EditorStyles.toolbarTextField, GUILayout.MaxWidth(100));
                if (logicName != LGInfoCache.LogicName)
                {
                    LGInfoCache.LogicName = logicName;
                    LGInfoCache.Graph.SetTitle(logicName);
                }
                EditorGUILayout.Separator();
            }
            else
                GUILayout.Label("逻辑图:空");
        }
        /// <summary>
        /// 绘制右边工具条
        /// </summary>
        private void toolbarView_onDrawRight()
        {
            if (_hasData)
            {
                EditorGUILayout.BeginHorizontal();
                string str = LGInfoCache.ParamCache.IsShow ? "隐藏逻辑图参数" : "显示逻辑图参数";
                bool res = GUILayout.Toggle(LGInfoCache.ParamCache.IsShow, str, EditorStyles.toolbarButton);
                if (res != LGInfoCache.ParamCache.IsShow)
                {
                    LGInfoCache.ParamCache.IsShow = res;
                    if (res)
                        _lgParamView.Show();
                    else
                        _lgParamView.Hide();
                }
                EditorGUILayout.Separator();
                if (GUILayout.Button("居中", EditorStyles.toolbarButton))
                {
                    LGInfoCache.Pos = Vector3.zero;
                    LGInfoCache.Scale = Vector3.one;
                    UpdateViewTransform(Vector3.zero, Vector3.one);
                }
                EditorGUILayout.Separator();
                if (GUILayout.Button("在项目中选中", EditorStyles.toolbarButton))
                {
                    UnityEditor.EditorGUIUtility.PingObject(LGInfoCache.Graph);
                    UnityEditor.Selection.activeObject = LGInfoCache.Graph;
                }
                EditorGUILayout.EndHorizontal();
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
