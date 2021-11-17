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

        private static string _configPath;
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static string ConfigPath => _configPath;

        //private static List<Assembly> _includeAssemblies = new List<Assembly>();

        ///// <summary>
        ///// 需要检索的程序集
        ///// </summary>
        //public static List<Assembly> IncludeAssemblies => _includeAssemblies;

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

        private ILogicConfig _logicConfig = null;

        public ILogicConfig LogicConfig => _logicConfig;

        private static void getCacheData()
        {
            ILogicConfig logicConfig = checkConfig();
            string filePath = Path.Combine(ConfigPath, "LGConfig.asset");
            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
            _instance = AssetDatabase.LoadAssetAtPath<LGCacheData>(filePath);
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<LGCacheData>();
                AssetDatabase.CreateAsset(_instance, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            _instance._logicConfig = logicConfig;
        }

        private static ILogicConfig checkConfig()
        {
            //IncludeAssemblies.Add(typeof(BaseLogicNode).Assembly);
            //IncludeAssemblies.Add(typeof(BaseNodeView).Assembly);
            string[] grids = AssetDatabase.FindAssets(typeof(LGCacheData).Name);
            if (grids.Length < 1)
            {
                throw new System.Exception("没有找到LogicEditorConfig文件所在地");
            }
            string path = AssetDatabase.GUIDToAssetPath(grids[0]);
            _configPath = Path.GetDirectoryName(path);

            var types = TypeCache.GetTypesDerivedFrom<ILogicConfig>();
            if (types.Count > 0)
            {
                Type type = types[0];
                ILogicConfig config = Activator.CreateInstance(type) as ILogicConfig;
                try
                {
                    string str = config.CONFIG_PATH;
                    //List<Assembly> assemblies = config.INCLUDE_ASSEMBLIES;
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        _configPath = str;
                    }
                    //if (assemblies != null)
                    //{
                    //    foreach (var assembly in assemblies)
                    //    {
                    //        if (!IncludeAssemblies.Contains(assembly))
                    //        {
                    //            IncludeAssemblies.Add(assembly);
                    //        }
                    //    }
                    //}
                    return config;
                }
                catch (Exception)
                {
                    Debug.LogError("配置文件异常,请检查配置文件");
                }
            }
            return null;
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
        /// 最后一次Format位置
        /// </summary>
        public string LastFormatPath;
        /// <summary>
        /// 变量面板信息
        /// </summary>
        [SerializeField]
        private LGPanelCache _variableCache;

        public LGPanelCache VariableCache
        {
            get
            {
                if (_variableCache == null)
                {
                    _variableCache = new LGPanelCache();
                    _variableCache.Pos = new Vector2(0, 20);
                    _variableCache.Size = new Vector2(180, 320);
                }
                return _variableCache;
            }
        }
        /// <summary>
        /// 组列表面板信息
        /// </summary>
        [SerializeField]
        private LGPanelCache _groupListCache;

        public LGPanelCache GroupListCache
        {
            get
            {
                if (_groupListCache == null)
                {
                    _groupListCache = new LGPanelCache();
                    _groupListCache.Pos = new Vector2(0, 20);
                    _groupListCache.Size = new Vector2(480, 120);
                }
                return _groupListCache;
            }
        }
        /// <summary>
        /// 方便查找缓存
        /// Key:OnlyId
        /// </summary>
        [NonSerialized]
        public Dictionary<string, BaseNodeView> NodeDic = new Dictionary<string, BaseNodeView>();
        /// <summary>
        /// 逻辑图信息
        /// </summary>
        [NonSerialized]
        public BaseLogicGraph Graph;

        /// <summary>
        /// 逻辑图面板
        /// </summary>
        [NonSerialized]
        public LogicGraphView View;

        public BaseNodeView GetNodeView(BaseLogicNode item) => NodeDic.ContainsKey(item.OnlyId) ? NodeDic[item.OnlyId] : null;
        public BaseNodeView GetNodeView(string onlyId) => NodeDic.ContainsKey(onlyId) ? NodeDic[onlyId] : null;
        public LGInfoCache()
        {
        }

        public LGInfoCache(BaseLogicGraph graph)
        {
            Graph = graph;
            OnlyId = graph.OnlyId;
            GraphClassName = graph.GetType().FullName;
            ResetPanelCache();
        }

        public void ResetPanelCache()
        {
            _variableCache = new LGPanelCache();
            _variableCache.Pos = new Vector2(0, 20);
            _variableCache.Size = new Vector2(180, 320);
            _groupListCache = new LGPanelCache();
            _groupListCache.Pos = new Vector2(0, 20);
            _groupListCache.Size = new Vector2(480, 120);
        }
    }

    /// <summary>
    /// 逻辑图面板信息缓存
    /// </summary>
    [Serializable]
    public class LGPanelCache
    {
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow = false;
        /// <summary>
        /// 当前图坐标
        /// </summary>
        public Vector2 Pos = Vector2.zero;
        /// <summary>
        /// 当前图的缩放
        /// </summary>
        public Vector2 Size = Vector2.one;
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
        public List<LNEditorCache> Nodes = new List<LNEditorCache>();

        /// <summary>
        /// 逻辑图对应的组
        /// </summary>
        public List<LGroupEditorCache> Groups = new List<LGroupEditorCache>();

        /// <summary>
        /// 当前逻辑图对应的默认节点
        /// </summary>
        public List<LNEditorCache> DefaultNodes = new List<LNEditorCache>();

        /// <summary>
        /// 当前逻辑图适用的格式化
        /// </summary>
        public List<LFEditorCache> Formats = new List<LFEditorCache>();

        private Type _curType = null;
        /// <summary>
        /// 获取逻辑图的类型
        /// </summary>
        /// <returns></returns>
        public Type GetGraphType()
        {
            if (_curType == null)
            {
                _curType = TypeCache.GetTypesDerivedFrom<BaseLogicGraph>().FirstOrDefault(a => a.FullName == GraphClassName);
            }
            return _curType;
        }

        /// <summary>
        /// 根据节点类型获取对应的编辑缓存
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public LNEditorCache GetEditorNode(Type nodeType) => Nodes.FirstOrDefault(a => a.GetNodeType() == nodeType);
        /// <summary>
        /// 根据节点类型获取对应的编辑缓存
        /// </summary>
        /// <param name="nodeFullName"></param>
        /// <returns></returns>
        public LNEditorCache GetEditorNode(string nodeFullName) => Nodes.FirstOrDefault(a => a.NodeClassName == nodeFullName);

        public void AddGroupTemplate(LGroupEditorCache groupEditorCache)
        {
            Groups.Add(groupEditorCache);
            onGroupChanged?.Invoke();
        }
        public void DelGroupTemplate(LGroupEditorCache groupEditorCache)
        {
            Groups.Remove(groupEditorCache);
            onGroupChanged?.Invoke();
        }
        public event Action onGroupChanged;

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
        /// 节点需要的端口
        /// </summary>
        public PortEnum PortType;

        /// <summary>
        /// 是否刷新过
        /// </summary>
        [HideInInspector]
        public bool IsRefresh;
        /// <summary>
        /// 是否显示
        /// </summary>
        [HideInInspector]
        public bool IsShow = true;
        /// <summary>
        /// 使用次数,用于做使用计数
        /// </summary>
        [HideInInspector]
        public int UseCount = int.MinValue;

        private Type _curType = null;
        /// <summary>
        /// 获取逻辑图的类型
        /// </summary>
        /// <returns></returns>
        public Type GetNodeType()
        {
            if (_curType == null)
            {
                _curType = TypeCache.GetTypesDerivedFrom<BaseLogicNode>().FirstOrDefault(a => a.FullName == NodeClassName);
            }
            return _curType;
        }

        private Type _curViewType = null;
        /// <summary>
        /// 获取节点视图类型
        /// </summary>
        /// <returns></returns>
        public Type GetViewType()
        {
            if (_curViewType == null)
            {
                _curViewType = TypeCache.GetTypesDerivedFrom<BaseNodeView>().FirstOrDefault(a => a.FullName == NodeViewClassName);
            }
            return _curViewType;
        }

        public void AddUseCount()
        {
            if (UseCount == int.MaxValue)
            {
                UseCount = int.MaxValue;
                return;
            }
            UseCount += 10;
        }
        public void SubUseCount()
        {
            if (UseCount == int.MinValue)
            {
                UseCount = int.MinValue;
                return;
            }
            UseCount -= 1;
        }
    }

    /// <summary>
    /// 逻辑图分组缓存
    /// </summary>
    [Serializable]
    public sealed class LGroupEditorCache
    {

        /// <summary>
        /// 分组名称
        /// </summary>
        public string Name = "";

        /// <summary>
        /// 分组节点缓存
        /// </summary>
        public List<LGroupNEditorCache> Nodes = new List<LGroupNEditorCache>();

        /// <summary>
        /// 可以修改
        /// </summary>
        public bool CanRename = true;
        /// <summary>
        /// 可以删除
        /// </summary>
        public bool CanDel = true;
    }

    /// <summary>
    /// 分组节点缓存
    /// </summary>
    [Serializable]
    public sealed class LGroupNEditorCache
    {
        public int Id;
        public string NodeClassFullName;
        public Vector2 Pos;
        public List<int> Childs = new List<int>();
        [NonSerialized]
        public string OnlyId;
        [NonSerialized]
        public BaseLogicNode Node;
    }

    /// <summary>
    /// 逻辑图格式化
    /// </summary>
    [Serializable]
    public sealed class LFEditorCache
    {
        /// <summary>
        /// 格式化名
        /// </summary>
        public string FormatName;
        /// <summary>
        /// 格式化类型名全称,含命名空间
        /// </summary>
        public string FormatClassName;
        /// <summary>
        /// 格式化后缀
        /// </summary>
        public string Extension;
        private Type _curType = null;
        public Type GetFormatType()
        {
            if (_curType == null)
            {
                _curType = typeof(ILogicFormat).Assembly.GetType(FormatClassName);
            }
            return _curType;
        }
    }
}
