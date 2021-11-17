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
        private LogicGraphView owner;
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
            if (group != null)
            {
                evt.menu.AppendAction("保存模板", m_onSaveTemplate);
            }
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
        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            if (element.userData is BaseNodeView nodeView)
            {
                if (owner.LGInfoCache.Graph.StartNodes.Contains(nodeView.Target))
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


        /// <summary>
        /// 保存模板
        /// </summary>
        /// <param name="obj"></param>
        private void m_onSaveTemplate(DropdownMenuAction obj)
        {
            LGroupEditorCache groupEditorCache = owner.LGEditorCache.Groups.FirstOrDefault(a => a.Name == group.Title);
            if (groupEditorCache != null)
            {
                EditorUtility.DisplayDialog("错误", "存在相同的模板", "确认");
                return;
            }
            saveTemplate(new LGroupEditorCache());
        }


        private void saveTemplate(LGroupEditorCache groupEditorCache)
        {
            groupEditorCache.Name = group.Title;
            int uniqueId = 1000;
            foreach (var item in group.Nodes)
            {
                var nodeView = owner.LGInfoCache.GetNodeView(item);
                if (nodeView != null && (nodeView as VariableNodeView) == null)
                {
                    LGroupNEditorCache nodeCache = new LGroupNEditorCache();
                    nodeCache.NodeClassFullName = nodeView.Target.GetType().FullName;
                    nodeCache.Pos = nodeView.Target.Pos - group.Pos;
                    nodeCache.Id = uniqueId++;
                    nodeCache.Node = nodeView.Target;
                    groupEditorCache.Nodes.Add(nodeCache);
                }
            }
            foreach (var item in groupEditorCache.Nodes)
            {
                List<BaseLogicNode> childs = item.Node.Childs;
                foreach (BaseLogicNode child in childs)
                {
                    var groupNode = groupEditorCache.Nodes.FirstOrDefault(a => a.Node == child);
                    if (groupNode != null)
                    {
                        item.Childs.Add(groupNode.Id);
                    }
                }
            }
            owner.LGEditorCache.AddGroupTemplate(groupEditorCache);
        }
    }
}
