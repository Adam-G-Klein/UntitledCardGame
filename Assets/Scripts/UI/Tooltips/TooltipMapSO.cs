using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine;

// The idea here is to eventually give this an image field, a header field, and a description field
// Give this editor-visible mapping everything that it needs to create a tooltip from what
// people enter in the scriptable object
[System.Serializable]
public class KeywordTooltipMapping {
    public string title;
    public string description;
    public int relateBehaviorIndex;
    public Image image;
    [HideInInspector]
    public TooltipViewModel tooltip { get {
        return new TooltipViewModel(title, description, relateBehaviorIndex, image);
    }}
    public TooltipKeyword tooltipKeyword;
    public KeywordTooltipMapping(string title, string description, int relateBehaviorIndex, Image image, TooltipKeyword tooltipKeyword) {
        this.title = title;
        this.description = description;
        this.tooltipKeyword = tooltipKeyword;
        this.relateBehaviorIndex = relateBehaviorIndex;
    }

}
[CreateAssetMenu(fileName = "TooltipMapSO", menuName = "ScriptableObjects/TooltipMapSO", order = 1)]
public class TooltipMapSO: ScriptableObject
{
    public List<KeywordTooltipMapping> effectTooltipMappings;
    public TooltipViewModel GetTooltip(TooltipKeyword tooltipKeyword)
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
            return new TooltipViewModel("Tooltip not found for tooltipKeyword: " + tooltipKeyword);
        }
        Debug.LogError("TooltipMapSO: effectTooltipMappings is null");
        return new TooltipViewModel("TooltipMapSO: effectTooltipMappings is null");
    }

    public bool HasTooltip(TooltipKeyword tooltipKeyword) {
        foreach(KeywordTooltipMapping mapping in effectTooltipMappings) {
            if(mapping.tooltipKeyword == tooltipKeyword) {
                return true;
            }
        }
        return false;
    }
}