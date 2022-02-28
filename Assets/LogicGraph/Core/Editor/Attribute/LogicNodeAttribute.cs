using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Logic.Editor
{
    /// <summary>
    /// 逻辑图节点
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class LogicNodeAttribute : Attribute
    {
        /// <summary>
        /// 节点类型
        /// </summary>
        public Type NodeType { get; private set; }

        private List<Type> _logicGraphs = new List<Type>();
        /// <summary>
        /// 逻辑图名字
        /// </summary>
        public List<Type> LogicGraphs { get => _logicGraphs; private set { _logicGraphs = value; } }

        /// <summary>
        /// 包含的逻辑图
        /// </summary>
        public Type[] IncludeGraphs = new Type[0];

        /// <summary>
        /// 排除的逻辑图
        /// 优先判断排除的
        /// </summary>
        public Type[] ExcludeGraphs = new Type[0];
        /// <summary>
        /// 节点名称
        /// </summary>
        public string MenuText { get; private set; }

        /// <summary>
        /// 拥有什么端口
        /// </summary>
        public PortEnum PortType = PortEnum.All;
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType">节点类型</param>
        /// <param name="menuText">菜单名</param>
        /// <param name="logicGraphs">可用的逻辑图(为空的话都可以用)</param>
        public LogicNodeAttribute(Type nodeType, string menuText)
        {
            NodeType = nodeType;
            MenuText = menuText;
        }

        public bool HasType(Type type)
        {
            bool result = true;
            if (ExcludeGraphs.Length > 0)
            {
                result = !ExcludeGraphs.Contains(type);
            }
            if (result && IncludeGraphs.Length > 0)
            {
                result = IncludeGraphs.Contains(type);
            }
            return result;
        }
    }

    /// <summary>
    /// 端口枚举
    /// </summary>
    [Flags]
    public enum PortEnum : byte
    {
        None = 0,
        /// <summary>
        /// 只有进
        /// </summary>
        In = 1,
        /// <summary>
        /// 只有出
        /// </summary>
        Out = 2,
        /// <summary>
        /// 二者皆有
        /// </summary>
        All = In | Out
    }
}
