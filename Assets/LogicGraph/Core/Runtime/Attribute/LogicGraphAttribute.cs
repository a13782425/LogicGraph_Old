using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NodePortAttribute : Attribute
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

        public NodePortAttribute() : this(PortDirEnum.In, PortShapeEnum.Circle) { }
        public NodePortAttribute(PortDirEnum dir) : this(dir, PortShapeEnum.Circle) { }
        public NodePortAttribute(PortShapeEnum shape) : this(PortDirEnum.In, shape) { }
        public NodePortAttribute(PortDirEnum dir, PortShapeEnum shape)
        {
            Dir = dir;
            Shape = shape;
        }
    }
}