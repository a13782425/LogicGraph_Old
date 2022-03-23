using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Editor
{
    /// <summary>
    /// 节点连线缓存
    /// </summary>
    public sealed class NodeEdgeData
    {
        public string InputNodeId { get; set; }
        public string OutputNodeId { get; set; }
        public string InputFieldName { get; set; }
        public string OutputFieldName { get; set; }
        public bool InputDefult { get; set; }
        public bool OutputDefult { get; set; }

        public void DrawLink(BaseGraphView graph)
        {

            BaseNodeView inputNodeView = graph.GetNodeView(InputNodeId);
            BaseNodeView outputNodeView = graph.GetNodeView(OutputNodeId);
            if (inputNodeView != null && outputNodeView != null)
            {
                NodePort inPort = default;
                NodePort outPort = default;
                if (InputDefult)
                    inPort = inputNodeView.Input;
                else
                    inPort = inputNodeView.GetPort(InputFieldName);
                if (OutputDefult)
                    outPort = outputNodeView.OutPut;
                else
                    outPort = outputNodeView.GetPort(OutputFieldName);
                outPort.AddPort(inPort);
                outPort.DrawLink(inPort);
            }

        }

        public void Init(NodePort input, NodePort output)
        {
            this.OutputNodeId = output.nodeView.target.OnlyId;
            this.InputNodeId = input.nodeView.target.OnlyId;
            if (output.PortType == PortTypeEnum.Default)
            {
                this.OutputDefult = true;
                this.OutputFieldName = "";
            }
            else
            {
                this.OutputDefult = false;
                this.OutputFieldName = output.fieldInfo.Name;
            }

            if (input.PortType == PortTypeEnum.Default)
            {
                this.InputDefult = true;
                this.InputFieldName = "";
            }
            else
            {
                this.InputDefult = false;
                this.InputFieldName = input.fieldInfo.Name;
            }
        }
    }

}
