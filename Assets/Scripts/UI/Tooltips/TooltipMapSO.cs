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
    public Sprite image;
    [HideInInspector]
    public TooltipViewModel tooltip { get {
        return new TooltipViewModel(title, description, relateBehaviorIndex, image != null ? image.texture : null);
    }}
    public TooltipKeyword tooltipKeyword;
    public KeywordTooltipMapping(string title, string description, int relateBehaviorIndex, Sprite image, TooltipKeyword tooltipKeyword) {
        this.title = title;
        this.description = description;
        this.tooltipKeyword = tooltipKeyword;
        this.relateBehaviorIndex = relateBehaviorIndex;
        this.image = image;
    }

}

[System.Serializable]
public class DescriptionIconTooltipMapping {
    public string title;
    public DescriptionToken.DescriptionIconType descriptionIconType;
    public string description;
    public Sprite iconImage;

    public TooltipViewModel tooltip { get {
        return new TooltipViewModel(title, description, -1, iconImage != null ? iconImage.texture : null);
    }}

    public DescriptionIconTooltipMapping(DescriptionToken.DescriptionIconType descriptionIconType, string title, string description, Sprite iconImage) {
        this.descriptionIconType = descriptionIconType;
        this.title = title;
        this.description = description;
        this.iconImage = iconImage;
    }
}


[CreateAssetMenu(fileName = "TooltipMapSO", menuName = "ScriptableObjects/TooltipMapSO", order = 1)]
public class TooltipMapSO: ScriptableObject
{
    public List<KeywordTooltipMapping> effectTooltipMappings;

    public List<DescriptionIconTooltipMapping> descriptionIconTooltipMappings = new();
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

    public TooltipViewModel GetTooltip(DescriptionToken.DescriptionIconType descriptionIconType)
    {
        foreach(DescriptionIconTooltipMapping mapping in descriptionIconTooltipMappings)
        {
            if(mapping.descriptionIconType == descriptionIconType)
            {
                return mapping.tooltip;
            }
        }
        Debug.LogError("Tooltip not found for descriptionIconType: " + descriptionIconType);
        return new TooltipViewModel("Tooltip not found for descriptionIconType: " + descriptionIconType);
    }

    public bool HasTooltip(DescriptionToken.DescriptionIconType descriptionIconType) {
        foreach(DescriptionIconTooltipMapping mapping in descriptionIconTooltipMappings) {
            if(mapping.descriptionIconType == descriptionIconType) {
                return true;
            }
        }
        return false;
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