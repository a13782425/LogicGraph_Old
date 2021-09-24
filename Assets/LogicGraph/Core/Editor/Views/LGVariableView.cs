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
    public sealed class LGVariableView : Blackboard
    {
        private LogicGraphView _graphView;

        private VisualElement root;
        private VisualElement content;

        private ScrollView scrollView;

        public LGVariableView()
        {
            styleSheets.Add(LogicUtils.GetVariableStyle());
            scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.horizontalScroller.RemoveFromHierarchy();
            root = this.Q("content");

            content = this.Q<VisualElement>(name: "contentContainer");

            style.overflow = Overflow.Hidden;

            AddToClassList("lgvariable");
            style.position = Position.Absolute;
            content.RemoveFromHierarchy();
            root.Add(scrollView);
            scrollView.Add(content);
            AddToClassList("scrollable");

            this.Q<Button>("addButton").clicked += m_onAddClicked;
            this.Q("subTitleLabel").RemoveFromHierarchy();
            title = "逻辑图变量";
        }

        public void InitializeGraphView(LogicGraphView graphView)
        {
            this._graphView = graphView;
            SetPosition(new Rect(graphView.LGInfoCache.VariableCache.Pos, graphView.LGInfoCache.VariableCache.Size));
            hierarchy.Add(new Resizer(() =>
            {
                graphView.LGInfoCache.VariableCache.Size = layout.size;
            }));
            RegisterCallback<MouseUpEvent>((e) =>
            {
                graphView.LGInfoCache.VariableCache.Pos = layout.position;
                e.StopPropagation();
            });
            graphView.onUpdateLGVariable += m_updateVariableList;
        }
        public void Hide()
        {
            this.content.Clear();
            this.visible = false;
        }

        public void Show()
        {
            this.visible = true;
            m_updateVariableList();
        }
        private void m_onAddClicked()
        {
            var parameterType = new GenericMenu();

            foreach (var varType in m_getVariableTypes())
                parameterType.AddItem(new GUIContent(m_getNiceNameFromType(varType)), false, () =>
                {
                    string uniqueName = "New" + m_getNiceNameFromType(varType);
                    uniqueName = m_getUniqueName(uniqueName);
                    _graphView.AddLGVariable(uniqueName, varType);
                });

            parameterType.ShowAsContext();
        }
        private void m_updateVariableList()
        {
            content.Clear();

            foreach (var variable in _graphView.LGInfoCache.Graph.Variables)
            {
                var row = new BlackboardRow(new LGVariableFieldView(_graphView, variable), new LGVariablePropertyView(_graphView, variable));
                row.expanded = false;

                content.Add(row);
            }
        }

        private IEnumerable<Type> m_getVariableTypes()
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<BaseVariable>())
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
            name = name.Replace("Variable", "");

            return ObjectNames.NicifyVariableName(name);
        }
        private string m_getUniqueName(string name)
        {
            // Generate unique name
            string uniqueName = name;
            int i = 0;
            while (_graphView.LGInfoCache.Graph.Variables.Any(e => e.Name == name))
                name = uniqueName + (i++);
            return name;
        }


    }
}
