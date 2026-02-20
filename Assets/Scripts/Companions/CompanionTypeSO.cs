using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    [Space(8)]
    public StartingDeck startingDeck;
    public CompanionRarity rarity;

    [Header("Base level one companion (empty if this is L1)")]
    public CompanionTypeSO baseCompanionType;

    public List<CacheConfiguration> cacheValueConfigs;

    [Header("Card pool")]
    public CardPoolSO cardPool;
    [Header("Which pack the companion is a part of")]
    public PackSO pack;

    [Header("Companion passive abilities")]
    [SerializeReference]
    public List<EntityAbility> abilitiesV2;
    [Header("Companion upgrade / combination spec")]

    [Tooltip("Level 1 companions are the lowest rarity; they upgrade to level 2 companions, which then upgrade to level 3")]
    public CompanionLevel level;
    [SerializeField]
    public CompanionTypeSO upgradeTo;
    // USE GETTOOLTIP TO get tooltip for generated cards and the like
    public TooltipViewModel tooltip;

    [Header("Companion keepsake descriptions for team signing")]
    public string keepsakeTitle;
    public string keepsakeDescription;

    public TooltipViewModel GetTooltip()
    {
        TooltipViewModel tooltipViewModel = new TooltipViewModel(tooltip.lines);
        foreach (EntityAbility entityAbility in abilitiesV2)
        {
            foreach (EffectStep step in entityAbility.effectSteps)
            {
                Debug.Log("CompanionTypeSO.GetTooltip(): Found effect step " + step.effectStepName);
                if (step is ITooltipProvider)
                {
                    ITooltipProvider tooltipProvider = (ITooltipProvider)step;
                    // + is overridden in Tooltip class to concatenate plaintext strings
                    // this code should stay operable when images are added if we update the
                    // operation override
                    tooltipViewModel += tooltipProvider.GetTooltip();
                    Debug.Log("CompanionTypeSO.GetTooltip(): Added tooltip " + tooltip.plainText);
                }
            }      
        }
        return tooltipViewModel;
    }
}
