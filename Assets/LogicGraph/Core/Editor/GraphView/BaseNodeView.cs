using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;
using Object = UnityEngine.Object;

namespace Logic.Editor
{

    public partial class BaseNodeView : Node
    {
        public string OnlyId => target.OnlyId;
        /// <summary>
        /// 所属的逻辑视图
        /// </summary>
        public BaseGraphView owner { get; private set; }

        /// <summary>
        /// 当前节点视图对应的节点
        /// </summary>
        public BaseLogicNode target { get; private set; }

        protected LNEditorCache nodeEditorCache { get; private set; }
        /// <summary>
        /// 入端口
        /// </summary>
        public NodePort Input { get; protected set; }

        /// <summary>
        /// 出端口
        /// </summary>
        public NodePort OutPut { get; protected set; }

        /// <summary>
        /// 显示锁
        /// </summary>
        public virtual bool ShowLock => true;
        /// <summary>
        /// 显示状态
        /// </summary>
        public virtual bool ShowState => true;

        /// <summary>
        /// 状态图标
        /// </summary>
        public virtual LogicIconEnum StateIcon => LogicIconEnum.Shape;

        /// <summary>
        /// 全部端口
        /// </summary>
        private Dictionary<string, NodePort> _allPort = new Dictionary<string, NodePort>();

        private VisualElement _contentContainer = null;
        public override VisualElement contentContainer => _contentContainer;

        public override string title
        {
            get => base.title;
            set
            {
                target.Title = value;
                base.title = value;
            }
        }
        /// <summary>
        /// 当前节点视图的宽度
        /// </summary>
        public float width
        {
            get => this.style.width.value.value;
            protected set
            {
                this.style.width = value;
            }
        }

        public Color TitleBackgroundColor
        {
            get => titleContainer.style.backgroundColor.value;
            set => titleContainer.style.backgroundColor = value;
        }
        public Color ContentBackgroundColor
        {
            get => _contentContainer.style.backgroundColor.value;
            set => _contentContainer.style.backgroundColor = value;
        }

        public BaseNodeView()
        {
        }

        public void Initialize(BaseGraphView owner, BaseLogicNode node)
        {
            this.target = node;
            this.owner = owner;
            this.nodeEditorCache = owner.EditorCache.GetEditorNode(node.GetType());
            this.title = node.Title;
            initNodeView();
            this.owner.AddElement(this);
            this.SetPosition(new Rect(this.target.Pos, Vector2.zero));
            OnCreate();
        }

        protected virtual void initNodeView()
        {
            styleSheets.Add(LogicUtils.GetNodeStyle());
            this.width = 180;
            //移除右上角折叠按钮
            titleButtonContainer.RemoveFromHierarchy();
            topContainer.style.height = 24;
            _contentContainer = new VisualElement();
            this.topContainer.parent.Add(_contentContainer);
            _contentContainer.name = "center";
            _contentContainer.style.backgroundColor = new Color(0, 0, 0, 0.5f);
            initNodeTitle();
        }

        private void initNodeTitle()
        {
            m_checkTitle();
            var nodeEditorCache = owner.EditorCache.GetEditorNode(target.GetType());
            if ((nodeEditorCache.PortType & PortDirEnum.In) > 0)
            {
                //Input = AddPort("In", Direction.Input);
                Input = ShowPort("In", PortDirEnum.In, PortTypeEnum.Default);
                inputContainer.Add(Input);
            }
            else
            {
                inputContainer.RemoveFromHierarchy();
            }
            if ((nodeEditorCache.PortType & PortDirEnum.Out) > 0)
            {
                OutPut = ShowPort("Out", PortDirEnum.Out, PortTypeEnum.Default);
                //OutPut = AddPort("Out", Direction.Output);
                outputContainer.Add(OutPut);
            }
            else
            {
                outputContainer.RemoveFromHierarchy();
            }
        }
        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="port">从哪个端口添加</param>
        /// <param name="child"></param>
        [Obsolete]
        public virtual void AddChild(NodePort port, BaseLogicNode child)
        {
            this.target.Childs.Add(child);
        }
        /// <summary>
        /// 移除一个节点
        /// </summary>
        /// <param name="port">从哪个端口添加</param>
        /// <param name="child"></param>
        [Obsolete]
        public virtual void RemoveChild(NodePort port, BaseLogicNode child)
        {
            this.target.Childs.Remove(child);
        }
        /// <summary>
        /// 添加一个参数
        /// </summary>
        /// <param name="node">参数节点</param>
        /// <param name="curPort">进来的端口</param>
        /// <param name="accessor">参数存取器</param>
        [Obsolete]
        public virtual void AddVariable(VariableNode varNode, NodePort curPort, ParamAccessor accessor) { Debug.LogError("添加:" + varNode.variable.Name + ":" + accessor); }

        /// <summary>
        /// 移除一个参数
        /// </summary>
        /// <param name="node">参数节点</param>
        /// <param name="curPort">进来的端口</param>
        /// <param name="accessor">参数存取器</param>
        [Obsolete]
        public virtual void DelVariable(VariableNode varNode, NodePort curPort, ParamAccessor accessor) { Debug.LogError("移除:" + varNode.variable.Name + ":" + accessor); }
        /// <summary>
        /// 节点被创建
        /// 视图没有被创建
        /// </summary>
        public virtual void OnCreate() { }

        /// <summary>
        /// 当节点被销毁时候销毁
        /// </summary>
        public virtual void OnDestroy() { }

