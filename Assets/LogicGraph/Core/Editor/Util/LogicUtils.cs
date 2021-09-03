using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Logic.Editor
{
    public static class LogicUtils
    {
        /// <summary>
        /// 编辑器路径
        /// </summary>
        public readonly static string EDITOR_PATH;
        /// <summary>
        /// 编辑器样式路径
        /// </summary>
        public readonly static string EDITOR_STYLE_PATH;

        /// <summary>
        /// 方块端口样式
        /// </summary>
        public readonly static string PORT_CUBE;

        static LogicUtils()
        {
            Type lgType = typeof(LGWindow);
            string[] guids = AssetDatabase.FindAssets(lgType.Name);
            foreach (var item in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(item);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript.GetClass() == lgType)
                {
                    EDITOR_PATH = Path.GetDirectoryName(path);
                    break;
                }
            }
            EDITOR_STYLE_PATH = Path.Combine(EDITOR_PATH, "Style");
            PORT_CUBE = "cube";
        }

        #region Style静态

        public static StyleSheet GetNodeStyle()
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_STYLE_PATH, "BaseNodeView.uss"));
        }
        public static StyleSheet GetPortStyle()
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_STYLE_PATH, "PortView.uss"));
        }
        public static StyleSheet GetEdgeStyle()
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_STYLE_PATH, "EdgeView.uss"));
        }
        public static VisualTreeAsset GetPinnedView()
        {
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(EDITOR_STYLE_PATH, "PinnedElement.uxml"));
        }
        public static StyleSheet GetPinnedStyle ()
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_STYLE_PATH, "PinnedElementView.uss"));
        }
        #endregion

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
