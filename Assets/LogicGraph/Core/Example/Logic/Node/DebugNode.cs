using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logic;
using System;

public class DebugNode : BaseLogicNode
{
    public string log = "";

    public int abc = 0;

    public float test = 0;

    public int abc1 = 0;
    ///// <summary>
    ///// 条件
    ///// </summary>
    //[SerializeReference]
    //public List<BaseLogicNode> Conditions = new List<BaseLogicNode>();

    [NodePort(PortShapeEnum.Cube, LinkName = "abc", VarTypes = new Type[] { typeof(float) })]
    [SerializeReference]
    public VariableNode param001;

    [NodePort(PortShapeEnum.Cube, VarTypes = new Type[] { typeof(float) })]
    [SerializeReference]
    public List<VariableNode> param002;

    [NodePort(PortDirEnum.Out, PortShapeEnum.Cube, VarTypes = new Type[] { typeof(float) })]
    [SerializeReference]
    public VariableNode param003;

    [NodePort(PortDirEnum.Out, PortShapeEnum.Cube, VarTypes = new Type[] { typeof(float) })]
    [SerializeReference]
    public List<VariableNode> param004;

    [NodePort()]
    [SerializeReference]
    public BaseLogicNode child001;

    [NodePort()]
    [SerializeReference]
    public List<BaseLogicNode> child002;

    [NodePort(PortDirEnum.Out)]
    [SerializeReference]
    public BaseLogicNode child003;

    [NodePort(PortDirEnum.Out)]
    [SerializeReference]
    public List<BaseLogicNode> child004;

    //[NodeToggle]
    public bool aaa;
    public double bbb;
    public PortTypeEnum ccc;
    public Vector2 ddd;
    public Vector3 eee;
    public Color fff;
    public AnimationCurve ggg;

    public GameObject hhh;
}
