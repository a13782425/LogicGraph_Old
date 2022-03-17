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
        ShowUI("aaa","cc");
        ShowUI("bbb");
        ShowUI("ccc");
        ShowUI("ddd");
        ShowUI("eee");
        ShowUI("fff");
    }

}
