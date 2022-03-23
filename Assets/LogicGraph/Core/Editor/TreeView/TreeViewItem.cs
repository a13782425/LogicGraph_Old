using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Editor
{
    public class TreeViewItem : ITreeViewItem
    {
        public TreeViewItem _parent;

        private List<ITreeViewItem> m_Children;

        public int id { get; private set; }

        public ITreeViewItem parent => _parent;

        public IEnumerable<ITreeViewItem> children => m_Children;

        public bool hasChildren => m_Children != null && m_Children.Count > 0;

        public Action<ITreeViewItem> onSelect { get; set; }

        public Action<ITreeViewItem> onClick { get; set; }
        public object userData { get; set; }
        public string name { get; set; }

        public TreeViewItem(int id, List<ITreeViewItem> children = null)
        {
            this.id = id;
            if (children == null)
            {
                return;
            }
            foreach (TreeViewItem child in children)
            {
                AddChild(child);
            }
        }

        public void AddChild(ITreeViewItem child)
        {
            TreeViewItem treeViewItem = child as TreeViewItem;
            if (treeViewItem != null)
            {
                if (m_Children == null)
                {
                    m_Children = new List<ITreeViewItem>();
                }
                m_Children.Add(treeViewItem);
                treeViewItem._parent = this;
            }
        }

        public void AddChildren(IList<ITreeViewItem> children)
        {
            foreach (ITreeViewItem child in children)
            {
                AddChild(child);
            }
        }

        public void RemoveChild(ITreeViewItem child)
        {
            if (m_Children != null)
            {
                TreeViewItem treeViewItem = child as TreeViewItem;
                if (treeViewItem != null)
                {
                    m_Children.Remove(treeViewItem);
                }
            }
        }
    }
}
