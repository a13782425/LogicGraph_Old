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
        void Init(BaseNodeView nodeView, FieldInfo fieldInfo);
        //object value { get; set; }

        ///// <summary>
        ///// 显示
        ///// </summary>
        //void Show();

        ///// <summary>
        ///// 隐藏
        ///// </summary>
        //void Hide();
    }

    public interface INodeElement<T> : INodeElement
    {
        event Action<T> onValueChanged;
    }
}
