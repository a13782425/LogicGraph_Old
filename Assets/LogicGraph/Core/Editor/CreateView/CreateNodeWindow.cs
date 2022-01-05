using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Logic.Editor.LogicUtils;

namespace Logic.Editor
{
    public class CreateNodeWindow : EditorWindow
    {

        private const string CREATE_NODE_VIEW_TREE = "NodeCreateElement.uxml";
        private const string CREATE_NODE_VIEW_STYLE = "NodeCreateView.uss";

        private VisualElement _top;

        private VisualElement _center;

        private VisualElement _bottom;

        private Type _curNodeType;


        /// <summary>
        /// 节点类型
        /// </summary>
        private PopupField<Type> _nodeTypePopField;
        /// <summary>
        /// 节点名称
        /// </summary>
        private TextField _nodeNameTextField;

        /// <summary>
        /// 进出端口类型
        /// </summary>
        private EnumField _portTypeEnumField;

        /// <summary>
        /// 包含的遮罩
        /// </summary>
        private MaskField _includeMaskField;
        /// <summary>
        /// 排除的遮罩
        /// </summary>
        private MaskField _excludeMaskField;

        private ScrollView _scrollView;
        private VisualElement _content;
        private Button _genBtn;

        private List<Type> _graphTypeList = new List<Type>();

