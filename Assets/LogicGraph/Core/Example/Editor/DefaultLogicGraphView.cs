using Logic;
using Logic.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[LogicGraph("Ä¬ÈÏÂß¼­Í¼", typeof(DefaultLogicGraph), typeof(StartNode))]
public class DefaultLogicGraphView : BaseGraphView
{
    public override List<BaseVariable> DefaultVars => base.DefaultVars;

    public override List<Type> VarTypes => new List<Type>() { typeof(ColorVariable), typeof(FloatVariable) };
}
