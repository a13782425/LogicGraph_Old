﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class NodeVector2Field : Vector2Field, INodeElement<Vector2>
    {
        public BaseNodeView nodeView { get; private set; }

        public FieldInfo fieldInfo { get; private set; }

        public event Action<Vector2> onValueChanged;

        public void Init(BaseNodeView nodeView, FieldInfo fieldInfo, string titleName)
        {
            NodeElementUtils.SetBaseFieldStyle(this);
            this.nodeView = nodeView;
            this.fieldInfo = fieldInfo;
            this.label = this.CheckTitle(titleName);
            this.value = (Vector2)fieldInfo.GetValue(nodeView.target);
            this.RegisterCallback<ChangeEvent<Vector2>>((e) => OnValueChange(e.newValue));
        }

        private void OnValueChange(Vector2 newValue)
        {
            if (onValueChanged != null)
                this.onValueChanged?.Invoke(newValue);
            else
                fieldInfo?.SetValue(nodeView.target, newValue);
        }
    }
}
