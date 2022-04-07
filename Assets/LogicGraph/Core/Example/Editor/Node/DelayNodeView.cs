using Logic.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[LogicNode(typeof(DelayNode), "ϵͳ/��ʱ", IncludeGraphs = new Type[] { typeof(DefaultLogicGraph) })]
public class DelayNodeView : BaseNodeView<DelayNode>
{
    public override LogicIconEnum StateIcon => LogicIconEnum.Gear;

    public override void ShowUI()
    {
        ShowPort("delayVar", "��ʱʱ��:");
    }
}
