using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Logic.Editor.LogicUtils;

namespace Logic.Editor
{
    public sealed class NodeDescribeView : VisualElement
    {
        static readonly string elementStyle = "NodeDescribeView.uss";
        static readonly string elementTree = "NodeDescribeElement.uxml";

        private LogicGraphView _onwer;

        private VisualElement _main;
        private TextField _desTextField;
        private BaseNodeView _curNodeView;

        private bool _editCancelled = false;
        public NodeDescribeView(LogicGraphView graphView)
        {
            _onwer = graphView;
            var tpl = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(EDITOR_STYLE_PATH, elementTree));

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_STYLE_PATH, elementStyle)));
            _main = tpl.CloneTree();
            _main.AddToClassList("mainContainer");
            this.name = "NodeDescribeView";
            ClearClassList();
            AddToClassList("nodeDescribe");

            hierarchy.Add(_main);

            _desTextField = _main.Q<TextField>("des_input");
            _desTextField.RegisterCallback<FocusOutEvent>(m_onEditFinished);
            _desTextField.RegisterCallback<KeyDownEvent>(m_onEditorOnKeyDown);
        }

        private void m_onEditorOnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Escape:
                    _editCancelled = true;
                    _desTextField.Q(TextInputBaseField<string>.textInputUssName).Blur();
                    break;
                case KeyCode.Return:
                    _desTextField.Q(TextInputBaseField<string>.textInputUssName).Blur();
                    break;
            }
        }

        private void m_onEditFinished(FocusOutEvent evt)
        {
            if (!_editCancelled)
            {
                _curNodeView.Target.Describe = _desTextField.text;
                _curNodeView.View.tooltip = _desTextField.text;
            }
            _editCancelled = true;
            this.Hide();
        }

        public void Hide()
        {
            this.style.display = DisplayStyle.None;
        }

        internal void Show(BaseNodeView nodeView)
        {
            this.style.display = DisplayStyle.Flex;
            _editCancelled = false;
            _desTextField.value = nodeView.Target.Describe;
            _curNodeView = nodeView;
            _desTextField.Focus();
        }
    }
}
