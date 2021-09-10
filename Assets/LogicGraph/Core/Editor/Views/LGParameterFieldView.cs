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
    public sealed class LGParameterFieldView : BlackboardField
    {
        private LogicGraphView graphView;

        public BaseParameter param { get; private set; }

        public LGParameterFieldView(LogicGraphView graphView, BaseParameter param) : base(null, param.Name, param.Value.GetType().Name)
        {
            this.graphView = graphView;
            this.param = param;
            //this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            this.Q("icon").AddToClassList("parameter-" + param.Value.GetType().Name);
            this.Q("icon").visible = true;

            (this.Q("textField") as TextField).RegisterValueChangedCallback((e) =>
            {
                param.Name = e.newValue;
                text = e.newValue;
            });
        }

        protected override void BuildFieldContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("重命名", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("删除", (a) =>
            {
                graphView.DelLGParam(param);
            }, DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
    }
}