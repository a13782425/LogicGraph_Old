using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class LGParameterView : GraphElement
    {
        private LogicGraphView graphView;

        private VisualElement main;
        private VisualElement root;
        private VisualElement content;
        private VisualElement header;


        private Label titleLabel;
        private ScrollView scrollView;



        public override string title
        {
            get { return titleLabel.text; }
            set { titleLabel.text = value; }
        }

        public LGParameterView()
        {
            var tpl = LogicUtils.GetPinnedView();
            styleSheets.Add(LogicUtils.GetPinnedStyle());
            styleSheets.Add(LogicUtils.GetExposedStyle());
            main = tpl.CloneTree();
            main.AddToClassList("mainContainer");
            scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.horizontalScroller.RemoveFromHierarchy();
            root = main.Q("content");

            header = main.Q("header");

            titleLabel = main.Q<Label>(name: "titleLabel");
            content = main.Q<VisualElement>(name: "contentContainer");

            hierarchy.Add(main);
            capabilities |= Capabilities.Movable | Capabilities.Resizable;
            style.overflow = Overflow.Hidden;

            ClearClassList();
            AddToClassList("pinnedElement");

            style.position = Position.Absolute;
            content.RemoveFromHierarchy();
            root.Add(scrollView);
            scrollView.Add(content);
            AddToClassList("scrollable");
            content.style.paddingTop = 6;
            content.style.paddingLeft = 6;
            content.style.paddingRight = 6;
            content.style.paddingBottom = 6;

            this.style.minWidth = 180;
            this.style.minHeight = 320;
            header.Add(new Button(m_onAddClicked)
            {
                text = "+"
            });
            title = "逻辑图参数";
        }

        public void InitializeGraphView(LogicGraphView graphView)
        {
            this.graphView = graphView;
            SetPosition(new Rect(graphView.LGInfoCache.ParamCache.Pos, graphView.LGInfoCache.ParamCache.Size));
            this.AddManipulator(new Dragger { clampToParentEdges = true });
            hierarchy.Add(new Resizer(() =>
            {
                graphView.LGInfoCache.ParamCache.Size = layout.size;
            }));
            RegisterCallback<DragUpdatedEvent>(e =>
            {
                e.StopPropagation();
            });
            RegisterCallback<MouseDownEvent>((e) => e.StopPropagation());
            RegisterCallback<MouseUpEvent>((e) =>
            {
                graphView.LGInfoCache.ParamCache.Pos = layout.position;
                e.StopPropagation();
            });
            graphView.onUpdateLGParam += m_updateParameterList;
        }
        public void Hide()
        {
            this.content.Clear();
            this.visible = false;
        }

        public void Show()
        {
            this.visible = true;
            m_updateParameterList();
        }
        private void m_onAddClicked()
        {
            var parameterType = new GenericMenu();

            foreach (var paramType in m_getParameterTypes())
                parameterType.AddItem(new GUIContent(m_getNiceNameFromType(paramType)), false, () =>
                {
                    string uniqueName = "New " + m_getNiceNameFromType(paramType);
                    uniqueName = m_getUniqueName(uniqueName);
                    graphView.AddLGParam(uniqueName, paramType);
                });

            parameterType.ShowAsContext();
        }
        private void m_updateParameterList()
        {
            content.Clear();

            foreach (var param in graphView.LGInfoCache.Graph.Params)
            {
                var row = new BlackboardRow(new LGParameterFieldView(graphView, param), new LGParameterPropertyView(graphView, param));
                row.expanded = false;

                content.Add(row);
            }
        }

        private IEnumerable<Type> m_getParameterTypes()
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<BaseParameter>())
            {
                if (type.IsGenericType)
                    continue;

                yield return type;
            }
        }
        private string m_getNiceNameFromType(Type type)
        {
            string name = type.Name;

            // Remove parameter in the name of the type if it exists
            name = name.Replace("Parameter", "");

            return ObjectNames.NicifyVariableName(name);
        }
        private string m_getUniqueName(string name)
        {
            // Generate unique name
            string uniqueName = name;
            int i = 0;
            while (graphView.LGInfoCache.Graph.Params.Any(e => e.Name == name))
                name = uniqueName + " " + i++;
            return name;
        }


    }
}
