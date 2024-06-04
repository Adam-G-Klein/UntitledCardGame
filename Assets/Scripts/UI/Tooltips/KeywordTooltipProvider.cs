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
        LoadTooltipMap();
        return tooltipMap.GetTooltip(keyword);
    }

    public bool HasTooltip(StatusEffect statusEffect)
    {
        LoadTooltipMap();
        return statusEffectToTooltipKeyword.ContainsKey(statusEffect) && HasTooltip(statusEffectToTooltipKeyword[statusEffect]);
    }

    public bool HasTooltip(TooltipKeyword tooltipKeyword)
    {
        LoadTooltipMap();
        return tooltipMap.HasTooltip(tooltipKeyword);
    }

    public Tooltip GetTooltip(StatusEffect statusEffect)
    {
        if(HasTooltip(statusEffect))
        {
            return GetTooltip(statusEffectToTooltipKeyword[statusEffect]);
        }
        Debug.LogError("Call HasTooltip first: Tooltip not found for statusEffect " + statusEffect);
        return new Tooltip("Tooltip not found for statusEffect: " + statusEffect);
    }

    private void LoadTooltipMap() {
        if(tooltipMap == null)
        {
            tooltipMap = Resources.Load<TooltipMapSO>("TooltipConfig/TooltipMapSO");
        }
    }
}