//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEditor;
//using UnityEditor.Experimental.GraphView;
//using UnityEngine;
//using UnityEngine.UIElements;
//using static Logic.Editor.LGCacheData;
//using Logic.Editor;

//namespace Logic.Editor
//{
//    public sealed partial class LogicGraphView1 : GraphView
//    {
//        private bool _hasData = false;

//        private LGWindow1 _window;
//        public LGWindow1 Window => _window;
//        public LGInfoCache LGInfoCache => _window.LGInfoCache;
//        public LGEditorCache LGEditorCache => _window.LGEditorCache;

//        private BaseLogicGraphView _logicGraphView;
//        /// <summary>
//        /// 逻辑图变量面板
//        /// </summary>
//        private LGVariableView _lgVariableView;

//        /// <summary>
//        /// 工具栏面板
//        /// </summary>
//        private ToolbarView _toolbarView;

//        /// <summary>
//        /// 分组列表
//        /// </summary>
//        private GroupListView _groupListView;
//        ///// <summary>
//        ///// 节点描述
//        ///// </summary>
//        //private NodeDescribeView _nodeDescribeView;

//        /// <summary>
//        /// 创建文档面板
//        /// </summary>
//        private CreateLNSearchWindow _createLNSearch = null;

//        private EdgeConnectorListener _connectorListener;
//        /// <summary>
//        /// 端口连接监听器
//        /// </summary>
//        public EdgeConnectorListener ConnectorListener => _connectorListener;

//        /// <summary>
//        /// 刷新界面数据
//        /// </summary>
//        public event Action onUpdateLGVariable;

//        public LogicGraphView1(LGWindow1 lgWindow)
//        {
//            _window = lgWindow;
//            //_toolbarView = new ToolbarView();
//            //_toolbarView.onDrawLeft += toolbarView_onDrawLeft;
//            //_toolbarView.onDrawRight += toolbarView_onDrawRight;
//            //this.Add(_toolbarView);
//            //扩展大小与父对象相同
//            this.StretchToParentSize();
//        }

//        public void ShowLogic()
//        {
//            Input.imeCompositionMode = IMECompositionMode.On;
//            this.LGInfoCache.View = this;
//            this.LGInfoCache.NodeDic.Clear();
//            m_addGridBackGround();
//            this.AddManipulator(new ContentDragger());
//            this.AddManipulator(new SelectionDragger());
//            this.AddManipulator(new RectangleSelector());
//            this.AddManipulator(new ClickSelector());
//            this.RegisterCallback<GeometryChangedEvent>(m_onGeometryChanged);
//            this.RegisterCallback<KeyDownEvent>(m_onKeyDownEvent);
//            this.RegisterCallback<DragPerformEvent>(m_onDragPerformEvent);
//            this.RegisterCallback<DragUpdatedEvent>(m_onDragUpdatedEvent);
//            graphViewChanged = m_onGraphViewChanged;
//            viewTransformChanged = m_onViewTransformChanged;
//            viewTransform.position = LGInfoCache.Graph.Pos;
//            viewTransform.scale = LGInfoCache.Graph.Scale;
//            m_showLogic();

//            //逻辑图变量
//            _lgVariableView = new LGVariableView();
//            //_lgVariableView.InitializeGraphView(this);
//            this.Add(_lgVariableView);
//            if (LGInfoCache.VariableCache.IsShow)
//                _lgVariableView.Show();
//            else
//                _lgVariableView.Hide();

//            //分组列表
//            _groupListView = new GroupListView();
//            _groupListView.InitializeGraphView(this);
//            this.Add(_groupListView);

//            if (LGInfoCache.GroupListCache.IsShow)
//                _groupListView.Show();
//            else
//                _groupListView.Hide();

//            //_nodeDescribeView = new NodeDescribeView(this);
//            //_nodeDescribeView.Hide();
//            //this.Add(_nodeDescribeView);
//            _hasData = true;
//        }

//        /// <summary>
//        /// 添加一个逻辑图参数
//        /// </summary>
//        /// <param name="paramName"></param>
//        /// <param name="paramType"></param>
//        public void DelLGVariable(BaseVariable variable)
//        {
//            List<VariableNodeView> nodeViews = new List<VariableNodeView>();
//            foreach (var item in LGInfoCache.NodeDic)
//            {
//                if (item.Value is VariableNodeView nodeView)
//                {
//                    if (nodeView.Target is VariableNode node)
//                    {
//                        //if (node.varId == variable.OnlyId)
//                        //{
//                        //    nodeViews.Add(nodeView);
//                        //}
//                    }
//                }
//            }
//            LGInfoCache.Graph.Variables.Remove(variable);
//            selection.Clear();
//            selection.AddRange(nodeViews.Select<VariableNodeView, Node>((a) => a.View));
//            this.DeleteSelection();
//            this.onUpdateLGVariable?.Invoke();
//        }

