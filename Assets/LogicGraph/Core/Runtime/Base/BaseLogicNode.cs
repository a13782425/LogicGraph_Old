using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    /// <summary>
    /// 逻辑节点基类
    /// </summary>
    [Serializable]
    public abstract class BaseLogicNode
    {
        [SerializeField]
        public string Title;
#if UNITY_EDITOR
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
        /// <summary>
        /// 是否上锁
        /// </summary>
        [SerializeField]
        public bool IsLock = false;

        /// <summary>
        /// 节点描述
        /// </summary>
        [SerializeField]
        private string _describe = "";
        /// <summary>
        /// 节点描述
        /// </summary>
        public virtual string Describe { get => _describe; set => _describe = value; }

#endif
        [SerializeField]
        private string _onlyId = "";
        public string OnlyId => _onlyId;

        [SerializeReference]
        private List<BaseLogicNode> _childs = new List<BaseLogicNode>();
        public List<BaseLogicNode> Childs => _childs;

        [NonSerialized]
        protected BaseLogicGraph logicGraph;

        private bool _isComplete = false;
        /// <summary>
        /// 是否执行完毕
        /// </summary>
        public bool IsComplete { get => _isComplete; protected set => _isComplete = value; }

        private bool _isSkip = false;
        /// <summary>
        /// 是否跳过子节点
        /// </summary>
        public bool IsSkip { get => _isSkip; protected set => _isSkip = value; }

        public BaseLogicNode()
        {
            Title = this.GetType().Name;
            _onlyId = Guid.NewGuid().ToString();
        }

        public bool Initialize(BaseLogicGraph graph)
        {
            this.logicGraph = graph;
            OnEnable();
            return true;
        }

        /// <summary>
        /// 获取子节点
        /// </summary>
        /// <returns></returns>
        public virtual List<BaseLogicNode> GetChild() => Childs;
        /// <summary>
        /// 节点初始化的时候调用
        /// </summary>
        protected virtual bool OnEnable() => true;
        /// <summary>
        /// 节点初始化的时候执行调用
        /// </summary>
        public virtual bool OnExecute() { IsComplete = true; return true; }
        /// <summary>
        /// 节点的Update,在OnExecute之后每帧都执行
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual bool OnUpdate(float deltaTime) => true;

        /// <summary>
        /// 节点停止调用
        /// 只有正在执行的节点才会被调用
        /// </summary>
        /// <returns></returns>
        public virtual bool OnStop() => true;

        /// <summary>
        /// 节点退出
        /// 当逻辑图完成时候
        /// 无论逻辑图是否正常退出全部节点都会执行
        /// </summary>
        /// <returns></returns>
        public virtual bool OnExit() => true;
    }

}