using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static Logic.Editor.LGCacheData;
using Object = UnityEngine.Object;

namespace Logic.Editor
{
    /// <summary>
    /// 逻辑图缓存操作类
    /// </summary>
    public static class LGCacheOp
    {
        /// <summary>
        /// 获得对应逻辑图的编辑器信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static LGEditorCache GetEditorCache(LGInfoCache info)
       => Instance.LGEditorList.FirstOrDefault(a => a.GraphClassName == info.GraphClassName);

        /// <summary>
        /// 获取逻辑图
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static BaseLogicGraph GetLogicGraph(LGInfoCache info) => AssetDatabase.LoadAssetAtPath<BaseLogicGraph>(info.AssetPath);

        public static LGInfoCache GetLogicInfo(BaseLogicGraph logic) => Instance.LGInfoList.FirstOrDefault(a => a.OnlyId == logic.OnlyId);
        /// <summary>
        /// 保存缓存文件
        /// </summary>
        public static void Save()
        {
            Instance.Save();
        }

        public static void Refresh()
        {
            m_checkTypes();
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RemoveLogicGraph(string graphPath)
        {
            var graphCache = Instance.LGInfoList.FirstOrDefault(a => a.AssetPath == graphPath);
            if (graphCache != null)
            {
                Object[] panels = Resources.FindObjectsOfTypeAll(typeof(LGWindow));
                LGWindow panel = null;
                foreach (var item in panels)
                {
                    if (item is LGWindow p)
                    {
                        if (p.LGInfoCache == graphCache)
                        {
                            panel = p;
                            break;
                        }
                    }
                }
                if (panel != null)
                {
                    panel.Close();
                }
                Instance.LGInfoList.RemoveAll(a => a.AssetPath == graphPath);
                Save();
            }
        }

        public static void AddLogicGraph(string graphPath, bool newGuid = false)
        {
            if (Instance.LGInfoList.FirstOrDefault(a => a.AssetPath == graphPath) != null)
            {
                return;
            }
            BaseLogicGraph logicGraph = AssetDatabase.LoadAssetAtPath<BaseLogicGraph>(graphPath);
            if (logicGraph == null)
                return;
            string fileName = Path.GetFileNameWithoutExtension(graphPath);
            string logicTypeName = logicGraph.GetType().FullName;
            LGInfoCache graphCache = new LGInfoCache();
            if (Instance.LGInfoList.FirstOrDefault(a => a.OnlyId == logicGraph.OnlyId) != null)
            {
                logicGraph.ResetGuid();
            }
            graphCache.GraphClassName = logicTypeName;
            graphCache.LogicName = logicGraph.Title;
            graphCache.AssetPath = graphPath;
            graphCache.OnlyId = logicGraph.OnlyId;
            Instance.LGInfoList.Add(graphCache);
            EditorUtility.SetDirty(logicGraph);
            Save();
            logicGraph = null;
        }

        private static void m_checkTypes()
        {
            foreach (var item in Instance.LGEditorList)
            {
                item.IsRefresh = false;
                foreach (var node in item.Nodes)
                {
                    node.IsRefresh = false;
                }
            }
            m_refreshLogicGraph();
            Instance.LGEditorList.RemoveAll(a => !a.IsRefresh);
            List<Type> nodeViewTypes = TypeCache.GetTypesDerivedFrom<BaseNodeView>().ToList();
            foreach (var item in Instance.LGEditorList)
            {
                m_refreshLogicNode(nodeViewTypes, item);
            }
            m_refreshFormat();

            foreach (var item in Instance.LGEditorList)
            {
                item.Nodes.RemoveAll(a => !a.IsRefresh);
                item.Nodes.Sort((entry1, entry2) =>
                {
                    for (var i = 0; i < entry1.NodeLayers.Length; i++)
                    {
                        if (i >= entry2.NodeLayers.Length)
                            return 1;
                        var value = entry1.NodeLayers[i].CompareTo(entry2.NodeLayers[i]);
                        if (value != 0)
                        {
                            // Make sure that leaves go before nodes
                            if (entry1.NodeLayers.Length != entry2.NodeLayers.Length && (i == entry1.NodeLayers.Length - 1 || i == entry2.NodeLayers.Length - 1))
                                return entry1.NodeLayers.Length < entry2.NodeLayers.Length ? -1 : 1;
                            return value;
                        }
                    }
                    return 0;
                });

            }
        }

        /// <summary>
        /// 刷新逻辑图
        /// </summary>
        private static void m_refreshLogicGraph()
        {
            var types = TypeCache.GetTypesDerivedFrom<BaseLogicGraph>();
            Type logicGraphAttr = typeof(LogicGraphAttribute);
            //循环查询逻辑图
            foreach (var item in types)
            {
                //如果当前类型是逻辑图
                var graphAttr = item.GetCustomAttribute<LogicGraphAttribute>();
                if (graphAttr != null)
                {
                    LGEditorCache graphData = Instance.LGEditorList.FirstOrDefault(a => a.GraphClassName == item.FullName);
                    if (graphData == null)
                    {
                        graphData = new LGEditorCache();
                        graphData.GraphClassName = item.FullName;

                        Instance.LGEditorList.Add(graphData);
                    }
                    if (graphData.Groups.FirstOrDefault(a => a.Name == "默认分组") == null)
                    {
                        LGroupEditorCache groupCache = new LGroupEditorCache();
                        groupCache.Name = "默认分组";
                        groupCache.CanDel = false;
                        groupCache.CanRename = false;
                        graphData.Groups.Add(groupCache);
                    }
                    graphData.DefaultNodes.Clear();
                    graphData.GraphName = graphAttr.LogicName;
                    graphData.IsRefresh = true;
                }
            }
        }

        /// <summary>
        /// 刷新逻辑图节点
        /// </summary>
        private static void m_refreshLogicNode(List<Type> nodeViewTypes, LGEditorCache lGEditorCache)
        {
            Type graphType = lGEditorCache.GetGraphType();
            //如果当前类型是逻辑图
            LogicGraphAttribute graphAttr = graphType.GetCustomAttribute<LogicGraphAttribute>();
            List<string> defaultClasses = graphAttr.DefaultNodes.Select(a => a.FullName).ToList();
            foreach (var viewType in nodeViewTypes)
            {
                var nodeAttr = viewType.GetCustomAttribute<LogicNodeAttribute>();
                if (nodeAttr != null)
                {
                    if (nodeAttr.HasType(graphType))
                    {
                        var nodeType = nodeAttr.NodeType;
                        LNEditorCache nodeData = lGEditorCache.Nodes.FirstOrDefault(a => a.NodeClassName == nodeType.FullName);
                        if (nodeData == null)
                        {
                            nodeData = new LNEditorCache();
                            nodeData.UseCount = int.MinValue;
                            lGEditorCache.Nodes.Add(nodeData);
                        }
                        string[] strs = nodeAttr.MenuText.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        nodeData.NodeClassName = nodeType.FullName;
                        nodeData.NodeViewClassName = viewType.FullName;
                        nodeData.NodeLayers = strs;
                        nodeData.NodeName = strs[strs.Length - 1];
                        nodeData.IsEnable = nodeAttr.IsEnable;
                        nodeData.NodeFullName = nodeAttr.MenuText;
                        nodeData.PortType = nodeAttr.PortType;
                        nodeData.IsRefresh = true;
                        if (defaultClasses.Contains(nodeType.FullName))
                        {
                            lGEditorCache.DefaultNodes.Add(nodeData);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 刷新格式化信息
        /// </summary>
        /// <param name="types"></param>
        private static void m_refreshFormat()
        {
            List<Type> types = TypeCache.GetTypesDerivedFrom<ILogicFormat>().ToList();
            foreach (var item in types)
            {
                //如果当前类型是逻辑图节点
                var formatAttr = item.GetCustomAttribute<LogicFormatAttribute>();
                if (formatAttr != null)
                {
                    Type graphType = formatAttr.LogicGraphType;
                    var graphEditor = Instance.LGEditorList.FirstOrDefault(a => a.GraphClassName == graphType.FullName);
                    var formatConfig = graphEditor.Formats.FirstOrDefault(a => a.FormatName == formatAttr.Name);
                    if (formatConfig == null)
                    {
                        formatConfig = new LFEditorCache();
                        graphEditor.Formats.Add(formatConfig);
                    }
                    formatConfig.FormatName = formatAttr.Name;
                    formatConfig.FormatClassName = item.FullName;
                    formatConfig.Extension = formatAttr.Extension;
                }
            }
        }
    }
}
