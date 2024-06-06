using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The idea here is to eventually give this an image field, a header field, and a description field
// Give this editor-visible mapping everything that it needs to create a tooltip from what
// people enter in the scriptable object
[System.Serializable]
public class KeywordTooltipMapping {
    public Tooltip tooltip;
    public TooltipKeyword tooltipKeyword;
    public KeywordTooltipMapping(Tooltip tooltip, TooltipKeyword tooltipKeyword) {
        this.tooltip = tooltip;
        this.tooltipKeyword = tooltipKeyword;
    }

}
[CreateAssetMenu(fileName = "TooltipMapSO", menuName = "ScriptableObjects/TooltipMapSO", order = 1)]
public class TooltipMapSO: ScriptableObject
{
    public List<KeywordTooltipMapping> effectTooltipMappings;
    public Tooltip GetTooltip(TooltipKeyword tooltipKeyword)
    {
        if(effectTooltipMappings != null)
        {
            foreach(KeywordTooltipMapping mapping in effectTooltipMappings)
            {
                if(mapping.tooltipKeyword == tooltipKeyword)
                {
                    return mapping.tooltip;
                }
            }
            Debug.LogError("Tooltip not found for tooltipKeyword: " + tooltipKeyword);
            return new Tooltip("Tooltip not found for tooltipKeyword: " + tooltipKeyword);
        }
        Debug.LogError("TooltipMapSO: effectTooltipMappings is null");
        return new Tooltip("TooltipMapSO: effectTooltipMappings is null");
    }
}