using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using static Logic.Editor.LGCacheData;

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
        internal static LGEditorCache GetEditorCache(LGInfoCache info)
       => Instance.LGEditorList.FirstOrDefault(a => a.GraphClassName == info.GraphClassName);

        /// <summary>
        /// 获取逻辑图
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        internal static BaseLogicGraph GetLogicGraph(LGInfoCache info) => AssetDatabase.LoadAssetAtPath<BaseLogicGraph>(info.AssetPath);


        /// <summary>
        /// 保存缓存文件
        /// </summary>
        public static void Save()
        {
            Instance.Save();
        }

        internal static void Refresh()
        {
            List<Type> types = new List<Type>();
            types.AddRange(typeof(BaseNodeView).Assembly.GetTypes());
            types.AddRange(typeof(BaseLogicNode).Assembly.GetTypes());
            m_checkTypes(types);
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void m_checkTypes(List<Type> types)
        {
            foreach (var item in Instance.LGEditorList)
            {
                item.IsRefresh = false;
                foreach (var node in item.Nodes)
                {
                    node.IsRefresh = false;
                }
            }
            m_refreshLogicGraph(types);
            m_refreshLogicNode(types);

            Instance.LGEditorList.RemoveAll(a => !a.IsRefresh);
            m_refreshStartNode();
          
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
        /// 刷新开始节点
        /// </summary>
        private static void m_refreshStartNode()
        {
            string fullname = typeof(StartNodeView).FullName;
            foreach (var item in Instance.LGEditorList)
            {
                var nodeCache = item.Nodes.FirstOrDefault(a => a.NodeClassName == fullname);
                if (nodeCache == null)
                {
                    nodeCache = new LNEditorCache();
                    nodeCache.NodeClassName = typeof(StartNode).FullName;
                    nodeCache.NodeViewClassName = fullname;
                    nodeCache.UseCount = int.MinValue;
                    nodeCache.NodeLayers = new string[] { "系统", "开始" };
                    nodeCache.NodeName = "开始";
                    nodeCache.NodeFullName = "系统/开始";
                    item.Nodes.Add(nodeCache);
                }
                nodeCache.PortType = PortEnum.Out;
                nodeCache.IsRefresh = true;
                nodeCache.IsShow = false;
            }
        }

        /// <summary>
        /// 刷新逻辑图
        /// </summary>
        /// <param name="types"></param>
        private static void m_refreshLogicGraph(List<Type> types)
        {
            //逻辑图类型
            Type _logicGraphType = typeof(BaseLogicGraph);
            Type _logicGraphAttr = typeof(LogicGraphAttribute);
            //循环查询逻辑图
            foreach (var item in types)
            {
                if (!item.IsAbstract && !item.IsInterface)
                {
                    if (_logicGraphType.IsAssignableFrom(item))
                    {
                        //如果当前类型是逻辑图
                        object[] graphAttrs = item.GetCustomAttributes(_logicGraphAttr, false);
                        if (graphAttrs != null && graphAttrs.Length > 0)
                        {
                            LogicGraphAttribute logicGraph = graphAttrs[0] as LogicGraphAttribute;
                            LGEditorCache graphData = Instance.LGEditorList.FirstOrDefault(a => a.GraphClassName == item.FullName);
                            if (graphData == null)
                            {
                                graphData = new LGEditorCache();
                                graphData.GraphClassName = item.FullName;
                                Instance.LGEditorList.Add(graphData);
                            }
                            graphData.GraphName = logicGraph.LogicName;
                            graphData.IsRefresh = true;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 刷新逻辑图节点
        /// </summary>
        /// <param name="types"></param>
        private static void m_refreshLogicNode(List<Type> types)
        {
            //逻辑图节点类型
            Type _logicNodeViewType = typeof(BaseNodeView);
            Type _logicNodeAttr = typeof(LogicNodeAttribute);
            //循环查询逻辑图节点视图
            foreach (var item in types)
            {
                if (!item.IsAbstract && !item.IsInterface)
                {
                    if (_logicNodeViewType.IsAssignableFrom(item))
                    {
                        //如果当前类型是逻辑图节点
                        object[] nodeAttrs = item.GetCustomAttributes(_logicNodeAttr, false);
                        if (nodeAttrs != null && nodeAttrs.Length > 0)
                        {
                            LogicNodeAttribute logicNode = nodeAttrs[0] as LogicNodeAttribute;
                            Type nodeType = logicNode.NodeType;
                            foreach (var graphData in Instance.LGEditorList)
                            {
                                if (logicNode.HasType(graphData.GetGraphType()))
                                {
                                    LNEditorCache nodeData = graphData.Nodes.FirstOrDefault(a => a.NodeClassName == nodeType.FullName);
                                    if (nodeData == null)
                                    {
                                        nodeData = new LNEditorCache();
                                        nodeData.NodeClassName = nodeType.FullName;
                                        nodeData.NodeViewClassName = item.FullName;
                                        nodeData.UseCount = int.MinValue;
                                        graphData.Nodes.Add(nodeData);
                                    }
                                    string[] strs = logicNode.MenuText.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                                    nodeData.NodeLayers = strs;
                                    nodeData.NodeName = strs[strs.Length - 1];
                                    nodeData.NodeFullName = logicNode.MenuText;
                                    nodeData.PortType = logicNode.PortType;
                                    nodeData.IsRefresh = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
