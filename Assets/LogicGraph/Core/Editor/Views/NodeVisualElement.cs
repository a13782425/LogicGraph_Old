using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEditor.Experimental.GraphView;
//using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{

    public interface INodeVisualElement
    {
        /// <summary>
        /// 添加一个UI
        /// </summary>
        /// <param name="ui"></param>
        void AddUI(VisualElement ui);
        /// <summary>
        /// 右键点击
        /// </summary>
        event Action<ContextualMenuPopulateEvent> onGenericMenu;
    }

}
