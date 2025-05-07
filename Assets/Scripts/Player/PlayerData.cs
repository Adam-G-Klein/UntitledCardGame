using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData 
{
    public int gold = 0;
    public int shopLevel = 0;
    public int shopLevelIncrementsEarned = 0;
    public int manaPerTurn = 3;
    // TODO: make this a scriptableobject for things they have and haven't done
    public bool seenTutorial = false;
    public bool inTutorialRun = true;
} 