using Logic.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[LogicNode(typeof(DebugNode), "ϵͳ/��ӡ��־", IncludeGraphs = new Type[] { typeof(DefaultLogicGraph) })]
public class DebugNodeView : BaseNodeView<DebugNode>
{
    public override void ShowUI()
    {
        this.ShowUI("log", node.log, "��־:");
    }
}
