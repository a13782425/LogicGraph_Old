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
        private BaseGraphView _graphView;

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

        public void InitializeGraphView(BaseGraphView graphView)
        {
            this._graphView = graphView;
            SetPosition(new Rect(new Vector2(0, 20), new Vector2(180, 320)));
            //hierarchy.Add(new Resizer(() =>
            //{
            //    graphView.Target.VariableCache.Size = layout.size;
            //}));
            //RegisterCallback<MouseUpEvent>((e) =>
            //{
            //    graphView.LGInfoCache.VariableCache.Pos = layout.position;
            //    e.StopPropagation();
            //});
            graphView.onVariableModify += m_updateVariableList;
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
                    _graphView.AddVariable(uniqueName, varType);
                });

            parameterType.ShowAsContext();
        }
        private void m_updateVariableList()
        {
            content.Clear();

            foreach (var variable in _graphView.Target.Variables)
            {
                var row = new BlackboardRow(new LGVariableFieldView(_graphView, variable), new LGVariablePropertyView(_graphView, variable));
                row.expanded = false;

                content.Add(row);
            }
        }

        private IEnumerable<Type> m_getVariableTypes()
        {
            if (_graphView != null)
            {
                List<Type> types = _graphView.VarTypes.Where(a => a.IsSubclassOf(typeof(BaseVariable))).Distinct().ToList();
                if (types.Count == 0)
                {
                    types = TypeCache.GetTypesDerivedFrom<BaseVariable>().ToList();
                }
                foreach (var type in types)
                {
                    yield return type;
                }
            }
        }
        private string m_getNiceNameFromType(Type type)
        {
            string name = type.Name;

            // Remove parameter in the name of the type if it exists
            name = name.Replace("Variable", "");

            return name;
        }
        private string m_getUniqueName(string name)
        {
            // Generate unique name
            string uniqueName = name;
            int i = 0;
            while (_graphView.Target.Variables.Any(e => e.Name == name))
                name = uniqueName + (i++);
            return name;
        }


    }
}
