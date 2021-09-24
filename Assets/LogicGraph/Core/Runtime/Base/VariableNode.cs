using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Logic
{
    [Serializable]
    public sealed class VariableNode : BaseLogicNode
    {
        public string varId = "";

        /// <summary>
        /// 数据
        /// </summary>
        public BaseVariable variable { get; private set; }

        protected override bool OnEnable()
        {
            variable = logicGraph.GetVariableById(varId);
            return base.OnEnable();
        }
        public object GetValue() => variable.Value;
        public void SetValue(object value) => variable.Value = value;
    }
}
