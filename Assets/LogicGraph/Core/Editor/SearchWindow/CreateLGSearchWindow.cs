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
    public sealed class CreateLGSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public event Func<SearchTreeEntry, SearchWindowContext, bool> onSelectHandler;
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("创建逻辑图")));

            foreach (LGEditorCache item in LogicProvider.LGEditorList)
            {
                entries.Add(new SearchTreeEntry(new GUIContent(item.GraphName)) { level = 1, userData = item });
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