//        #region 重写方法

//        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
//        {
//            if (!_hasData)
//            {
//                evt.menu.AppendAction("创建逻辑图", m_onCreateLogic, DropdownMenuAction.AlwaysEnabled);
//                evt.menu.AppendAction("打开逻辑图", m_onOpenLogic, DropdownMenuAction.AlwaysEnabled);
//            }
//            else
//            {
//                evt.menu.AppendAction("创建节点", m_onCreateNodeWindow, DropdownMenuAction.AlwaysEnabled);
//                //evt.menu.AppendAction("创建分组", m_onCreateGroup, DropdownMenuAction.AlwaysEnabled);
//                evt.menu.AppendAction("创建默认节点", m_onCreateDefaultNode, DropdownMenuAction.AlwaysEnabled);
//                evt.menu.AppendSeparator();

//                if (LGInfoCache.VariableCache.IsShow)
//                {
//                    evt.menu.AppendAction("逻辑图变量面板", m_onVarPanelVisible, DropdownMenuAction.Status.Checked);
//                }
//                else
//                {
//                    evt.menu.AppendAction("逻辑图变量面板", m_onVarPanelVisible, DropdownMenuAction.Status.Normal);
//                }
//                if (LGInfoCache.GroupListCache.IsShow)
//                {
//                    evt.menu.AppendAction("分组列表面板", m_onGroupListPanelVisible, DropdownMenuAction.Status.Checked);
//                }
//                else
//                {
//                    evt.menu.AppendAction("分组列表面板", m_onGroupListPanelVisible, DropdownMenuAction.Status.Normal);
//                }
//                evt.menu.AppendSeparator();
//                evt.menu.AppendSeparator();
//                evt.menu.AppendAction("保存", m_onSaveCallback, DropdownMenuAction.AlwaysEnabled);
//                evt.menu.AppendSeparator();
//                foreach (var item in LGEditorCache.Formats)
//                {
//                    evt.menu.AppendAction("导出: " + item.FormatName, m_onFormatCallback, DropdownMenuAction.AlwaysEnabled, item);
//                }
//            }
//        }

//        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
//        {
//            var compatiblePorts = new List<Port>();
//            if (startPort.direction == Direction.Input)
//            {
//                return compatiblePorts;
//            }
//            if (startPort.capacity == Port.Capacity.Single)
//            {
//                if (startPort.connections.Count() > 0)
//                {
//                    return compatiblePorts;
//                }
//            }
//            if (startPort is PortView portView)
//            {
//                //LogicNodeBaseView startNodeView = startPort.node as LogicNodeBaseView;
//                foreach (var port in ports.ToList())
//                {

//                    if (port.direction == Direction.Output)
//                    {
//                        continue;
//                    }
//                    if (portView.node == port.node)
//                    {
//                        continue;
//                    }
//                    if (portView.connections.FirstOrDefault(a => a.input == port) != null)
//                    {
//                        continue;
//                    }
//                    var tarPort = port as PortView;
//                    if (tarPort == null)
//                    {
//                        continue;
//                    }
//                    if (tarPort.capacity == Port.Capacity.Single)
//                    {
//                        if (tarPort.connections.Count() > 0)
//                        {
//                            continue;
//                        }
//                    }
//                    switch (portView.Owner)
//                    {
//                        case VariableNodeView paramView:
//                            if (!tarPort.IsDefault && tarPort.CanLink(portView))
//                            {
//                                compatiblePorts.Add(port);
//                            }
//                            break;
//                        case BaseNodeView1 nodeView:
//                            if (tarPort.Owner is VariableNodeView)
//                            {
//                                if (!portView.IsDefault && portView.CanLink(tarPort))
//                                {
//                                    compatiblePorts.Add(port);
//                                }
//                            }
//                            else if (tarPort.CanLink(portView))
//                            {
//                                compatiblePorts.Add(port);
//                            }
//                            break;
//                        default:
//                            break;
//                    }
//                }
//            }
//            return compatiblePorts;
//        }

//        #endregion

//        #region 私有方法

//        private void m_showLogic()
//        {
//            m_initializeEdgeConnectorListener();
//            m_initializeNodeViews();
//            m_initializeGroupViews();
//        }

