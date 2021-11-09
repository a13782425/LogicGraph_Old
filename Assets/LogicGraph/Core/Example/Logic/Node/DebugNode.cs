using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logic;
using System;

public class DebugNode : BaseLogicNode
{
    public string log = "";

    /// <summary>
    /// 条件
    /// </summary>
    [SerializeReference]
    public List<BaseLogicNode> Conditions = new List<BaseLogicNode>();

    [SerializeReference]
    public VariableNode Parameter;

}
