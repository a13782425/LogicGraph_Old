using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Logic.Editor
{

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
        /// 当前的逻辑图
        /// </summary>
        public BaseLogicGraph Graph;

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
        /// 图类型名全称,含命名空间
        /// </summary>
        public string GraphViewClassName;

        /// <summary>
        /// 当前逻辑图所对应的节点
        /// </summary>
        public List<LNEditorCache> Nodes = new List<LNEditorCache>();

        /// <summary>
        /// 当前逻辑图对应的默认节点
        /// </summary>
        public List<LNEditorCache> DefaultNodes = new List<LNEditorCache>();
        /// <summary>
        /// 当前逻辑图对应的默认节点
        /// </summary>
        public List<string> DefaultNodeFullNames = new List<string>();

        /// <summary>
        /// 当前逻辑图适用的格式化
        /// </summary>
        public List<LFEditorCache> Formats = new List<LFEditorCache>();

        /// <summary>
        /// 逻辑图类型
        /// </summary>
        public Type GraphType { get; set; }

        /// <summary>
        /// 视图类型
        /// </summary>
        public Type ViewType { get; set; }

        /// <summary>
        /// 根据节点类型获取对应的编辑缓存
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public LNEditorCache GetEditorNode(Type nodeType) => Nodes.FirstOrDefault(a => a.NodeType == nodeType);
        /// <summary>
        /// 根据节点类型获取对应的编辑缓存
        /// </summary>
        /// <param name="nodeFullName"></param>
        /// <returns></returns>
        public LNEditorCache GetEditorNode(string nodeFullName) => Nodes.FirstOrDefault(a => a.NodeClassName == nodeFullName);
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
        /// 是否启用
        /// </summary>
        public bool IsEnable = true;
        /// <summary>
        /// 节点需要的端口
        /// </summary>
        public PortDirEnum PortType;

        /// <summary>
        /// 视图类型
        /// </summary>
        public Type NodeType { get; set; }

        /// <summary>
        /// 字段信息
        /// key:字段名 
        /// </summary>
        public Dictionary<string, FieldInfo> FieldInfos = new Dictionary<string, FieldInfo>();
        /// <summary>
        /// 字段类型
        /// key:字段名,value:特性类型
        /// </summary>
        public Dictionary<string, Type> FieldTypes = new Dictionary<string, Type>();

        /// <summary>
        /// 视图类型
        /// </summary>
        public Type ViewType { get; set; }
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
        /// <summary>
        /// 格式化类型
        /// </summary>
        public Type FormatType;
    }
}
