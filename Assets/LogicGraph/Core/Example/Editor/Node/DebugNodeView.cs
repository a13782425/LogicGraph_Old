using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logic.Editor;
using UnityEngine.UIElements;
using Logic;

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
        TitleBackgroundColor = Color.red;
        ContentBackgroundColor = Color.green;
    }
    public override void ShowUI()
    {
        var text = GetInputField("日志:", node.log);
        text.RegisterCallback<InputEvent>(onInputEvent);
        this.AddUI(text);
        //_port = AddPort("条件", UnityEditor.Experimental.GraphView.Direction.Output, true);
        //this.AddUI(_port);
        _port = AddPort("参数", UnityEditor.Experimental.GraphView.Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Single, isCube: true);
        this.AddUI(_port);
        this.AddUI(AddPort("测试", UnityEditor.Experimental.GraphView.Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Single, isCube: true));
        this.AddUI(GetInputField("aa", 0));
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

    public override void AddChild(BaseLogicNode child)
    {
        if (child is DebugNode)
        {
            node.Conditions.Add(child);
        }
        else
            base.AddChild(child);
    }
    public override void AddVariable(VariableNode paramNode, PortView curPort, ParamAccessor accessor)
    {
        if (accessor == ParamAccessor.Get)
        {
            node.Parameter = paramNode;
        }
    }
    public override void DelVariable(VariableNode paramNode, PortView curPort, ParamAccessor accessor)
    {
        if (accessor == ParamAccessor.Get)
        {
            node.Parameter = null;
        }
    }
    public override void DrawLink()
    {
        base.DrawLink();
        //this.node.Conditions.RemoveAll(a => a == null);
        //foreach (var item in this.node.Conditions)
        //{
        //    var nodeView = graphCache.GetNodeView(item);
        //    DrawLink(nodeView, _port);
        //}
        if (this.node.Parameter.variable != null)
        {
            DrawLink(_port, graphCache.GetNodeView(this.node.Parameter).OutPut);
        }
    }

    public override bool CanLink(PortView ownerPort, PortView waitLinkPort)
    {
        if (!ownerPort.IsDefault)
        {
            if (waitLinkPort.Owner is VariableNodeView parameter)
            {
                if (ownerPort.direction == UnityEditor.Experimental.GraphView.Direction.Input)
                {
                    return (parameter.Target as VariableNode).variable.Value is float;
                }
                if (ownerPort.direction == UnityEditor.Experimental.GraphView.Direction.Output)
                {
                    return (parameter.Target as VariableNode).variable.Value is Color;
                }
                return false;
            }
        }
        return base.CanLink(ownerPort, waitLinkPort);
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