        /// <summary>
        /// 显示UI
        /// 子类实现
        /// </summary>
        public virtual void ShowUI() { }
        /// <summary>
        /// 当前节点是否可以被链接
        /// </summary>
        /// <param name="ownerPort">自己的端口</param>
        /// <param name="waitLinkPort">等待链接的端口</param>
        /// <returns></returns>
        [Obsolete]
        public virtual bool CanLink(NodePort ownerPort, NodePort waitLinkPort) { return true; }
        /// <summary>
        /// 划线
        /// </summary>
        public virtual void DrawLink()
        {
            if (this.target is VariableNode)
            {
                return;
            }
            if (OutPut == null)
            {
                target.Childs.Clear();
            }
            DrawLink(target.Childs);
            foreach (var item in _allPort)
            {
                item.Value.DrawLink();
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!this.selected || this.owner.selection.Count > 1)
            {
                return;
            }
            evt.menu.AppendAction("查看节点代码", onOpenNodeScript);
            evt.menu.AppendAction("查看界面代码", onOpenNodeViewScript);
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("删除", (a) => owner.DeleteSelection());
            evt.StopPropagation();
        }
    }
    /// <summary>
    /// 子类调用方法
    /// </summary>
    partial class BaseNodeView
    {
        ///// <summary>
        ///// 添加一个端口
        ///// </summary>
        ///// <param name="labelName"></param>
        ///// <param name="direction"></param>
        ///// <param name="isCube"></param>
        ///// <returns></returns>
        //protected PortView AddPort(string labelName, Direction direction, Capacity capacity = Capacity.Multi, bool isCube = false)
        //{
        //    PortView portView = PortView.CreatePort(labelName, direction, capacity, isCube, this.owner.ConnectorListener);
        //    portView.Initialize(this, this.OnlyId);
        //    return portView;
        //}
        protected void DrawLink(List<BaseLogicNode> nodeList)
        {
            nodeList.RemoveAll(a => a == null);
            if (nodeList.Count > 0)
            {
                for (int i = nodeList.Count - 1; i >= 0; i--)
                {
                    BaseNodeView nodeView = owner.GetNodeView(nodeList[i]);
                    if (nodeView == null)
                    {
                        continue;
                    }
                    DrawLink(nodeView);
                }
            }
        }

        protected void DrawLink(BaseNodeView input) => DrawLink(input, this.OutPut);

