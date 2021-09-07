using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    /// <summary>
    /// 逻辑图类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class LogicGraphAttribute : Attribute
    {

        /// <summary>
        /// 逻辑图名字
        /// </summary>
        public string LogicName { get; private set; }

        /// <summary>
        /// 默认节点名
        /// </summary>
        public Type[] DefaultNodes { get;private set; }

        public LogicGraphAttribute(string str,params Type[] nodeType)
        {
            LogicName = str;
            DefaultNodes = nodeType ?? new Type[0];
        }
    }
}