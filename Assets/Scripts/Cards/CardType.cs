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

[System.Serializable]
public class DescriptionToken
{
    public enum TokenType
    {
        Text,
        NewLine,
        Icon,
        NewLineHorizontalDiv,
    }

    public enum DescriptionIconType
    {
        None,
        Attack,
        Block,
        Draw,
        Random,
        Adjacent,
        Leftmost,
        Strength,
        TemporaryStrength,
        Bleed,
        Energy,
        Self,
        SelfAndAdjacent,
        Discard,
        Exhaust,
        WhenDiscarded,
        WhenExhausted,
        InHandEndOfTurn,
        AnyTarget,
        RandomRat,
        Heal,
        IfSelfBelowHalfHP,
        IfSelfLostHPThisTurn,
        GreekFire,
        ShuffleIntoDeck,
        CreateInHand,
        NumAttackCardsInHand,
        NumCardsInHand,
        IfLastCardPlayedWasAttack,
        IfOnlyAttackCardInHand,
        IfLessThan3CardsInHand,
        IfExhaustedCardThisTurn,
        WhenDrawn,
        AOETarget,
        AttackBuff,
        BlockBuff,
    }
    public TokenType tokenType;
    public string text;
    public DescriptionIconType icon;
}

[Serializable]
public class IconDescriptionTooltipLineOverride
{
    public int lineIndex;
    public string bonusText = "";
    public bool hidden = false;
}

[CreateAssetMenu(
    fileName ="New Card",
    menuName = "Cards/New Card Type")]
[Serializable]
public class CardType: IdentifiableSO, ITooltipProvider
{
    public string Name;
    public string Description;
    // Experimental field for a description with hieroglyphic shorthand.
    public List<DescriptionToken> IconDescription = new();

    [Header("Per-line control over icon description tooltip lines.\n" +
            "By default every line gets a tooltip with empty text; add an entry here to\n" +
            "give a specific line (by index) bonus explanatory text, or to hide it entirely.")]
    public List<IconDescriptionTooltipLineOverride> IconDescriptionTooltipLineOverrides = new();

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
        {CardModification.UntilLeavesHandManaDecrease, 0},
        {CardModification.ThisCombatManaDecrease, 0},
        {CardModification.FixedBlockIncrease, 0},
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
                string prefix = "Gain passive:\n";
                if (!activateStep.power.Stackable)
                    prefix = "Gain unique passive:\n";
                return prefix + activateStep.power.description;
            }
        }
        return Description;
    }

    public bool HasIconDescription()
    {
        return IconDescription != null && IconDescription.Count > 0;
    }

    public List<DescriptionToken> GetIconDescriptionTokens(bool fillDefaultValues = true)
    {
        // Fill out the default values in the icon description.
        List<DescriptionToken> filledTokens = new List<DescriptionToken>();
        foreach (DescriptionToken token in IconDescription)
        {
            if (token.tokenType != DescriptionToken.TokenType.Text)
            {
                filledTokens.Add(token);
                continue;
            }
            string filledText = token.text;
            if (fillDefaultValues)
            {
                foreach (var defaultValue in defaultValues)
                {
                    filledText = filledText.Replace($"{{{defaultValue.key}}}", $"{defaultValue.value}");
                }
            }
            filledTokens.Add(new DescriptionToken
            { tokenType = DescriptionToken.TokenType.Text, text = filledText });
        }
        return filledTokens;
    }

    public List<DescriptionToken> GetIconDescriptionTokensWithStylizedValues(Dictionary<string, int> intMap)
    {
        // Fill out the default values in the icon description.
        List<DescriptionToken> filledTokens = new List<DescriptionToken>();
        foreach (DescriptionToken token in IconDescription)
        {
            if (token.tokenType != DescriptionToken.TokenType.Text)
            {
                filledTokens.Add(token);
                continue;
            }
            string description = token.text;
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
                        styledValue = $"{currentValue}"; // removed bold here as it just felt weird for the numbers to have different weight for no reason
                    }

                    description = description.Replace($"{{{defaultValue.key}}}", styledValue);

                }
                else
                {
                    // If the value isn't in the map, use the default value unstylized
                    description = description.Replace($"{{{defaultValue.key}}}", $"<b>{defaultValue.value}</b>");
                }
            }
            filledTokens.Add(new DescriptionToken
            { tokenType = DescriptionToken.TokenType.Text, text = description });
        }
        return filledTokens;
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

        if (exhaustsWhenPlayed)
        {
            tooltip += KeywordTooltipProvider.Instance.GetTooltip(TooltipKeyword.ExhaustsWhenPlayed);

        }
        if (retain)
        {
            tooltip += KeywordTooltipProvider.Instance.GetTooltip(TooltipKeyword.Retain);
        }

        if (HasIconDescription())
        {
            Debug.Log("CardType.GetTooltip(): Generating icon description tooltip");
            // Loop through the description tokens with icons.
            List<DescriptionToken> tokens = GetIconDescriptionTokens(fillDefaultValues: false);

            // Split across newlines
            List<List<DescriptionToken>> descriptionLines = new List<List<DescriptionToken>> { new List<DescriptionToken>() };
            foreach (DescriptionToken token in tokens)
            {
                if (token.tokenType == DescriptionToken.TokenType.NewLine || token.tokenType == DescriptionToken.TokenType.NewLineHorizontalDiv)
                {
                    descriptionLines.Add(new List<DescriptionToken>());
                    continue;
                }
                descriptionLines.Last().Add(token);
            }
            for (int i = 0; i < descriptionLines.Count; i++)
            {
                List<DescriptionToken> line = descriptionLines[i];
                IconDescriptionTooltipLineOverride lineOverride = IconDescriptionTooltipLineOverrides
                    .FirstOrDefault(o => o.lineIndex == i);
                if (lineOverride != null && lineOverride.hidden)
                {
                    continue;
                }
                string description = lineOverride?.bonusText ?? "";
                tooltip += new TooltipViewModel(description, line);
            }
        }

        foreach (TooltipKeyword keyword in tooltips)
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
        tooltip += EffectWorkflow.GetEffectStepTooltips(tooltipWorkflows, x => true);
        return tooltip;
    }
}

