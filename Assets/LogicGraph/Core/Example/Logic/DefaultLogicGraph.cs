using Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[LogicGraph("默认逻辑图", typeof(StartNode))]
public class DefaultLogicGraph : BaseLogicGraph
{
    public override List<BaseVariable> DefaultVars => new List<BaseVariable>() { new BoolVariable() { Name = "test" } };
}

