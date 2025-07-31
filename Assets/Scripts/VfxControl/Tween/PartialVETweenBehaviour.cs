using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;


// Runtime representation of a Tween clip.
public class PartialVETweenBehaviour : PlayableBehaviour
{
    public FXExperience fXExperience;
    public string startLocationKey;
    public string endLocationKey;
    public float percentage = 1.0f;
    public bool reversedPartial = false;
    public string visualElementKey;
    private VisualElement cachedVisualElement = null;

    public AnimationCurve curve;

    public Vector3 GetStartLocation() {
        if (!fXExperience.ContainsLocationForKey(startLocationKey)) {
            return Vector3.zero;
        } else {
            return fXExperience.GetLocationFromKey(startLocationKey);
        }
    }

    public Vector3 GetEndLocation() {
        if (!fXExperience.ContainsLocationForKey(endLocationKey)) {
            return Vector3.zero;
        } else {
            return fXExperience.GetLocationFromKey(endLocationKey);
        }
    }

    public VisualElement GetVisualElement() {
        if (!fXExperience.ContainsVisualElementForKey(visualElementKey)) {
            return null;
        } else {
            return fXExperience.GetVisualElementFromKey(visualElementKey);
        }
    }
}