        protected void DrawLink(BaseNodeView input, NodePort outPort)
        {
            if (input.Input == null)
            {
                //当子节点入口被删除时候,要删除自己的子节点
                this.RemoveChild(outPort, input.target);
                return;
            }
            EdgeView edge = new EdgeView();
            edge.input = input.Input;
            edge.output = outPort;
            input.Input.Connect(edge);
            outPort.Connect(edge);
            this.owner.AddElement(edge);
            this.owner.schedule.Execute(() =>
            {
                edge.UpdateEdgeControl();
            }).ExecuteLater(1);
        }
        protected void DrawLink(NodePort input, NodePort outPort)
        {
            if (input == null)
            {
                owner.Window.ShowNotification(new GUIContent("入端口为空,请检查代码"));
                Debug.LogError("入端口为空,请检查代码");
                return;
            }
            if (outPort == null)
            {
                owner.Window.ShowNotification(new GUIContent("出端口为空,请检查代码"));
                Debug.LogError("出端口为空,请检查代码");
                return;
            }
            EdgeView edge = new EdgeView();
            edge.input = input;
            edge.output = outPort;
            input.Connect(edge);
            outPort.Connect(edge);
            this.owner.AddElement(edge);
            this.owner.schedule.Execute(() =>
            {
                edge.UpdateEdgeControl();
            }).ExecuteLater(1);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="port"></param>
        protected void Disconnect(PortView port)
        {
            var connects = port.connections.ToList();
            this.owner.selection.Clear();
            this.owner.selection.AddRange(connects.OfType<GraphElement>());
            this.owner.DeleteSelection();
        }
        /// <summary>
        /// 添加一个UI元素到节点视图中
        /// </summary>
        /// <param name="ui"></param>
        protected void AddUI(VisualElement element)
        {
            this.Add(element);
        }
        protected void AddUI(INodeElement element)
        {
            this.Add(element as VisualElement);
        }
        protected Label GetLabel(string defaultValue = "")
        {
            Label label = new Label();
            label.text = defaultValue;
            return label;
        }

        protected TextField GetInputField(string titleText, string defaultValue, Action<string> changed = null)
        {
            TextField field = new TextField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.multiline = true;
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<string>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected Toggle GetInputField(string titleText, bool defaultValue, Action<bool> changed = null)
        {
            Toggle field = new Toggle();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<bool>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected IntegerField GetInputField(string titleText, int defaultValue, Action<int> changed = null)
        {
            IntegerField field = new IntegerField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<int>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected FloatField GetInputField(string titleText, float defaultValue, Action<float> changed = null)
        {
            FloatField field = new FloatField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<float>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected DoubleField GetInputField(string titleText, double defaultValue, Action<double> changed = null)
        {
            DoubleField field = new DoubleField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<double>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected EnumField GetInputField(string titleText, Enum defaultValue, Action<Enum> changed = null)
        {
            EnumField field = new EnumField();
            field.Init(defaultValue);
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<Enum>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected Vector2Field GetInputField(string titleText, Vector2 defaultValue, Action<Vector2> changed = null)
        {
            Vector2Field field = new Vector2Field();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<Vector2>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected Vector3Field GetInputField(string titleText, Vector3 defaultValue, Action<Vector3> changed = null)
        {
            Vector3Field field = new Vector3Field();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<Vector3>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected Vector4Field GetInputField(string titleText, Vector4 defaultValue, Action<Vector4> changed = null)
        {
            Vector4Field field = new Vector4Field();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<Vector4>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected Vector2IntField GetInputField(string titleText, Vector2Int defaultValue, Action<Vector2Int> changed = null)
        {
            Vector2IntField field = new Vector2IntField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<Vector2Int>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected Vector3IntField GetInputField(string titleText, Vector3Int defaultValue, Action<Vector3Int> changed = null)
        {
            Vector3IntField field = new Vector3IntField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<Vector3Int>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected ColorField GetInputField(string titleText, Color defaultValue, Action<Color> changed = null)
        {
            ColorField field = new ColorField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<Color>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected GradientField GetInputField(string titleText, Gradient defaultValue, Action<Gradient> changed = null)
        {
            GradientField field = new GradientField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<Gradient>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected CurveField GetInputField(string titleText, AnimationCurve defaultValue, Action<AnimationCurve> changed = null)
        {
            CurveField field = new CurveField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<AnimationCurve>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected BoundsField GetInputField(string titleText, Bounds defaultValue, Action<Bounds> changed = null)
        {
            BoundsField field = new BoundsField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<Bounds>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected BoundsIntField GetInputField(string titleText, BoundsInt defaultValue, Action<BoundsInt> changed = null)
        {
            BoundsIntField field = new BoundsIntField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<BoundsInt>>((e) => changed?.Invoke(e.newValue));
            return field;
        }
        protected GameObjectField GetInputField(string titleText, GameObject defaultValue, Action<GameObject> changed = null)
        {
            GameObjectField field = new GameObjectField();
            field.allowSceneObjects = false;
            field.label = titleText;
            SetBaseFieldStyle(field.objField);
            field.value = defaultValue;
            field.onValueChange += changed;
            return field;
        }
        protected INodeElement ShowUI(string fieldName)
        {
            INodeElement nodeElement = null;
            if (this.nodeEditorCache.FieldInfos.ContainsKey(fieldName))
            {
                FieldInfo field = this.nodeEditorCache.FieldInfos[fieldName];
                Type attrType = this.nodeEditorCache.FieldTypes[fieldName];
                Type elementType = NodeElementUtils.ElementMapping[attrType];
                switch (attrType)
                {
                    case Type t when t == typeof(NodePortAttribute):
                        nodeElement = NodePort.CreatePort(field, owner.ConnectorListener);
                        break;
                    default:
                        nodeElement = Activator.CreateInstance(elementType) as INodeElement;
                        break;
                }
                nodeElement.Init(this, field);
                this.AddUI(nodeElement);
                return nodeElement;

            }
            return null;
        }
        protected INodeElement<T> ShowUI<T>(string fieldName)
        {
            return ShowUI(fieldName) as INodeElement<T>;
        }
        protected NodePort ShowPort(string fieldName)
        {
            NodePort port = ShowUI(fieldName) as NodePort;
            _allPort.Add(fieldName, port);
            return port;
        }
        protected NodePort ShowPort(string title, PortDirEnum dir, PortTypeEnum portType)
        {
            var port = NodePort.CreatePort(title, dir, portType, owner.ConnectorListener);
            port.Init(this, null);
            return port;
        }

        public NodePort GetPort(string fieldName)
        {
            if (_allPort.ContainsKey(fieldName))
            {
                return _allPort[fieldName];
            }
            return null;
        }
        public List<GraphElement> GetCollectElements()
        {
            List<NodePort> ports = new List<NodePort>();
            if (Input != null)
            {
                ports.Add(Input);
            }
            if (OutPut != null)
            {
                ports.Add(OutPut);
            }
            ports.AddRange(_allPort.Values);
            return (from d in ports.SelectMany((NodePort p) => p.connections) where (d.capabilities & Capabilities.Deletable) != 0 select d).Cast<GraphElement>().ToList();
        }
        /// <summary>
        /// 设置字段组件的默认样式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        protected void SetBaseFieldStyle<T>(BaseField<T> element)
        {
            element.style.minHeight = 24;
            element.style.marginTop = 2;
            element.style.marginRight = 2;
            element.style.marginLeft = 2;
            element.style.marginBottom = 2;
            element.style.unityTextAlign = TextAnchor.MiddleLeft;
            element.labelElement.style.minWidth = 50;
            element.labelElement.style.fontSize = 12;
        }
    }
    /// <summary>
    /// 重写
    /// </summary>
    partial class BaseNodeView
    {
        public override void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc)
        {
            base.CollectElements(collectedElementSet, conditionFunc);
            collectedElementSet.UnionWith((from d in contentContainer.Children().OfType<Port>().SelectMany((Port c) => c.connections)
                                           where (d.capabilities & Capabilities.Deletable) != 0
                                           select d).Cast<GraphElement>());
        }
        public override void SetPosition(Rect newPos)
        {
            if (target.IsLock)
            {
                return;
            }
            base.SetPosition(newPos);
            target.Pos = newPos.position;
        }
    }
    //右键
    partial class BaseNodeView
    {
        /// <summary>
        /// 查看节点代码
        /// </summary>
        /// <param name="obj"></param>
        protected void onOpenNodeScript(DropdownMenuAction obj)
        {
            string[] guids = AssetDatabase.FindAssets(this.target.GetType().Name);
            foreach (var item in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(item);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript != null && monoScript.GetClass() == this.target.GetType())
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), -1);
                    break;
                }
            }
        }
        /// <summary>
        /// 查看界面代码
        /// </summary>
        /// <param name="obj"></param>
        protected void onOpenNodeViewScript(DropdownMenuAction obj)
        {
            string[] guids = AssetDatabase.FindAssets(this.GetType().Name);
            foreach (var item in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(item);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript != null && monoScript.GetClass() == this.GetType())
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), -1);
                    break;
                }
            }
        }
    }
    //Title
    partial class BaseNodeView
    {
        /// <summary>
        /// 运行状态
        /// </summary>
        private VisualElement _runState;
        private VisualElement _lock;
        private TextField _titleEditor;
        private Label _titleItem;
        private bool _editTitleCancelled = false;
        private void m_checkTitle()
        {
            m_initState();
            //找到Title对应的元素
            _titleItem = this.Q<Label>("title-label");
            _titleItem.RegisterCallback<MouseDownEvent>(m_onMouseDownEvent);
            _titleEditor = new TextField();
            _titleEditor.name = "title-field";
            titleContainer.Add(_titleEditor);
            m_initLock();
            _titleEditor.style.display = DisplayStyle.None;
            VisualElement visualElement2 = _titleEditor.Q(TextInputBaseField<string>.textInputUssName);
            visualElement2.RegisterCallback<FocusOutEvent>(m_onEditTitleFinished);
            visualElement2.RegisterCallback<KeyDownEvent>(m_onTitleEditorOnKeyDown);
        }

        private void m_initState()
        {
            _runState = new VisualElement();
            _runState.name = "run-state";
            _runState.tooltip = "运行状态";
            titleContainer.Insert(0, _runState);
            string str = $"run-state-{StateIcon.ToString().ToLower()}";
            _runState.AddToClassList(str);
            _runState.style.display = ShowState ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void m_initLock()
        {
            _lock = new Button();
            _lock.name = "lock-icon";
            _lock.ClearClassList();
            _lock.AddToClassList(target.IsLock ? "lock" : "unlock");
            _lock.style.display = ShowLock ? DisplayStyle.Flex : DisplayStyle.None;
            _lock.RegisterCallback<PointerUpEvent>(m_lockClick);
            titleContainer.Add(_lock);
        }

        private void m_lockClick(PointerUpEvent evt)
        {
            target.IsLock = !target.IsLock;
            _lock.ClearClassList();
            if (target.IsLock)
            {
                _lock.AddToClassList("lock");
            }
            else
                _lock.AddToClassList("unlock");

        }

        private void m_onTitleEditorOnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Escape:
                    _editTitleCancelled = true;
                    _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Blur();
                    break;
                case KeyCode.Return:
                    _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Blur();
                    break;
            }
        }

        private void m_onEditTitleFinished(FocusOutEvent evt)
        {
            _titleItem.style.display = DisplayStyle.Flex;
            _titleEditor.style.display = DisplayStyle.None;
            if (!_editTitleCancelled)
            {
                this.title = _titleEditor.text;
            }
            _editTitleCancelled = true;
        }

        private void m_onMouseDownEvent(MouseDownEvent evt)
        {
            if (evt.clickCount == 2 && evt.button == 0)
            {
                _titleItem.style.display = DisplayStyle.None;
                _titleEditor.style.display = DisplayStyle.Flex;
                _titleEditor.value = _titleItem.text;
                _titleEditor.SelectAll();
                _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Focus();
                _editTitleCancelled = false;
            }
        }
    }
    public abstract class BaseNodeView<TNode> : BaseNodeView where TNode : BaseLogicNode
    {
        protected TNode node => target as TNode;
    }
    //    public abstract class BaseNodeView1
    //    {
    //        public string OnlyId => Target.OnlyId;

    //        private Node _view;

    //        public Node View => _view;

    //        /// <summary>
    //        /// 所属的逻辑视图
    //        /// </summary>
    //        protected BaseGraphView owner { get; private set; }

    //        /// <summary>
    //        /// 当前节点视图对应的节点
    //        /// </summary>
    //        public BaseLogicNode Target { get; private set; }

    //        /// <summary>
    //        /// 入端口
    //        /// </summary>
    //        public PortView Input { get; protected set; }

    //        /// <summary>
    //        /// 出端口
    //        /// </summary>
    //        public PortView OutPut { get; protected set; }

    //        /// <summary>
    //        /// 显示锁
    //        /// </summary>
    //        public virtual bool ShowLock => true;
    //        /// <summary>
    //        /// 显示状态
    //        /// </summary>
    //        public virtual bool ShowState => true;

    //        /// <summary>
    //        /// 状态图标
    //        /// </summary>
    //        public virtual LogicIconEnum StateIcon => LogicIconEnum.Shape;

    //        private float _width = 180;
    //        /// <summary>
    //        /// 当前节点视图的宽度
    //        /// </summary>
    //        public float Width
    //        {
    //            get => _width;
    //            protected set
    //            {
    //                _width = value;
    //                if (this._view != null)
    //                {
    //                    this._view.style.width = _width;
    //                }
    //            }
    //        }

    //        public Color TitleBackgroundColor
    //        {
    //            get => (_view as INodeVisualElement).TitleBackgroundColor;
    //            set => (_view as INodeVisualElement).TitleBackgroundColor = value;
    //        }
    //        public Color ContentBackgroundColor
    //        {
    //            get => (_view as INodeVisualElement).ContentBackgroundColor;
    //            set => (_view as INodeVisualElement).ContentBackgroundColor = value;
    //        }
    //        public string Tooltip
    //        {
    //            get => this.Target.Describe;
    //            set
    //            {
    //                this.Target.Describe = value;
    //                this._view.tooltip = value;
    //            }
    //        }

    //        private string _title = "";
    //        public string Title
    //        {
    //            get { return _title; }
    //            set
    //            {
    //                _title = value;
    //                Target.Title = value;
    //                if (this.View != null)
    //                {
    //                    this.View.title = value;
    //                }
    //            }
    //        }

    //        private static Action<PortView, bool> onPortSetDefault;

    //        #region 公共方法
    //        static BaseNodeView1()
    //        {
    //            PropertyInfo prop = typeof(PortView).GetProperty("IsDefault", BindingFlags.Instance | BindingFlags.Public);
    //            var method = prop.GetSetMethod(true);
    //            onPortSetDefault = (Action<PortView, bool>)Delegate.CreateDelegate(typeof(Action<PortView, bool>), null, method);
    //        }

    //        /// <summary>
    //        /// 初始化
    //        /// </summary>
    //        public void Initialize(BaseGraphView owner, BaseLogicNode node)
    //        {
    //            Target = node;
    //            this._title = node.Title;
    //            this.owner = owner;
    //            //this.graphCache = owner.LGInfoCache;
    //            this._view = GetNode();
    //            if (this.View is INodeVisualElement nodeView)
    //            {
    //                nodeView.onGenericMenu += OnGenericMenu;
    //                this._view.tooltip = node.Describe;
    //            }
    //            else
    //            {
    //                throw new Exception("界面视图需要实现INodeVisualElement接口");
    //            }
    //            this.owner.AddElement(_view);
    //            OnCreate();
    //        }

    //        /// <summary>
    //        /// 获取一个Node
    //        /// </summary>
    //        /// <returns></returns>
    //        protected virtual Node GetNode()
    //        {
    //            NodeVisualElement view = new NodeVisualElement(this, owner);
    //            var nodeEditorCache = owner.EditorCache.GetEditorNode(Target.GetType());
    //            if ((nodeEditorCache.PortType & PortEnum.In) > PortEnum.None)
    //            {
    //                Input = AddPort("In", Direction.Input);
    //                onPortSetDefault.Invoke(Input, true);
    //                view.inputContainer.Add(Input);
    //            }
    //            else
    //            {
    //                view.inputContainer.RemoveFromHierarchy();
    //            }
    //            if ((nodeEditorCache.PortType & PortEnum.Out) > PortEnum.None)
    //            {
    //                OutPut = AddPort("Out", Direction.Output);
    //                onPortSetDefault.Invoke(OutPut, true);
    //                view.outputContainer.Add(OutPut);
    //            }
    //            else
    //            {
    //                view.outputContainer.RemoveFromHierarchy();
    //            }
    //            return view;
    //        }

    //        /// <summary>
    //        /// 节点被创建
    //        /// 视图没有被创建
    //        /// </summary>
    //        public virtual void OnCreate() { }

    //        /// <summary>
    //        /// 当节点被销毁时候销毁
    //        /// </summary>
    //        public virtual void OnDestroy() { }

    //        /// <summary>
    //        /// 显示UI
    //        /// 子类实现
    //        /// </summary>
    //        public virtual void ShowUI() { }

    //        /// <summary>
    //        /// 显示参数界面
    //        /// </summary>
    //        public virtual void ShowParamUI() { }
    //        /// <summary>
    //        /// 添加子节点
    //        /// </summary>
    //        /// <param name="port">从哪个端口添加</param>
    //        /// <param name="child"></param>
    //        public virtual void AddChild(PortView port, BaseLogicNode child)
    //        {
    //            this.Target.Childs.Add(child);
    //        }
    //        /// <summary>
    //        /// 移除一个节点
    //        /// </summary>
    //        /// <param name="port">从哪个端口添加</param>
    //        /// <param name="child"></param>
    //        public virtual void RemoveChild(PortView port, BaseLogicNode child)
    //        {
    //            this.Target.Childs.Remove(child);
    //        }

    //        /// <summary>
    //        /// 添加一个参数
    //        /// </summary>
    //        /// <param name="node">参数节点</param>
    //        /// <param name="curPort">进来的端口</param>
    //        /// <param name="accessor">参数存取器</param>
    //        public virtual void AddVariable(VariableNode varNode, PortView curPort, ParamAccessor accessor) { Debug.LogError("添加:" + varNode.variable.Name + ":" + accessor); }

    //        /// <summary>
    //        /// 移除一个参数
    //        /// </summary>
    //        /// <param name="node">参数节点</param>
    //        /// <param name="curPort">进来的端口</param>
    //        /// <param name="accessor">参数存取器</param>
    //        public virtual void DelVariable(VariableNode varNode, PortView curPort, ParamAccessor accessor) { Debug.LogError("移除:" + varNode.variable.Name + ":" + accessor); }

    //        /// <summary>
    //        /// 当前节点是否可以被链接
    //        /// </summary>
    //        /// <param name="ownerPort">自己的端口</param>
    //        /// <param name="waitLinkPort">等待链接的端口</param>
    //        /// <returns></returns>
    //        public virtual bool CanLink(PortView ownerPort, PortView waitLinkPort) { return true; }

    //        public virtual void DrawLink()
    //        {
    //            if (OutPut == null)
    //            {
    //                Target.Childs.Clear();
    //            }
    //            DrawLink(Target.Childs);
    //        }

    //        protected void DrawLink(List<BaseLogicNode> nodeList)
    //        {
    //            nodeList.RemoveAll(a => a == null);
    //            if (nodeList.Count > 0)
    //            {
    //                for (int i = nodeList.Count - 1; i >= 0; i--)
    //                {
    //                    BaseNodeView1 nodeView = owner.GetNodeView(nodeList[i]);
    //                    if (nodeView == null)
    //                    {
    //                        this.RemoveChild(this.OutPut, nodeList[i]);
    //                        continue;
    //                    }
    //                    DrawLink(nodeView);
    //                }
    //            }
    //        }

    //        protected void DrawLink(BaseNodeView1 input) => DrawLink(input, this.OutPut);

    //        protected void DrawLink(BaseNodeView1 input, PortView outPort)
    //        {
    //            if (input.Input == null)
    //            {
    //                //当子节点入口被删除时候,要删除自己的子节点
    //                this.RemoveChild(outPort, input.Target);
    //                return;
    //            }
    //            EdgeView edge = new EdgeView();
    //            edge.input = input.Input;
    //            edge.output = outPort;
    //            input.Input.Connect(edge);
    //            outPort.Connect(edge);
    //            this.owner.AddElement(edge);
    //            this.owner.schedule.Execute(() =>
    //            {
    //                edge.UpdateEdgeControl();
    //            }).ExecuteLater(1);
    //        }
    //        protected void DrawLink(PortView input, PortView outPort)
    //        {
    //            if (input == null)
    //            {
    //                owner.Window.ShowNotification(new GUIContent("入端口为空,请检查代码"));
    //                Debug.LogError("入端口为空,请检查代码");
    //                return;
    //            }
    //            if (outPort == null)
    //            {
    //                owner.Window.ShowNotification(new GUIContent("出端口为空,请检查代码"));
    //                Debug.LogError("出端口为空,请检查代码");
    //                return;
    //            }
    //            EdgeView edge = new EdgeView();
    //            edge.input = input;
    //            edge.output = outPort;
    //            input.Connect(edge);
    //            outPort.Connect(edge);
    //            this.owner.AddElement(edge);
    //            this.owner.schedule.Execute(() =>
    //            {
    //                edge.UpdateEdgeControl();
    //            }).ExecuteLater(1);
    //        }

    //        /// <summary>
    //        /// 断开连接
    //        /// </summary>
    //        /// <param name="port"></param>
    //        protected void Disconnect(PortView port)
    //        {
    //            var connects = port.connections.ToList();
    //            this.owner.selection.Clear();
    //            this.owner.selection.AddRange(connects.OfType<GraphElement>());
    //            this.owner.DeleteSelection();
    //        }
    //        #endregion


    //        #region 私有子类
    //        /// <summary>
    //        /// 绑定右键
    //        /// </summary>
    //        /// <param name="evt"></param>
    //        protected virtual void OnGenericMenu(ContextualMenuPopulateEvent evt)
    //        {
    //            evt.menu.AppendAction("查看节点代码", onOpenNodeScript);
    //            evt.menu.AppendAction("查看界面代码", onOpenNodeViewScript);
    //            evt.menu.AppendAction("编辑描述", onOpenNodeDescribe);
    //            evt.menu.AppendSeparator();
    //            evt.menu.AppendAction("删除", (a) => owner.DeleteSelection());
    //        }

    //        /// <summary>
    //        /// 添加一个端口
    //        /// </summary>
    //        /// <param name="labelName"></param>
    //        /// <param name="direction"></param>
    //        /// <param name="isCube"></param>
    //        /// <returns></returns>
    //        protected PortView AddPort(string labelName, Direction direction, Capacity capacity = Capacity.Multi, bool isCube = false)
    //        {
    //            PortView portView = PortView.CreatePort(labelName, direction, capacity, isCube, this.owner.ConnectorListener);
    //            portView.Initialize(this, this.OnlyId);
    //            return portView;
    //        }

    //        /// <summary>
    //        /// 添加一个UI元素到节点视图中
    //        /// </summary>
    //        /// <param name="ui"></param>
    //        protected void AddUI(VisualElement element)
    //        {
    //            (_view as INodeVisualElement).AddUI(element);
    //        }
    //        protected Label GetLabel(string defaultValue = "")
    //        {
    //            Label label = new Label();
    //            label.text = defaultValue;
    //            return label;
    //        }

    //        protected TextField GetInputField(string titleText, string defaultValue, Action<string> changed = null)
    //        {
    //            TextField field = new TextField();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.multiline = true;
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<string>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected Toggle GetInputField(string titleText, bool defaultValue, Action<bool> changed = null)
    //        {
    //            Toggle field = new Toggle();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<bool>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected IntegerField GetInputField(string titleText, int defaultValue, Action<int> changed = null)
    //        {
    //            IntegerField field = new IntegerField();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<int>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected FloatField GetInputField(string titleText, float defaultValue, Action<float> changed = null)
    //        {
    //            FloatField field = new FloatField();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<float>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected DoubleField GetInputField(string titleText, double defaultValue, Action<double> changed = null)
    //        {
    //            DoubleField field = new DoubleField();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<double>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected EnumField GetInputField(string titleText, Enum defaultValue, Action<Enum> changed = null)
    //        {
    //            EnumField field = new EnumField();
    //            field.Init(defaultValue);
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<Enum>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected Vector2Field GetInputField(string titleText, Vector2 defaultValue, Action<Vector2> changed = null)
    //        {
    //            Vector2Field field = new Vector2Field();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<Vector2>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected Vector3Field GetInputField(string titleText, Vector3 defaultValue, Action<Vector3> changed = null)
    //        {
    //            Vector3Field field = new Vector3Field();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<Vector3>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected Vector4Field GetInputField(string titleText, Vector4 defaultValue, Action<Vector4> changed = null)
    //        {
    //            Vector4Field field = new Vector4Field();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<Vector4>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected Vector2IntField GetInputField(string titleText, Vector2Int defaultValue, Action<Vector2Int> changed = null)
    //        {
    //            Vector2IntField field = new Vector2IntField();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<Vector2Int>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected Vector3IntField GetInputField(string titleText, Vector3Int defaultValue, Action<Vector3Int> changed = null)
    //        {
    //            Vector3IntField field = new Vector3IntField();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<Vector3Int>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected ColorField GetInputField(string titleText, Color defaultValue, Action<Color> changed = null)
    //        {
    //            ColorField field = new ColorField();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<Color>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected GradientField GetInputField(string titleText, Gradient defaultValue, Action<Gradient> changed = null)
    //        {
    //            GradientField field = new GradientField();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<Gradient>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected CurveField GetInputField(string titleText, AnimationCurve defaultValue, Action<AnimationCurve> changed = null)
    //        {
    //            CurveField field = new CurveField();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<AnimationCurve>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected BoundsField GetInputField(string titleText, Bounds defaultValue, Action<Bounds> changed = null)
    //        {
    //            BoundsField field = new BoundsField();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<Bounds>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected BoundsIntField GetInputField(string titleText, BoundsInt defaultValue, Action<BoundsInt> changed = null)
    //        {
    //            BoundsIntField field = new BoundsIntField();
    //            field.label = titleText;
    //            SetBaseFieldStyle(field);
    //            field.value = defaultValue;
    //            field.RegisterCallback<ChangeEvent<BoundsInt>>((e) => changed?.Invoke(e.newValue));
    //            return field;
    //        }
    //        protected GameObjectField GetInputField(string titleText, GameObject defaultValue, Action<GameObject> changed = null)
    //        {
    //            GameObjectField field = new GameObjectField();
    //            field.allowSceneObjects = false;
    //            field.label = titleText;
    //            SetBaseFieldStyle(field.objField);
    //            field.value = defaultValue;
    //            field.onValueChange += changed;
    //            return field;
    //        }

    //        /// <summary>
    //        /// 设置字段组件的默认样式
    //        /// </summary>
    //        /// <typeparam name="T"></typeparam>
    //        /// <param name="element"></param>
    //        protected void SetBaseFieldStyle<T>(BaseField<T> element)
    //        {
    //            element.style.minHeight = 24;
    //            element.style.marginTop = 2;
    //            element.style.marginRight = 2;
    //            element.style.marginLeft = 2;
    //            element.style.marginBottom = 2;
    //            element.style.unityTextAlign = TextAnchor.MiddleLeft;
    //            element.labelElement.style.minWidth = 50;
    //            element.labelElement.style.fontSize = 12;
    //        }

    //        /// <summary>
    //        /// 移除子节点
    //        /// </summary>
    //        /// <param name="obj"></param>
    //        protected void onRemoveChild(DropdownMenuAction obj)
    //        {
    //            if (obj.userData is BaseNodeView1 nodeView)
    //            {
    //                Edge edge = this.OutPut.connections.FirstOrDefault(a => a.input.node == nodeView.View);
    //                this.owner.DeleteElements(new Edge[] { edge });
    //                owner.Save();
    //            }

    //        }

    //        /// <summary>
    //        /// 查看节点代码
    //        /// </summary>
    //        /// <param name="obj"></param>
    //        protected void onOpenNodeScript(DropdownMenuAction obj)
    //        {
    //            string[] guids = AssetDatabase.FindAssets(this.Target.GetType().Name);
    //            foreach (var item in guids)
    //            {
    //                string path = AssetDatabase.GUIDToAssetPath(item);
    //                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
    //                if (monoScript != null && monoScript.GetClass() == this.Target.GetType())
    //                {
    //                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), -1);
    //                    break;
    //                }
    //            }
    //        }
    //        /// <summary>
    //        /// 查看界面代码
    //        /// </summary>
    //        /// <param name="obj"></param>
    //        protected void onOpenNodeViewScript(DropdownMenuAction obj)
    //        {
    //            string[] guids = AssetDatabase.FindAssets(this.GetType().Name);
    //            foreach (var item in guids)
    //            {
    //                string path = AssetDatabase.GUIDToAssetPath(item);
    //                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
    //                if (monoScript != null && monoScript.GetClass() == this.GetType())
    //                {
    //                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), -1);
    //                    break;
    //                }
    //            }
    //        }

    //        /// <summary>
    //        /// 打开描述
    //        /// </summary>
    //        /// <param name="obj"></param>
    //        protected void onOpenNodeDescribe(DropdownMenuAction obj)
    //        {
    //            //owner.ShowNodeDescribe(this);
    //        }
    //        #endregion


    //        private class NodeVisualElement : Node, INodeVisualElement
    //        {
    //            private BaseNodeView1 nodeView;

    //            public event Action<ContextualMenuPopulateEvent> onGenericMenu;
    //            /// <summary>
    //            /// 逻辑图视图
    //            /// </summary>
    //            private BaseGraphView _graphView;

    //            /// <summary>
    //            /// 重新定义的内容容器
    //            /// </summary>
    //            private VisualElement m_content { get; set; }
    //            public Color TitleBackgroundColor { get => titleContainer.style.backgroundColor.value; set => titleContainer.style.backgroundColor = value; }
    //            public Color ContentBackgroundColor { get => ContentContainer.style.backgroundColor.value; set => ContentContainer.style.backgroundColor = value; }
    //            public VisualElement ContentContainer { get; private set; }
    //            public NodeVisualElement(BaseNodeView1 nodeView, BaseGraphView graphView)
    //            {
    //                this.nodeView = nodeView;
    //                _graphView = graphView;
    //                userData = nodeView;
    //                styleSheets.Add(LogicUtils.GetNodeStyle());
    //                this.style.width = this.nodeView.Width;
    //                //移除右上角折叠按钮
    //                titleButtonContainer.RemoveFromHierarchy();
    //                topContainer.style.height = 24;
    //                ContentContainer = new VisualElement();
    //                this.topContainer.parent.Add(ContentContainer);
    //                ContentContainer.name = "center";
    //                ContentContainer.style.backgroundColor = new Color(0, 0, 0, 0.5f);
    //                m_checkTitle();
    //                this.title = this.nodeView.Title;
    //                base.SetPosition(new Rect(this.nodeView.Target.Pos, Vector2.zero));
    //            }

    //            public void AddUI(VisualElement ui)
    //            {
    //                ContentContainer.Add(ui);
    //            }

    //            public override void SetPosition(Rect newPos)
    //            {
    //                if (nodeView.Target.IsLock)
    //                {
    //                    return;
    //                }
    //                base.SetPosition(newPos);
    //                nodeView.Target.Pos = newPos.position;
    //            }

    //            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    //            {
    //                if (!this.selected || this._graphView.selection.Count > 1)
    //                {
    //                    return;
    //                }
    //                onGenericMenu?.Invoke(evt);
    //                evt.StopPropagation();
    //            }

    //#if UNITY_2020_1_OR_NEWER
    //            public override void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc)
    //            {
    //                base.CollectElements(collectedElementSet, conditionFunc);
    //                collectedElementSet.UnionWith((from d in ContentContainer.Children().OfType<Port>().SelectMany((Port c) => c.connections)
    //                                               where (d.capabilities & Capabilities.Deletable) != 0
    //                                               select d).Cast<GraphElement>());
    //            }
    //#endif

    //            #region Title

    //            /// <summary>
    //            /// 运行状态
    //            /// </summary>
    //            private VisualElement _runState;
    //            private VisualElement _lock;
    //            private TextField _titleEditor;
    //            private Label _titleItem;
    //            private bool _editTitleCancelled = false;
    //            private void m_checkTitle()
    //            {
    //                m_initState();
    //                //找到Title对应的元素
    //                _titleItem = this.Q<Label>("title-label");
    //                _titleItem.RegisterCallback<MouseDownEvent>(m_onMouseDownEvent);
    //                _titleEditor = new TextField();
    //                _titleEditor.name = "title-field";
    //                titleContainer.Add(_titleEditor);
    //                m_initLock();
    //                _titleEditor.style.display = DisplayStyle.None;
    //                VisualElement visualElement2 = _titleEditor.Q(TextInputBaseField<string>.textInputUssName);
    //                visualElement2.RegisterCallback<FocusOutEvent>(m_onEditTitleFinished);
    //                visualElement2.RegisterCallback<KeyDownEvent>(m_onTitleEditorOnKeyDown);
    //            }

    //            private void m_initState()
    //            {
    //                _runState = new VisualElement();
    //                _runState.name = "run-state";
    //                _runState.tooltip = "运行状态";
    //                titleContainer.Insert(0, _runState);
    //                string str = $"run-state-{nodeView.StateIcon.ToString().ToLower()}";
    //                _runState.AddToClassList(str);
    //                _runState.style.display = nodeView.ShowState ? DisplayStyle.Flex : DisplayStyle.None;
    //            }

    //            private void m_initLock()
    //            {
    //                _lock = new Button();
    //                _lock.name = "lock-icon";
    //                _lock.ClearClassList();
    //                _lock.AddToClassList(nodeView.Target.IsLock ? "lock" : "unlock");
    //                _lock.style.display = nodeView.ShowLock ? DisplayStyle.Flex : DisplayStyle.None;
    //                _lock.RegisterCallback<PointerUpEvent>(m_lockClick);
    //                titleContainer.Add(_lock);
    //            }

    //            private void m_lockClick(PointerUpEvent evt)
    //            {
    //                nodeView.Target.IsLock = !nodeView.Target.IsLock;
    //                _lock.ClearClassList();
    //                if (nodeView.Target.IsLock)
    //                {
    //                    _lock.AddToClassList("lock");
    //                }
    //                else
    //                    _lock.AddToClassList("unlock");

    //            }

    //            private void m_onTitleEditorOnKeyDown(KeyDownEvent evt)
    //            {
    //                switch (evt.keyCode)
    //                {
    //                    case KeyCode.Escape:
    //                        _editTitleCancelled = true;
    //                        _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Blur();
    //                        break;
    //                    case KeyCode.Return:
    //                        _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Blur();
    //                        break;
    //                }
    //            }

    //            private void m_onEditTitleFinished(FocusOutEvent evt)
    //            {
    //                _titleItem.style.display = DisplayStyle.Flex;
    //                _titleEditor.style.display = DisplayStyle.None;
    //                if (!_editTitleCancelled)
    //                {
    //                    this.nodeView.Title = _titleEditor.text;
    //                }
    //                _editTitleCancelled = true;
    //            }

    //            private void m_onMouseDownEvent(MouseDownEvent evt)
    //            {
    //                if (evt.clickCount == 2 && evt.button == 0)
    //                {
    //                    _titleItem.style.display = DisplayStyle.None;
    //                    _titleEditor.style.display = DisplayStyle.Flex;
    //                    _titleEditor.value = _titleItem.text;
    //                    _titleEditor.SelectAll();
    //                    _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Focus();
    //                    _editTitleCancelled = false;
    //                }
    //            }

    //            #endregion
    //        }
    //    }
    //    public abstract class BaseNodeView1<T> : BaseNodeView1 where T : BaseLogicNode
    //    {
    //        protected T node => Target as T;
    //    }

}