//        private void m_initializeEdgeConnectorListener()
//        {
//            //_connectorListener = new EdgeConnectorListener(this);
//        }

//        private void m_initializeNodeViews()
//        {
//            List<BaseLogicNode> nodes = _window.LGInfoCache.Graph.Nodes;
//            nodes.ForEach(a => m_showNode(a, false));
//            _window.LGInfoCache.NodeDic.Values.ToList().ForEach(a => a.DrawLink());
//        }
//        private void m_initializeGroupViews()
//        {
//            List<BaseLogicGroup> groups = _window.LGInfoCache.Graph.Groups;
//            groups.ForEach(a => m_showGroup(a));
//        }
//        /// <summary>
//        /// 创建一个节点并添加到逻辑图相应的地方
//        /// </summary>
//        /// <param name="nodeType"></param>
//        /// <param name="pos"></param>
//        /// <param name="record"></param>
//        /// <returns></returns>
//        private BaseLogicNode m_createNode(Type nodeType, Vector2 pos)
//        {
//            BaseLogicNode logicNode = Activator.CreateInstance(nodeType) as BaseLogicNode;
//            LNEditorCache nodeEditor = LGEditorCache.Nodes.FirstOrDefault(a => a.NodeClassName == nodeType.FullName);
//            this.LGInfoCache.Graph.Nodes.Add(logicNode);
//            if (nodeEditor != null)
//            {
//                if (this.LGEditorCache.DefaultNodes.Contains(nodeEditor))
//                {
//                    this.LGInfoCache.Graph.StartNodes.Add(logicNode);
//                }
//                logicNode.Title = nodeEditor.NodeName;
//            }
//            logicNode.Pos = pos;
//            logicNode.Initialize(this.LGInfoCache.Graph);
//            this.Save();
//            return logicNode;
//        }
//        private void m_showNode(BaseLogicNode node, bool record = true)
//        {
//            string fullName = node.GetType().FullName;
//            BaseNodeView1 nodeView = null;
//            LNEditorCache nodeEditor = null;
//            if (node is VariableNode)
//            {
//                nodeView = new VariableNodeView();
//            }
//            else
//            {
//                nodeEditor = LGEditorCache.Nodes.FirstOrDefault(a => a.NodeClassName == fullName);
//                nodeView = Activator.CreateInstance(nodeEditor.GetViewType()) as BaseNodeView1;
//            }
//            LGInfoCache.NodeDic.Add(node.OnlyId, nodeView);
//            //nodeView.Initialize(this, node);
//            nodeView.ShowUI();
//            if (record && nodeEditor != null)
//            {
//                nodeEditor.AddUseCount();
//                LGEditorCache.Nodes.ForEach(a =>
//                  {
//                      if (a != nodeEditor)
//                      {
//                          a.SubUseCount();
//                      }
//                  });
//            }
//        }
//        private BaseLogicGroup m_createGroup(LGroupEditorCache groupEditorCache, Vector2 pos)
//        {
//            BaseLogicGroup group = new BaseLogicGroup();

//            group.Pos = pos;
//            group.Title = groupEditorCache.Name;
//            Dictionary<int, BaseLogicNode> dic = new Dictionary<int, BaseLogicNode>();
//            foreach (var item in groupEditorCache.Nodes)
//            {
//                var nodeEditor = LGEditorCache.GetEditorNode(item.NodeClassFullName);
//                if (nodeEditor != null)
//                {
//                    var node = m_createNode(nodeEditor.GetNodeType(), item.Pos + pos);
//                    item.Node = node;
//                    dic.Add(item.Id, node);
//                    group.Nodes.Add(node.OnlyId);
//                }
//            }
//            foreach (var item in groupEditorCache.Nodes)
//            {
//                if (item.Node != null)
//                {
//                    foreach (var child in item.Childs)
//                    {
//                        if (dic.ContainsKey(child))
//                        {
//                            item.Node.Childs.Add(dic[child]);
//                        }
//                    }
//                    m_showNode(item.Node, false);
//                }
//            }
//            foreach (var item in groupEditorCache.Nodes)
//            {
//                if (item.Node != null)
//                {
//                    _window.LGInfoCache.NodeDic[item.Node.OnlyId]?.DrawLink();
//                }
//            }
//            LGInfoCache.Graph.Groups.Add(group);
//            return group;
//        }
//        private void m_showGroup(BaseLogicGroup group)
//        {
//            GroupView groupView = new GroupView();
//            //groupView.Initialize(this, group);
//            this.AddElement(groupView);
//        }

