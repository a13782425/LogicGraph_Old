using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Logic.Editor
{
    public static class LogicUtils
    {
        private static Texture2D scriptIcon = (EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D);
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

        /// <summary>
        /// 窗口最小大小
        /// </summary>
        public readonly static Vector2 MIN_SIZE;

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
            MIN_SIZE = new Vector2(640, 360);
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
        public static StyleSheet GetGridStyle()
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_STYLE_PATH, "GroupView.uss"));
        }
        public static VisualTreeAsset GetPinnedView()
        {
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(EDITOR_STYLE_PATH, "ParamElement.uxml"));
        }
        public static StyleSheet GetPinnedStyle()
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_STYLE_PATH, "ParamElementView.uss"));
        }
        public static StyleSheet GetExposedStyle()
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_STYLE_PATH, "LGParameterView.uss"));
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
        /// <summary>
        /// 设置输入框的颜色
        /// </summary>
        public static void SetInputColor<TValueType>(this TextInputBaseField<TValueType> input, Color color)
        {
            input.Q("unity-text-input").style.backgroundColor = color;
        }
        /// <summary>
        /// 设置输入框文字的颜色
        /// </summary>
        public static void SetTextColor<TValueType>(this TextInputBaseField<TValueType> input, Color color)
        {
            input.Q("unity-text-input").style.color = color;
        }
        /// <summary>
        /// 获取输入框的颜色
        /// </summary>
        public static Color GetInputColor<TValueType>(this TextInputBaseField<TValueType> input)
        {
            return input.Q("unity-text-input").style.backgroundColor.value;
        }
        /// <summary>
        /// 获取输入框文字的颜色
        /// </summary>
        public static Color GetTextColor<TValueType>(this TextInputBaseField<TValueType> input)
        {
            return input.Q("unity-text-input").style.color.value;
        }
        #endregion


        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as BaseLogicGraph;

            if (asset != null)
            {
                LGWindow.ShowLGPanel(LGCacheOp.GetLogicInfo(asset));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 刷新
        /// </summary>
        [MenuItem("Framework/逻辑图/扫描逻辑图")]
        private static void RefreshLogic()
        {
            LGCacheData.Instance.LGEditorList.Clear();
            LGCacheOp.Refresh();
            LGCacheData.Instance.LGInfoList.Clear();
            string[] strs = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories);
            foreach (var item in strs)
            {
                string fileName = item.Replace(Application.dataPath, "Assets");
                BaseLogicGraph logicGraph = AssetDatabase.LoadAssetAtPath<BaseLogicGraph>(fileName);
                if (logicGraph != null)
                {
                    LGInfoCache infoCache = new LGInfoCache();
                    infoCache.AssetPath = fileName.Replace('\\', '/');
                    infoCache.FileName = Path.GetFileNameWithoutExtension(infoCache.AssetPath);
                    infoCache.LogicName = logicGraph.Title;
                    infoCache.GraphClassName = logicGraph.GetType().FullName;
                    infoCache.OnlyId = logicGraph.OnlyId;
                    infoCache.ParamCache.Pos = new Vector2(0, 20);
                    infoCache.ParamCache.Size = new Vector2(180, 320);
                    LGCacheData.Instance.LGInfoList.Add(infoCache);
                }
            }
            LGCacheOp.Save();
        }

        /// <summary>
        /// 创建节点
        /// </summary>
        [MenuItem("Assets/Create/LogicGraph/Node C# Script", false, 89)]
        private static void CreateNode()
        {
            string path = Path.Combine(EDITOR_PATH, "Template/LogicNodeTemplate.cs");
            CreateFromTemplate<DoCreateNodeCodeFile>(
                "NewNode.cs",
                path
            );
        }
        /// <summary>
        /// 创建逻辑图
        /// </summary>
        [MenuItem("Assets/Create/LogicGraph/Graph C# Script", false, 89)]
        private static void CreateGraph()
        {
            string path = Path.Combine(EDITOR_PATH, "Template/LogicGraphTemplate.cs");
            CreateFromTemplate<DoCreateGraphCodeFile>(
                "NewGraph.cs",
                path
            );
        }
        public static void CreateFromTemplate<T>(string initialName, string templatePath) where T : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<T>(),
                initialName,
                scriptIcon,
                templatePath
            );
        }
        /// <summary>
        /// 创建脚本
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="templatePath">模板路径</param>
        /// <returns></returns>
        internal static UnityEngine.Object CreateScript(string className, string pathName, string templatePath)
        {
            string templateText = string.Empty;

            UTF8Encoding encoding = new UTF8Encoding(true, false);
            templatePath = templatePath += ".txt";
            if (File.Exists(templatePath))
            {
                /// Read procedures.
                StreamReader reader = new StreamReader(templatePath);
                templateText = reader.ReadToEnd();
                reader.Close();

                templateText = templateText.Replace("{CLASS_NAME}", className);

                StreamWriter writer = new StreamWriter(Path.GetFullPath(pathName), false, encoding);
                writer.Write(templateText);
                writer.Close();

                AssetDatabase.ImportAsset(pathName);
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
            }
            else
            {
                Debug.LogError(string.Format("The template file was not found: {0}", templatePath));
                return null;
            }
        }
        private class DoCreateGraphCodeFile : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                Object o = CreateScript(Path.GetFileNameWithoutExtension(pathName).Replace(" ", string.Empty), pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }
        private class DoCreateNodeCodeFile : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string className = Path.GetFileNameWithoutExtension(pathName).Replace(" ", string.Empty);
                Object o = CreateScript(className, pathName, resourceFile);
                string fileName = Path.GetFileNameWithoutExtension(pathName);
                string tempPath = Path.Combine(Path.GetDirectoryName(resourceFile), "LogicNodeViewTemplate.cs.txt");
                string viewPath = Path.Combine(Path.GetDirectoryName(pathName), $"{fileName}View.cs");
                CreateScript(className, viewPath, tempPath);
                ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }
    }
}
