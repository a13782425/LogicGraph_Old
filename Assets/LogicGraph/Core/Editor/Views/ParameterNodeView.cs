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
    public sealed class ParameterNodeView : BaseNodeView<ParameterNode>
    {

        protected override Node GetNode()
        {
            var node = new ParameterNodeVisualElement(this, this.owner);
            node.inputContainer.RemoveFromHierarchy();
            node.outputContainer.RemoveFromHierarchy();
            var titleLabel = node.titleContainer.Q("title-label");
            titleLabel.RemoveFromHierarchy();
            node.titleContainer.Add(node.inputContainer);
            node.titleContainer.Add(titleLabel);
            node.titleContainer.Add(node.outputContainer);
            Input = AddPort("", Direction.Output);
            //Input.Q("type").RemoveFromHierarchy();
            node.outputContainer.Add(Input);
            OutPut = AddPort("", Direction.Input);
            //OutPut.Q("type").RemoveFromHierarchy();
            node.inputContainer.Add(OutPut);
            var contents = node.Q("contents");
            contents.RemoveFromHierarchy();
            return node;
        }
        public override void OnCreate()
        {
            Input.portColor = node.param.GetColor();
            OutPut.portColor = node.param.GetColor();
        }
        protected override void OnGenericMenu(ContextualMenuPopulateEvent evt)
        {

        }

        public override bool CanLink(PortView ownerPort, PortView waitLinkPort)
        {
            if (waitLinkPort.IsDefault)
            {
                return false;
            }
            if (waitLinkPort.Owner is ParameterNodeView)
            {
                return false;
            }
            return true;
        }

        private class ParameterNodeVisualElement : Node, INodeVisualElement
        {
            private BaseNodeView nodeView { get; set; }

            public event Action<ContextualMenuPopulateEvent> onGenericMenu;
            /// <summary>
            /// 逻辑图视图
            /// </summary>
            private LogicGraphView _graphView;

            /// <summary>
            /// 重新定义的内容容器
            /// </summary>
            private VisualElement m_content { get; set; }
            public ParameterNodeVisualElement(BaseNodeView nodeView, LogicGraphView graphView)
            {
                this.nodeView = nodeView;
                _graphView = graphView;
                userData = nodeView;
                styleSheets.Add(LogicUtils.GetParamNodeStyle());
                //移除右上角折叠按钮
                titleButtonContainer.RemoveFromHierarchy();
                topContainer.style.height = 24;
                m_content = topContainer.parent;
                m_content.style.backgroundColor = new Color(0, 0, 0, 0.5f);
                this.title = this.nodeView.Title;
                this.transform.position = this.nodeView.Target.Pos;
                //this.SetPosition(new Rect(this.nodeView.Target.Pos, Vector2.zero));
                this.AddToClassList("paramNode");
            }

            public void AddUI(VisualElement ui)
            {
                m_content.Add(ui);
            }

            public override void SetPosition(Rect newPos)
            {
                base.SetPosition(newPos);
                nodeView.Target.Pos = newPos.position;
            }

            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
                if (!this.selected || this._graphView.selection.Count > 1)
                {
                    return;
                }
                onGenericMenu?.Invoke(evt);
                evt.StopPropagation();
            }
        }

    }

}