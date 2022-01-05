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
    public sealed class GroupListFieldView : BlackboardField
    {
        private LogicGraphView owner;

        public LGroupEditorCache groupEditor { get; private set; }
        public GroupListFieldView(LogicGraphView graphView, LGroupEditorCache groupEditor) : base(null, groupEditor.Name, "")
        {
            this.owner = graphView;
            this.groupEditor = groupEditor;
            this.Q("icon").RemoveFromHierarchy();
            this.Q("typeLabel").RemoveFromHierarchy();
            this.Q("input").RemoveFromHierarchy();
            this.Q("output").RemoveFromHierarchy();

            (this.Q("textField") as TextField).RegisterValueChangedCallback((e) =>
            {
                text = e.newValue;
            });
            (this.Q("textField") as TextField).RegisterCallback<FocusOutEvent>((e) =>
            {
                if (m_checkVerifyVarName(text))
                {
                    LGroupEditorCache group = graphView.LGEditorCache.Groups.FirstOrDefault(a => a.Name == text);
                    if (group != null && group != groupEditor)
                    {
                        text = this.groupEditor.Name;
                        graphView.Window.ShowNotification(new GUIContent("分组名不能重复"));
                    }
                    else
                        this.groupEditor.Name = text;
                }
                else
                {
                    text = this.groupEditor.Name;

                }
            });
#if UNITY_2019
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
#endif
        }

        public override bool IsRenamable()
        {
            return base.IsRenamable() && groupEditor.CanRename;
        }
        private bool m_checkVerifyVarName(string varName)
        {
            varName = varName.Trim();
            if (string.IsNullOrWhiteSpace(varName))
            {
                owner.Window.ShowNotification(new GUIContent("分组名不能为空"));
                return false;
            }
            char[] strs = varName.ToArray();
            if (strs.Length > 20)
            {
                owner.Window.ShowNotification(new GUIContent("分组名不能超过20个字符"));
                return false;
            }
            bool result = true;
            int length = 0;
            while (length < strs.Length)
            {
                char c = strs[length];
                if (c == ' ')
                {
                    result = false;
                    goto End;
                }
                length++;
            }
        End: if (!result)
            {
                owner.Window.ShowNotification(new GUIContent("分组名不合法"));
            }
            return result;
        }
#if UNITY_2020_1_OR_NEWER
        protected override void BuildFieldContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (groupEditor.CanRename)
            {
                evt.menu.AppendAction("重命名", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            }
            if (groupEditor.CanDel)
            {
                evt.menu.AppendAction("删除", (a) =>
                {
                    owner.LGEditorCache.DelGroupTemplate(groupEditor);
                }, DropdownMenuAction.AlwaysEnabled);
            }
            evt.StopPropagation();
        }
#elif UNITY_2019
        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            while (evt.menu.MenuItems().Count > 0)
            {
                evt.menu.RemoveItemAt(0);
            }
            if (groupEditor.CanRename)
            {
                evt.menu.AppendAction("重命名", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            }
            if (groupEditor.CanDel)
            {
                evt.menu.AppendAction("删除", (a) =>
                {
                    owner.LGEditorCache.DelGroupTemplate(groupEditor);
                }, DropdownMenuAction.AlwaysEnabled);
            }
            evt.StopPropagation();
        }
#endif

    }
}
