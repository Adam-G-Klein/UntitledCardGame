using System;
using System.Collections.Generic;
using UnityEngine;

public class KeywordTooltipProvider: GenericSingleton<KeywordTooltipProvider>
{

    public Dictionary<StatusEffect, TooltipKeyword> statusEffectToTooltipKeyword = new Dictionary<StatusEffect, TooltipKeyword>() {
        {StatusEffect.Strength, TooltipKeyword.Strength},
        {StatusEffect.Defended, TooltipKeyword.Block},
        {StatusEffect.TemporaryStrength, TooltipKeyword.TemporaryStrength},
    };

    public TooltipMapSO tooltipMap = null;
    public Tooltip GetTooltip(TooltipKeyword keyword)
    {
        if(tooltipMap == null)
        {
            tooltipMap = Resources.Load<TooltipMapSO>("TooltipConfig/TooltipMapSO");
        }
        return tooltipMap.GetTooltip(keyword);
    }

    public bool HasTooltip(StatusEffect statusEffect)
    {
        return statusEffectToTooltipKeyword.ContainsKey(statusEffect);
    }

    public Tooltip GetTooltip(StatusEffect statusEffect)
    {
        if(statusEffectToTooltipKeyword.ContainsKey(statusEffect))
        {
            return GetTooltip(statusEffectToTooltipKeyword[statusEffect]);
        }
        Debug.LogError("Call HasTooltip first: Tooltip not found for statusEffect " + statusEffect);
        return new Tooltip("Tooltip not found for statusEffect: " + statusEffect);
    }
}