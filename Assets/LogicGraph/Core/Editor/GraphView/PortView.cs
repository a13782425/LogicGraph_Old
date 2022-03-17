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
        /// <summary>
        /// 是否是默认端口
        /// 即In和Out端口
        /// </summary>
        public bool IsDefault { get; private set; }
        public string OnlyId { get; private set; }
        public BaseNodeView Owner { get; private set; }

        //public NodePortElement PortElement { get; private set; }

        private bool _isCube = false;
        public PortView(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
        }
        //public static PortView CreatePort(Direction direction, Capacity capacity, PortShapeEnum shape, EdgeConnectorListener edgeConnectorListener, NodePortElement element)
        //{
        //    var port = new PortView(Orientation.Horizontal, direction, capacity, null);
        //    port.PortElement = element;
        //    port.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
        //    port.AddManipulator(port.m_EdgeConnector);
        //    port.styleSheets.Add(LogicUtils.GetPortStyle());
        //    switch (shape)
        //    {
        //        case PortShapeEnum.Cube:
        //            port.AddToClassList(LogicUtils.PORT_CUBE);
        //            port.visualClass = "Port_Cube" + direction;
        //            break;
        //        default:
        //            port.visualClass = "Port_" + direction;
        //            break;
        //    }
        //    return port;
        //}

        public static PortView CreatePort(string labelName, Direction direction, Capacity capacity, bool isCube, EdgeConnectorListener edgeConnectorListener)
        {
            var port = new PortView(Orientation.Horizontal, direction, capacity, null);
            port.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
            port.AddManipulator(port.m_EdgeConnector);
            port.portName = labelName;
            port.styleSheets.Add(LogicUtils.GetPortStyle());
            port._isCube = isCube;

            return port;
        }

        public void Initialize(BaseNodeView nodeView, string onlyId)
        {
            this.OnlyId = onlyId;
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

        /// <summary>
        /// 是否可以链接到
        /// </summary>
        /// <param name="waitLinkPort"></param>
        /// <returns></returns>
        public bool CanLink(NodePort waitLinkPort)
        {
            return true;
            //return Owner.CanLink(this, waitLinkPort);
        }
    }
}
