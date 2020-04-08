using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//Base class used for all type of action like display an object, trigger an animation etc..
//Look at derived class for sample on how to write your own
public abstract class GameAction : MonoBehaviour
{
    public abstract void Activated();
}

//Base Class used to activate GameAction when a specific action is done
//Look at derived class to see how to write your own
public abstract class GameTrigger : MonoBehaviour
{
    public GameAction[] actions;

    public void Trigger()
    {
        foreach (GameAction g in actions)
            g.Activated();
    }
}