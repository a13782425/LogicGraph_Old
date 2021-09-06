using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logic.Editor;
using UnityEngine.UIElements;

[LogicNode(typeof(DebugNode), "系统/打印日志")]
public class DebugNodeView : BaseNodeView
{
    private DebugNode node;

    public override bool ShowParamPanel => true;

    private PortView _port;
    public override void OnCreate()
    {
        Width = 200;
        node = Target as DebugNode;
    }
    public override void ShowUI()
    {
        var text = GetInputField("日志:", node.log);
        text.RegisterCallback<InputEvent>(onInputEvent);
        this.AddUI(text);
        _port = AddPort("条件", UnityEditor.Experimental.GraphView.Direction.Output, true);
        this.AddUI(_port);
    }

    public override void ShowParamUI()
    {
        var text = GetInputField("日志:", node.log);
        text.RegisterCallback<InputEvent>(onInputEvent);
        this.AddUIToParamPanel(text);

    }

    private void onInputEvent(InputEvent evt)
    {
        node.log = evt.newData;
    }

    public override void AddChild(BaseNodeView child)
    {
        if (child.Target is DebugNode)
        {
            node.Conditions.Add(child.Target);
        }
        else
            base.AddChild(child);
    }

    public override void DrawLink()
    {
        base.DrawLink();
        this.node.Conditions.RemoveAll(a => a == null);
        foreach (var item in this.node.Conditions)
        {
            var nodeView = graphCache.GetNodeView(item);
            DrawLink(nodeView, _port);
        }

    }
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
