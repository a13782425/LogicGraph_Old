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
        public string Title { get => _title; set => _title = value; }
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
        private List<BaseVariable> _variables = new List<BaseVariable>();

        /// <summary>
        /// 单一逻辑图内部变量,所有节点均可访问
        /// </summary>       
        public List<BaseVariable> Variables => _variables;

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
        /// 获取一个节点
        /// </summary>
        /// <param name="onlyId"></param>
        /// <returns></returns>
        public BaseLogicNode GetNodeById(string onlyId) => Nodes.FirstOrDefault(a => a.OnlyId == onlyId);
        /// <summary>
        /// 获取一个变量
        /// </summary>
        /// <param name="onlyId"></param>
        /// <returns></returns>
        public BaseVariable GetVariableById(string onlyId) => Variables.FirstOrDefault(a => a.OnlyId == onlyId);
        /// <summary>
        /// 获取一个变量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BaseVariable GetVariableByName(string name) => Variables.FirstOrDefault(a => a.Name == name);
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

        //public void OnBeforeSerialize()
        //{
        //    Debug.LogError("OnBeforeSerialize");
        //    Debug.LogError(this.GetType());
        //}

        //public void OnAfterDeserialize()
        //{
        //    Debug.LogError("OnAfterDeserialize");
        //    Debug.LogError(this.OnlyId);
        //}

#endif
    }
}