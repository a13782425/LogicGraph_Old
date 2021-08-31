using System;
using System.Collections;
using System.Collections.Generic;
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

        [SerializeReference]
        private List<BaseLogicNode> _logicNodeList = new List<BaseLogicNode>();
        /// <summary>
        /// 逻辑图节点
        /// </summary>
        public List<BaseLogicNode> LogicNodeList => _logicNodeList;

        /// <summary>
        /// 逻辑图开始节点
        /// 一切罪恶的开始
        /// </summary>
        [SerializeReference]
        public BaseLogicNode StartNode;

        public BaseLogicGraph()
        {
            _onlyId = Guid.NewGuid().ToString();
        }
    }
}