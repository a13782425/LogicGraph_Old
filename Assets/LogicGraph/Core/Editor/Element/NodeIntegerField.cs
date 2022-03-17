using System;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class NodeIntegerField : IntegerField, INodeElement<int>
    {
        public BaseNodeView nodeView { get; private set; }

        public FieldInfo fieldInfo { get; private set; }

        public event Action<int> onValueChanged;

        public void Init(BaseNodeView nodeView, FieldInfo fieldInfo)
        {
            NodeElementUtils.SetBaseFieldStyle(this);
            this.nodeView = nodeView;
            this.fieldInfo = fieldInfo;
            NodeIntAttribute attr = fieldInfo.GetCustomAttribute<NodeIntAttribute>();
            this.label = attr.Title;
            this.value = (int)fieldInfo.GetValue(nodeView.target);
            this.RegisterCallback<ChangeEvent<int>>((e) => OnValueChange(e.newValue));
        }

        private void OnValueChange(int newValue)
        {
            if (onValueChanged != null)
                this.onValueChanged?.Invoke(value);
            else
                fieldInfo?.SetValue(nodeView.target, value);
        }
    }
}
