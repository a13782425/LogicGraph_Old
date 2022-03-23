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
        //public string varId = "";
        public string varName = "";

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
                    _variable = logicGraph.GetVariableByName(varName);
                }
                return _variable;
            }
        }

#if UNITY_EDITOR
        public override string Describe
        {
            get => base.Describe; set
            {
                base.Describe = value;
                if (variable != null)
                {
                    variable.Describe = value;
                }
            }
        }
#endif

        protected override bool OnEnable()
        {
            _variable = logicGraph.GetVariableByName(varName);
            return base.OnEnable();
        }
        public object GetValue() => variable.Value;
        public void SetValue(object value) => variable.Value = value;
    }
}
