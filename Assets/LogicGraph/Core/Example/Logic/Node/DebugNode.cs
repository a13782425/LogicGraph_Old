using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logic;
using System;

public class DebugNode : BaseLogicNode
{
    [NodeInput(Title = "日志")]
    public string log = "";

    [NodeInt(Title = "测试")]
    public int abc = 0;

    [NodeInt(Title = "今天天气不错")]
    public int abc1 = 0;
    ///// <summary>
    ///// 条件
    ///// </summary>
    //[SerializeReference]
    //public List<BaseLogicNode> Conditions = new List<BaseLogicNode>();

    [NodePort("aaa", PortShapeEnum.Cube, VarTypes = new Type[] { typeof(float) })]
    [SerializeReference]
    public VariableNode param001;

    [NodePort("bbb", PortShapeEnum.Cube, VarTypes = new Type[] { typeof(float) })]
    [SerializeReference]
    public List<VariableNode> param002;

    [NodePort("ccc", PortDirEnum.Out, PortShapeEnum.Cube, VarTypes = new Type[] { typeof(float) })]
    [SerializeReference]
    public VariableNode param003;

    [NodePort("ddd", PortDirEnum.Out, PortShapeEnum.Cube, VarTypes = new Type[] { typeof(float) })]
    [SerializeReference]
    public List<VariableNode> param004;

    [NodePort("eee")]
    [SerializeReference]
    public BaseLogicNode child001;

    [NodePort("fff")]
    [SerializeReference]
    public List<BaseLogicNode> child002;

    [NodePort("ggg", PortDirEnum.Out)]
    [SerializeReference]
    public BaseLogicNode child003;

    [NodePort("hhh", PortDirEnum.Out)]
    [SerializeReference]
    public List<BaseLogicNode> child004;

}
