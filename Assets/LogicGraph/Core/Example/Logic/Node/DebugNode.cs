using Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugNode : BaseLogicNode
{
    public string log = "";

    public override bool OnExecute()
    {
        Debug.LogError(log);
        IsComplete = true;
        return base.OnExecute();
    }
}
