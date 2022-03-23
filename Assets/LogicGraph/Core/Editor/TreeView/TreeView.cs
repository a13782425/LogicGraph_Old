using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Logic.Editor.LogicUtils;

namespace Logic.Editor
{
    public class TreeView : VisualElement
    {
        private const string TREEVIEW_STYLE = "TreeView/TreeView.uss";

        private const string TOGGLE_NAME = "tree-view_item-toggle";
        private const string ITEM_PARENT_NAME = "tree-view_item-parent";
        private const string ITEM_INDENT_NAME = "tree-view_item-indent";
        private const string ITEM_INDENT_CONTENT_NAME = "tree-view_item-indent-content";
        private struct TreeViewItemWrapper
        {
            public int depth;

            public ITreeViewItem item;

            public int id => item.id;

            public bool hasChildren => item.hasChildren;
        }
        /// <summary>
        /// 打开列表
        /// </summary>
        private List<int> _expandedItemIds;
        private ListView _listView;
        private ScrollView _listViewScroll;
        private List<TreeViewItemWrapper> _itemWrappers;
        private Func<VisualElement> _onMakeItem;
        public Func<VisualElement> onMakeItem
        {
            get => _onMakeItem;
            set
            {
                _onMakeItem = value;
                Refresh();
            }
        }

        private List<ITreeViewItem> _items = new List<ITreeViewItem>();
        public List<ITreeViewItem> items
        {
            get => _items; set
            {
                _items = value;
                Refresh();
            }
        }

        public Action<VisualElement, ITreeViewItem> _onBindItem;
        public Action<VisualElement, ITreeViewItem> onBindItem
        {
            get => _onBindItem;
            set
            {
                _onBindItem = value;
                Refresh();
            }
        }

