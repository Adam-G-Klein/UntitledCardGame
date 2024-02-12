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
    public Sprite sprite;
    public StartingDeck startingDeck;
    public Sprite keepsake;
    public CardPoolSO cardPool;
    public SpeakerTypeSO speakerType;

    [SerializeReference]
    public CompanionAbility ability;

    [SerializeReference]
    public CompanionUpgradeSO upgradeInfo;
}
