using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class NodeTextField : TextField, INodeElement<string>
    {
        public VisualElement view => this;

        public BaseNodeView nodeView { get; private set; }

        public FieldInfo fieldInfo { get; private set; }

        public event Action<string> onValueChanged;

        public void Init(BaseNodeView nodeView, FieldInfo fieldInfo)
        {
            NodeElementUtils.SetBaseFieldStyle(this);
            this.nodeView = nodeView;
            this.fieldInfo = fieldInfo;
            NodeInputAttribute attr = fieldInfo.GetCustomAttribute<NodeInputAttribute>();
            this.label = attr.Title;
            this.value = fieldInfo.GetValue(nodeView.target) as string;
            this.RegisterCallback<ChangeEvent<string>>((e) => OnValueChange(e.newValue));
        }

        private void OnValueChange(string newValue)
        {
            if (onValueChanged != null)
                this.onValueChanged?.Invoke(value);
            else
                fieldInfo?.SetValue(nodeView.target, value);
        }
    }
}
