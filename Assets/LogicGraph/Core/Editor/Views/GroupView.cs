using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class GroupView : Group
    {
        public LogicGraphView owner;
        public BaseLogicGroup group;

        Label titleLabel;
        ColorField colorField;



        public GroupView()
        {
            styleSheets.Add(LogicUtils.GetGridStyle());
        }

        private static void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("删除模板", null);
            evt.menu.AppendAction("保存模板", null);
            evt.menu.AppendAction("删除", null);
            evt.StopPropagation();
        }

        public void Initialize(LogicGraphView graphView, BaseLogicGroup group)
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
                if (!owner.LGInfoCache.NodeDic.ContainsKey(nodeGUID))
                {
                    group.Nodes.Remove(nodeGUID);
                    continue;
                }
                var nodeView = owner.LGInfoCache.NodeDic[nodeGUID];

                AddElement(nodeView.View);
            }
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                var nodeView = element.userData as BaseNodeView;

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
                    if (elem.userData is BaseNodeView nodeView)
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
