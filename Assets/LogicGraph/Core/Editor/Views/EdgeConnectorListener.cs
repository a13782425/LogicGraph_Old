using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Logic.Editor
{
    public sealed class EdgeConnectorListener : IEdgeConnectorListener
    {
        public readonly LogicGraphView graphView;
        public EdgeConnectorListener(LogicGraphView graphView)
        {
            this.graphView = graphView;
        }
        public void OnDrop(GraphView graphView, Edge edge)
        {
            var edgeView = edge as EdgeView;

            if (edgeView?.input == null || edgeView?.output == null)
                return;
            //bool wasOnTheSamePort = false;
            graphView.AddElement(edgeView);
            PortView output = edgeView.output as PortView;
            PortView input = edgeView.input as PortView;
            output.Owner.AddChild(input.Owner.Target);
            input.Connect(edgeView);
            output.Connect(edgeView);
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
        }
    }
}
