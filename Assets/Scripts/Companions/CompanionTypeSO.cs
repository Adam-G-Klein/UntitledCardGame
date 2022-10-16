using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CompanionType", 
    menuName = "Companions/Companion Type")]
public class CompanionTypeSO : ScriptableObject
{
    public string companionName;
    public Sprite sprite;
    public int maxHealth;
    public StartingDeck startingDeck;
}