//        private GraphViewChange m_onGraphViewChanged(GraphViewChange graphViewChange)
//        {
//            if (graphViewChange.elementsToRemove != null)
//            {
//                List<GraphElement> removeList = graphViewChange.elementsToRemove.ToList();

//#if UNITY_2019
//                graphViewChange.elementsToRemove.Clear();
//                List<GraphElement> removeList2 = removeList.ToList();
//#endif
//                foreach (GraphElement item in removeList)
//                {
//                    switch (item)
//                    {
//                        case EdgeView edgeView:
//                            PortView input = edgeView.input as PortView;
//                            PortView output = edgeView.output as PortView;
//                            if (input.Owner is VariableNodeView inParamView)
//                                output.Owner.DelVariable(inParamView.Target as VariableNode, output, ParamAccessor.Set);
//                            else if (output.Owner is VariableNodeView outParamView)
//                                input.Owner.DelVariable(outParamView.Target as VariableNode, input, ParamAccessor.Get);
//                            else
//                                output.Owner.RemoveChild(output, input.Owner.Target);
//                            break;
//                        case Node node:
//                            BaseNodeView1 baseNode = node.userData as BaseNodeView1;
//                            LGInfoCache.Graph.Nodes.Remove(baseNode.Target);
//                            LGInfoCache.Graph.StartNodes.Remove(baseNode.Target);
//                            baseNode.OnDestroy();
//#if UNITY_2019
//                            if (node is INodeVisualElement nodeVisual && nodeVisual.ContentContainer != null)
//                            {
//                                removeList2 = removeList2.Union((from d in nodeVisual.ContentContainer.Children().OfType<Port>().SelectMany((Port c) => c.connections)
//                                                                 where (d.capabilities & Capabilities.Deletable) != 0
//                                                                 select d).Cast<GraphElement>()).ToList();
//                            }
//#endif
//                            break;
//                        case GroupView groupView:
//                            LGInfoCache.Graph.Groups.Remove(groupView.Group);
//                            break;
//                        case LGVariableFieldView varFieldView:
//                            if (varFieldView.param.CanDel)
//                                DelLGVariable(varFieldView.param);
//                            else
//                            {
//                                Window.ShowNotification(new GUIContent("无法删除默认变量"));
//#if UNITY_2019
//                                removeList2.Remove(item);
//#elif UNITY_2020
//                                graphViewChange.elementsToRemove.Remove(item);
//#endif
//                            }
//                            break;
//                        case GroupListFieldView groupListField:
//                            if (groupListField.groupEditor.CanDel)
//                                LGEditorCache.DelGroupTemplate(groupListField.groupEditor);
//                            else
//                            {
//                                Window.ShowNotification(new GUIContent("无法删除默认分组"));
//#if UNITY_2019
//                                removeList2.Remove(item);
//#elif UNITY_2020
//                                graphViewChange.elementsToRemove.Remove(item);
//#endif
//                            }
//                            break;
//                        default:
//                            break;
//                    }
//                }
//#if UNITY_2019
//                graphViewChange.elementsToRemove.AddRange(removeList2);
//#endif
//            }
//            return graphViewChange;
//        }

//        private void m_onViewTransformChanged(GraphView graphView)
//        {
//            if (LGInfoCache != null)
//            {
//                LGInfoCache.Graph.Pos = viewTransform.position;
//                LGInfoCache.Graph.Scale = viewTransform.scale;
//            }
//        }

//        /// <summary>
//        /// 大小发生改变调用
//        /// </summary>
//        /// <param name="evt"></param>
//        private void m_onGeometryChanged(GeometryChangedEvent evt)
//        {
//        }
//        private void m_onKeyDownEvent(KeyDownEvent evt)
//        {
//            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
//            {
//                //保存
//                this.Save();
//                evt.StopImmediatePropagation();
//                return;
//            }
//            if (evt.ctrlKey && evt.keyCode == KeyCode.D)
//            {
//                Vector2 screenPos = _window.GetScreenPosition(evt.originalMousePosition);
//                //经过计算得出节点的位置
//                var windowMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, screenPos - _window.position.position);
//                var nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
//                //复制
//                foreach (ISelectable item in selection)
//                {
//                    switch (item)
//                    {
//                        case Node node:
//                            m_duplicateNodeView(node.userData as BaseNodeView1, nodePosition);
//                            break;
//                        default:
//                            break;
//                    }
//                }
//                evt.StopImmediatePropagation();
//                return;
//            }
//        }
//        private void m_onDragPerformEvent(DragPerformEvent evt)
//        {
//            var mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
//            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
//            if (dragData != null)
//            {
//                var exposedFieldViews = dragData.OfType<BlackboardField>();
//                if (exposedFieldViews.Any())
//                {
//                    foreach (var fieldView in exposedFieldViews)
//                    {
//                        switch (fieldView)
//                        {
//                            case LGVariableFieldView varFieldView:
//                                VariableNode node = m_createNode(typeof(VariableNode), mousePos) as VariableNode;
//                                //node.varId = varFieldView.param.OnlyId;
//                                node.Title = varFieldView.param.Name;
//                                m_showNode(node, false);
//                                break;
//                            case GroupListFieldView groupFieldView:
//                                BaseLogicGroup group = m_createGroup(groupFieldView.groupEditor, mousePos);
//                                m_showGroup(group);
//                                break;
//                            default:
//                                break;
//                        }


