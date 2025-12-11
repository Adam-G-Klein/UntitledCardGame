using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

[System.Serializable]
public class CardValue
{
    public string key;
    public int value;
}

[CreateAssetMenu(
    fileName ="New Card",
    menuName = "Cards/New Card Type")]
[Serializable]
public class CardType: IdentifiableSO, ITooltipProvider
{
    public string Name;
    public string Description;
    public List<CardValue> defaultValues = new();
    public int Cost;
    public Sprite Artwork;
    public CardCategory cardCategory;
    public PackSO packFrom;
    public GameObject vfxPrefab;
    // For unplayable status cards
    public bool playable = true;

    // When the card is played, it will be exhausted.
    public bool exhaustsWhenPlayed = false;

    // When the card is drawn, it will be retained across turns.
    // When you play it, it goes to the discard as normal.
    public bool retain = false;

    [SerializeReference]
    public List<EffectWorkflow> effectWorkflows = null;

    [SerializeReference]
    public EffectWorkflow onExhaustEffectWorkflow;

    // When discarded by a card or another effect, activate an effect workflow.
    // This should not trigger at the end of turn.
    [SerializeReference]
    public EffectWorkflow onDiscardEffectWorkflow;

    [SerializeReference]
    // When the card is kept in hand for the end of the turn, we can do
    // an optional effect workflow, like "lose 2 HP".
    public EffectWorkflow inPlayerHandEndOfTurnWorkflow;
    [SerializeReference]
    // When the card is drawn, we can do an optional effect workflow, like "draw 1 card".
    public EffectWorkflow onDrawEffectWorkflow;

    [SerializeField]
    // TODO: re-arch this if it becomes a big enough pain
    [Header("Add any tooltipKeyword from Resources/TooltipConfig/TooltipMap\n" +
            "NOTE: Some effects (ApplyStatus, some CardInHand/Deck effects) auto-add to the tooltip,\n" +
            "Adding their keywords here will cause duplicates in the resulting tooltip")]
    public List<TooltipKeyword> tooltips;

    [Header("A tooltip for the card that should be displayed when the card is generated.\n" +
            "You can add new tooltip keywords in the tooltip map.")]
    public TooltipKeyword tooltipKeyword = TooltipKeyword.NONE;

