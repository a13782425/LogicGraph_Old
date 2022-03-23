using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Logic.Editor
{

    public sealed class NodeObjectField<T> : ObjectField, INodeElement<T>
    {
        public BaseNodeView nodeView { get; private set; }

        public FieldInfo fieldInfo { get; private set; }


        public event Action<T> onValueChanged;

        public void Init(BaseNodeView nodeView, FieldInfo fieldInfo, string titleName)
        {
            NodeElementUtils.SetBaseFieldStyle(this);
            this.objectType = typeof(T);
            this.allowSceneObjects = false;
            this.nodeView = nodeView;
            this.fieldInfo = fieldInfo;
            this.label = this.CheckTitle(titleName);
            this.value = (Object)fieldInfo.GetValue(nodeView.target);
            this.RegisterCallback<ChangeEvent<Object>>((e) => OnValueChange(e.newValue));
        }

        private void OnValueChange(Object newValue)
        {
            T val = (T)Convert.ChangeType(newValue, typeof(T));
            if (onValueChanged != null)
                this.onValueChanged?.Invoke(val);
            else
                fieldInfo?.SetValue(nodeView.target, val);
        }
    }
}
