using UnityEngine;
using UIElements = UnityEngine.UIElements;

/*
    A world-space trail that rides a UI Toolkit element while it moves, then lets its last particles fade where
    they are once the element is gone. Used by the reshuffle (CompanionView.ShuffleDiscardIntoDrawVFX): each little
    card that flies from the discard pile to the draw pile draws a trail behind it, tinted the pack's colour.
*/
public class UIElementTrail : MonoBehaviour
{
    [Tooltip("Seconds the trail lingers after its element is gone, for the particles still fading.")]
    [SerializeField] private float linger = 0.6f;
    [Tooltip("Keep the prefab's own colours instead of tinting them the pack's (the reshuffle's gold and pink).")]
    [SerializeField] private bool keepOwnColours = false;

    private UIElements.VisualElement target;
    private bool released;
    // the element's first frame or two can be before its owner has placed it; the trail waits so it doesn't streak
    // in from there
    private int framesUntilEmit = 2;

    // Spawns prefab riding element, its particles tinted (multiplied) by tint. Does nothing without a prefab.
    public static void Attach(GameObject prefab, UIElements.VisualElement element, Color tint)
    {
        if (prefab == null || element == null) return;
        GameObject trail = Instantiate(prefab);
        UIElementTrail own = trail.GetComponent<UIElementTrail>();
        if (own != null && own.keepOwnColours) tint = Color.white;
        tint = Color.Lerp(tint, Color.white, 0.45f);   // a glow: lighter than the pack's colour, so it reads on the board
        foreach (ParticleSystem ps in trail.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            ParticleSystem.MinMaxGradient c = main.startColor;
            if (c.mode == ParticleSystemGradientMode.TwoColors) main.startColor = new ParticleSystem.MinMaxGradient(c.colorMin * tint, c.colorMax * tint);
            else main.startColor = c.color * tint;
        }
        foreach (ParticleSystem ps in trail.GetComponentsInChildren<ParticleSystem>(true)) ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        UIElementTrail follow = trail.GetComponent<UIElementTrail>();
        if (follow == null) follow = trail.AddComponent<UIElementTrail>();
        follow.target = element;
        follow.LateUpdate();
    }

    private void LateUpdate()
    {
        if (released) return;
        if (target == null || target.panel == null)
        {
            released = true;
            foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>()) ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            Destroy(gameObject, linger);
            return;
        }
        Rect bounds = target.worldBound;
        Camera cam = Camera.main;
        // NaN until the element's first layout
        if (cam == null || float.IsNaN(bounds.center.x) || float.IsNaN(bounds.center.y)) return;
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(bounds.center.x, Screen.height - bounds.center.y, 0f));
        transform.position = new Vector3(world.x, world.y, UIDocumentGameObjectPlacer.zPlane);
        if (framesUntilEmit > 0 && --framesUntilEmit == 0)
            foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>(true)) ps.Play(false);
    }
}
