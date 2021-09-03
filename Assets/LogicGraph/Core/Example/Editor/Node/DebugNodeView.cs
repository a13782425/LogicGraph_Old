using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logic.Editor;
using UnityEngine.UIElements;

[LogicNode(typeof(DebugNode), "系统/打印日志")]
public class DebugNodeView : BaseNodeView
{
    private DebugNode node;


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
        var port = AddPort("测试", UnityEditor.Experimental.GraphView.Direction.Input, true);
        this.AddUI(port);
        //this.AddUI(GetPort(UnityEditor.Experimental.GraphView.Direction.Output));
    }

    private void onInputEvent(InputEvent evt)
    {
        node.log = evt.newData;
    }
}
