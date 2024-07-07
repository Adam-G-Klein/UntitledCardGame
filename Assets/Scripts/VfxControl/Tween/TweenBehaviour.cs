using System;
using UnityEngine;
using UnityEngine.Playables;


// Runtime representation of a Tween clip.
public class TweenBehaviour : PlayableBehaviour
{
    public Vector3 startLocation;
    public Vector3 endLocation;

    public AnimationCurve curve;
}