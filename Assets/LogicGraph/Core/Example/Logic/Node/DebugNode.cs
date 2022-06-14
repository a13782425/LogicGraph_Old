using Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugNode : BaseLogicNode
{
    public string log = "";

    public Vector2 pos ;
    [NodePort(PortShapeEnum.Cube, LinkName = "pos", VarTypes = new Type[] { typeof(Color) })]
    [SerializeReference]
    public VariableNode posVar;
    public override bool OnExecute()
    {
        Debug.LogError(log);
        IsComplete = true;
        return base.OnExecute();
    }
}
