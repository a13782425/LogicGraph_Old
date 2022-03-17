using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public interface INodeElement
    {
        BaseNodeView nodeView { get; }
        FieldInfo fieldInfo { get; }
        void Init(BaseNodeView nodeView, FieldInfo fieldInfo, string title);
    }

    public interface INodeElement<T> : INodeElement
    {
        event Action<T> onValueChanged;
    }
}
