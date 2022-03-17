using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class LGVariablePropertyView : VisualElement
    {
        private BaseGraphView graphView;

        public BaseVariable param { get; private set; }

        public LGVariablePropertyView(BaseGraphView graphView, BaseVariable param)
        {
            this.graphView = graphView;
            this.param = param;
            BaseVariable defaultVar = graphView.DefaultVars.FirstOrDefault(a => a.Name == param.Name);
            if (param.HasDefaultValue && defaultVar == null)
            {
                Add(new Label("导出:"));
                Toggle toggle = new Toggle();
                toggle.style.marginLeft = 24;
                toggle.value = param.Export;
                toggle.RegisterCallback<ChangeEvent<bool>>(a => param.Export = a.newValue);
                Add(toggle);
            }
            Label label = new Label("默认值:");
            Add(label);
            if (defaultVar == null)
            {
                VisualElement uiElement = param.GetUI();
                uiElement.style.marginLeft = 24;
                Add(uiElement);
            }
            else
            {
                Label defaultLabel = new Label("默认参数不能赋初始值");
                defaultLabel.style.marginLeft = 24;
                Add(defaultLabel);
            }
            //VisualElement uiElement = param.GetUI();
            //uiElement.style.marginLeft = 24;
            //Add(uiElement);

            //TextField text = new TextField("描述:");
            //text.multiline = true;
            //text.style.minHeight = 12;
            //text.style.marginTop = 2;
            //text.style.marginRight = 2;
            //text.style.marginLeft = 2;
            //text.style.marginBottom = 8;
            //text.style.unityTextAlign = TextAnchor.MiddleLeft;
            //text.labelElement.style.minWidth = 45;
            //text.labelElement.style.fontSize = 12;
            //text.value = param.Describe;
            //text.RegisterCallback<ChangeEvent<string>>((e) => param.Describe = e.newValue);
            //Add(text);
        }
    }
}
