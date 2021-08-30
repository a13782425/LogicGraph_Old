using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    /// <summary>
    /// 逻辑节点基类
    /// </summary>
    public abstract class BaseLogicNode
    {
        [SerializeField]
        private string _onlyId = "";
        public string OnlyId => _onlyId;
        public BaseLogicNode()
        {
            _onlyId = Guid.NewGuid().ToString();
        }
    }

}