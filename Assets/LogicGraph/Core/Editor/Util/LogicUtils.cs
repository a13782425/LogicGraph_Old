using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Logic.Editor
{
    public static class LogicUtils
    {
        #region 拓展
        /// <summary>
        /// 根据窗体获取位置
        /// </summary>
        /// <param name="window"></param>
        /// <param name="localPos">本地位置</param>
        public static Vector2 GetScreenPosition(this UnityEditor.EditorWindow window, Vector2 localPos)
        {
            return window.position.position + localPos;
        }
        #endregion
    }
}
