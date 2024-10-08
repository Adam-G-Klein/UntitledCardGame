using System;
using UnityEngine;
using UnityEngine.Playables;


// Runtime representation of a Tween clip.
public class PartialTweenBehaviour : PlayableBehaviour
{
    public FXExperience fXExperience;
    public Vector3 editorStartLocation;
    public Vector3 editorEndLocation;
    public string startLocationKey;
    public string endLocationKey;
    public bool shouldRotate;
    public float percentage = 1.0f;
    public bool reversedPartial = false;

    public AnimationCurve curve;

    public Vector3 GetStartLocation() {
        if (fXExperience == null) {
            return editorStartLocation;
        }
        
        if (!fXExperience.ContainsLocationForKey(startLocationKey)) {
            return editorStartLocation;
        } else {
            return fXExperience.GetLocationFromKey(startLocationKey);
        }
    }

    public Vector3 GetEndLocation() {
        if (fXExperience == null) {
            return editorEndLocation;
        }

        if (!fXExperience.ContainsLocationForKey(endLocationKey)) {
            return editorEndLocation;
        } else {
            return fXExperience.GetLocationFromKey(endLocationKey);
        }
    }
}