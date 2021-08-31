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
    /// <summary>
    /// 逻辑图缓存数据
    /// 会很庞大很庞大
    /// </summary>
    [Serializable]
    public sealed class LGCacheData : ScriptableObject
    {

        #region 单例
        private static LGCacheData _instance;
        public static LGCacheData Instance
        {
            get
            {
                if (_instance == null)
                {
                    getCacheData();
                }
                return _instance;
            }
        }

        #endregion

        [SerializeField]
        private List<LGInfoCache> _lgInfoList = new List<LGInfoCache>();
        /// <summary>
        /// 逻辑图信息缓存
        /// </summary>
        public List<LGInfoCache> LGInfoList => _lgInfoList;

        [SerializeField]
        private List<LGEditorCache> _lgEditorList = new List<LGEditorCache>();
        /// <summary>
        /// 逻辑图编辑器信息缓存
        /// </summary>
        public List<LGEditorCache> LGEditorList => _lgEditorList;

        private static void getCacheData()
        {
            string[] grids = AssetDatabase.FindAssets(typeof(LGCacheData).Name);
            if (grids.Length < 1)
            {
                throw new System.Exception("没有找到LogicEditorConfig文件所在地");
            }
            string path = AssetDatabase.GUIDToAssetPath(grids[0]);
            string filePath = Path.Combine(Path.GetDirectoryName(path), "LGConfig.asset");
            _instance = AssetDatabase.LoadAssetAtPath<LGCacheData>(filePath);
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<LGCacheData>();
                AssetDatabase.CreateAsset(_instance, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public void Save()
        {
            if (_instance != null)
            {
                EditorUtility.SetDirty(Instance);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }

    /// <summary>
    /// 逻辑图信息缓存
    /// 存放当前逻辑图信息
    /// </summary>
    [Serializable]
    public sealed class LGInfoCache
    {
        public string OnlyId;
        /// <summary>
        /// 
        /// </summary>
        public string Title;
        /// <summary>
        /// 逻辑图名
        /// </summary>
        public string LogicName;
        /// <summary>
        /// 图类型名全称,含命名空间
        /// </summary>
        public string GraphClassName;
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName;
        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath;
        /// <summary>
        /// 当前逻辑图节点缓存
        /// </summary>
        public List<LNInfoCache> Nodes = new List<LNInfoCache>();

        /// <summary>
        /// 逻辑图信息
        /// </summary>
        [NonSerialized]
        public BaseLogicGraph Graph;
        /// <summary>
        /// 逻辑图面板
        /// </summary>
        [NonSerialized]
        public LGPanelView View;

        public LGInfoCache()
        {

        }

        public LGInfoCache(BaseLogicGraph graph)
        {
            Graph = graph;
            OnlyId = graph.OnlyId;
            GraphClassName = graph.GetType().FullName;
        }
    }

    /// <summary>
    /// 逻辑节点缓存
    /// 存放当前逻辑节点的信息
    /// </summary>
    [Serializable]
    public sealed class LNInfoCache
    {

        /// <summary>
        /// 唯一Id
        /// </summary>
        public string OnlyId;
        public string Title;
        public Vector2 Pos;
        [NonSerialized]
        public BaseLogicNode Node;
        public LNInfoCache(BaseLogicNode node)
        {
            Node = node;
            OnlyId = node.OnlyId;
        }

        public LNInfoCache()
        {

        }
    }

    /// <summary>
    /// 逻辑图编辑器信息缓存
    /// 用来记录逻辑图的编辑器数据
    /// </summary>
    [Serializable]
    public sealed class LGEditorCache
    {
        /// <summary>
        /// 图名
        /// </summary>
        public string GraphName;
        /// <summary>
        /// 图类型名全称,含命名空间
        /// </summary>
        public string GraphClassName;
        /// <summary>
        /// 是否刷新过
        /// </summary>
        public bool IsRefresh;
        /// <summary>
        /// 当前逻辑图所对应的节点
        /// </summary>
        public List<LNEditorCache> LogicNodes = new List<LNEditorCache>();



        private Type _curType = null;
        /// <summary>
        /// 获取逻辑图的类型
        /// </summary>
        /// <returns></returns>
        public Type GetGraphType()
        {
            if (_curType == null)
            {
                _curType = typeof(BaseLogicNode).Assembly.GetType(GraphClassName);
                if (_curType == null)
                {
                    _curType = typeof(BaseLogicNodeView).Assembly.GetType(GraphClassName);
                }
            }
            return _curType;
        }
    }


    /// <summary>
    /// 逻辑节点编辑器缓存
    /// 存放逻辑节点的编辑器信息
    /// </summary>
    [Serializable]
    public sealed class LNEditorCache
    {
        /// <summary>
        /// 节点名
        /// </summary>
        public string NodeName;
        /// <summary>
        /// 节点全名
        /// </summary>
        public string NodeFullName;
        /// <summary>
        /// 节点层级
        /// </summary>
        public string[] NodeLayers;
        /// <summary>
        /// 节点类型全称,含命名空间
        /// </summary>
        public string NodeClassName;
        /// <summary>
        /// 节点视图类型全称,含命名空间
        /// </summary>
        public string NodeViewClassName;
        /// <summary>
        /// 是否刷新过
        /// </summary>
        public bool IsRefresh;
        /// <summary>
        /// 使用次数,用于做使用计数
        /// </summary>
        public int UseCount = int.MinValue;
    }

}
