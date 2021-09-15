using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Logic
{
    public sealed class ParameterNode : BaseLogicNode
    {
        public string paramId = "";

        /// <summary>
        /// 数据
        /// </summary>
        public BaseParameter param { get; private set; }

        public ParameterAccessor accessor;

        protected override bool OnEnable()
        {
            param = logicGraph.GetParamById(paramId);
            return base.OnEnable();
        }
        public object GetValue() => param.Value;
        public void SetValue(object value) => param.Value = value;
    }
    public enum ParameterAccessor
    {
        Get,
        Set
    }
}
