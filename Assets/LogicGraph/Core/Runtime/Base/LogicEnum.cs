using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    /// <summary>
    /// 端口枚举
    /// </summary>
    [Flags]
    public enum PortDirEnum : byte
    {
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
    /// <summary>
    /// 端口形状
    /// </summary>
    public enum PortShapeEnum : byte
    {
        /// <summary>
        /// 圆
        /// </summary>
        Circle = 1,
        /// <summary>
        /// 正方形
        /// </summary>
        Cube = 2,
    }
    /// <summary>
    /// 端口类型
    /// </summary>
    public enum PortTypeEnum : byte
    {
        /// <summary>
        /// 默认端口
        /// 即在title上的
        /// </summary>
        Default = 0,
        /// <summary>
        /// 变量端口
        /// 变量端口只能连接对应的变量
        /// </summary>
        Variable = 1,
        /// <summary>
        /// 自定义端口
        /// </summary>
        Custom
    }
}
