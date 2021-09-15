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
    public sealed class LGParameterView : Blackboard
    {
        private LogicGraphView _graphView;

        private VisualElement root;
        private VisualElement content;

        private ScrollView scrollView;

        public LGParameterView()
        {
            styleSheets.Add(LogicUtils.GetExposedStyle());
            scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.horizontalScroller.RemoveFromHierarchy();
            root = this.Q("content");

            content = this.Q<VisualElement>(name: "contentContainer");

            style.overflow = Overflow.Hidden;

            AddToClassList("lgparameter");
            style.position = Position.Absolute;
            content.RemoveFromHierarchy();
            root.Add(scrollView);
            scrollView.Add(content);
            AddToClassList("scrollable");

            this.Q<Button>("addButton").clicked += m_onAddClicked;
            this.Q("subTitleLabel").RemoveFromHierarchy();
            title = "逻辑图参数";
        }

        public void InitializeGraphView(LogicGraphView graphView)
        {
            this._graphView = graphView;
            SetPosition(new Rect(graphView.LGInfoCache.ParamCache.Pos, graphView.LGInfoCache.ParamCache.Size));
            hierarchy.Add(new Resizer(() =>
            {
                graphView.LGInfoCache.ParamCache.Size = layout.size;
            }));
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
                    _graphView.AddLGParam(uniqueName, paramType);
                });

            parameterType.ShowAsContext();
        }
        private void m_updateParameterList()
        {
            content.Clear();

            foreach (var param in _graphView.LGInfoCache.Graph.Params)
            {
                var row = new BlackboardRow(new LGParameterFieldView(_graphView, param), new LGParameterPropertyView(_graphView, param));
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
            while (_graphView.LGInfoCache.Graph.Params.Any(e => e.Name == name))
                name = uniqueName + " " + i++;
            return name;
        }


    }
}
