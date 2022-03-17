using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class NodeToggle : Toggle, INodeElement<bool>
    {
        public BaseNodeView nodeView { get; private set; }

        public FieldInfo fieldInfo { get; private set; }

        public event Action<bool> onValueChanged;

        public void Init(BaseNodeView nodeView, FieldInfo fieldInfo, string titleName)
        {
            NodeElementUtils.SetBaseFieldStyle(this);
            this.nodeView = nodeView;
            this.fieldInfo = fieldInfo;
            this.label = this.CheckTitle(titleName);
            this.value = (bool)fieldInfo.GetValue(nodeView.target);
            this.RegisterCallback<ChangeEvent<bool>>((e) => OnValueChange(e.newValue));
        }

        private void OnValueChange(bool newValue)
        {
            if (onValueChanged != null)
                this.onValueChanged?.Invoke(newValue);
            else
                fieldInfo?.SetValue(nodeView.target, newValue);
        }
    }
}
