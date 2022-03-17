using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    /// <summary>
    /// 分组列表集合
    /// </summary>
    public sealed class GroupListView : Blackboard
    {
        //private LogicGraphView1 owner;

        private VisualElement root;
        private VisualElement content;

        private ScrollView scrollView;

        public GroupListView()
        {
            styleSheets.Add(LogicUtils.GetGroupListStyle());
            scrollView = new ScrollView(ScrollViewMode.Horizontal);
            scrollView.verticalScroller.RemoveFromHierarchy();
            root = this.Q("content");

            content = this.Q<VisualElement>(name: "contentContainer");

            style.overflow = Overflow.Hidden;

            AddToClassList("grouplist");
            style.position = Position.Absolute;
            content.RemoveFromHierarchy();
            root.Add(scrollView);
            scrollView.Add(content);
            AddToClassList("scrollable");

            this.Q("addButton").RemoveFromHierarchy();
            this.Q("subTitleLabel").RemoveFromHierarchy();
            this.hierarchy.RemoveAt(this.hierarchy.childCount - 1);
            title = "分组列表";
        }
        //public void InitializeGraphView(LogicGraphView1 graphView)
        //{
        //    this.owner = graphView;
        //    m_updateVariableList();
        //    SetPosition(new Rect(graphView.LGInfoCache.GroupListCache.Pos, graphView.LGInfoCache.GroupListCache.Size));
        //    //hierarchy.Add(new Resizer(() =>
        //    //{
        //    //    graphView.LGInfoCache.VariableCache.Size = layout.size;
        //    //}));
        //    RegisterCallback<MouseUpEvent>((e) =>
        //    {
        //        graphView.LGInfoCache.GroupListCache.Pos = layout.position;
        //        e.StopPropagation();
        //    });
        //}

        public void Show()
        {
            this.visible = true;
            m_updateVariableList();
            //owner.LGEditorCache.onGroupChanged += m_updateVariableList;
        }

        public void Hide()
        {
            this.content.Clear();
            this.visible = false;
            //owner.LGEditorCache.onGroupChanged -= m_updateVariableList;
        }
        private void m_updateVariableList()
        {
            content.Clear();

            //foreach (var item in owner.LGEditorCache.Groups)
            //{
            //    var row = new BlackboardRow(new GroupListFieldView(owner, item), null);
            //    row.Q("expandButton").RemoveFromHierarchy();
            //    row.expanded = false;

            //    content.Add(row);
            //}
        }
    }
}
