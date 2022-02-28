using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Logic.Editor
{
    [CustomEditor(typeof(BaseLogicGraph), true)]
    public sealed class BaseLogicGraph_Inspector : UnityEditor.Editor
    {
        private BaseLogicGraph _logic;
        private LGEditorCache _cache;
        private LGInfoCache _infoCache;

        void OnEnable()
        {
            _logic = target as BaseLogicGraph;
            _infoCache = LGCacheOp.GetLogicInfo(_logic);
            _infoCache.Graph = _logic;
            _cache = LGCacheOp.GetEditorCache(_infoCache);
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("打开"))
            {
                LGWindow.ShowLGPanel(LGCacheOp.GetLogicInfo(_logic));
            }
            if (GUILayout.Button("修复"))
            {
                string path = AssetDatabase.GetAssetPath(_logic);
                Debug.LogError(path);
                using (StreamReader input = new StreamReader(path, Encoding.UTF8))
                {
                    var yaml = new YamlStream();
                    yaml.Load(input);

                    Debug.LogError(yaml.Documents.Count);
                    //var node = yaml.Documents[0].RootNode["references"];
                    foreach (var item in yaml.Documents[0].AllNodes)
                    {
                        Debug.Log(item.ToString());
                    }
                   
                }
            }
            if (_cache != null)
            {
                for (int i = 0; i < _cache.Formats.Count; i++)
                {
                    LFEditorCache format = _cache.Formats[i];

                    if (GUILayout.Button("导出:" + format.FormatName))
                    {
                        string savePath = Application.dataPath;
                        string saveFile = "undefined";
                        if (!string.IsNullOrEmpty(_infoCache.LastFormatPath))
                        {
                            savePath = Path.GetDirectoryName(_infoCache.LastFormatPath);
                            saveFile = Path.GetFileNameWithoutExtension(_infoCache.LastFormatPath);
                        }
                        string filePath = EditorUtility.SaveFilePanel("导出", savePath, saveFile, format.Extension);
                        if (string.IsNullOrWhiteSpace(filePath))
                        {
                            return;
                        }
                        var logicFormat = Activator.CreateInstance(format.GetFormatType()) as ILogicFormat;
                        bool res = logicFormat.ToFormat(_infoCache, filePath);
                        if (res)
                        {
                            string tempPath = filePath.Replace("\\", "/");
                            int index = tempPath.IndexOf("Assets");
                            if (index > 0)
                            {
                                _infoCache.LastFormatPath = tempPath.Substring(index, tempPath.Length - index);
                            }
                            Debug.Log($"导出: {format.FormatName} 成功");
                            AssetDatabase.Refresh();
                        }
                        else
                        {
                            Debug.Log($"导出: {format.FormatName} 失败");
                        }
                    }
                }
            }
            UnityEditor.EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            UnityEditor.EditorGUI.EndDisabledGroup();
        }
    }
}
