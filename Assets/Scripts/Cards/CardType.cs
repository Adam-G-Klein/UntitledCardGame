using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(
    fileName ="Card",
    menuName = "Cards/New Card Type")]
[Serializable]
public class CardType: ScriptableObject, ITooltipProvider
{
    public string Name;
    public string Description;
    public int Cost;
    public Sprite Artwork;
    public CardCategory cardCategory;
    public GameObject vfxPrefab;
    // For unplayable status cards
    public bool playable = true;

    // When the card is played, it will be exhausted.
    public bool exhaustsWhenPlayed = false;

    [SerializeReference]
    public List<EffectWorkflow> effectWorkflows;

    [SerializeReference]
    public EffectWorkflow onExhaustEffectWorkflow;

    [SerializeReference]
    // When the card is kept in hand for the end of the turn, we can do
    // an optional effect workflow, like "lose 2 HP".
    public EffectWorkflow inPlayerHandEndOfTurnWorkflow;

    [SerializeField]
    // TODO: re-arch this if it becomes a big enough pain
    [Header("Add any tooltipKeyword from Resources/TooltipConfig/TooltipMap\n" +
            "NOTE: Some effects (ApplyStatus, some CardInHand/Deck effects) auto-add to the tooltip,\n" +
            "Adding their keywords here will cause duplicates in the resulting tooltip")]
    public List<TooltipKeyword> tooltips;

    // Revisit this implementation of card type level modifications
    public Dictionary<CardModification, int> cardModifications = new Dictionary<CardModification, int>() {
        {CardModification.FixedDamageIncrease, 0},
        {CardModification.DoubleDamageIncrease, 0},
        {CardModification.ThisTurnManaDecrease, 0}
    };

    public void ResetCardModifications() {
        cardModifications = new Dictionary<CardModification, int>();
        foreach(int i in Enum.GetValues(typeof(CardModification))) {
            cardModifications.Add((CardModification)i, 0);
        }
    }

    public void ChangeCardModification(CardModification modification, int scale) {
        cardModifications[modification] += scale;
    }

    public TooltipViewModel GetTooltip() {
        TooltipViewModel tooltip = new TooltipViewModel(empty: true);
        foreach(TooltipKeyword keyword in tooltips) {
            tooltip += KeywordTooltipProvider.Instance.GetTooltip(keyword);
        }
        return tooltip;
    }
}

public enum CardCategory {
    None,
    Attack,
    NonAttack,
    Saga,
    Status
}