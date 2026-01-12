using System;
using System.Collections.Generic;
using UnityEngine;

// ADAPTER PATTERN
public class KeywordTooltipProvider: GenericSingleton<KeywordTooltipProvider>
{

    public Dictionary<StatusEffectType, TooltipKeyword> statusEffectToTooltipKeyword = new Dictionary<StatusEffectType, TooltipKeyword>() {
        {StatusEffectType.Strength, TooltipKeyword.Strength},
        {StatusEffectType.Defended, TooltipKeyword.Block},
        {StatusEffectType.TemporaryStrength, TooltipKeyword.TemporaryStrength},
        {StatusEffectType.Orb, TooltipKeyword.Aura},
        {StatusEffectType.Charge, TooltipKeyword.Charge},
        {StatusEffectType.MaxBlockToLoseAtEndOfTurn, TooltipKeyword.MaxBlockLost},
        {StatusEffectType.Burn, TooltipKeyword.Burn},
        {StatusEffectType.ExtraCardsToDealNextTurn, TooltipKeyword.ExtraCardsToDealNextTurn}
    };

    public TooltipMapSO tooltipMap = null;
    public TooltipViewModel GetTooltip(TooltipKeyword keyword)
    {
        LoadTooltipMap();
        return tooltipMap.GetTooltip(keyword);
    }

    public bool HasTooltip(StatusEffectType statusEffect)
    {
        LoadTooltipMap();
        return statusEffectToTooltipKeyword.ContainsKey(statusEffect) && HasTooltip(statusEffectToTooltipKeyword[statusEffect]);
    }

    public bool HasTooltip(TooltipKeyword tooltipKeyword)
    {
        LoadTooltipMap();
        return tooltipMap.HasTooltip(tooltipKeyword);
    }

    public TooltipViewModel GetTooltip(StatusEffectType statusEffect)
    {
        if(HasTooltip(statusEffect))
        {
            return GetTooltip(statusEffectToTooltipKeyword[statusEffect]);
        }
        Debug.LogError("Call HasTooltip first: Tooltip not found for statusEffect " + statusEffect);
        return new TooltipViewModel("Tooltip not found for statusEffect: " + statusEffect);
    }

    public bool HasTooltip(DescriptionToken.DescriptionIconType descriptionIconType)
    {
        LoadTooltipMap();
        return tooltipMap.HasTooltip(descriptionIconType);
    }

    public TooltipViewModel GetTooltip(DescriptionToken.DescriptionIconType descriptionIconType)
    {
        if(HasTooltip(descriptionIconType))
        {
            return tooltipMap.GetTooltip(descriptionIconType);
        }
        Debug.LogError("Call HasTooltip first: Tooltip not found for descriptionIconType " + descriptionIconType);
        return new TooltipViewModel("Tooltip not found for descriptionIconType: " + descriptionIconType);
    }

    private void LoadTooltipMap() {
        if(tooltipMap == null)
        {
            tooltipMap = Resources.Load<TooltipMapSO>("TooltipConfig/TooltipMapSO");
        }
    }
}