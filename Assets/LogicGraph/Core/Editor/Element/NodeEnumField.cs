using System;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class NodeEnumField<T> : EnumField, INodeElement<T>
    {
        public BaseNodeView nodeView { get; private set; }

        public FieldInfo fieldInfo { get; private set; }

        public event Action<T> onValueChanged;

        public void Init(BaseNodeView nodeView, FieldInfo fieldInfo, string titleName)
        {
            NodeElementUtils.SetBaseFieldStyle(this);
            Enum en = (Enum)fieldInfo.GetValue(nodeView.target);
            this.Init(en);
            this.nodeView = nodeView;
            this.fieldInfo = fieldInfo;
            this.label = this.CheckTitle(titleName);
            this.value = en;
            this.RegisterCallback<ChangeEvent<Enum>>((e) => OnValueChange(e.newValue));
        }

        private void OnValueChange(Enum newValue)
        {
            if (onValueChanged != null)
                this.onValueChanged?.Invoke((T)Enum.Parse(typeof(T), newValue.ToString()));
            else
                fieldInfo?.SetValue(nodeView.target, newValue);
        }
    }
}
