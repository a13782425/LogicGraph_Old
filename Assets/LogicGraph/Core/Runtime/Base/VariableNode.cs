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

        private BaseVariable _variable = null;
        /// <summary>
        /// 数据
        /// </summary>
        public BaseVariable variable
        {
            get
            {
                if (_variable == null)
                {
                    _variable = logicGraph.GetVariableById(varId);
                }
                return _variable;
            }
        }

        protected override bool OnEnable()
        {
            _variable = logicGraph.GetVariableById(varId);
            return base.OnEnable();
        }
        public object GetValue() => variable.Value;
        public void SetValue(object value) => variable.Value = value;
    }
}
