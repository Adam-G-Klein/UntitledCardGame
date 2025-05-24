using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;



[CreateAssetMenu(
    fileName = "NewAchievementSO",
    menuName = "ScriptableObjects/achievementSO")]
[System.Serializable]
public class AchievementSO : ScriptableObject
{
    public string achievementName;
    public PackSO relatedPackSO;
    public GameActionType gameActionType;
    public int target;
    public int lockedInProgress;
    public int currentProgress;
    public bool isCompleted;
    public bool isOneTimeUnlock;  
}
