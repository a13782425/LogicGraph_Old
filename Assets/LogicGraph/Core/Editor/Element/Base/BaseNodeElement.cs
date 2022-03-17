//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine.UIElements;

//namespace Logic.Editor
//{

//    public abstract class BaseNodeElement : INodeElement
//    {
//        public abstract VisualElement view { get; }

//        public FieldInfo fieldInfo { get; private set; }
//        public BaseNodeView nodeView { get; private set; }
//        public BaseNodeElement(BaseNodeView nodeView, FieldInfo fieldInfo)
//        {
//            this.nodeView = nodeView;
//            this.fieldInfo = fieldInfo;
//        }
//        public virtual void Hide()
//        {
//            view.style.display = DisplayStyle.None;
//        }

//        public virtual void Show()
//        {
//            view.style.display = DisplayStyle.Flex;
//        }

//        public void Init(BaseNodeView nodeView, FieldInfo fieldInfo)
//        {
           
//        }
//    }

//    public abstract class BaseNodeElement<T> : BaseNodeElement
//    {


//        public virtual event Action<T> onValueChanged;

//        /// <summary>
//        /// 获取view的值
//        /// 不对Node操作
//        /// </summary>
//        public virtual T value { get; set; }
//        protected BaseNodeElement(BaseNodeView nodeView, FieldInfo fieldInfo) : base(nodeView, fieldInfo)
//        {
//        }

//        protected virtual void OnValueChange(T value)
//        {
//            if (onValueChanged != null)
//                this.onValueChanged?.Invoke(value);
//            else
//            {
//                fieldInfo.SetValue(nodeView.target, value);
//            }
//        }

       
//    }
//}
