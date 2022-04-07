using Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayNode : BaseLogicNode
{

    public float delayTime;

    [NodePort(PortShapeEnum.Cube, LinkName = "delayTime", VarTypes = new Type[] { typeof(float) })]
    [SerializeReference]
    public VariableNode delayVar;

    private float maxTime = 0f;
    private float temp = 0f;

    public override bool OnExecute()
    {
        temp = 0;
        IsComplete = false;
        if (delayVar == null)
        {
            maxTime = delayTime;
        }
        else
        {
            maxTime = (float)delayVar.variable.Value;
        }
        return true;
    }

    public override bool OnUpdate(float deltaTime)
    {
        temp += deltaTime * Time.timeScale;
        if (temp >= maxTime)
        {
            IsComplete = true;
        }
        return base.OnUpdate(deltaTime);
    }
}
