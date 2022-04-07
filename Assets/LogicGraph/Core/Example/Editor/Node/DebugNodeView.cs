using Logic.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[LogicNode(typeof(DebugNode), "系统/打印日志", IncludeGraphs = new Type[] { typeof(DefaultLogicGraph) })]
public class DebugNodeView : BaseNodeView<DebugNode>
{
    public override void ShowUI()
    {
        this.ShowUI("log", node.log, "日志:");
    }
}
