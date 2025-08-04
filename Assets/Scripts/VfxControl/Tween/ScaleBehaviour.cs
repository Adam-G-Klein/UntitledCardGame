using System;
using UnityEngine;
using UnityEngine.Playables;

public class ScaleBehaviour : PlayableBehaviour
{
    public float startingScale;
    public float endingScale;
    public AnimationCurve curve;

    private bool shouldInitialize = true;
    public Vector3 initialScale = Vector3.zero;
}
