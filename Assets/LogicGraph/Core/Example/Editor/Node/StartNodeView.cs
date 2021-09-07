using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Editor
{
    [LogicNode(typeof(StartNode), "系统/开始", typeof(DefaultLogicGraph), PortType = PortEnum.Out)]
    public sealed class StartNodeView : BaseNodeView
    {
    }
}