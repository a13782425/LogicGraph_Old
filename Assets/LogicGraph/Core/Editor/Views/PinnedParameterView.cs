using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    /// <summary>
    /// 属性固定界面
    /// </summary>
    public sealed class PinnedParameterView : GraphElement
    {
        private VisualElement main;
        private VisualElement root;
        private VisualElement content;
        private VisualElement header;


        private Label titleLabel;
        private ScrollView scrollView;
        bool _scrollable;

        public override string title
        {
            get { return titleLabel.text; }
            set { titleLabel.text = value; }
        }
        public bool scrollable
        {
            get
            {
                return _scrollable;
            }
            set
            {
                if (_scrollable == value)
                    return;

                _scrollable = value;

                style.position = Position.Absolute;
                if (_scrollable)
                {
                    content.RemoveFromHierarchy();
                    root.Add(scrollView);
                    scrollView.Add(content);
                    AddToClassList("scrollable");
                }
                else
                {
                    scrollView.RemoveFromHierarchy();
                    content.RemoveFromHierarchy();
                    root.Add(content);
                    RemoveFromClassList("scrollable");
                }
            }
        }

        public PinnedParameterView()
        {
            var tpl = LogicUtils.GetPinnedView();
            styleSheets.Add(LogicUtils.GetPinnedStyle());

            main = tpl.CloneTree();
            main.AddToClassList("mainContainer");
            scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);

            root = main.Q("content");

            header = main.Q("header");

            titleLabel = main.Q<Label>(name: "titleLabel");
            content = main.Q<VisualElement>(name: "contentContainer");

            hierarchy.Add(main);

            capabilities |= Capabilities.Movable | Capabilities.Resizable;
            style.overflow = Overflow.Hidden;

            ClearClassList();
            AddToClassList("pinnedElement");

            //this.AddManipulator(new Dragger { clampToParentEdges = true });

            scrollable = true;

            RegisterCallback<DragUpdatedEvent>(e =>
            {
                e.StopPropagation();
            });

            title = "参数面板";
        }

        public void InitializeGraphView(LogicGraphView graphView)
        {
            //SetPosition(pinnedElement.position);

            //onResized += () =>
            //{
            //    pinnedElement.position.size = layout.size;
            //};

            //RegisterCallback<MouseUpEvent>(e =>
            //{
            //    //pinnedElement.position.position = layout.position;
            //});

        }

        public void ResetPosition()
        {
            //pinnedElement.position = new Rect(Vector2.zero, PinnedElement.defaultSize);
            //SetPosition(pinnedElement.position);
        }


    }
}
