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
        private string _name;
        [SerializeField]
        private string _onlyId = "";
        public string OnlyId => _onlyId;

        [SerializeReference]
        private List<BaseLogicNode> _childs = new List<BaseLogicNode>();
        public List<BaseLogicNode> Childs => _childs;

        public BaseLogicNode()
        {
            _name = this.GetType().Name;
            _onlyId = Guid.NewGuid().ToString();
        }
    }

}