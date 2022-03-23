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
        ShowUI("log", node.log);
        ShowUI("abc", node.abc);
        ShowUI("abc1", node.abc1);
        ShowPort("param001").onAfterAddPort += DebugNodeView_onAfterAddPort;
        ShowUI("aaa", node.aaa);
        ShowPort("param002");
        ShowUI("bbb", node.bbb);
        ShowPort("param003").onAfterAddPort += DebugNodeView_onAfterAddPort;
        ShowUI("ccc", node.ccc);
        ShowPort("param004");
        ShowUI("ddd", node.ddd);
        ShowPort("child001");
        ShowUI("eee", node.eee);
        ShowPort("child002");
        ShowUI("fff", node.fff);
        ShowPort("child003");
        ShowUI("ggg", node.ggg);
        ShowPort("child004");
        ShowUI("hhh", node.hhh);
    }

    private void DebugNodeView_onAfterAddPort(NodePort arg1, NodePort arg2)
    {
        Debug.LogError("add");
    }
}
