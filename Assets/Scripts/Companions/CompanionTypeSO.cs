using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CompanionType", 
    menuName = "Companions/Companion Type")]
public class CompanionTypeSO : ScriptableObject
{
    public string companionName;
    public int maxHealth;
    public int initialCardsDealtPerTurn = 1;
    [Header("Art Assets")]
    public Sprite sprite;
    public StartingDeck startingDeck;
    public Sprite keepsake;
    public Sprite cardBack; 
    public Sprite cardFrame; 
    public Sprite typeIcon; 
    public GameObject cardIdleVfxPrefab;
    [Header("Card pool")]
    public CardPoolSO cardPool;
    [Header("Dialogue hook-in")]
    public SpeakerTypeSO speakerType;
    [Header("Abilities and upgrades")]

    [SerializeReference]
    public List<CompanionAbility> abilities;

    [SerializeReference]
    public CompanionUpgradeSO upgradeInfo;
}
