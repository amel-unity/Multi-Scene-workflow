using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This will check if all targets referenced are destroyed, and trigger the action when this is true
/// </summary>
public class TargetChecker : GameTrigger
{
    public Target[] targetsToCheck;

    void Update()
    {
        bool allDone = true;
        for(int i = 0; i < targetsToCheck.Length && allDone; ++i)
        {
            allDone &= targetsToCheck[i].Destroyed;
        }

        if (allDone)
        {
            Trigger();
            Destroy(gameObject);
        }
    }
    
    
#if UNITY_EDITOR

    public void OnDrawGizmosSelected()
    {
        if (targetsToCheck == null)
            return;
        
        foreach (var target in targetsToCheck)
        {
            Handles.DrawDottedLine(transform.position, target.transform.position, 10);
        }
    }
#endif
}