    // Revisit this implementation of card type level modifications
    public Dictionary<CardModification, int> cardModifications = new Dictionary<CardModification, int>() {
        {CardModification.FixedDamageIncrease, 0},
        {CardModification.DoubleDamageIncrease, 0},
        {CardModification.ThisTurnManaDecrease, 0},
        {CardModification.ThisCombatManaDecrease, 0},
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

    private string GetBaseDescription()
    {
        if (cardCategory == CardCategory.Passive)
        {
            // Get the description text from the activePower effect step (should be same as passive).
            ActivatePower activateStep = effectWorkflows[0].effectSteps.OfType<ActivatePower>().ToList().FirstOrDefault();
            if (activateStep != null)
            {
                return "Gain passive: " + activateStep.power.description;
            }
        }
        return Description;
    }

    public string GetName()
    {
        if (cardCategory == CardCategory.Passive)
        {
            // Get the title text from the activePower effect step (should be same as passive).
            ActivatePower activateStep = effectWorkflows[0].effectSteps.OfType<ActivatePower>().ToList().FirstOrDefault();
            if (activateStep != null)
            {
                return activateStep.power.title;
            }
        }
        return Name;
    }

    public string GetDescription()
    {
        string description = GetBaseDescription();
        foreach (var defaultValue in defaultValues)
        {
            description = description.Replace($"{{{defaultValue.key}}}", $"{defaultValue.value}");
        }
        return description;
    }

    public string GetDescriptionWithUpdatedValues(Dictionary<string, int> intMap)
    {
        string description = GetBaseDescription();
        // Loop through each default value and check if it exists in document.intMap
        foreach (var defaultValue in defaultValues)
        {
            string key = defaultValue.key;
            if (intMap.ContainsKey(key))
            {
                int currentValue = intMap[key];
                string styledValue;

                if (currentValue > defaultValue.value)
                {
                    styledValue = $"<color=#045700><b>{currentValue}</b></color>";
                }
                else if (currentValue < defaultValue.value)
                {
                    styledValue = $"<color=red><b>{currentValue}</b></color>";
                }
                else
                {
                    styledValue = $"<b>{currentValue}</b>";
                }

                description = description.Replace($"{{{defaultValue.key}}}", styledValue);

            }
            else
            {
                // If the value isn't in the map, use the default value unstylized
                description = description.Replace($"{{{defaultValue.key}}}", $"<b>{defaultValue.value}</b>");
            }
        }
        return description;
    }

    public TooltipKeyword GetTooltipKeyword() {
        return tooltipKeyword;
    }

    public TooltipViewModel GetTooltip()
    {
        TooltipViewModel tooltip = new TooltipViewModel(empty: true);
        List<TooltipKeyword> tooltipKeywords = new();
        tooltipKeywords.AddRange(tooltips);
        if (!tooltipKeywords.Contains(TooltipKeyword.Exhaust) && exhaustsWhenPlayed)
        {
            tooltipKeywords.Add(TooltipKeyword.Exhaust);
        }
        if (!tooltipKeywords.Contains(TooltipKeyword.Retain) && retain)
        {
            tooltipKeywords.Add(TooltipKeyword.Retain);
        }
        foreach (TooltipKeyword keyword in tooltipKeywords)
        {
            Debug.Log("CardType.GetTooltip(): Adding tooltip keyword " + keyword);
            tooltip += KeywordTooltipProvider.Instance.GetTooltip(keyword);
        }
        List<EffectWorkflow> tooltipWorkflows = new();
        tooltipWorkflows.AddRange(effectWorkflows);
        if (inPlayerHandEndOfTurnWorkflow != null)
        {
            tooltipWorkflows.Add(inPlayerHandEndOfTurnWorkflow);
        }
        if (onExhaustEffectWorkflow != null)
        {
            tooltipWorkflows.Add(onExhaustEffectWorkflow);
        }
        if (onDiscardEffectWorkflow != null)
        {
            tooltipWorkflows.Add(onDiscardEffectWorkflow);
        }
        foreach (EffectWorkflow workflow in tooltipWorkflows)
        {
            if (workflow == null)
            {
                continue;
            }
            foreach (EffectStep step in workflow.effectSteps)
            {
                Debug.Log("CardType.GetTooltip(): Found effect step " + step.effectStepName);
                if (step is ITooltipProvider)
                {
                    ITooltipProvider tooltipProvider = (ITooltipProvider)step;
                    // + is overridden in Tooltip class to concatenate plaintext strings
                    // this code should stay operable when images are added if we update the
                    // operation override
                    tooltip += tooltipProvider.GetTooltip();
                    Debug.Log("CardType.GetTooltip(): Added tooltip " + tooltip.plainText);
                }
            }
        }
        return tooltip;
    }
}

public enum CardCategory
{
    None,
    Attack,
    NonAttack,
    Saga,
    Status,
    Passive,
}

// CardFilter specifies a filter to apply to cards.
// We take the intersection of the conditions; all must apply to be true.
[System.Serializable]
public class CardFilter
{
    public enum RetainedCardFilter
    {
        None,
        OnlyRetainedCards,
        OnlyNonRetainedCards
    }

    // If empty, will not be applied.
    // Otherwise, will return true if and only if the card belongs to one of the categories.
    public List<CardCategory> cardCategoriesToInclude;

    // If none, will not be applied.
    // Otherwise, will return true if that card is in hand and retained.
    // Note: only applies to "PlayableCard" objects.
    public RetainedCardFilter retainedCardsFilter;

    // Match an exact card type.
    public CardType exactCardType = null;

    // Returns true if the card is included by the filter.
    //
    public bool ApplyFilter(Card card)
    {
        bool includeCard = true;
        if (cardCategoriesToInclude.Count > 0)
        {
            includeCard &= cardCategoriesToInclude.Contains(card.cardType.cardCategory);
        }
        if (exactCardType != null)
        {
            includeCard &= card.cardType == exactCardType;
        }
        return includeCard;
    }

    public bool ApplyFilter(PlayableCard card)
    {
        bool includeCard = true;
        // Retained card filter only applies to PlayableCards.
        if (retainedCardsFilter != RetainedCardFilter.None)
        {
            includeCard &= retainedCardsFilter == RetainedCardFilter.OnlyRetainedCards == card.retained;
        }
        return includeCard && ApplyFilter(card.card);
    }
}