public enum CardCategory
{
    None,
    Attack,
    Skill,
    Saga,
    Charm,
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

    public enum ManaCostFilter
    {
        None,
        ExactManaCost,
        AtLeastManaCost,
        AtMostManaCost
    }

    // If empty, will not be applied.
    // Otherwise, will return true if and only if the card belongs to one of the categories.
    public List<CardCategory> cardCategoriesToInclude;

    // If none, will not be applied.
    // Otherwise, will return true if that card is in hand and retained.
    // Note: only applies to "PlayableCard" objects.
    public RetainedCardFilter retainedCardsFilter;

    public ManaCostFilter manaCostFilter = ManaCostFilter.None;
    public int manaCost = 0;

    // Match an exact card type.
    public CardType exactCardType = null;

    // Match if belonging to the selected companion instance.
    public bool companionInstanceFilterEnabled = false;

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
        if (manaCostFilter != ManaCostFilter.None)
        {
            switch (manaCostFilter)
            {
                case ManaCostFilter.ExactManaCost:
                    includeCard &= card.cardType.Cost == manaCost;
                    break;
                case ManaCostFilter.AtLeastManaCost:
                    includeCard &= card.cardType.Cost >= manaCost;
                    break;
                case ManaCostFilter.AtMostManaCost:
                    includeCard &= card.cardType.Cost <= manaCost;
                    break;
            }
        }
        return includeCard;
    }

    public bool ApplyFilter(PlayableCard card, List<CompanionInstance> companionInstancesToInclude = null)
    {
        bool includeCard = true;
        // Retained card filter only applies to PlayableCards.
        if (retainedCardsFilter != RetainedCardFilter.None)
        {
            includeCard &= retainedCardsFilter == RetainedCardFilter.OnlyRetainedCards == card.retained;
        }
        // Companion type filter only applies to PlayableCards.
        if (companionInstancesToInclude != null && companionInstancesToInclude.Count > 0 && companionInstanceFilterEnabled)
        {
            includeCard &= companionInstancesToInclude.Select(ci => ci.deckInstance).Contains(card.deckFrom);
        }
        return includeCard && ApplyFilter(card.card);
    }
}
