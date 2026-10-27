using System.Collections.Generic;
using UnityEngine;

/*
    For attacks that hit every enemy. The game spawns the effect once per target, all on the same frame.
    This splits the prefab in two:

      Area - played ONCE for the whole cast, placed over the enemy row (its centre, or just left of it for
             effects that travel left to right, like a flamethrower).
      Hit  - kept on every target: a small hit coloured to the effect, so each enemy shows it took damage.

    When the area travels (travelSpeed > 0), each target's hit waits until the wave reaches it.
    Purely visual: damage is applied by the game exactly when it was before.
*/
public class AreaEffect : MonoBehaviour
{
    public enum Anchor { RowCentre, LeftOfRow, ScreenLeft }

    [SerializeField] private Transform area;
    [SerializeField] private Transform hit;
    [SerializeField] private Anchor anchor = Anchor.RowCentre;
    [Tooltip("LeftOfRow: how far left of the leftmost enemy the area starts.")]
    [SerializeField] private float leftPad = 2.5f;
    [Tooltip("Seconds of build-up before the area reaches any enemy.")]
    [SerializeField] private float hitDelay = 0.3f;
    [Tooltip("World units per second the area travels (0 = every hit lands together).")]
    [SerializeField] private float travelSpeed = 0f;
    [Tooltip("On: the wave sweeps in from the left edge of the row. Off: it spreads out from the area's centre.")]
    [SerializeField] private bool travelFromLeft = true;
    [Tooltip("On: the area lies over the player's rats (enemy attacks that hit every rat) instead of the enemy row.")]
    [SerializeField] private bool companionRow = false;

    // positions used when no enemies are in the scene (edit-mode previews)
    public static List<Vector3> PreviewTargets;
    public static float? PreviewScreenLeft;

    // the left edge of the view, so screen-wide effects start off-screen-left whatever the row looks like
    private float ScreenLeftX(float fallbackMinX)
    {
        if (!Application.isPlaying && PreviewScreenLeft.HasValue) return PreviewScreenLeft.Value - 0.5f;
        Camera cam = Camera.main;
        if (cam == null) return fallbackMinX - leftPad;
        float depth = Mathf.Abs(transform.position.z - cam.transform.position.z);
        return cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth)).x - 0.5f;
    }
    private static readonly Dictionary<string, int> lastCastFrame = new Dictionary<string, int>();
    private bool done;

    private void Awake() { Setup(); }

    public void Setup()
    {
        if (done) return;
        done = true;
        List<Vector3> targets = TargetPositions();
        float minX = float.MaxValue, maxX = float.MinValue, sumY = 0f;
        foreach (Vector3 p in targets) { minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x); sumY += p.y; }
        float rowY = sumY / Mathf.Max(1, targets.Count);
        Vector3 areaPos;
        if (anchor == Anchor.ScreenLeft) areaPos = new Vector3(ScreenLeftX(minX), rowY, transform.position.z);
        else if (anchor == Anchor.LeftOfRow) areaPos = new Vector3(minX - leftPad, rowY, transform.position.z);
        else areaPos = new Vector3((minX + maxX) * 0.5f, rowY, transform.position.z);

        // one area per cast: the first instance spawned this frame keeps it
        string key = name.Replace("(Clone)", "").Trim();
        bool first = Application.isPlaying
            ? !lastCastFrame.TryGetValue(key, out int last) || last != Time.frameCount
            : !previewKept.Contains(key);
        if (area != null)
        {
            if (first)
            {
                if (Application.isPlaying) lastCastFrame[key] = Time.frameCount;
                else previewKept.Add(key);
                area.position = areaPos;
                area.rotation = Quaternion.identity;
            }
            else if (Application.isPlaying) Destroy(area.gameObject);
            else DestroyImmediate(area.gameObject);
        }

        float delay = hitDelay;
        if (travelSpeed > 0f)
        {
            float startX = anchor == Anchor.RowCentre ? minX : areaPos.x;
            float distance = travelFromLeft ? transform.position.x - startX : Mathf.Abs(transform.position.x - areaPos.x);
            delay += Mathf.Max(0f, distance) / travelSpeed;
        }
        if (hit != null && delay > 0f) DelayAll(hit, delay);
        DestroyAfterSeconds destroy = GetComponent<DestroyAfterSeconds>();
        if (destroy != null) destroy.seconds += Mathf.CeilToInt(delay);
    }

    private static readonly HashSet<string> previewKept = new HashSet<string>();
    public static void ResetPreview() { previewKept.Clear(); }

    private List<Vector3> TargetPositions()
    {
        var list = new List<Vector3>();
        if (Application.isPlaying && companionRow)
        {
            foreach (CompanionInstance c in FindObjectsOfType<CompanionInstance>())
                if (c != null && c.gameObject.activeInHierarchy) list.Add(c.transform.position);
        }
        else if (Application.isPlaying)
        {
            foreach (EnemyInstance e in FindObjectsOfType<EnemyInstance>())
                if (e != null && e.gameObject.activeInHierarchy) list.Add(e.transform.position);
        }
        if (list.Count == 0 && PreviewTargets != null) list.AddRange(PreviewTargets);
        if (list.Count == 0) list.Add(transform.position);
        return list;
    }

    private static void DelayAll(Transform root, float delay)
    {
        foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = ps.main;
            ParticleSystem.MinMaxCurve d = main.startDelay;
            main.startDelay = d.mode == ParticleSystemCurveMode.TwoConstants
                ? new ParticleSystem.MinMaxCurve(d.constantMin + delay, d.constantMax + delay)
                : new ParticleSystem.MinMaxCurve(d.constant + delay);
        }
    }
}
