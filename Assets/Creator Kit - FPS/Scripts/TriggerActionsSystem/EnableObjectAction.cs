using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableObjectAction : GameAction
{
    public GameObject ObjectToEnable;
    public bool SetEnableState = true;
    
    public override void Activated()
    {
        ObjectToEnable.SetActive(SetEnableState);
        Destroy(this);
    }
}
