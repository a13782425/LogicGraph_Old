using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class PortView : Port
    {

        public BaseNodeView Owner { get; private set; }
        private bool _isCube = false;
        public PortView(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
        }
        public static PortView CreatePort(string labelName, Direction direction, bool isCube, EdgeConnectorListener edgeConnectorListener)
        {
            var port = new PortView(Orientation.Horizontal, direction, Capacity.Multi, null);
            port.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
            port.AddManipulator(port.m_EdgeConnector);
            port.portName = labelName;
            port.styleSheets.Add(LogicUtils.GetPortStyle());
            port._isCube = isCube;
          

            return port;
        }
        public void Initialize(BaseNodeView nodeView, string name)
        {
            this.Owner = nodeView;
            if (_isCube)
            {
                this.AddToClassList(LogicUtils.PORT_CUBE);
                visualClass = "Port_Cube" + direction;
            }
            else
            {
                visualClass = "Port_" + direction;
            }
        }

    }
}