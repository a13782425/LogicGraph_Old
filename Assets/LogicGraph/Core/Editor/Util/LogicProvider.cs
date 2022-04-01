using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Logic.Editor
{
    public static class LogicProvider
    {

        private static List<LGEditorCache> _lgEditorList = new List<LGEditorCache>();
        /// <summary>
        /// 逻辑图模板缓存
        /// </summary>
        public static List<LGEditorCache> LGEditorList => _lgEditorList;
        private static List<LGInfoCache> _lgInfoList = new List<LGInfoCache>();
        /// <summary>
        /// 逻辑图信息缓存
        /// </summary>
        public static List<LGInfoCache> LGInfoList => _lgInfoList;

        private static List<ILogicConfig> _logicConfigList = new List<ILogicConfig>();

        static LogicProvider()
        {
            BuildLogicConfig();
            BuildGraphCache();
            BuildFormatCache();
            BuildNodeCache();
            BuildGraphAssetCache();
        }


        public static LGInfoCache GetLogicInfo(BaseLogicGraph logic) => LGInfoList.FirstOrDefault(a => a.OnlyId == logic.OnlyId);
        public static LGInfoCache GetLogicInfo(string onlyId) => LGInfoList.FirstOrDefault(a => a.OnlyId == onlyId);
        /// <summary>
        /// 获得对应逻辑图的编辑器信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static LGEditorCache GetEditorCache(LGInfoCache info) => GetEditorCache(info.GraphClassName);
        /// <summary>
        /// 获得对应逻辑图的编辑器信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static LGEditorCache GetEditorCache(BaseLogicGraph info) => GetEditorCache(info.GetType().FullName);
        /// <summary>
        /// 获得对应逻辑图的编辑器信息
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static LGEditorCache GetEditorCache(string className) => LGEditorList.FirstOrDefault(a => a.GraphClassName == className);

        /// <summary>
        /// 打开窗口回调
        /// </summary>
        public static void OpenWindow() => _logicConfigList.ForEach(a => a.OpenWindow());
        /// <summary>
        /// 打开逻辑图回调
        /// </summary>
        public static void OpenLogicGraph(BaseLogicGraph graph) => _logicConfigList.ForEach(a => a.OpenLogicGraph(graph));

        /// <summary>
        /// 生成逻辑图配置
        /// </summary>
        private static void BuildLogicConfig()
        {
            var types = TypeCache.GetTypesDerivedFrom<ILogicConfig>();
            foreach (var type in types)
            {
                if (!type.IsAbstract && !type.IsInterface)
                {
                    ILogicConfig config = Activator.CreateInstance(type) as ILogicConfig;
                    _logicConfigList.Add(config);
                }
            }
        }

        /// <summary>
        /// 生成逻辑图缓存
        /// </summary>
        private static void BuildGraphCache()
        {
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<BaseGraphView>();
            //循环查询逻辑图
            foreach (var item in types)
            {
                //如果当前类型是逻辑图
                var graphAttr = item.GetCustomAttribute<LogicGraphAttribute>();
                if (graphAttr != null)
                {
                    LGEditorCache graphData = new LGEditorCache();
                    graphData.GraphClassName = graphAttr.GraphType.FullName;
                    graphData.GraphViewClassName = item.FullName;
                    graphData.DefaultNodes.Clear();
                    graphData.DefaultNodeFullNames = graphAttr.DefaultNodes.Select(a => a.FullName).ToList();
                    graphData.GraphName = graphAttr.LogicName;
                    graphData.GraphType = graphAttr.GraphType;
                    graphData.ViewType = item;
                    LGEditorList.Add(graphData);
                }
            }

        }
        private static void BuildFormatCache()
        {
            List<Type> types = TypeCache.GetTypesDerivedFrom<ILogicFormat>().ToList();
            foreach (var item in types)
            {
                //如果当前类型是逻辑图节点
                var formatAttr = item.GetCustomAttribute<LogicFormatAttribute>();
                if (formatAttr != null)
                {
                    Type graphType = formatAttr.LogicGraphType;
                    LGEditorCache graphEditor = LGEditorList.FirstOrDefault(a => a.GraphClassName == graphType.FullName);
                    if (graphEditor != null)
                    {
                        LFEditorCache formatConfig = new LFEditorCache();
                        formatConfig.FormatName = formatAttr.Name;
                        formatConfig.FormatClassName = item.FullName;
                        formatConfig.Extension = formatAttr.Extension;
                        formatConfig.FormatType = item;
                        graphEditor.Formats.Add(formatConfig);
                    }
                }
            }
        }
        /// <summary>
        /// 生成节点缓存
        /// </summary>
        private static void BuildNodeCache()
        {
            List<Type> nodeViewTypes = TypeCache.GetTypesDerivedFrom<BaseNodeView>().ToList();
            foreach (LGEditorCache lgEditor in LGEditorList)
            {
                foreach (Type nodeViewType in nodeViewTypes)
                {
                    var nodeAttr = nodeViewType.GetCustomAttribute<LogicNodeAttribute>();
                    if (nodeAttr != null)
                    {
                        if (nodeAttr.HasType(lgEditor.GraphType))
                        {
                            var nodeType = nodeAttr.NodeType;
                            LNEditorCache nodeData = new LNEditorCache();
                            string[] strs = nodeAttr.MenuText.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            nodeData.NodeClassName = nodeType.FullName;
                            nodeData.NodeViewClassName = nodeViewType.FullName;
                            nodeData.NodeLayers = strs;
                            nodeData.NodeName = strs[strs.Length - 1];
                            nodeData.IsEnable = nodeAttr.IsEnable;
                            nodeData.NodeFullName = nodeAttr.MenuText;
                            nodeData.PortType = nodeAttr.PortType;
                            nodeData.NodeType = nodeAttr.NodeType;
                            nodeData.ViewType = nodeViewType;
                            m_praseNodeField(nodeData);
                            lgEditor.Nodes.Add(nodeData);
                            if (lgEditor.DefaultNodeFullNames.Contains(nodeType.FullName))
                            {
                                lgEditor.DefaultNodes.Add(nodeData);
                            }
                        }
                    }
                }

                lgEditor.Nodes.Sort((entry1, entry2) =>
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
        /// 生成逻辑图资源缓存
        /// </summary>
        private static void BuildGraphAssetCache()
        {
            HashSet<string> hashKey = new HashSet<string>();
            LGInfoList.Clear();
            string[] guids = AssetDatabase.FindAssets("t:BaseLogicGraph");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BaseLogicGraph logicGraph = AssetDatabase.LoadAssetAtPath<BaseLogicGraph>(assetPath);
                if (logicGraph == null)
                    continue;
                if (hashKey.Contains(logicGraph.OnlyId))
                {
                    logicGraph.ResetGuid();
                }
                string logicTypeName = logicGraph.GetType().FullName;
                LGInfoCache graphCache = new LGInfoCache();
                graphCache.GraphClassName = logicTypeName;
                graphCache.LogicName = logicGraph.Title;
                graphCache.AssetPath = assetPath;
                graphCache.OnlyId = logicGraph.OnlyId;
                hashKey.Add(logicGraph.OnlyId);
                LGInfoList.Add(graphCache);
            }
        }


        private static void m_praseNodeField(LNEditorCache nodeCache)
        {
            FieldInfo[] fields = nodeCache.NodeType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (NodeElementUtils.ElementMapping.ContainsKey(field.FieldType))
                {
                    nodeCache.FieldInfos.Add(field.Name, field);
                    //nodeCache.FieldTypes.Add(field.Name, attrType);
                }
                //NodeFieldAttribute fieldAttr = field.GetCustomAttribute<NodeFieldAttribute>();
                //if (fieldAttr != null)
                //{
                //    Type attrType = fieldAttr.GetType();
                //    if (NodeElementUtils.ElementMapping.ContainsKey(attrType))
                //    {
                //        nodeCache.FieldInfos.Add(field.Name, field);
                //        nodeCache.FieldTypes.Add(field.Name, attrType);
                //    }
                //}
            }
        }
        /// <summary>
        /// 每次资源变化调用
        /// </summary>
        private sealed class LogicImport : AssetPostprocessor
        {
            /// <summary>
            /// 所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
            /// </summary>
            /// <param name="importedAsset">导入的资源</param>
            /// <param name="deletedAssets">删除的资源</param>
            /// <param name="movedAssets">移动后资源路径</param>
            /// <param name="movedFromAssetPaths">移动前资源路径</param>
            public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                bool hasAsset = false;
                foreach (var str in movedFromAssetPaths)
                {
                    //移动前资源路径
                    if (Path.GetExtension(str) == ".asset")
                    {
                        hasAsset = true;
                        goto End;
                    }
                }
                foreach (var str in movedAssets)
                {
                    //移动后资源路径
                    if (Path.GetExtension(str) == ".asset")
                    {
                        hasAsset = true;
                        goto End;
                    }
                }
                foreach (string str in importedAsset)
                {
                    if (Path.GetExtension(str) == ".asset")
                    {
                        hasAsset = true;
                        goto End;
                    }
                }
                foreach (string str in deletedAssets)
                {
                    if (Path.GetExtension(str) == ".asset")
                    {
                        hasAsset = true;
                        goto End;
                    }
                }

            End: if (hasAsset)
                {
                    LogicProvider.BuildGraphAssetCache();
                }
            }
        }
    }
}
