using System;
using UnityEngine;
using UnityEngine.Playables;


// Runtime representation of a Tween clip.
public class TweenBehaviour : PlayableBehaviour
{
    public FXExperience fXExperience;
    public Vector3 editorStartLocation;
    public Vector3 editorEndLocation;
    public string startLocationKey;
    public string endLocationKey;

    public AnimationCurve curve;
}