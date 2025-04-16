using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class VisualElementUtils
{
    public static void RegisterSelected(VisualElement ve, Action action) {
        ve.RegisterCallback<ClickEvent>(evt => action());
        ve.RegisterCallback<NavigationSubmitEvent>(evt => action());
    }

    public static void ProcessSliderInput(Slider slider, GFGInputAction action) {
        // If we end up having vertical sliders, I apologize for not writing code for it
        float sliderIncrement = (slider.lowValue - slider.highValue) / 10f;

        switch (action) {
            // These two are definitely opposite of what I would have expected
            case GFGInputAction.LEFT:
                slider.value += sliderIncrement;
                break;
            
            case GFGInputAction.RIGHT:
                slider.value -= sliderIncrement;
                break;
        }
    }
}