        public TreeView()
        {
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_PATH, TREEVIEW_STYLE)));
            _expandedItemIds = new List<int>();
            _itemWrappers = new List<TreeViewItemWrapper>();
            _listView = new ListView();
            _listView.name = "tree-view_list-view";
            _listView.AddToClassList("tree-view_list-view");
            _listView.itemsSource = _itemWrappers;
            _listView.onItemsChosen += m_onItemsChosen;
            _listView.itemHeight = 24;
            _listView.makeItem = makeTreeItem;
            _listView.bindItem = bindTreeItem;
            _listViewScroll = _listView.Q<ScrollView>();
            _listViewScroll.contentContainer.RegisterCallback<KeyDownEvent>(m_onKeyDown);
            this.Add(_listView);
        }
        public void Refresh()
        {
            m_generateWrappers();
            if (_listView != null)
            {
                _listView.Refresh();
            }
        }
        private void bindTreeItem(VisualElement element, int index)
        {
            ITreeViewItem item = _itemWrappers[index].item;
            VisualElement visualElement = element.Q(ITEM_INDENT_CONTENT_NAME);
            visualElement.Clear();
            for (int i = 0; i < _itemWrappers[index].depth; i++)
            {
                VisualElement visualElement2 = new VisualElement();
                visualElement2.AddToClassList(ITEM_INDENT_NAME);
                visualElement.Add(visualElement2);
            }
            Toggle toggle = element.Q<Toggle>(TOGGLE_NAME);
            toggle.SetValueWithoutNotify(IsExpandedByIndex(index));
            toggle.userData = index;
            if (item.hasChildren)
            {
                toggle.visible = true;
                element.AddToClassList(ITEM_PARENT_NAME);
            }
            else
            {
                toggle.visible = false;
                if (element.ClassListContains(ITEM_PARENT_NAME))
                {
                    element.RemoveFromClassList(ITEM_PARENT_NAME);
                }
            }
            onBindItem?.Invoke(element, item);
        }

        private VisualElement makeTreeItem()
        {
            VisualElement visualElement = new VisualElement();
            visualElement.style.flexDirection = FlexDirection.Row;
            VisualElement indentContent = new VisualElement();
            indentContent.name = ITEM_INDENT_CONTENT_NAME;
            indentContent.style.flexDirection = FlexDirection.Row;
            indentContent.AddToClassList(ITEM_INDENT_CONTENT_NAME);
            visualElement.Add(indentContent);
            Toggle toggle = new Toggle
            {
                name = TOGGLE_NAME
            };
            toggle.AddToClassList(Foldout.toggleUssClassName);
            toggle.RegisterValueChangedCallback(m_toggleExpandedState);
            visualElement.Add(toggle);
            if (onMakeItem != null)
            {
                visualElement.Add(onMakeItem.Invoke());
            }
            return visualElement;
        }
        private void m_onKeyDown(KeyDownEvent evt)
        {
            int selectedIndex = _listView.selectedIndex;
            bool flag = true;
            switch (evt.keyCode)
            {
                case KeyCode.RightArrow:
                    if (!IsExpandedByIndex(selectedIndex))
                    {
                        ExpandItemByIndex(selectedIndex);
                    }
                    break;
                case KeyCode.LeftArrow:
                    if (IsExpandedByIndex(selectedIndex))
                    {
                        CollapseItemByIndex(selectedIndex);
                    }
                    break;
                default:
                    flag = false;
                    break;
            }
            if (flag)
            {
                evt.StopPropagation();
            }
        }

        private void m_createWrappers(IEnumerable<ITreeViewItem> items, int depth, ref List<TreeViewItemWrapper> wrappers)
        {
            int num = 0;
            foreach (ITreeViewItem item2 in items)
            {
                TreeViewItemWrapper treeViewItemWrapper = default(TreeViewItemWrapper);
                treeViewItemWrapper.depth = depth;
                treeViewItemWrapper.item = item2;
                TreeViewItemWrapper item = treeViewItemWrapper;
                wrappers.Add(item);
                if (_expandedItemIds.Contains(item2.id) && item2.hasChildren)
                {
                    m_createWrappers(item2.children, depth + 1, ref wrappers);
                }
                num++;
            }
        }
        private void m_generateWrappers()
        {
            _itemWrappers.Clear();
            if (items != null)
            {
                m_createWrappers(items, 0, ref _itemWrappers);
            }
        }
        private bool IsExpandedByIndex(int index)
        {
            return _expandedItemIds.Contains(_itemWrappers[index].id);
        }
        private bool IsExpanded(TreeViewItemWrapper wrapper)
        {
            return _expandedItemIds.Contains(wrapper.id);
        }
        private void CollapseItemByIndex(int index)
        {
            if (_itemWrappers[index].item.hasChildren)
            {
                _expandedItemIds.Remove(_itemWrappers[index].item.id);
                int num = 0;
                int i = index + 1;
                for (int depth = _itemWrappers[index].depth; i < _itemWrappers.Count && _itemWrappers[i].depth > depth; i++)
                {
                    num++;
                }
                _itemWrappers.RemoveRange(index + 1, num);
                _listView.Refresh();
            }
        }
        private void CollapseItem(TreeViewItemWrapper wrapper)
        {
            if (wrapper.item.hasChildren)
            {
                int index = _listView.itemsSource.IndexOf(wrapper);
                _expandedItemIds.Remove(wrapper.item.id);
                int num = 0;
                int i = index + 1;
                for (int depth = wrapper.depth; i < _itemWrappers.Count && _itemWrappers[i].depth > depth; i++)
                {
                    num++;
                }
                _itemWrappers.RemoveRange(index + 1, num);
                _listView.Refresh();
            }
        }
        private void ExpandItemByIndex(int index)
        {
            if (_itemWrappers[index].item.hasChildren)
            {
                List<TreeViewItemWrapper> wrappers = new List<TreeViewItemWrapper>();
                m_createWrappers(_itemWrappers[index].item.children, _itemWrappers[index].depth + 1, ref wrappers);
                _itemWrappers.InsertRange(index + 1, wrappers);
                _expandedItemIds.Add(_itemWrappers[index].item.id);
                _listView.Refresh();
            }
        }
        private void ExpandItem(TreeViewItemWrapper wrapper)
        {
            if (wrapper.item.hasChildren)
            {
                int index = _listView.itemsSource.IndexOf(wrapper);
                List<TreeViewItemWrapper> wrappers = new List<TreeViewItemWrapper>();
                m_createWrappers(wrapper.item.children, wrapper.depth + 1, ref wrappers);
                _itemWrappers.InsertRange(index + 1, wrappers);
                _expandedItemIds.Add(wrapper.item.id);
                _listView.Refresh();
            }
        }
        private void m_toggleExpandedState(ChangeEvent<bool> evt)
        {
            Toggle toggle = evt.target as Toggle;
            int index = (int)toggle.userData;
            bool flag = IsExpandedByIndex(index);
            if (flag)
            {
                CollapseItemByIndex(index);
            }
            else
            {
                ExpandItemByIndex(index);
            }
            _listViewScroll.contentContainer.Focus();
        }

        private void m_onItemsChosen(IEnumerable<object> obj)
        {
            obj.OfType<TreeViewItemWrapper>().ToList().ForEach(item =>
            {
                if (item.hasChildren)
                {
                    if (!IsExpanded(item))
                    {
                        ExpandItem(item);
                    }
                    else
                    {
                        CollapseItem(item);
                    }
                    _listViewScroll.contentContainer.Focus();
                }
                item.item.onClick?.Invoke(item.item);
            });
        }
    }
}
