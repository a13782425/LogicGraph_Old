using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class LGParameterPropertyView : VisualElement
    {
        private LogicGraphView graphView;

        public BaseParameter param { get; private set; }

        public LGParameterPropertyView(LogicGraphView graphView, BaseParameter param)
        {
            this.graphView = graphView;
            this.param = param;



            Add(param.GetUI());
        }
    }
}
