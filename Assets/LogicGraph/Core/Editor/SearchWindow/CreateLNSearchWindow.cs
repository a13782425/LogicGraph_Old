using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Logic.Editor.LGCacheData;

namespace Logic.Editor
{
    public sealed class CreateLNSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public event Func<SearchTreeEntry, SearchWindowContext, bool> onSelectHandler;

        private LGEditorCache _editorData;
        public void Init(LGEditorCache editorCache)
        {
            _editorData = editorCache;
        }
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var searchTrees = new List<SearchTreeEntry>();
            searchTrees.Add(new SearchTreeGroupEntry(new GUIContent("创建节点")));

            AddRecommendTree(searchTrees);
            AddNodeTree(searchTrees);
            return searchTrees;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            bool? res = onSelectHandler?.Invoke(searchTreeEntry, context);
            return res.HasValue ? res.Value : false;
        }

        /// <summary>
        /// 添加常用节点树
        /// </summary>
        /// <param name="searchTrees"></param>
        /// <param name="logicNodes"></param>
        private void AddRecommendTree(List<SearchTreeEntry> searchTrees)
        {
            Type startType = typeof(StartNode);
            List<LNEditorCache> logicNodes = _editorData.Nodes.ToList();
            logicNodes.Sort((a, b) =>
            {
                if (a.UseCount == b.UseCount)
                {
                    return 0;
                }
                else if (a.UseCount < b.UseCount)
                {
                    return 1;
                }
                return -1;
            });

            var recommends = logicNodes.Take(10);
            searchTrees.Add(new SearchTreeGroupEntry(new GUIContent("常用")) { level = 1 });
            foreach (LNEditorCache nodeConfig in recommends)
            {
                if (nodeConfig.GetNodeType() == startType)
                {
                    continue;
                }
                searchTrees.Add(new SearchTreeEntry(new GUIContent(nodeConfig.NodeFullName)) { level = 2, userData = nodeConfig });
            }
        }
       
        /// <summary>
        /// 添加节点树
        /// </summary>
        /// <param name="searchTrees"></param>
        private void AddNodeTree(List<SearchTreeEntry> searchTrees)
        {
            Type startType = typeof(StartNode);
            List<string> groups = new List<string>();
            foreach (LNEditorCache nodeConfig in _editorData.Nodes)
            {
                if (nodeConfig.GetNodeType() == startType)
                {
                    continue;
                }
                int createIndex = int.MaxValue;

                for (int i = 0; i < nodeConfig.NodeLayers.Length - 1; i++)
                {
                    string group = nodeConfig.NodeLayers[i];
                    if (i >= groups.Count)
                    {
                        createIndex = i;
                        break;
                    }
                    if (groups[i] != group)
                    {
                        groups.RemoveRange(i, groups.Count - i);
                        createIndex = i;
                        break;
                    }
                }
                for (int i = createIndex; i < nodeConfig.NodeLayers.Length - 1; i++)
                {
                    string group = nodeConfig.NodeLayers[i];
                    groups.Add(group);
                    searchTrees.Add(new SearchTreeGroupEntry(new GUIContent(group)) { level = i + 1 });
                }

                searchTrees.Add(new SearchTreeEntry(new GUIContent(nodeConfig.NodeName)) { level = nodeConfig.NodeLayers.Length, userData = nodeConfig });
            }
        }
    }
}