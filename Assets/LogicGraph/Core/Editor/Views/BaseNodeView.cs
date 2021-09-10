using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Logic.Editor
{
    public abstract class BaseNodeView
    {
        public string OnlyId => Target.OnlyId;
        private LGInfoCache _graphCache;
        protected LGInfoCache graphCache => _graphCache;

        private NodeView _view;

        public Node View => _view;


        private LogicGraphView _owner;

        private LNEditorCache _nodeEditorCache;

        /// <summary>
        /// 当前节点视图对应的节点
        /// </summary>
        public BaseLogicNode Target { get; private set; }

        /// <summary>
        /// 入端口
        /// </summary>
        public PortView Input { get; private set; }

        /// <summary>
        /// 出端口
        /// </summary>
        public PortView OutPut { get; private set; }

        /// <summary>
        /// 是否显示参数界面
        /// </summary>
        public virtual bool ShowParamPanel => false;

        private float _width = 100;
        /// <summary>
        /// 当前节点视图的宽度
        /// </summary>
        public float Width
        {
            get => _width;
            protected set
            {
                _width = value;
                if (this._view != null)
                {
                    this._view.style.width = _width;
                }
            }
        }

        private string _title = "";
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                Target.Title = value;
                if (this.View != null)
                {
                    this.View.title = value;
                }
            }
        }

        private static Action<PortView, bool> onPortSetDefault;
        #region 公共方法
        static BaseNodeView()
        {
            PropertyInfo prop = typeof(PortView).GetProperty("IsDefault", BindingFlags.Instance | BindingFlags.Public);
            var method = prop.GetSetMethod(true);
            onPortSetDefault = (Action<PortView, bool>)Delegate.CreateDelegate(typeof(Action<PortView, bool>), null, method);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(LogicGraphView owner, BaseLogicNode node)
        {
            Target = node;
            this._title = node.Title;
            this._owner = owner;
            this._graphCache = owner.LGInfoCache;
            this._nodeEditorCache = owner.LGEditorCache.GetEditorNode(node.GetType());
            OnCreate();
            _view = new NodeView(this);
            this._owner.AddElement(_view);
            _view.Initialize();
            m_initializePorts();

        }

        /// <summary>
        /// 节点被创建
        /// 视图没有被创建
        /// </summary>
        public virtual void OnCreate() { }

        /// <summary>
        /// 显示UI
        /// 子类实现
        /// </summary>
        public virtual void ShowUI() { }

        /// <summary>
        /// 显示参数界面
        /// </summary>
        public virtual void ShowParamUI() { }
        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="owner"></param>
        public virtual void AddChild(BaseNodeView child)
        {
            this.Target.Childs.Add(child.Target);
        }
        /// <summary>
        /// 移除一个节点
        /// </summary>
        /// <param name="child"></param>
        public virtual void RemoveChild(BaseNodeView child)
        {
            this.Target.Childs.Remove(child.Target);
        }

        /// <summary>
        /// 当前节点是否可以被链接
        /// </summary>
        /// <param name="ownerPort">自己的端口</param>
        /// <param name="waitLinkPort">等待链接的端口</param>
        /// <returns></returns>
        public virtual bool CanLink(PortView ownerPort, PortView waitLinkPort) { return true; }

        public virtual void DrawLink()
        {
            DrawLink(Target.Childs);
        }

        protected void DrawLink(List<BaseLogicNode> nodeList)
        {
            nodeList.RemoveAll(a => a == null);
            foreach (BaseLogicNode item in nodeList)
            {
                BaseNodeView nodeView = _graphCache.GetNodeView(item);
                DrawLink(nodeView);
            }
        }

        protected void DrawLink(BaseNodeView input) => DrawLink(input, this.OutPut);

        protected void DrawLink(BaseNodeView input, PortView outPort)
        {
            EdgeView edge = new EdgeView();
            edge.input = input.Input;
            edge.output = outPort;
            input.Input.Connect(edge);
            outPort.Connect(edge);
            this._owner.AddElement(edge);
            this._owner.schedule.Execute(() =>
            {
                edge.UpdateEdgeControl();
            }).ExecuteLater(1);
        }

        #endregion


        #region 私有子类
        /// <summary>
        /// 绑定右键
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void OnGenericMenu(ContextualMenuPopulateEvent evt)
        {

            for (int i = 0; i < Target.Childs.Count; i++)
            {
                BaseLogicNode item = Target.Childs[i];
                BaseNodeView nodeView = _graphCache.GetNodeView(item);
                evt.menu.AppendAction($"移除连接:{i + 1}.{nodeView.Title}", onRemoveChild, (a) => DropdownMenuAction.Status.Normal, nodeView);
            }
            if (Target.Childs.Count > 0)
            {
                evt.menu.AppendSeparator();
            }
            evt.menu.AppendAction("查看节点代码", onOpenNodeScript);
            evt.menu.AppendAction("查看界面代码", onOpenNodeViewScript);
        }
        /// <summary>
        /// 添加一个端口
        /// </summary>
        /// <param name="labelName"></param>
        /// <param name="direction"></param>
        /// <param name="isCube"></param>
        /// <returns></returns>
        protected PortView AddPort(string labelName, Direction direction, bool isCube = false)
        {
            PortView portView = PortView.CreatePort(labelName, direction, isCube, this._owner.ConnectorListener);
            portView.Initialize(this);
            return portView;
        }
        /// <summary>
        /// 添加一个UI元素到参数面板
        /// ShowParamPanel 必须为True 此方法才有效
        /// </summary>
        /// <param name="ui"></param>
        protected void AddUIToParamPanel(VisualElement element)
        {
            if (ShowParamPanel)
            {
                if (element is Port)
                {
                    Debug.LogError("Port 节点无法添加到参数面板");
                    return;
                }
                _owner.AddParamElement(element);
            }
        }
        protected void Repaint()
        {
            this._view.Repaint();
            this._owner.RepaintParamPanel();
            this.ShowUI();
            this.ShowParamUI();
        }
        /// <summary>
        /// 添加一个UI元素到节点视图中
        /// </summary>
        /// <param name="ui"></param>
        protected void AddUI(VisualElement element)
        {
            _view.AddUI(element);
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
        protected Toggle GetInputField(string titleText, bool defaultValue, Action<int> changed = null)
        {
            Toggle field = new Toggle();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<int>>((e) => changed?.Invoke(e.newValue));
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
        protected ObjectField GetInputField(string titleText, Object defaultValue, Action<Object> changed = null)
        {
            ObjectField field = new ObjectField();
            field.label = titleText;
            SetBaseFieldStyle(field);
            field.value = defaultValue;
            field.RegisterCallback<ChangeEvent<Object>>((e) => changed?.Invoke(e.newValue));
            return field;
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
            element.labelElement.style.minWidth = 50;
            element.labelElement.style.fontSize = 12;
        }

        /// <summary>
        /// 移除子节点
        /// </summary>
        /// <param name="obj"></param>
        protected void onRemoveChild(DropdownMenuAction obj)
        {
            if (obj.userData is BaseNodeView nodeView)
            {
                Edge edge = this.OutPut.connections.FirstOrDefault(a => a.input.node == nodeView.View);
                this._owner.DeleteElements(new Edge[] { edge });
                _owner.Save();
            }

        }

        /// <summary>
        /// 查看节点代码
        /// </summary>
        /// <param name="obj"></param>
        protected void onOpenNodeScript(DropdownMenuAction obj)
        {
            string[] guids = AssetDatabase.FindAssets(this.Target.GetType().Name);
            foreach (var item in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(item);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript.GetClass() == this.Target.GetType())
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
                if (monoScript.GetClass() == this.GetType())
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), -1);
                    break;
                }
            }
        }

        #endregion


        #region 私有方法
        private void m_initializePorts()
        {
            if ((_nodeEditorCache.PortType & PortEnum.In) > PortEnum.None)
            {
                Input = AddPort("In", Direction.Input);
                onPortSetDefault.Invoke(Input, true);
                this._view.inputContainer.Add(Input);
            }
            else
            {
                this._view.inputContainer.RemoveFromHierarchy();
            }
            if ((_nodeEditorCache.PortType & PortEnum.Out) > PortEnum.None)
            {
                OutPut = AddPort("Out", Direction.Output);
                onPortSetDefault.Invoke(OutPut, true);
                this._view.outputContainer.Add(OutPut);
            }
            else
            {
                this._view.outputContainer.RemoveFromHierarchy();
            }
        }
        #endregion


        private class NodeView : Node
        {
            public BaseNodeView LogicNodeView { get; private set; }

            /// <summary>
            /// 重新定义的内容容器
            /// </summary>
            private VisualElement m_content { get; set; }
            public NodeView(BaseNodeView nodeView)
            {
                LogicNodeView = nodeView;
                userData = nodeView;
                styleSheets.Add(LogicUtils.GetNodeStyle());
                this.style.width = LogicNodeView.Width;
                //移除右上角折叠按钮
                titleButtonContainer.RemoveFromHierarchy();
                //this.titleContainer.Remove(titleButtonContainer);
                topContainer.style.height = 24;
                m_content = topContainer.parent;
                m_content.style.backgroundColor = new Color(0, 0, 0, 0.5f);
                m_checkTitle();
            }
            public void AddUI(VisualElement ui)
            {
                m_content.Add(ui);
            }

            public void Initialize()
            {
                this.title = LogicNodeView.Title;
                this.SetPosition(new Rect(LogicNodeView.Target.Pos, Vector2.zero));
            }

            public override void SetPosition(Rect newPos)
            {
                base.SetPosition(newPos);
                LogicNodeView.Target.Pos = newPos.position;
            }

            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
                if (!this.selected || this.LogicNodeView._owner.selection.Count > 1)
                {
                    return;
                }
                LogicNodeView.OnGenericMenu(evt);
            }

            public void Repaint()
            {
                m_content.Clear();
            }

            #region Title

            private TextField _titleEditor;
            private Label _titleItem;
            private bool _editTitleCancelled = false;
            private void m_checkTitle()
            {
                //找到Title对应的元素
                _titleItem = this.Q<Label>("title-label");
                _titleItem.style.flexGrow = 1;
                _titleItem.style.marginRight = 6;
                _titleItem.style.unityTextAlign = TextAnchor.MiddleCenter;
                _titleItem.RegisterCallback<MouseDownEvent>(m_onMouseDownEvent);
                _titleEditor = new TextField();
                _titleItem.parent.Add(_titleEditor);
                _titleEditor.style.flexGrow = 1;
                _titleEditor.style.marginRight = 6;
                _titleEditor.style.unityTextAlign = TextAnchor.MiddleCenter;
                _titleEditor.style.display = DisplayStyle.None;
                VisualElement visualElement2 = _titleEditor.Q(TextInputBaseField<string>.textInputUssName);
                visualElement2.RegisterCallback<FocusOutEvent>(m_onEditTitleFinished);
                visualElement2.RegisterCallback<KeyDownEvent>(m_onTitleEditorOnKeyDown);
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
                    this.LogicNodeView.Title = _titleEditor.text;
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

            #endregion
        }
    }
    public abstract class BaseNodeView<T> : BaseNodeView where T : BaseLogicNode
    {
        protected T node => Target as T;
    }

}