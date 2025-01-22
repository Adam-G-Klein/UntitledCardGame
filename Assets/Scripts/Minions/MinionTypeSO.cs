using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MinionType",
    menuName = "Minions/Minion Type")]
public class MinionTypeSO : ScriptableObject
{
    public string minionName;
    public int maxHealth;
    public int baseAttackDamage;
    public Sprite sprite;
    public StartingDeck startingDeck;
}
