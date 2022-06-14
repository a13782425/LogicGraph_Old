using Logic.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[LogicNode(typeof(DebugNode), "ϵͳ/��ӡ��־", IncludeGraphs = new Type[] { typeof(DefaultLogicGraph) })]
public class DebugNodeView : BaseNodeView<DebugNode>
{
    public override void ShowUI()
    {
        //this.inputContainer.Clear();
        //var portContainer = new VisualElement() { style = { flexDirection = FlexDirection.Column } };
        //portContainer.style.flexDirection = FlexDirection.Row;
        //var portInputView = new PortInputView() { style = { position = Position.Absolute } };
        //portContainer.Add(portInputView);
        //portContainer.Add(ShowPort("In", Logic.PortDirEnum.In));
        //this.inputContainer.Add(portContainer);
        //portInputView.AddToClassList("disabled");
        this.ShowUI("log", node.log, "��־:");
        ShowPort("posVar", "����:");
    }
}
