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
    public sealed class LGVariableFieldView : BlackboardField
    {
        private LogicGraphView graphView;

        public BaseVariable param { get; private set; }

        private string _newName = "";

        public LGVariableFieldView(LogicGraphView graphView, BaseVariable param) : base(null, param.Name, param.Value.GetType().Name)
        {
            this.graphView = graphView;
            this.param = param;
#if !UNITY_2020_1_OR_NEWER
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
#endif
            this.Q("icon").style.backgroundColor = param.GetColor();
            this.Q("icon").visible = true;

            (this.Q("textField") as TextField).RegisterValueChangedCallback((e) =>
            {
                text = e.newValue;
            });
            (this.Q("textField") as TextField).RegisterCallback<FocusOutEvent>((e) =>
            {
                if (m_checkVerifyVarName(text))
                {
                    BaseVariable variable = graphView.LGInfoCache.Graph.Variables.FirstOrDefault(a => a.Name == text);
                    if (variable != null && variable != param)
                    {
                        text = this.param.Name;
                        graphView.Window.ShowNotification(new GUIContent("一个逻辑图中变量名不能重复"));
                    }
                    else
                        this.param.Name = text;
                }
                else
                {
                    text = this.param.Name;
                    graphView.Window.ShowNotification(new GUIContent("变量名不合法或过长"));
                }
            });
        }

        private bool m_checkVerifyVarName(string varName)
        {
            varName = varName.Trim();
            if (string.IsNullOrWhiteSpace(varName))
            {
                return false;
            }
            char[] strs = varName.ToArray();
            if (strs.Length > 9)
            {
                return false;
            }
            bool result = true;
            int length = 0;
            while (length < strs.Length)
            {
                char c = strs[length];
                if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && c != '_')
                {
                    if (length == 0)
                    {
                        result = false;
                        goto End;
                    }
                    else if (c < '0' || c > '9')
                    {
                        result = false;
                        goto End;
                    }

                }
                length++;
            }
        End: return result;
        }

#if UNITY_2020_1_OR_NEWER
        protected override void BuildFieldContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("重命名", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("删除", (a) =>
            {
                graphView.DelLGVariable(param);
            }, DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
#else
        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            while (evt.menu.MenuItems().Count > 0)
            {
                evt.menu.RemoveItemAt(0);
            }
            evt.menu.AppendAction("重命名", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("删除", (a) =>
            {
                graphView.DelLGVariable(param);
            }, DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
#endif
    }
}