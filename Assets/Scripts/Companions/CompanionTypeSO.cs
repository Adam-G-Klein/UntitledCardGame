using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CompanionLevel {
    LevelOne,
    LevelTwo,
    LevelThree
}

public enum CompanionRarity {
    COMMON,
    UNCOMMON,
    RARE
}

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
    public Sprite fullSprite;
    public Sprite backgroundImage;
    public Sprite entityFrame;
    public Sprite companionManagementFrame;
    public StartingDeck startingDeck;
    public Sprite keepsake;
    public Sprite cardBack;
    public Sprite cardFrame;
    public Sprite typeIcon;
    public Sprite portrait;
    public Sprite teamSelectSprite;
    public CompanionRarity rarity;
    public GameObject cardIdleVfxPrefab;
    public AudioClip genericCompanionSFX;
    public GameObject genericCompanionVFX;
    [Header("Card pool")]
    public CardPoolSO cardPool;
    [Header("Dialogue hook-in")]
    public SpeakerTypeSO speakerType;
    [Header("Companion passive abilities")]


    [SerializeReference]
    public List<EntityAbility> abilitiesV2;
    [Header("Companion upgrade / combination spec")]

    [Tooltip("Level 1 companions are the lowest rarity; they upgrade to level 2 companions, which then upgrade to level 3")]
    public CompanionLevel level;
    [SerializeField]
    public CompanionTypeSO upgradeTo;
    public TooltipViewModel tooltip;

    [Header("Companion keepsake descriptions for team signing")]
    public string keepsakeTitle;
    public string keepsakeDescription;
}
