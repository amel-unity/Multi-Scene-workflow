using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FPSKit/LevelRoomGroup", fileName = "LevelRoomGroup")]
public class LevelRoomGroup : ScriptableObject
{
    public LevelRoom[] levelPart;
}
