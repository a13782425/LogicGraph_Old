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
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
        [SerializeField]
        private string _onlyId = "";
        public string OnlyId => _onlyId;

        [SerializeReference]
        private List<BaseLogicNode> _childs = new List<BaseLogicNode>();
        public List<BaseLogicNode> Childs => _childs;

        public BaseLogicNode()
        {
            Title = this.GetType().Name;
            _onlyId = Guid.NewGuid().ToString();
        }
    }

}