using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class LogicGraphAttribute : Attribute
    {
        /// <summary>
        /// 逻辑图名字
        /// </summary>
        public string LogicName { get; private set; }

        public Type GraphType { get; private set; }

        /// <summary>
        /// 默认节点名
        /// </summary>
        public Type[] DefaultNodes { get; private set; }

        public LogicGraphAttribute(string str, Type graphType, params Type[] nodeType)
        {
            LogicName = str;
            GraphType = graphType;
            DefaultNodes = nodeType ?? new Type[0];
        }
    }
}
