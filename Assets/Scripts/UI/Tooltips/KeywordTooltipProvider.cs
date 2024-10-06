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

    private void LoadTooltipMap() {
        if(tooltipMap == null)
        {
            tooltipMap = Resources.Load<TooltipMapSO>("TooltipConfig/TooltipMapSO");
        }
    }
}