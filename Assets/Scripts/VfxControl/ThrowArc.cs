using UnityEngine;

/*
    Lifts a thrown object along an arc. The FX Experience's tween moves the parent in a straight line from
    thrower to target; this child rises and falls over the same flight time, so the object is lobbed.
    The flight starts when the object is enabled (the timeline's activation clip).

    The lift is applied in world space, at right angles to the throw and always on the upper side, so it
    works under a tween that rotates its target to face the throw (and under a zero-scaled parent). With
    tiltWithArc on, the object also pitches along the curve: nose up as it leaves, nose down as it lands.
*/
public class ThrowArc : MonoBehaviour
{
    [Tooltip("Seconds from release to landing; match the tween clip's duration.")]
    [SerializeField] private float flightTime = 0.55f;
    [Tooltip("How high the arc peaks above the straight line, in world units.")]
    [SerializeField] private float height = 1.4f;
    [Tooltip("Pitch the object along the arc (for blades and other things that point where they fly).")]
    [SerializeField] private bool tiltWithArc = false;

    private float startTime;
    private Vector3 lastParentPos;
    private float forwardSpeed;

    private void OnEnable()
    {
        startTime = Time.time;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        lastParentPos = transform.parent != null ? transform.parent.position : transform.position;
        forwardSpeed = 0f;
    }

    private void LateUpdate()
    {
        Transform parent = transform.parent;
        if (parent == null) return;
        float duration = Mathf.Max(flightTime, 0.01f);
        float t = Mathf.Clamp01((Time.time - startTime) / duration);

        // the side of the throw line that faces up the screen
        Vector3 up = parent.up;
        float side = up.y < 0f ? -1f : 1f;
        transform.position = parent.position + up * side * height * 4f * t * (1f - t);

        if (!tiltWithArc) return;
        // the parent's speed along the line, measured as it moves (the tween's curve can vary it)
        Vector3 parentPos = parent.position;
        if (Time.deltaTime > 0f)
        {
            float speed = (parentPos - lastParentPos).magnitude / Time.deltaTime;
            forwardSpeed = forwardSpeed <= 0f ? speed : Mathf.Lerp(forwardSpeed, speed, 0.5f);
        }
        lastParentPos = parentPos;
        float rise = height * 4f * (1f - 2f * t) / duration;
        float pitch = forwardSpeed > 0.01f ? Mathf.Atan2(rise, forwardSpeed) * Mathf.Rad2Deg : 0f;
        transform.localRotation = Quaternion.Euler(0f, 0f, pitch * side);
    }
}
