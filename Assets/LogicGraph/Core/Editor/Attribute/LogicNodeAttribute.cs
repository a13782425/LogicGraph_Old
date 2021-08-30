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
        /// 节点名称
        /// </summary>
        public string MenuText { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType">节点类型</param>
        /// <param name="menuText">菜单名</param>
        /// <param name="logicGraphs">可用的逻辑图(为空的话都可以用)</param>
        public LogicNodeAttribute(Type nodeType, string menuText, params Type[] logicGraphs)
        {
            NodeType = nodeType;
            MenuText = menuText;
            if (logicGraphs == null)
            {
                _logicGraphs = logicGraphs.ToList();
            }
        }

        public bool HasType(Type type)
        {
            if (_logicGraphs.Count == 0)
            {
                return true;
            }
            return _logicGraphs.Contains(type);
        }
    }
}
