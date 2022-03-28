using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class GroupView : Group
    {
        private BaseGraphView owner;
        private BaseLogicGroup group;
        public BaseLogicGroup Group => group;


        Label titleLabel;
        ColorField colorField;

        public GroupView()
        {
            styleSheets.Add(LogicUtils.GetGridStyle());
        }

        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //evt.StopPropagation();
        }



        public void Initialize(BaseGraphView graphView, BaseLogicGroup group)
        {
            this.group = group;
            owner = graphView;

            title = group.Title;
            SetPosition(new Rect(group.Pos, group.Size));

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>(TitleChangedCallback);
            titleLabel = headerContainer.Q<Label>();

            colorField = new ColorField { value = group.Color, name = "headerColorPicker" };
            colorField.RegisterValueChangedCallback(e =>
            {
                UpdateGroupColor(e.newValue);
            });
            UpdateGroupColor(group.Color);

            headerContainer.Add(colorField);

            InitializeInnerNodes();
        }

        void InitializeInnerNodes()
        {
            foreach (var nodeGUID in group.Nodes.ToList())
            {
                if (!owner.HasNodeView(nodeGUID))
                {
                    group.Nodes.Remove(nodeGUID);
                    continue;
                }
                var nodeView = owner.GetNodeView(nodeGUID);

                AddElement(nodeView);
            }
        }
        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            if (element is BaseNodeView nodeView)
            {
                if (owner.Target.StartNodes.Contains(nodeView.target))
                {
                    reasonWhyNotAccepted = "无法将默认节点添加到组";
                    return false;
                }

            }
            return base.AcceptsElement(element, ref reasonWhyNotAccepted);
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                var nodeView = element as BaseNodeView;

                // Adding an element that is not a node currently supported
                if (nodeView == null)
                    continue;

                if (!group.Nodes.Contains(nodeView.OnlyId))
                    group.Nodes.Add(nodeView.OnlyId);
            }
            base.OnElementsAdded(elements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            // Only remove the nodes when the group exists in the hierarchy
            if (parent != null)
            {
                foreach (var elem in elements)
                {
                    if (elem is BaseNodeView nodeView)
                    {
                        group.Nodes.Remove(nodeView.OnlyId);
                    }
                }
            }

            base.OnElementsRemoved(elements);
        }

        public void UpdateGroupColor(Color newColor)
        {
            group.Color = newColor;
            style.backgroundColor = newColor;
        }

        void TitleChangedCallback(ChangeEvent<string> e)
        {
            group.Title = e.newValue;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            group.Pos = newPos.position;
            group.Size = newPos.size;
        }
    }
}
