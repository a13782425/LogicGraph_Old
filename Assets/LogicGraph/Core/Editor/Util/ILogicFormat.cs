using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic.Editor
{
    /// <summary>
    /// 逻辑图格式化
    /// </summary>
    public interface ILogicFormat
    {
        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="graph">逻辑图</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        bool ToFormat(BaseLogicGraph graph, string path);
    }

}