//                    }
//                }
//            }
//        }
//        private void m_onDragUpdatedEvent(DragUpdatedEvent evt)
//        {
//            List<ISelectable> dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
//            bool dragging = false;
//            if (dragData != null)
//                dragging = dragData.OfType<BlackboardField>().Any();
//            if (dragging)
//                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
//        }
//        private void m_duplicateNodeView(BaseNodeView1 baseNodeView, Vector2 nodePosition)
//        {
//            if (baseNodeView is VariableNodeView)
//            {
//                _window.ShowNotification(new GUIContent($"参数节点不能复制,请在参数界面拖拽"));
//                return;
//            }
//            if (LGEditorCache.DefaultNodes.Exists(a => a.NodeClassName == baseNodeView.Target.GetType().FullName))
//            {
//                _window.ShowNotification(new GUIContent($"默认节点不能复制"));
//                return;
//            }
//            string nodeFullName = baseNodeView.Target.GetType().FullName;
//            m_showNode(m_createNode(baseNodeView.Target.GetType(), nodePosition));
//        }
//        #endregion

//    }

//    /// <summary>
//    /// 公共方法集散地
//    /// </summary>
//    partial class LogicGraphView1
//    {
//        /// <summary>
//        /// 保存
//        /// </summary>
//        public void Save()
//        {
//            if (LGInfoCache != null && LGInfoCache.Graph != null)
//            {
//                EditorUtility.SetDirty(LGInfoCache.Graph);
//            }
//            LGCacheOp.Save();
//        }

//        public void OnDestroy()
//        {
//            if (_hasData)
//            {
//                _lgVariableView.Hide();
//                _groupListView.Hide();
//                Save();
//            }
//        }

//        /// <summary>
//        /// 添加一个逻辑图参数
//        /// </summary>
//        /// <param name="varName"></param>
//        /// <param name="varType"></param>
//        public void AddLGVariable(string varName, Type varType)
//        {
//            var variable = Activator.CreateInstance(varType) as BaseVariable;
//            variable.Name = varName;
//            LGInfoCache.Graph.Variables.Add(variable);
//            this.onUpdateLGVariable?.Invoke();
//        }

//        /// <summary>
//        /// 显示节点描述面板
//        /// </summary>
//        /// <param name="baseNodeView"></param>
//        public void ShowNodeDescribe(BaseNodeView1 baseNodeView)
//        {
//            //this._nodeDescribeView.Show(baseNodeView);
//        }
//    }

//    /// <summary>
//    /// 右键菜单
//    /// </summary>
//    partial class LogicGraphView1
//    {
//        /// <summary>
//        /// 创建逻辑图搜索框
//        /// </summary>
//        /// <param name="obj"></param>
//        private void m_onCreateLogic(DropdownMenuAction obj)
//        {
//            var menuWindowProvider = ScriptableObject.CreateInstance<CreateLGSearchWindow>();
//            menuWindowProvider.onSelectHandler += m_onCreateMenuSelectEntry;
//            Vector2 screenPos = _window.GetScreenPosition(obj.eventInfo.mousePosition);
//            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
//        }

//        /// <summary>
//        /// 打开逻辑图搜索框
//        /// </summary>
//        /// <param name="obj"></param>
//        private void m_onOpenLogic(DropdownMenuAction obj)
//        {
//            var menuWindowProvider = ScriptableObject.CreateInstance<OpenLGSearchWindow>();
//            menuWindowProvider.onSelectHandler += m_onOpenMenuSelectEntry;

