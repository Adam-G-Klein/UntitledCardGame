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
public class CompanionTypeSO : IdentifiableSO
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
    public Sprite cardBack;
    public Sprite cardFrame;
    public Sprite cardTypeIconOverride;
    public CompanionRarity rarity;
    public GameObject cardIdleVfxPrefab;
    public AudioClip genericCompanionSFX;
    public GameObject genericCompanionVFX;

    public List<CacheConfiguration> cacheValueConfigs;

    [Header("Card pool")]
    public CardPoolSO cardPool;
    [Header("Which pack the companion is a part of")]
    public PackSO pack;
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
