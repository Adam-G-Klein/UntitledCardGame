using System.Collections.Generic;
using UnityEngine;

// VFX: the thrown part of an effect (`thrown`) flies from one of the player's rats to where the effect plays (its
// target) along an arc, pointing along its flight, and stays where it lands. Each copy spawned in a cast comes from
// the next rat down the line (Blade Volley plays once per card in hand). Purely visual.
public class ThrowFromCompanion : MonoBehaviour
{
    public Transform thrown;
    [Tooltip("Seconds from release to landing (the effect's own hit layers wait this long).")]
    public float flightTime = 0.32f;
    [Tooltip("How high the arc peaks above the straight line, in world units.")]
    public float height = 1.2f;
    [Tooltip("-1: the next rat down the line each copy. 0 or more: that rat (left to right); hidden when there are fewer rats.")]
    public int ratIndex = -1;

    // positions used when no rats are in the scene (edit-mode previews)
    public static List<Vector3> PreviewRats;

    private static int next;
    private Vector3 from, to;
    private float start;
    private bool ready;

    private void Awake() { Setup(); }

    public void Setup()
    {
        if (ready || thrown == null) return;
        ready = true;
        to = transform.position;
        var rats = new List<Vector3>();
        if (Application.isPlaying)
            foreach (CompanionInstance c in FindObjectsOfType<CompanionInstance>())
                if (c != null && c.gameObject.activeInHierarchy) rats.Add(c.transform.position);
        if (rats.Count == 0 && PreviewRats != null) rats.AddRange(PreviewRats);
        if (rats.Count == 0) rats.Add(to + new Vector3(-5f, -0.3f, 0f));
        rats.Sort((a, b) => a.x.CompareTo(b.x));
        if (ratIndex >= rats.Count) { thrown.gameObject.SetActive(false); return; }
        from = ratIndex >= 0 ? rats[ratIndex] : rats[next++ % rats.Count];
        from.z = to.z;
        start = Time.time;
        Pose(0f);
    }

    public static void ResetPreview() { next = 0; }

    private void Update() { if (Application.isPlaying) Pose(Time.time - start); }

    public void Pose(float t)
    {
        if (!ready) Setup();
        if (thrown == null || !thrown.gameObject.activeSelf) return;
        float k = Mathf.Clamp01(t / Mathf.Max(0.01f, flightTime));
        thrown.position = Vector3.Lerp(from, to, k) + Vector3.up * height * 4f * k * (1f - k);
        Vector3 dir = (to - from) + Vector3.up * height * 4f * (1f - 2f * k);
        thrown.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }
}