        [MenuItem("Framework/逻辑图/创建节点视图", priority = 101)]
        private static void CreateNodeView()
        {
            CreateWindow<CreateNodeWindow>();
        }
        private void OnEnable()
        {
            titleContent = new GUIContent("创建节点视图");
            this.minSize = new Vector2(480, 640);
            this.maxSize = new Vector2(480, 640);

            this.rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_STYLE_PATH, CREATE_NODE_VIEW_STYLE)));

            _top = new VisualElement();
            _top.name = "top";
            this.rootVisualElement.Add(_top);
            _center = new VisualElement();
            _center.name = "center";
            this.rootVisualElement.Add(_center);
            _bottom = new VisualElement();
            _bottom.name = "bottom";
            this.rootVisualElement.Add(_bottom);

            List<Type> nodeTypes = GetNodeList();
            if (nodeTypes.Count == 0)
            {
                this.ShowNotification(new GUIContent("没有节点可以生成"));
                return;
            }
            _nodeTypePopField = new PopupField<Type>("节点选择:", nodeTypes, 0);
            _nodeTypePopField.RegisterCallback<ChangeEvent<Type>>(m_nodeTypeChange);
            _portTypeEnumField = new EnumField("节点类型:", PortEnum.All);
            _nodeNameTextField = new TextField("节点名称:");
            _nodeNameTextField.multiline = false;
            _graphTypeList = GetGraphList();
            List<string> list = _graphTypeList.Select(a => a.Name).ToList();

            _includeMaskField = new MaskField("归属图:", list, 0);
            _excludeMaskField = new MaskField("排除图:", list, 0);

            this._top.Add(_nodeTypePopField);
            this._top.Add(_portTypeEnumField);
            this._top.Add(_nodeNameTextField);
            this._top.Add(_includeMaskField);
            this._top.Add(_excludeMaskField);


            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.horizontalScroller.RemoveFromHierarchy();
            _scrollView.AddToClassList("scrollable");
            _content = new VisualElement();
            _content.name = "contentContainer";
            this._center.Add(_scrollView);
            _scrollView.Add(_content);

            var addBtn = new Button(onAddClick);
            addBtn.text = "添加";
            addBtn.name = "add_btn";
            this._center.Add(addBtn);
            _genBtn = new Button(onGenClick);
            _genBtn.text = "生成";

            this._bottom.Add(_genBtn);
            m_setShowType(nodeTypes[0]);
        }

        private void onAddClick()
        {
            _content.Add(new CreateNodeFieldView(_curNodeType));
        }

        private void onGenClick()
        {
            if (string.IsNullOrWhiteSpace(_nodeNameTextField.value))
            {
                this.ShowNotification(new GUIContent("节点名称不能为空"));
                return;
            }
            if ((PortEnum)_portTypeEnumField.value == PortEnum.None)
            {
                this.ShowNotification(new GUIContent("默认端口不能为空"));
                return;
            }
            string nodeClassName = _nodeTypePopField.value.Name;
            string filePath = EditorUtility.SaveFilePanel("保存", Application.dataPath, nodeClassName + "View", "cs");
            if (string.IsNullOrWhiteSpace(filePath))
            {
                this.ShowNotification(new GUIContent("生成路径为空"));
                return;
            }
            string viewClassName = Path.GetFileNameWithoutExtension(filePath);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using Logic;");
            sb.AppendLine("using Logic.Editor;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using UnityEditor;");
            sb.AppendLine("using UnityEditor.UIElements;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UIElements;");
            sb.AppendLine();
            string attrStr = $"[LogicNode(typeof({nodeClassName}), \"{_nodeNameTextField.value}\"";
            if (_includeMaskField.value > 0)
            {
                //存在需要包含的
                attrStr += ", IncludeGraphs = new Type[] {";
                for (int i = 0; i < _graphTypeList.Count; i++)
                {
                    int num = 1 << i;
                    if ((_includeMaskField.value & num) == num)
                    {
                        attrStr += $" typeof({_graphTypeList[i].Name}),";
                    }
                }
                attrStr = attrStr.Substring(0, attrStr.Length - 1);
                attrStr += " }";
            }
            if (_excludeMaskField.value > 0)
            {
                //存在需要排除的
                attrStr += ", ExcludeGraphs = new Type[] {";
                for (int i = 0; i < _graphTypeList.Count; i++)
                {
                    int num = 1 << i;
                    if ((_excludeMaskField.value & num) == num)
                    {
                        attrStr += $" typeof({_graphTypeList[i].Name}),";
                    }
                }
                attrStr = attrStr.Substring(0, attrStr.Length - 1);
                attrStr += " }";
            }
            PortEnum portEnum = (PortEnum)_portTypeEnumField.value;
            if (portEnum != PortEnum.All)
            {
                attrStr += $", PortType = PortEnum.{portEnum.ToString()}";
            }

            attrStr += ")]";

            sb.AppendLine(attrStr);

            sb.AppendLine($"public class {viewClassName} : BaseNodeView<{nodeClassName}>");
            sb.AppendLine("{");

            //TODO: 生成类内的内容

            sb.AppendLine("}");
            File.WriteAllText(filePath, sb.ToString(), new System.Text.UTF8Encoding(false));
            this.ShowNotification(new GUIContent("生成成功"));
            AssetDatabase.Refresh();
        }


        private List<Type> GetNodeList()
        {
            var varType = typeof(VariableNode);
            List<Type> nodeTypes = TypeCache.GetTypesDerivedFrom<BaseLogicNode>().Where(a => !a.IsAbstract && a != varType).ToList();
            List<Type> nodeViewTypes = TypeCache.GetTypesDerivedFrom<BaseNodeView>().Where(a => !a.IsAbstract).ToList();

            foreach (var item in nodeViewTypes)
            {
                LogicNodeAttribute nodeAttr = item.GetCustomAttribute<LogicNodeAttribute>();
                if (nodeAttr != null)
                {
                    nodeTypes.Remove(nodeAttr.NodeType);
                }
            }
            return nodeTypes;
        }

        private List<Type> GetGraphList()
        {
            List<Type> graphTypes = TypeCache.GetTypesDerivedFrom<BaseLogicGraph>().ToList();

            return graphTypes;
        }


        private void m_nodeTypeChange(ChangeEvent<Type> evt)
        {
            m_setShowType(evt.newValue);
        }


        private void m_setShowType(Type newValue)
        {
            _curNodeType = newValue;
            _content.Clear();
        }
    }
}
