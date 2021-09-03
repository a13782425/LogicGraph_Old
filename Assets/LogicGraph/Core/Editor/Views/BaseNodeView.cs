using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public abstract class BaseNodeView
    {
        public string OnlyId => _nodeCache.OnlyId;
        private LGInfoCache _graphCache;

        private NodeView _view;

        public Node View => _view;


        private LogicGraphView _owner;

        private LNInfoCache _nodeCache;
        public LNInfoCache nodeCache => _nodeCache;

        private LNEditorCache _nodeEditorCache;

        /// <summary>
        /// 当前节点视图对应的节点
        /// </summary>
        public BaseLogicNode Target => nodeCache.Node;

        /// <summary>
        /// 入端口
        /// </summary>
        public PortView Input { get; private set; }

        /// <summary>
        /// 出端口
        /// </summary>
        public PortView OutPut { get; private set; }

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

        #region 公共方法

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(LogicGraphView owner, LNInfoCache nodeCache)
        {
            this._owner = owner;
            this._graphCache = owner.LGInfoCache;
            this._nodeCache = nodeCache;
            this._nodeEditorCache = owner.LGEditorCache.GetEditorNode(nodeCache.Node.GetType());
            OnCreate();
            _view = new NodeView(this);
            this._owner.AddElement(_view);
            _view.Initialize();
            m_initializePorts();

        }


        public PortView AddPort(string labelName, Direction direction, bool isCube = false)
        {
            PortView portView = PortView.CreatePort(labelName, direction, isCube, this._owner.ConnectorListener);
            portView.Initialize(this, "");
            return portView;
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

        public void DrawLink()
        {
            nodeCache.Node.Childs.RemoveAll(a => a == null);
            foreach (var item in nodeCache.Node.Childs)
            {
                var nodeCache = _graphCache.Nodes.FirstOrDefault(a => a.OnlyId == item.OnlyId);
                EdgeView edge = new EdgeView();
                edge.input = nodeCache.View.Input;
                edge.output = OutPut;
                nodeCache.View.Input.Connect(edge);
                OutPut.Connect(edge);
                _owner.AddElement(edge);
                _owner.schedule.Execute(() =>
                {
                    edge.UpdateEdgeControl();
                }).ExecuteLater(1);
            }
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
                var item = Target.Childs[i];
                var nodeCache = _graphCache.NodeDic[item.OnlyId];
                evt.menu.AppendAction($"移除连接:{i + 1}.{nodeCache.Title}", onRemoveParent, (a) => DropdownMenuAction.Status.Normal, item);
            }
            if (Target.Childs.Count > 0)
            {
                evt.menu.AppendSeparator();
            }
            evt.menu.AppendAction("查看节点代码", onOpenNodeScript);
            evt.menu.AppendAction("查看界面代码", onOpenNodeViewScript);
        }

        /// <summary>
        /// 添加一个UI元素到节点视图中
        /// </summary>
        /// <param name="ui"></param>
        protected void AddUI(VisualElement ui)
        {
            _view.AddUI(ui);
        }
        protected Label GetLabel(string defaultValue = "")
        {
            Label label = new Label();
            label.text = defaultValue;
            return label;
        }
        protected TextField GetInputField(string titleText, string defaultValue)
        {
            TextField textField = new TextField();
            textField.label = titleText;
            SetBaseFieldStyle(textField);
            textField.multiline = true;
            textField.value = defaultValue;
            return textField;
        }
        protected IntegerField GetInputField(string titleText, int defaultValue)
        {
            IntegerField intField = new IntegerField();
            intField.label = titleText;
            SetBaseFieldStyle(intField);
            intField.value = defaultValue;
            return intField;
        }
        protected FloatField GetInputField(string titleText, float defaultValue)
        {
            FloatField floatField = new FloatField();
            floatField.label = titleText;
            SetBaseFieldStyle(floatField);
            floatField.value = defaultValue;
            return floatField;
        }
        protected DoubleField GetInputField(string titleText, double defaultValue)
        {
            DoubleField doubleField = new DoubleField();
            doubleField.label = titleText;
            SetBaseFieldStyle(doubleField);
            doubleField.value = defaultValue;
            return doubleField;
        }
        protected EnumField GetInputField(string titleText, Enum defaultValue)
        {
            EnumField enumField = new EnumField();
            enumField.Init(defaultValue);
            enumField.label = titleText;
            SetBaseFieldStyle(enumField);
            enumField.value = defaultValue;
            return enumField;
        }
        protected Vector2Field GetInputField(string titleText, Vector2 defaultValue)
        {
            Vector2Field vector2Field = new Vector2Field();
            vector2Field.label = titleText;
            SetBaseFieldStyle(vector2Field);

            vector2Field.value = defaultValue;
            return vector2Field;
        }
        protected Vector3Field GetInputField(string titleText, Vector3 defaultValue)
        {
            Vector3Field vector3Field = new Vector3Field();
            vector3Field.label = titleText;
            SetBaseFieldStyle(vector3Field);
            vector3Field.value = defaultValue;
            return vector3Field;
        }
        protected Vector4Field GetInputField(string titleText, Vector4 defaultValue)
        {
            Vector4Field vector4Field = new Vector4Field();
            vector4Field.label = titleText;
            SetBaseFieldStyle(vector4Field);
            vector4Field.value = defaultValue;
            return vector4Field;
        }
        protected Vector2IntField GetInputField(string titleText, Vector2Int defaultValue)
        {
            Vector2IntField vector2IntField = new Vector2IntField();
            vector2IntField.label = titleText;
            SetBaseFieldStyle(vector2IntField);
            vector2IntField.value = defaultValue;
            return vector2IntField;
        }
        protected Vector3IntField GetInputField(string titleText, Vector3Int defaultValue)
        {
            Vector3IntField vector3IntField = new Vector3IntField();
            vector3IntField.label = titleText;
            SetBaseFieldStyle(vector3IntField);
            vector3IntField.value = defaultValue;
            return vector3IntField;
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
        protected void onRemoveParent(DropdownMenuAction obj)
        {
            //string removeId = obj.userData.ToString();

            //if (_panelData.LogicPanelGraphView.AllNodeViews.ContainsKey(removeId))
            //{
            //    LogicNodeBaseView view = _panelData.LogicPanelGraphView.AllNodeViews[removeId];
            //    Edge edge = this.Output.connections.FirstOrDefault(a => a.input.node == view);
            //    this._panelData.LogicPanelGraphView.DeleteElements(new Edge[] { edge });
            //    _panelData.Save();
            //}
        }

        /// <summary>
        /// 断开父节点
        /// </summary>
        /// <param name="obj"></param>
        protected void onDisconnectParent(DropdownMenuAction obj)
        {
            //string disconnectId = obj.userData.ToString();

            //if (_panelData.LogicPanelGraphView.AllNodeViews.ContainsKey(disconnectId))
            //{
            //    LogicNodeBaseView view = _panelData.LogicPanelGraphView.AllNodeViews[disconnectId];
            //    Edge edge = this.Input.connections.FirstOrDefault(a => a.output.node == view);
            //    this._panelData.LogicPanelGraphView.DeleteElements(new Edge[] { edge });
            //    _panelData.Save();
            //}
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
                this._view.inputContainer.Add(Input);
            }
            else
            {
                this._view.inputContainer.RemoveFromHierarchy();
            }
            if ((_nodeEditorCache.PortType & PortEnum.Out) > PortEnum.None)
            {
                OutPut = AddPort("Out", Direction.Output);
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
                this.title = LogicNodeView.nodeCache.Title;
                this.SetPosition(new Rect(LogicNodeView.nodeCache.Pos, Vector2.zero));
            }

            public override void SetPosition(Rect newPos)
            {
                base.SetPosition(newPos);
                LogicNodeView.nodeCache.Pos = newPos.position;
            }

            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
                if (!this.selected || this.LogicNodeView._owner.selection.Count > 1)
                {
                    return;
                }
                LogicNodeView.OnGenericMenu(evt);
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
                    this.title = _titleEditor.text;
                    this.LogicNodeView.nodeCache.Title = this.title;
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


}