//            Vector2 screenPos = _window.GetScreenPosition(obj.eventInfo.mousePosition);
//            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
//        }
//        /// <summary>
//        /// 节点搜索窗
//        /// </summary>
//        /// <param name="obj"></param>
//        private void m_onCreateNodeWindow(DropdownMenuAction obj)
//        {
//            if (_createLNSearch == null)
//            {
//                _createLNSearch = ScriptableObject.CreateInstance<CreateLNSearchWindow>();
//                _createLNSearch.Init(LGEditorCache);
//                _createLNSearch.onSelectHandler += m_onCreateNodeSelectEntry;
//            }

//            Vector2 screenPos = _window.GetScreenPosition(obj.eventInfo.mousePosition);
//            SearchWindow.Open(new SearchWindowContext(screenPos), _createLNSearch);
//        }

//        /// <summary>
//        /// 创建默认节点
//        /// </summary>
//        /// <param name="obj"></param>
//        private void m_onCreateDefaultNode(DropdownMenuAction obj)
//        {
//            if (LGEditorCache.DefaultNodes.Count == 0)
//            {
//                _window.ShowNotification(new GUIContent("当前逻辑图没有默认节点,请联系程序员设置"));
//                return;
//            }
//            int createNum = 0;
//            Vector2 screenPos = _window.GetScreenPosition(obj.eventInfo.mousePosition);
//            //经过计算得出节点的位置
//            var windowMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, screenPos - _window.position.position);
//            var nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
//            foreach (var item in LGEditorCache.DefaultNodes)
//            {
//                if (LGInfoCache.Graph.StartNodes.FirstOrDefault(a => a.GetType() == item.GetNodeType()) != null)
//                {
//                    continue;
//                }
//                m_showNode(m_createNode(item.GetNodeType(), nodePosition), false);
//                createNum++;
//            }
//            if (createNum == 0)
//            {
//                _window.ShowNotification(new GUIContent("当前逻辑图已创建所有默认节点"));
//            }
//        }

//        /// <summary>
//        /// 格式化逻辑图
//        /// </summary>
//        /// <param name="obj"></param>
//        private void m_onFormatCallback(DropdownMenuAction obj)
//        {
//            LFEditorCache formatConfig = obj.userData as LFEditorCache;
//            if (formatConfig != null)
//            {
//                string savePath = Application.dataPath;
//                string saveFile = "undefined";
//                if (!string.IsNullOrEmpty(LGInfoCache.LastFormatPath))
//                {
//                    savePath = Path.GetDirectoryName(LGInfoCache.LastFormatPath);
//                    saveFile = Path.GetFileNameWithoutExtension(LGInfoCache.LastFormatPath);
//                }
//                string filePath = EditorUtility.SaveFilePanel("导出", savePath, saveFile, formatConfig.Extension);
//                if (string.IsNullOrWhiteSpace(filePath))
//                {
//                    _window.ShowNotification(new GUIContent("请选择导出路径"));
//                    return;
//                }
//                var logicFormat = Activator.CreateInstance(formatConfig.GetFormatType()) as ILogicFormat;
//                bool res = false;// logicFormat.ToFormat(LGInfoCache, filePath);
//                if (res)
//                {
//                    string tempPath = filePath.Replace("\\", "/");
//                    int index = tempPath.IndexOf("Assets");
//                    if (index > 0)
//                    {
//                        LGInfoCache.LastFormatPath = tempPath.Substring(index, tempPath.Length - index);
//                    }
//                    _window.ShowNotification(new GUIContent($"导出: {formatConfig.FormatName} 成功"));
//                }
//                else
//                {
//                    _window.ShowNotification(new GUIContent($"导出: {formatConfig.FormatName} 失败"));
//                }
//            }
//        }
//        /// <summary>
//        /// 变量面板显示隐藏
//        /// </summary>
//        /// <param name="obj"></param>
//        private void m_onVarPanelVisible(DropdownMenuAction obj)
//        {
//            LGInfoCache.VariableCache.IsShow = !LGInfoCache.VariableCache.IsShow;
//            if (LGInfoCache.VariableCache.IsShow)
//                _lgVariableView.Show();
//            else
//                _lgVariableView.Hide();
//        }
//        /// <summary>
//        /// 分组列表面板显示隐藏
//        /// </summary>
//        /// <param name="obj"></param>
//        private void m_onGroupListPanelVisible(DropdownMenuAction obj)
//        {
//            LGInfoCache.GroupListCache.IsShow = !LGInfoCache.GroupListCache.IsShow;
//            if (LGInfoCache.GroupListCache.IsShow)
//                _groupListView.Show();
//            else
//                _groupListView.Hide();
//        }

