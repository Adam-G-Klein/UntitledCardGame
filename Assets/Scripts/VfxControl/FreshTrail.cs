using UnityEngine;

/*
    VFX: a motion trail behind something an FX experience flies across the screen (the thrown daggers). The thrown
    object is switched on at its start point by the timeline, so a plain TrailRenderer would draw a streak from wherever
    it sat while hidden: this clears the trail for its first frames after it's enabled, then lets it emit.
*/
[RequireComponent(typeof(TrailRenderer))]
public class FreshTrail : MonoBehaviour
{
    private TrailRenderer trail;
    private int settleFrames;

    private void OnEnable()
    {
        trail = GetComponent<TrailRenderer>();
        trail.Clear();
        trail.emitting = false;
        settleFrames = 2;
    }

    private void LateUpdate()
    {
        if (settleFrames <= 0) return;
        settleFrames--;
        trail.Clear();
        if (settleFrames == 0) trail.emitting = true;
    }
}
