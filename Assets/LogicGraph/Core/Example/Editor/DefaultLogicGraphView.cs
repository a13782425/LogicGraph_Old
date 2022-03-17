using Logic;
using Logic.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[LogicGraph("Ĭ���߼�ͼ", typeof(DefaultLogicGraph), typeof(StartNode))]
public class DefaultLogicGraphView : BaseGraphView
{
    public override List<BaseVariable> DefaultVars => new List<BaseVariable>() { new FloatVariable() { Name = "test" } };

    public override List<Type> VarTypes => new List<Type>() { typeof(ColorVariable), typeof(FloatVariable) };
}
