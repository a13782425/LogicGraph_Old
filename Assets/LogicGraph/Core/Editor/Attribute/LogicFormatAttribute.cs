using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Editor
{
    /// <summary>
    /// 逻辑图格式化特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class LogicFormatAttribute : Attribute
    {
        /// <summary>
        /// 菜单名
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 对应的逻辑图
        /// </summary>
        public Type LogicGraphType { get; private set; }

        /// <summary>
        /// 拓展名
        /// </summary>
        public string Extension { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">菜单名</param>
        /// <param name="logicGraphType">逻辑图类型</param>
        /// <param name="extension">生成文件拓展名</param>
        public LogicFormatAttribute(string name, Type logicGraphType, string extension = "txt")
        {
            Name = name;
            LogicGraphType = logicGraphType;
            Extension = extension;
        }
    }
}
