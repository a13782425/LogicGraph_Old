using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public partial class BaseGraphView : GraphView
    {
        public LGWindow Window { get; private set; }
        /// <summary>
        /// ��ǰ�ڵ���ͼ��Ӧ�Ľڵ�
        /// </summary>
        public BaseLogicGraph Target { get; private set; }

        public LGEditorCache EditorCache { get; private set; }

        /// <summary>
        /// Ĭ�ϱ���
        /// </summary>
        public virtual List<BaseVariable> DefaultVars => new List<BaseVariable>();

        /// <summary>
        /// ��ǰ�߼�ͼ�����õı���
        /// </summary>
        public virtual List<Type> VarTypes => new List<Type>();

        /// <summary>
        /// ������һ���
        /// Key:OnlyId
        /// </summary>

        private Dictionary<string, BaseNodeView> NodeDic = new Dictionary<string, BaseNodeView>();
        /// <summary>
        /// �߼�ͼ�������
        /// </summary>
        private LGVariableView _lgVariableView;


        private EdgeConnectorListener _connectorListener;
        /// <summary>
        /// �˿����Ӽ�����
        /// </summary>
        public EdgeConnectorListener ConnectorListener => _connectorListener;

        /// <summary>
        /// �����������仯��
        /// </summary>
        public event Action onVariableModify;

        private LGUndo _undo = default;
        private LGEvent _event = default;
        /// <summary>
        /// ��ǰ�߼�ͼ���¼�����
        /// </summary>
        public LGEvent Event => _event;
        public BaseGraphView()
        {
            Input.imeCompositionMode = IMECompositionMode.On;
            m_addGridBackGround();
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.RegisterCallback<GeometryChangedEvent>(m_onGeometryChanged);
            this.RegisterCallback<KeyDownEvent>(m_onKeyDownEvent);
            this.RegisterCallback<DragPerformEvent>(m_onDragPerformEvent);
            this.RegisterCallback<DragUpdatedEvent>(m_onDragUpdatedEvent);
            _lgVariableView = new LGVariableView();
            _lgVariableView.InitializeGraphView(this);
            this.Add(_lgVariableView);
            //��չ��С�븸������ͬ
            this.StretchToParentSize();
        }



        public void Initialize(LGWindow lgWindow, BaseLogicGraph graph, LGEditorCache editorCache)
        {
            Target = graph;
            Window = lgWindow;
            EditorCache = editorCache;
            _undo = new LGUndo(this);
            _event = new LGEvent(this);
            _lgVariableView.Show();
            graphViewChanged = m_onGraphViewChanged;
            viewTransformChanged = m_onViewTransformChanged;
            viewTransform.position = Target.Pos;
            viewTransform.scale = Target.Scale;
            _connectorListener = new EdgeConnectorListener(this);
            List<BaseLogicNode> nodes = Target.Nodes;
            Target.Nodes.ForEach(n => AddNodeView(n));
            NodeDic.Values.ToList().ForEach(a => a.DrawLink());
            Target.Groups.ForEach(n => AddGroupView(n));
            Window.onDrawTopLeft = toolbarView_onDrawLeft;
            Window.onDrawTopRight = toolbarView_onDrawRight;
            Window.onDrawBottomRight = toolbarView_onDrawBottomRight;
        }

        /// <summary>
        /// �Ҽ�����
        /// </summary>
        /// <param name="evt"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("�����ڵ�", onCreateNodeWindow, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("��������", m_onCreateGroup, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("����Ĭ�Ͻڵ�", onCreateDefaultNode, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendSeparator();

            evt.menu.AppendAction("����", m_onSaveCallback, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendSeparator();
            foreach (var item in EditorCache.Formats)
            {
                evt.menu.AppendAction("����: " + item.FormatName, m_onFormatCallback, DropdownMenuAction.AlwaysEnabled, item);
            }
        }

        /// <summary>
        /// ���һ���߼�ͼ����
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="varType"></param>
        public void AddVariable(string varName, Type varType)
        {
            var variable = Activator.CreateInstance(varType) as BaseVariable;
            variable.Name = varName;
            Target.Variables.Add(variable);
            onVariableModify?.Invoke();
        }
        /// <summary>
        /// ���һ���߼�ͼ����
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="varType"></param>
        public void AddVariable(BaseVariable variable)
        {
            Target.Variables.Add(variable);
            onVariableModify?.Invoke();
        }
        /// <summary>
        /// ɾ��һ���߼�ͼ����
        /// </summary>
        /// <param name="variable"></param>
        public void DelVariable(BaseVariable variable)
        {
            Target.Variables.Remove(variable);
            onVariableModify?.Invoke();
        }

        /// <summary>
        /// ���һ���ڵ�
        /// </summary>
        public BaseLogicNode AddNode(Type nodeType, Vector2 pos)
        {
            BaseLogicNode logicNode = Activator.CreateInstance(nodeType) as BaseLogicNode;
            LNEditorCache nodeEditor = EditorCache.Nodes.FirstOrDefault(a => a.NodeClassName == nodeType.FullName);
            if (nodeEditor != null)
            {
                if (EditorCache.DefaultNodes.Contains(nodeEditor))
                {
                    Target.StartNodes.Add(logicNode);
                }
                logicNode.Title = nodeEditor.NodeName;
            }
            logicNode.Pos = pos;
            logicNode.Initialize(Target);
            Target.Nodes.Add(logicNode);
            this.Save();
            return logicNode;
        }
        /// <summary>
        /// ���һ���ڵ�
        /// </summary>
        public BaseLogicNode AddNode(BaseLogicNode logicNode)
        {
            LNEditorCache nodeEditor = EditorCache.Nodes.FirstOrDefault(a => a.NodeClassName == logicNode.GetType().FullName);
            if (nodeEditor != null)
            {
                if (EditorCache.DefaultNodes.Contains(nodeEditor))
                {
                    Target.StartNodes.Add(logicNode);
                }
                logicNode.Title = nodeEditor.NodeName;
            }
            logicNode.Initialize(Target);
            Target.Nodes.Add(logicNode);
            this.Save();
            return logicNode;
        }

        /// <summary>
        /// �Ƴ�һ���ڵ�
        /// </summary>
        public void DelNode(BaseLogicNode node)
        {
            Target.Nodes.Remove(node);
            Target.StartNodes.Remove(node);
            this.NodeDic.Remove(node.OnlyId);
        }

        public BaseNodeView AddNodeView(BaseLogicNode node)
        {
            string fullName = node.GetType().FullName;
            BaseNodeView nodeView = null;
            LNEditorCache nodeEditor = null;
            nodeEditor = EditorCache.Nodes.FirstOrDefault(a => a.NodeClassName == fullName);

            if (node is VariableNode)
                nodeView = new VariableNodeView();
            else
                nodeView = Activator.CreateInstance(nodeEditor.ViewType) as BaseNodeView;

            this.NodeDic.Add(node.OnlyId, nodeView);
            nodeView.Initialize(this, node);
            nodeView.ShowUI();
            return nodeView;
        }

        public GroupView AddGroupView(BaseLogicGroup group)
        {
            GroupView groupView = new GroupView();
            groupView.Initialize(this, group);
            this.AddElement(groupView);
            return groupView;
        }
        //public void DelNodeView(BaseNodeView view)
        //{
        //    view.OnDestroy();
        //}

        public bool HasNodeView(BaseLogicNode item) => HasNodeView(item.OnlyId);
        public bool HasNodeView(string onlyId) => NodeDic.ContainsKey(onlyId);
        /// <summary>
        /// ��ȡһ���ڵ���ͼ
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public BaseNodeView GetNodeView(BaseLogicNode item) => GetNodeView(item.OnlyId);
        /// <summary>
        /// ��ȡһ���ڵ���ͼ
        /// </summary>
        /// <param name="onlyId"></param>
        /// <returns></returns>
        public BaseNodeView GetNodeView(string onlyId) => NodeDic.ContainsKey(onlyId) ? NodeDic[onlyId] : null;

        public void Save()
        {
            EditorUtility.SetDirty(Target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        public void OnDestroy()
        {
            Save();
        }
    }

    /// <summary>
    /// ��д���ຯ��
    /// </summary>
    partial class BaseGraphView
    {
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            if (startPort.direction == Direction.Input)
            {
                goto End;
            }
            if (startPort is NodePort nodePort)
            {
                foreach (var port in ports.ToList())
                {
                    if (port.direction == Direction.Output)
                    {
                        continue;
                    }
                    if (nodePort.node == port.node)
                    {
                        continue;
                    }
                    if (nodePort.connections.FirstOrDefault(a => a.input == port) != null)
                    {
                        continue;
                    }
                    var tarPort = port as NodePort;
                    if (tarPort == null)
                    {
                        continue;
                    }
                    bool isResult = nodePort.CanLinkTo(tarPort);
                    if (isResult)
                        isResult = tarPort.CanLink(nodePort);
                    if (isResult)
                        compatiblePorts.Add(tarPort);
                }
            }
        End: return compatiblePorts;
        }
    }
    /// <summary>
    /// �¼�
    /// </summary>
    partial class BaseGraphView
    {
        private GraphViewChange m_onGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var fieldView in graphViewChange.edgesToCreate)
                {
                    Debug.LogError("123");
                }

            }
            if (graphViewChange.elementsToRemove != null)
            {
                List<GraphElement> removeList = graphViewChange.elementsToRemove.ToList();
                List<LGVariableFieldView> fieldViews = graphViewChange.elementsToRemove.OfType<LGVariableFieldView>().ToList();
                List<VariableNodeView> varNodeViews = NodeDic.Values.OfType<VariableNodeView>().ToList();
                foreach (var fieldView in fieldViews)
                {
                    if (fieldView.param.CanDel)
                    {
                        varNodeViews.ForEach(a =>
                        {
                            if (a.target is VariableNode node)
                            {
                                if (node.variable == fieldView.param)
                                {
                                    removeList.Add(a);
                                }
                            }
                        });
                    }
                }
                removeList.OfType<BaseNodeView>().ToList().ForEach(a => removeList = removeList.Union(a.GetCollectElements()).ToList());
                List<GraphElement> removeList2 = removeList.ToList();

                LGUndoData undoData = new LGUndoData(this);
                graphViewChange.elementsToRemove.Clear();

                foreach (GraphElement item in removeList)
                {
                    switch (item)
                    {
                        case EdgeView edgeView:
                            NodePort input = edgeView.input as NodePort;
                            NodePort output = edgeView.output as NodePort;
                            undoData.AddStep(input, output);
                            output.DelPort(input);
                            break;
                        case BaseNodeView nodeView:
                            undoData.AddStep(nodeView.target);
                            DelNode(nodeView.target);
                            nodeView.OnDestroy();
                            break;
                        case GroupView groupView:
                            undoData.AddStep(groupView.Group);
                            Target.Groups.Remove(groupView.Group);
                            break;
                        case LGVariableFieldView varFieldView:
                            if (varFieldView.param.CanDel)
                            {
                                undoData.AddStep(varFieldView.param);
                                DelVariable(varFieldView.param);
                            }
                            else
                            {
                                Window.ShowNotification(new GUIContent("�޷�ɾ��Ĭ�ϱ���"));
                                removeList2.Remove(item);
                            }
                            break;

                        default:
                            break;
                    }
                }
                graphViewChange.elementsToRemove.AddRange(removeList2);
                Undo.RegisterCompleteObjectUndo(Target, "Delete");
                //_undo.PushUndo(undoData);
            }
            return graphViewChange;
        }

        private void m_onViewTransformChanged(GraphView graphView)
        {
            Target.Pos = viewTransform.position;
            Target.Scale = viewTransform.scale;
        }
        /// <summary>
        /// ��С�����ı����
        /// </summary>
        /// <param name="evt"></param>
        private void m_onGeometryChanged(GeometryChangedEvent evt)
        {
        }
        /// <summary>
        /// ���̰���
        /// </summary>
        /// <param name="evt"></param>
        private void m_onKeyDownEvent(KeyDownEvent evt)
        {
            if (evt.ctrlKey)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.S:
                        {
                            //����
                            this.Save();
                            evt.StopPropagation();
                        }
                        break;
                    case KeyCode.Z:
                        {
                            Debug.LogError(Undo.GetCurrentGroup());
                            //����
                            Undo.RevertAllDownToGroup(Undo.GetCurrentGroup());
                            //Undo.PerformUndo();
                            //_undo.PopUndo();
                            evt.StopPropagation();
                        }
                        break;
                    case KeyCode.D:
                        {
                            //����
                            op_Duplicate(evt);
                            evt.StopPropagation();
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void op_Duplicate(KeyDownEvent evt)
        {
            Vector2 screenPos = Window.GetScreenPosition(evt.originalMousePosition);
            //��������ó��ڵ��λ��
            var windowMousePosition = Window.rootVisualElement.ChangeCoordinatesTo(Window.rootVisualElement.parent, screenPos - Window.position.position);
            Vector2 nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
            List<ISelectable> selectList = new List<ISelectable>();
            selectList.AddRange(selection);
            var groupList = selectList.OfType<GroupView>().ToList();
            if (groupList.Any())
            {
                foreach (var item in groupList)
                {
                    foreach (var nodeId in item.Group.Nodes)
                    {
                        var tempView = GetNodeView(nodeId);
                        if (!selectList.Contains(tempView))
                        {
                            selectList.Add(tempView);
                        }
                    }
                }
            }
            var nodeList = selectList.OfType<BaseNodeView>().ToList().Where(x => !EditorCache.DefaultNodeFullNames.Exists(a => a == x.target.GetType().FullName)).ToList();

            Vector2 centerPos = GetNodeCenter(nodeList.OfType<Node>().ToList());
            Dictionary<string, string> mappingDic = new Dictionary<string, string>();
            List<NodeEdgeData> edgeDatas = new List<NodeEdgeData>();
            foreach (BaseNodeView item in nodeList)
            {
                if (item is BaseNodeView nodeView)
                {
                    Vector2 pos = nodePosition + (nodeView.target.Pos - centerPos);
                    BaseLogicNode node = AddNode(nodeView.target.GetType(), pos);
                    if (node is VariableNode varNode)
                    {
                        VariableNode tarNode = nodeView.target as VariableNode;
                        varNode.varName = tarNode.varName;
                        varNode.Title = tarNode.Title;
                    }
                    AddNodeView(node);
                    edgeDatas.AddRange(GetNodeEdgeData(nodeView));
                    mappingDic.Add(nodeView.OnlyId, node.OnlyId);
                }
            }

            foreach (var item in selectList)
            {
                switch (item)
                {
                    case GroupView groupView:
                        {
                            BaseLogicGroup group = new BaseLogicGroup();
                            Vector2 pos = nodePosition + (groupView.Group.Pos - centerPos);
                            group.Pos = pos;
                            group.Title = groupView.Group.Title;
                            Target.Groups.Add(group);
                            GroupView newView = AddGroupView(group);
                            foreach (var nodeId in groupView.Group.Nodes)
                            {
                                if (mappingDic.ContainsKey(nodeId))
                                {
                                    string newId = mappingDic[nodeId];
                                    newView.AddElement(GetNodeView(newId));
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            foreach (var item in edgeDatas)
            {
                if (mappingDic.ContainsKey(item.InputNodeId) && mappingDic.ContainsKey(item.OutputNodeId))
                {
                    item.InputNodeId = mappingDic[item.InputNodeId];
                    item.OutputNodeId = mappingDic[item.OutputNodeId];
                    item.DrawLink(this);
                }
            }
        }
        private void m_onDragPerformEvent(DragPerformEvent evt)
        {
            var mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            if (dragData != null)
            {
                var exposedFieldViews = dragData.OfType<LGVariableFieldView>();
                if (exposedFieldViews.Any())
                {
                    foreach (LGVariableFieldView varFieldView in exposedFieldViews)
                    {
                        VariableNode node = AddNode(typeof(VariableNode), mousePos) as VariableNode;
                        node.Title = varFieldView.param.Name;
                        //node.varId = varFieldView.param.Name;
                        node.varName = varFieldView.param.Name;
                        AddNodeView(node);
                    }
                }
            }
        }
        private void m_onDragUpdatedEvent(DragUpdatedEvent evt)
        {
            List<ISelectable> dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            bool dragging = false;
            if (dragData != null)
                dragging = dragData.OfType<LGVariableFieldView>().Any();
            if (dragging)
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }
    }
    /// <summary>
    /// �Ҽ��˵�
    /// </summary>
    partial class BaseGraphView
    {
        /// <summary>
        /// �����ĵ����
        /// </summary>
        private CreateLNSearchWindow _createLNSearch = null;
        /// <summary>
        /// �ڵ�������
        /// </summary>
        /// <param name="obj"></param>
        protected void onCreateNodeWindow(DropdownMenuAction obj)
        {
            if (_createLNSearch == null)
            {
                _createLNSearch = ScriptableObject.CreateInstance<CreateLNSearchWindow>();
                _createLNSearch.Init(EditorCache);
                _createLNSearch.onSelectHandler += m_onCreateNodeSelectEntry;
            }

            Vector2 screenPos = Window.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), _createLNSearch);
        }

        /// <summary>
        /// ����Ĭ�Ͻڵ�
        /// </summary>
        /// <param name="obj"></param>
        private void onCreateDefaultNode(DropdownMenuAction obj)
        {
            if (EditorCache.DefaultNodes.Count == 0)
            {
                Window.ShowNotification(new GUIContent("��ǰ�߼�ͼû��Ĭ�Ͻڵ�,����ϵ����Ա����"));
                return;
            }
            int createNum = 0;
            Vector2 screenPos = Window.GetScreenPosition(obj.eventInfo.mousePosition);
            //��������ó��ڵ��λ��
            var windowMousePosition = Window.rootVisualElement.ChangeCoordinatesTo(Window.rootVisualElement.parent, screenPos - Window.position.position);
            var nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
            foreach (var item in EditorCache.DefaultNodes)
            {
                if (Target.StartNodes.FirstOrDefault(a => a.GetType() == item.NodeType) != null)
                {
                    continue;
                }
                AddNodeView(AddNode(item.NodeType, nodePosition));
                createNum++;
            }
            if (createNum == 0)
            {
                Window.ShowNotification(new GUIContent("��ǰ�߼�ͼ�Ѵ�������Ĭ�Ͻڵ�"));
            }
        }

        /// <summary>
        /// ��ʽ���߼�ͼ
        /// </summary>
        /// <param name="obj"></param>
        private void m_onFormatCallback(DropdownMenuAction obj)
        {
            LFEditorCache formatConfig = obj.userData as LFEditorCache;
            if (formatConfig != null)
            {
                string savePath = Application.dataPath;
                string saveFile = "undefined";
                if (!string.IsNullOrEmpty(Target.LastFormatPath))
                {
                    savePath = Path.GetDirectoryName(Target.LastFormatPath);
                    saveFile = Path.GetFileNameWithoutExtension(Target.LastFormatPath);
                }
                string filePath = EditorUtility.SaveFilePanel("����", savePath, saveFile, formatConfig.Extension);
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    Window.ShowNotification(new GUIContent("��ѡ�񵼳�·��"));
                    return;
                }
                var logicFormat = Activator.CreateInstance(formatConfig.FormatType) as ILogicFormat;
                bool res = logicFormat.ToFormat(Target, filePath);
                if (res)
                {
                    string tempPath = filePath.Replace("\\", "/");
                    int index = tempPath.IndexOf("Assets");
                    if (index > 0)
                    {
                        Target.LastFormatPath = tempPath.Substring(index, tempPath.Length - index);
                    }
                    Window.ShowNotification(new GUIContent($"����: {formatConfig.FormatName} �ɹ�"));
                    AssetDatabase.Refresh();
                }
                else
                {
                    Window.ShowNotification(new GUIContent($"����: {formatConfig.FormatName} ʧ��"));
                }
            }
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void m_onCreateGroup(DropdownMenuAction obj)
        {
            Vector2 screenPos = Window.GetScreenPosition(obj.eventInfo.mousePosition);
            //��������ó��ڵ��λ��
            var windowMousePosition = Window.rootVisualElement.ChangeCoordinatesTo(Window.rootVisualElement.parent, screenPos - Window.position.position);
            var groupPos = this.contentViewContainer.WorldToLocal(windowMousePosition);
            BaseLogicGroup group = new BaseLogicGroup();

            group.Pos = groupPos;
            group.Title = "Ĭ�Ϸ���";

            Target.Groups.Add(group);
            AddGroupView(group);
        }
        /// <summary>
        /// �����ļ�
        /// </summary>
        /// <param name="obj"></param>
        private void m_onSaveCallback(DropdownMenuAction obj)
        {
            Save();
        }
        /// <summary>
        /// �ڵ��������ص�
        /// </summary>
        /// <param name="searchTreeEntry"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool m_onCreateNodeSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            //��������ó��ڵ��λ��
            var windowMousePosition = this.ChangeCoordinatesTo(this, context.screenMousePosition - Window.position.position);
            var nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
            if (searchTreeEntry.userData is LNEditorCache nodeEditor)
            {
                AddNodeView(AddNode(nodeEditor.NodeType, nodePosition));
            }

            return true;
        }
    }

    /// <summary>
    /// ������
    /// </summary>
    partial class BaseGraphView
    {
        /// <summary>
        /// �������Ϲ�����
        /// </summary>
        private void toolbarView_onDrawLeft()
        {
            EditorGUILayout.Separator();
            GUILayout.Label("�߼�ͼ:");
            var logicName = EditorGUILayout.TextField(Target.Title, EditorStyles.toolbarTextField, GUILayout.MaxWidth(100));
            if (logicName != Target.Title)
            {
                Target.Title = logicName;
                LogicProvider.GetLogicInfo(Target).LogicName = logicName;
            }
        }

        /// <summary>
        /// �����ұ߹�����
        /// </summary>
        private void toolbarView_onDrawRight()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("����", EditorStyles.toolbarButton))
            {

            }
            //EditorGUILayout.Space(10);
            EditorGUILayout.Separator();
            if (GUILayout.Button("����", EditorStyles.toolbarButton))
            {
                Vector3 pos = GetNodeCenter(this.nodes.ToList());
                pos = pos * -1;
                pos.x = pos.x + this.contentRect.size.x * 0.5f;
                pos.y = pos.y + this.contentRect.size.y * 0.5f;
                Target.Pos = pos;
                Target.Scale = Vector3.one;
                UpdateViewTransform(pos, Vector3.one);
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("����Ŀ��ѡ��", EditorStyles.toolbarButton))
            {
                UnityEditor.EditorGUIUtility.PingObject(Target);
                UnityEditor.Selection.activeObject = Target;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void toolbarView_onDrawBottomRight()
        {
            GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel);
            textStyle.normal.textColor = Color.red;
            GUILayout.Label("disconnect", textStyle);
        }

    }

    /// <summary>
    /// ˽�й��߷���
    /// </summary>
    partial class BaseGraphView
    {
        /// <summary>
        /// ��ȡһ���ڵ���������out�˿�����
        /// </summary>
        /// <returns></returns>
        private List<NodeEdgeData> GetNodeEdgeData(BaseNodeView nodeView)
        {
            List<NodeEdgeData> list = new List<NodeEdgeData>();
            List<NodePort> allPorts = new List<NodePort>();
            if (nodeView.OutPut != null)
            {
                allPorts.Add(nodeView.OutPut);
            }
            allPorts.AddRange(nodeView.GetAllPorts().Where(a => a.PortDir == PortDirEnum.Out));
            allPorts.ForEach(port =>
            {
                foreach (Edge edge in port.connections)
                {
                    NodeEdgeData edgeData = new NodeEdgeData();
                    edgeData.Init(edge.input as NodePort, port);
                    list.Add(edgeData);
                }
            });
            return list;
        }

        private Vector3 GetNodeCenter(List<Node> nodes)
        {
            Vector2 minPos = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPos = new Vector2(float.MinValue, float.MinValue);

            foreach (var item in nodes)
            {
                Vector2 pos = item.GetPosition().position;
                if (pos.x < minPos.x)
                {
                    minPos.x = pos.x;
                }
                if (pos.y < minPos.y)
                {
                    minPos.y = pos.y;
                }
                if (pos.x > maxPos.x)
                {
                    maxPos.x = pos.x;
                }
                if (pos.y > maxPos.y)
                {
                    maxPos.y = pos.y;
                }
            }
            return new Vector3((minPos.x + maxPos.x) * 0.5f, (minPos.y + maxPos.y) * 0.5f);
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    partial class BaseGraphView
    {
        private class LGPanelViewGrid : GridBackground { }
        /// <summary>
        /// ��ӱ�������
        /// </summary>
        private void m_addGridBackGround()
        {
            //������񱳾�
            GridBackground gridBackground = new LGPanelViewGrid();
            gridBackground.name = "GridBackground";
            Insert(0, gridBackground);
            //���ñ������ŷ�Χ
            ContentZoomer contentZoomer = new ContentZoomer();
            contentZoomer.minScale = 0.05f;
            contentZoomer.maxScale = 2f;
            contentZoomer.scaleStep = 0.05f;
            this.AddManipulator(contentZoomer);
            //��չ��С�븸������ͬ
            this.StretchToParentSize();
        }
    }
}