using System;
using UnityEngine;
using UnityEngine.Playables;


// Runtime representation of a Tween clip.
public class PositionBehaviour : PlayableBehaviour
{
    public FXExperience fXExperience;
    public string locationKey;

    public Vector3 GetPosition(Vector3 defaultValue) {
        if (fXExperience == null) {
            return defaultValue;
        }

        if (!fXExperience.ContainsLocationForKey(locationKey)) {
            return defaultValue;
        }

        return fXExperience.GetLocationFromKey(locationKey);
    }
}