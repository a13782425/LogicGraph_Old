using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Editor
{
    /// <summary>
    /// 逻辑图配置
    /// 如果有配置则用配置
    /// 如果没有配置则用默认
    /// 请勿配置多个
    /// </summary>
    public interface ILogicConfig
    {
        /// <summary>
        /// 配置文件路径
        /// 主要用于缓存一些信息
        /// 默认在逻辑图Cache文件夹下,根据项目自行配置
        /// </summary>
        string CONFIG_PATH { get; }

        /// <summary>
        /// 所包含的程序集
        /// </summary>
        List<Assembly> INCLUDE_ASSEMBLIES { get; }

    }
}
