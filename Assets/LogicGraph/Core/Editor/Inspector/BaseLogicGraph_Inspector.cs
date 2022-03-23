using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Logic.Editor
{
    [CustomEditor(typeof(BaseLogicGraph), true)]
    public sealed class BaseLogicGraph_Inspector : UnityEditor.Editor
    {
        private BaseLogicGraph _logic;
        private LGEditorCache _cache;
        //private LGInfoCache _infoCache;

        /// <summary>
        /// 是否显示详情
        /// </summary>
        private bool _isShowDetail = false;
        void OnEnable()
        {
            _logic = target as BaseLogicGraph;
            //_infoCache = LogicProvider.GetLogicInfo(_logic);
            //_infoCache.Graph = _logic;
            _cache = LogicProvider.GetEditorCache(_logic);
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("打开"))
            {
                LGWindow.ShowLogic(_logic.OnlyId);
            }
            //if (GUILayout.Button("修复变量错误"))
            //{
            //    foreach (var item in _logic.Nodes)
            //    {
            //        if (item is VariableNode varNode)
            //        {
            //            var node = _logic.GetVariableById(varNode.varId);
            //            if (node != null)
            //            {
            //                varNode.varName = node.Name;
            //            }
            //        }
            //    }
            //    EditorUtility.SetDirty(_logic);
            //    AssetDatabase.SaveAssets();
            //    AssetDatabase.Refresh();
            //}
            if (GUILayout.Button("显示详情,但可能导致崩溃"))
            {
                _isShowDetail = !_isShowDetail;
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
                        if (!string.IsNullOrEmpty(_logic.LastFormatPath))
                        {
                            savePath = Path.GetDirectoryName(_logic.LastFormatPath);
                            saveFile = Path.GetFileNameWithoutExtension(_logic.LastFormatPath);
                        }
                        string filePath = EditorUtility.SaveFilePanel("导出", savePath, saveFile, format.Extension);
                        if (string.IsNullOrWhiteSpace(filePath))
                        {
                            return;
                        }
                        var logicFormat = Activator.CreateInstance(format.FormatType) as ILogicFormat;
                        bool res = logicFormat.ToFormat(_logic, filePath);
                        if (res)
                        {
                            string tempPath = filePath.Replace("\\", "/");
                            int index = tempPath.IndexOf("Assets");
                            if (index > 0)
                            {
                                _logic.LastFormatPath = tempPath.Substring(index, tempPath.Length - index);
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
            if (_isShowDetail)
            {
                //UnityEditor.EditorGUI.BeginDisabledGroup(true);
                base.OnInspectorGUI();
                //UnityEditor.EditorGUI.EndDisabledGroup();
            }
        }
    }
}
