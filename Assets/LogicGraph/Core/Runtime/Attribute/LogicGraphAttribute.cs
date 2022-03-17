using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace Logic
//{
//    /// <summary>
//    /// 逻辑图类型
//    /// </summary>
//    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
//    public class LogicGraphAttribute : Attribute
//    {

//        /// <summary>
//        /// 逻辑图名字
//        /// </summary>
//        public string LogicName { get; private set; }

//        /// <summary>
//        /// 默认节点名
//        /// </summary>
//        public Type[] DefaultNodes { get;private set; }

//        public LogicGraphAttribute(string str,params Type[] nodeType)
//        {
//            LogicName = str;
//            DefaultNodes = nodeType ?? new Type[0];
//        }
//    }
//}
namespace Logic
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public abstract class NodeFieldAttribute : Attribute
    {
        public string Title = "";
    }

    /// <summary>
    /// 文字输入框
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NodeInputAttribute : NodeFieldAttribute
    {
    }

    /// <summary>
    /// 整数输入框
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NodeIntAttribute : NodeFieldAttribute
    {
    }

    public class NodePortAttribute : NodeFieldAttribute
    {
        /// <summary>
        /// 端口朝向
        /// 不能为All
        /// </summary>
        public PortDirEnum Dir = PortDirEnum.In;
        /// <summary>
        /// 端口形状
        /// </summary>
        public PortShapeEnum Shape = PortShapeEnum.Circle;
        /// <summary>
        /// 接受变量类型
        /// 如果端口类型为Variable时生效
        /// </summary>
        public Type[] VarTypes = new Type[0];

        public NodePortAttribute() : this("", PortDirEnum.In, PortShapeEnum.Circle) { }
        public NodePortAttribute(string title) : this(title, PortDirEnum.In, PortShapeEnum.Circle) { }
        public NodePortAttribute(string title, PortDirEnum dir) : this(title, dir, PortShapeEnum.Circle) { }
        public NodePortAttribute(string title, PortShapeEnum shape) : this(title, PortDirEnum.In, shape) { }
        public NodePortAttribute(string title, PortDirEnum dir, PortShapeEnum shape)
        {
            Title = title;
            Dir = dir;
            Shape = shape;
        }
    }
}