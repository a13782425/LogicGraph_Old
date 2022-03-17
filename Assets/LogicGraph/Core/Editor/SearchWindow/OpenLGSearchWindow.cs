using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Logic.Editor
{
    /// <summary>
    /// 逻辑图列表搜索窗口
    /// </summary>
    public sealed class OpenLGSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public event Func<SearchTreeEntry, SearchWindowContext, bool> onSelectHandler;
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("打开逻辑图")));
            foreach (var item in LogicProvider.LGEditorList)
            {
                entries.Add(new SearchTreeGroupEntry(new GUIContent(item.GraphName)) { level = 1, userData = item });
                var datas = LogicProvider.LGInfoList.Where(a => a.GraphClassName == item.GraphClassName).ToList();
                foreach (var graph in datas)
                {
                    entries.Add(new SearchTreeEntry(new GUIContent(graph.LogicName)) { level = 2, userData = graph });
                }
            }
            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            bool? res = onSelectHandler?.Invoke(searchTreeEntry, context);
            return res.HasValue ? res.Value : false;
        }
    }
}