//        /// <summary>
//        /// 保存文件
//        /// </summary>
//        /// <param name="obj"></param>
//        private void m_onSaveCallback(DropdownMenuAction obj)
//        {
//            Save();
//        }
//    }

//    /// <summary>
//    /// 搜索窗体回调
//    /// </summary>
//    partial class LogicGraphView1
//    {
//        /// <summary>
//        /// 创建逻辑图搜索框回调
//        /// </summary>
//        /// <param name="searchTreeEntry"></param>
//        /// <param name="context"></param>
//        /// <returns></returns>
//        private bool m_onCreateMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
//        {
//            LGEditorCache configData = searchTreeEntry.userData as LGEditorCache;
//            string path = EditorUtility.SaveFilePanel("创建逻辑图", Application.dataPath, "LogicGraph", "asset");
//            if (string.IsNullOrEmpty(path))
//            {
//                EditorUtility.DisplayDialog("错误", "路径为空", "确定");
//                return false;
//            }
//            if (File.Exists(path))
//            {
//                EditorUtility.DisplayDialog("错误", "创建文件已存在", "确定");
//                return false;
//            }
//            string file = Path.GetFileNameWithoutExtension(path);
//            BaseLogicGraph graph = ScriptableObject.CreateInstance(configData.GraphClassName) as BaseLogicGraph;
//            graph.name = file;

//            if (graph.DefaultVars != null)
//            {
//                foreach (var item in graph.DefaultVars)
//                {
//                    item.CanRename = false;
//                    item.CanDel = false;
//                    graph.Variables.Add(item);
//                }
//            }
//            path = path.Replace(Application.dataPath, "Assets");
//            LGInfoCache graphCache = new LGInfoCache(graph);
//            graphCache.LogicName = file;
//            graphCache.AssetPath = path;
//            graph.Title = file;
//            Instance.LGInfoList.Add(graphCache);
//            AssetDatabase.CreateAsset(graph, path);
//            LGCacheOp.Save();
//            _window.SetLogic(graphCache);
//            return true;
//        }

//        /// <summary>
//        /// 打开逻辑图搜索框回调
//        /// </summary>
//        /// <param name="searchTreeEntry"></param>
//        /// <param name="context"></param>
//        /// <returns></returns>
//        private bool m_onOpenMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
//        {
//            LGInfoCache graphCache = searchTreeEntry.userData as LGInfoCache;
//            if (graphCache.VariableCache.Size.magnitude < 50 || graphCache.GroupListCache.Size.magnitude < 50)
//            {
//                graphCache.ResetPanelCache();
//            }
//            _window.SetLogic(graphCache);
//            return true;
//        }

//        /// <summary>
//        /// 节点搜索窗回调
//        /// </summary>
//        /// <param name="searchTreeEntry"></param>
//        /// <param name="context"></param>
//        /// <returns></returns>
//        private bool m_onCreateNodeSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
//        {
//            //经过计算得出节点的位置
//            var windowMousePosition = this.ChangeCoordinatesTo(this, context.screenMousePosition - _window.position.position);
//            var nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
//            if (searchTreeEntry.userData is LNEditorCache nodeEditor)
//            {
//                m_showNode(m_createNode(nodeEditor.GetNodeType(), nodePosition));
//            }

//            return true;
//        }
//    }

//    /// <summary>
//    /// 工具栏
//    /// </summary>
//    partial class LogicGraphView1
//    {
//        private string _toolbarSearch = "";
//        private string _toolbarTempSearch = "";
//        private int _toolbarIndex = 0;

//        private List<BaseLogicNode> _toolbarSearchList = new List<BaseLogicNode>();
//        /// <summary>
//        /// 绘制左边工具条
//        /// </summary>
//        private void toolbarView_onDrawLeft()
//        {
//            if (_hasData)
//            {
//                GUILayout.Label("逻辑图:");
//                var logicName = EditorGUILayout.TextField(LGInfoCache.LogicName, EditorStyles.toolbarTextField, GUILayout.MaxWidth(100));
//                if (logicName != LGInfoCache.LogicName)
//                {
//                    LGInfoCache.LogicName = logicName;
//                    LGInfoCache.Graph.Title = logicName;
//                }
//                EditorGUILayout.Separator();
//            }
//            else
//                GUILayout.Label("逻辑图:空");
//        }
//        /// <summary>
//        /// 绘制右边工具条
//        /// </summary>
//        private void toolbarView_onDrawRight()
//        {
//            if (_hasData)
//            {
//                EditorGUILayout.BeginHorizontal();

