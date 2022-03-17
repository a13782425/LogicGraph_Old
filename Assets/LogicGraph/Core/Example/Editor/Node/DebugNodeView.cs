using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logic.Editor;
using UnityEngine.UIElements;
using Logic;
using System;

[LogicNode(typeof(DebugNode), "系统/打印日志", IncludeGraphs = new Type[] { typeof(DefaultLogicGraph) })]
public class DebugNodeView : BaseNodeView<DebugNode>
{

    private NodePort _port;
    public override LogicIconEnum StateIcon => LogicIconEnum.Triangle;
    public override void OnCreate()
    {
        //TitleBackgroundColor = Color.red;
        //ContentBackgroundColor = Color.green;
    }
    public override void ShowUI()
    {
        ShowUI("log");
        ShowUI("abc");
        ShowUI("abc1");
        ShowPort("param001");
        ShowPort("param002");
        ShowPort("param003");
        ShowPort("param004");
        ShowPort("child001");
        ShowPort("child002");
        ShowPort("child003");
        ShowPort("child004");
    }



    //public override void AddVariable(VariableNode paramNode, NodePort curPort, ParamAccessor accessor)
    //{
    //    //if (accessor == ParamAccessor.Get)
    //    //{
    //    //    node.Parameter = paramNode;
    //    //}
    //}
    //public override void DelVariable(VariableNode paramNode, NodePort curPort, ParamAccessor accessor)
    //{
    //    //if (accessor == ParamAccessor.Get)
    //    //{
    //    //    node.Parameter = null;

    //    //}
    //}
    public override void DrawLink()
    {
        base.DrawLink();
        //this.node.Conditions.RemoveAll(a => a == null);
        //foreach (var item in this.node.Conditions)
        //{
        //    var nodeView = graphCache.GetNodeView(item);
        //    DrawLink(nodeView, _port);
        //}
        //if (this.node.Parameter != null && this.node.Parameter.variable != null)
        //{
        //    DrawLink(_port, owner.GetNodeView(this.node.Parameter).OutPut);
        //}
    }

    //public override bool CanLink(NodePort ownerPort, NodePort waitLinkPort)
    //{
    //    if (ownerPort.PortType != PortTypeEnum.Default)
    //    {
    //        if (waitLinkPort.nodeView is VariableNodeView parameter)
    //        {
    //            if (ownerPort.direction == UnityEditor.Experimental.GraphView.Direction.Input)
    //            {
    //                return (parameter.target as VariableNode).variable.Value is float;
    //            }
    //            if (ownerPort.direction == UnityEditor.Experimental.GraphView.Direction.Output)
    //            {
    //                return (parameter.target as VariableNode).variable.Value is Color;
    //            }
    //            return false;
    //        }
    //    }
    //    return base.CanLink(ownerPort, waitLinkPort);
    //}
    //public override bool CanLink(PortView ownerPort, PortView waitLinkPort)
    //{
    //    if (!ownerPort.IsDefault)
    //    {
    //        if (waitLinkPort.Owner is BaseNodeView)
    //        {
    //            return false;
    //        }
    //    }
    //    return true;
    //}
}
