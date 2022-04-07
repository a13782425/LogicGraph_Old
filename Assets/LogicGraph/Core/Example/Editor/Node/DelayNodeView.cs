using Logic.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[LogicNode(typeof(DelayNode), "系统/延时", IncludeGraphs = new Type[] { typeof(DefaultLogicGraph) })]
public class DelayNodeView : BaseNodeView<DelayNode>
{
    public override LogicIconEnum StateIcon => LogicIconEnum.Gear;

    public override void ShowUI()
    {
        ShowPort("delayVar", "延时时间:");
    }
}
