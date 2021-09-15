using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Logic
{

    /// <summary>
    /// 逻辑图基类
    /// </summary>
    [Serializable]
    public abstract class BaseLogicGraph : ScriptableObject
    {
        [SerializeField]
        private string _onlyId = "";
        public string OnlyId => _onlyId;
#if UNITY_EDITOR
        [SerializeField]
        private string _title = "";
        public string Title => _title;
#endif
        [SerializeReference]
        private List<BaseLogicNode> _nodes = new List<BaseLogicNode>();

        /// <summary>
        /// 逻辑图节点
        /// </summary>
        public List<BaseLogicNode> Nodes => _nodes;


        [SerializeReference]
        private List<BaseLogicNode> _startNodes = new List<BaseLogicNode>();
        /// <summary>
        /// 逻辑图开始节点
        /// 一切罪恶的开始
        /// </summary>       
        public List<BaseLogicNode> StartNodes => _startNodes;

        [SerializeReference]
        private List<BaseParameter> _params = new List<BaseParameter>();

        /// <summary>
        /// 单一逻辑图内部参数,所有节点均可访问
        /// </summary>       
        public List<BaseParameter> Params => _params;

#if UNITY_EDITOR
        [SerializeField]
        private List<BaseLogicGroup> _groups = new List<BaseLogicGroup>();
        /// <summary>
        /// 逻辑图组
        /// </summary>
        public List<BaseLogicGroup> Groups => _groups;
#endif

        public BaseLogicGraph()
        {
            _onlyId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 获取一个参数
        /// </summary>
        /// <param name="onlyId"></param>
        /// <returns></returns>
        public BaseParameter GetParamById(string onlyId) => Params.FirstOrDefault(a => a.OnlyId == onlyId);
        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init() => Nodes.ForEach(n => n.Initialize(this));


        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void OnUpdate(float deltaTime)
        {

        }


#if UNITY_EDITOR
        public void ResetGuid()
        {
            _onlyId = Guid.NewGuid().ToString();
        }

        public void SetTitle(string title)
        {
            _title = title;
        }
#endif
    }
}