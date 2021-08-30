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

        public LogicGraphAttribute(string str)
        {
            LogicName = str;
        }
    }
}