//                toolbarView_showLogicSearch();

//                EditorGUILayout.Space(10);
//                EditorGUILayout.Separator();
//                if (GUILayout.Button("居中", EditorStyles.toolbarButton))
//                {
//                    LGInfoCache.Graph.Pos = Vector3.zero;
//                    LGInfoCache.Graph.Scale = Vector3.one;
//                    UpdateViewTransform(Vector3.zero, Vector3.one);
//                }
//                EditorGUILayout.Separator();
//                if (GUILayout.Button("在项目中选中", EditorStyles.toolbarButton))
//                {
//                    UnityEditor.EditorGUIUtility.PingObject(LGInfoCache.Graph);
//                    UnityEditor.Selection.activeObject = LGInfoCache.Graph;
//                }
//                EditorGUILayout.EndHorizontal();
//            }
//        }

//        /// <summary>
//        /// 工具栏搜索
//        /// </summary>
//        private void toolbarView_showLogicSearch()
//        {
//            if (GUILayout.Button("上一个", EditorStyles.toolbarButton))
//            {
//                if (string.IsNullOrWhiteSpace(_toolbarTempSearch))
//                {
//                    return;
//                }
//                if (_toolbarTempSearch != _toolbarSearch)
//                {
//                    _toolbarSearch = _toolbarTempSearch;
//                    _toolbarIndex = 0;
//                    _toolbarSearchList = LGInfoCache.Graph.Nodes.FindAll(a => a.Title.Contains(_toolbarSearch));
//                }
//                else
//                {
//                    _toolbarIndex--;
//                }
//                toolbarSearch();
//            }
//            _toolbarTempSearch = EditorGUILayout.TextField(_toolbarTempSearch, EditorStyles.toolbarTextField, GUILayout.MaxWidth(150));
//            if (GUILayout.Button("下一个", EditorStyles.toolbarButton))
//            {
//                if (string.IsNullOrWhiteSpace(_toolbarTempSearch))
//                {
//                    return;
//                }
//                if (_toolbarTempSearch != _toolbarSearch)
//                {
//                    _toolbarSearch = _toolbarTempSearch;
//                    _toolbarIndex = 0;
//                    _toolbarSearchList = LGInfoCache.Graph.Nodes.FindAll(a => a.Title.Contains(_toolbarSearch));
//                }
//                else
//                {
//                    _toolbarIndex++;
//                }
//                toolbarSearch();
//            }
//        }

//        private void toolbarSearch()
//        {
//            if (_toolbarSearchList.Count == 0)
//            {
//                Window.ShowNotification(new GUIContent("没有搜到"));
//                return;
//            }
//            if (_toolbarIndex < 0)
//            {
//                _toolbarIndex = _toolbarSearchList.Count - 1;
//            }
//            else if (_toolbarIndex >= _toolbarSearchList.Count)
//            {
//                _toolbarIndex = 0;
//            }
//            BaseLogicNode node = _toolbarSearchList[_toolbarIndex];
//            BaseNodeView1 view = LGInfoCache.GetNodeView(node);

//            float halfW = this.localBound.width * 0.5f;
//            float halfH = this.localBound.height * 0.5f;

//            halfW -= view.View.localBound.width * 0.5f;
//            halfH -= view.View.localBound.height * 0.5f;
//            Vector2 pos = node.Pos;
//            pos.x -= halfW;
//            pos.y -= halfH;
//            UpdateViewTransform(pos * -1, Vector3.one);
//            this.ClearSelection();
//            this.AddToSelection(view.View);
//        }
//    }

//    /// <summary>
//    /// 背景网格
//    /// </summary>
//    partial class LogicGraphView1
//    {
//        private class LGPanelViewGrid : GridBackground { }
//        /// <summary>
//        /// 添加背景网格
//        /// </summary>
//        private void m_addGridBackGround()
//        {
//            //添加网格背景
//            GridBackground gridBackground = new LGPanelViewGrid();
//            gridBackground.name = "GridBackground";
//            Insert(0, gridBackground);
//            //设置背景缩放范围
//            ContentZoomer contentZoomer = new ContentZoomer();
//            contentZoomer.minScale = 0.05f;
//            contentZoomer.maxScale = 2f;
//            contentZoomer.scaleStep = 0.05f;
//            this.AddManipulator(contentZoomer);
//            //扩展大小与父对象相同
//            this.StretchToParentSize();
//        }
//    }

//}
