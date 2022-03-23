using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
//using UnityEditor.IMGUI.Controls;
using static Logic.Editor.LogicUtils;

namespace Logic.Editor
{
    public class GraphListPanel : VisualElement
    {
        internal class SampleTreeItem : TreeViewItem
        {
            private static int m_NextId = 0;

            public SampleTreeItem(string name, List<ITreeViewItem> children = null)
                : base(m_NextId++, children)
            {
                this.name = name;
            }

            public static void ResetNextId()
            {
                m_NextId = 0;
            }
        }

        private const string GRAPH_LIST_STYLE = "GraphListPanel.uss";

        private VisualElement panelBg;
        private VisualElement listBg;
        private VisualElement contentBg;

        private TreeView _treeView;

        public LGWindow Window { get; private set; }

        public GraphListPanel(LGWindow window)
        {
            Window = window;
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_STYLE_PATH, GRAPH_LIST_STYLE)));
            panelBg = new VisualElement();
            panelBg.name = "panelBg";
            this.Add(panelBg);
            panelBg.RegisterCallback<ClickEvent>(m_onPanelClick);
            listBg = new VisualElement();
            listBg.name = "listBg";
            listBg.RegisterCallback<ClickEvent>(m_onListPanelClick);
            panelBg.Add(listBg);
            contentBg = new VisualElement();
            contentBg.name = "contentBg";
            listBg.Add(contentBg);


            List<ITreeViewItem> list = new List<ITreeViewItem>();
            SampleTreeItem createTree = new SampleTreeItem("创建逻辑图");
            createTree.userData = "root";
            createTree.AddChild(new SampleTreeItem("默认逻辑图"));
            list.Add(createTree);
            SampleTreeItem openTree = new SampleTreeItem("打开逻辑图");
            openTree.userData = "root";
            list.Add(openTree);

            _treeView = new TreeView
            {
                name = "tree-view"
            };
            Func<VisualElement> makeItem = () => new Label();


            contentBg.Add(_treeView);
            _treeView.onMakeItem = makeItem;
            _treeView.onBindItem = m_bindItem;
            _treeView.items = new List<ITreeViewItem>();
        }

        private void RefreshData()
        {
            List<ITreeViewItem> list = _treeView.items;
            list.Clear();
            SampleTreeItem createTree = new SampleTreeItem("创建逻辑图");
            createTree.userData = "root";
            foreach (var item in LogicProvider.LGEditorList)
            {
                SampleTreeItem tree = new SampleTreeItem(item.GraphName);
                tree.userData = item;
                tree.onClick = m_createLogic;
                createTree.AddChild(tree);
            }
            SampleTreeItem openTree = new SampleTreeItem("打开逻辑图");
            openTree.userData = "root";

            foreach (var item in LogicProvider.LGEditorList)
            {
                SampleTreeItem childTree = new SampleTreeItem(item.GraphName);
                childTree.userData = "child";
                openTree.AddChild(childTree);

                foreach (var info in LogicProvider.LGInfoList)
                {
                    if (info.GraphClassName == item.GraphClassName)
                    {
                        SampleTreeItem temp = new SampleTreeItem(info.LogicName);
                        temp.userData = info;
                        temp.onClick = m_openLogic;
                        childTree.AddChild(temp);
                    }
                }

            }
            list.Add(createTree);
            list.Add(openTree);
            _treeView.Refresh();
        }

        private void m_openLogic(ITreeViewItem obj)
        {
            LGInfoCache graphCache = obj.userData as LGInfoCache;
            Window.GraphOnlyId = graphCache.OnlyId;
            this.Hide();
        }

        private void m_createLogic(ITreeViewItem obj)
        {
            LGEditorCache configData = obj.userData as LGEditorCache;
            string path = EditorUtility.SaveFilePanel("创建逻辑图", Application.dataPath, "LogicGraph", "asset");
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("错误", "路径为空", "确定");
                return;
            }
            if (File.Exists(path))
            {
                EditorUtility.DisplayDialog("错误", "创建文件已存在", "确定");
                return;
            }
            string file = Path.GetFileNameWithoutExtension(path);
            BaseLogicGraph graph = ScriptableObject.CreateInstance(configData.GraphType) as BaseLogicGraph;
            BaseGraphView graphView = Activator.CreateInstance(configData.ViewType) as BaseGraphView;
            graph.name = file;
            if (graphView.DefaultVars != null)
            {
                foreach (var item in graphView.DefaultVars)
                {
                    item.CanRename = false;
                    item.CanDel = false;
                    graph.Variables.Add(item);
                }
            }
            graphView = null;
            path = path.Replace(Application.dataPath, "Assets");
            graph.Title = file;
            AssetDatabase.CreateAsset(graph, path);
            AssetDatabase.Refresh();
            Window.GraphOnlyId = graph.OnlyId;
            this.Hide();
        }

        private void m_bindItem(VisualElement element, ITreeViewItem item)
        {
            Label label = element.Q<Label>();
            if (label != null)
            {
                label.text = item.name;
            }
        }

        private void m_onListPanelClick(ClickEvent evt)
        {
            evt.StopPropagation();
        }

        private void m_onPanelClick(ClickEvent evt)
        {
            this.Hide();
            evt.StopPropagation();
        }

        public void Show()
        {
            RefreshData();
            this.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            this.style.display = DisplayStyle.None;
        }
    }
}
