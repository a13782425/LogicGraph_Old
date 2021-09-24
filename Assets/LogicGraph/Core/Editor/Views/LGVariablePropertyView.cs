using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class LGVariablePropertyView : VisualElement
    {
        private LogicGraphView graphView;

        public BaseVariable param { get; private set; }

        public LGVariablePropertyView(LogicGraphView graphView, BaseVariable param)
        {
            this.graphView = graphView;
            this.param = param;
            Add(param.GetUI());
        }
    }
}
