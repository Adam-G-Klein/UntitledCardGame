using System.Collections.Generic;
using UnityEngine;

/*
    The low-health border: when the party is low (its health summed over every ally, current over max, a fallen
    ally counting as 0) red watercolour wicks in from the screen's edges, its fibres creeping inward (CS_InkBorder,
    with the low-health texture). It comes in below showBelow, reaches further and darkens as health drops, and
    beats like a heart below pulseBelow.

    It sits closer to the edges than the boss ink border (BossInkBorder) and is drawn over it, so in a boss fight
    the red lies inside the dark ink. It shows in every fight. Health is read through HitTiming.ShownHealth, so it
    moves with the health bars rather than ahead of them. Purely visual. CombatEntityManager.registerCompanion
    spawns it (once per fight) from the encounter constants.
*/
public class LowHealthBorder : MonoBehaviour
{
    [Tooltip("The blood, a full-screen sprite drawn with CS_InkBorder.")]
    [SerializeField] private SpriteRenderer ink;
    [Tooltip("Party health below which it shows.")]
    [SerializeField] private float showBelow = 0.5f;
    [Tooltip("The front's depth as it comes in, and at no health left, as fractions of the screen's height.")]
    [SerializeField] private float minDepth = 0.015f;
    [SerializeField] private float maxDepth = 0.04f;
    [Tooltip("Its opacity as it comes in; it's fully opaque at no health left.")]
    [SerializeField] private float minAlpha = 0.55f;
    [Tooltip("Below this party health it beats, pushing in by this much of the screen's height on each beat.")]
    [SerializeField] private float pulseBelow = 0.25f;
    [SerializeField] private float pulseDepth = 0.01f;
    [Tooltip("Seconds between heartbeats, at pulseBelow and at no health left.")]
    [SerializeField] private float beatTime = 1.2f;
    [SerializeField] private float fastBeatTime = 0.75f;
    [Tooltip("Seconds it takes to follow a change in health, and to fade in or out.")]
    [SerializeField] private float settleTime = 0.5f;
    [SerializeField] private float fadeTime = 0.6f;

    private static LowHealthBorder instance;

    public static void Ensure(GameObject prefab)
    {
        if (prefab == null || instance != null) return;
        instance = Instantiate(prefab).GetComponent<LowHealthBorder>();
    }

    private readonly Dictionary<CombatInstance, int> maxHealth = new Dictionary<CombatInstance, int>();
    private float depth, depthVelocity, shown, beat;
    private MaterialPropertyBlock block;

    private void Start()
    {
        depth = minDepth;
        BossInkBorder.SetAlpha(ink, 0f);
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null || CombatEntityManager.isDestroyed) { ink.enabled = false; return; }

        float health = PartyHealth();
        float low = health < showBelow ? 1f - health / showBelow : 0f;   // 0 as it comes in, 1 at no health left
        shown = Mathf.MoveTowards(shown, health < showBelow ? 1f : 0f, Time.unscaledDeltaTime / fadeTime);
        ink.enabled = shown > 0f;
        if (!ink.enabled) return;

        float target = Mathf.Lerp(minDepth, maxDepth, low * (2f - low));   // quick at first, so dropping below half reads
        depth = Mathf.SmoothDamp(depth, target, ref depthVelocity, settleTime, Mathf.Infinity, Time.unscaledDeltaTime);
        float pulse = 0f;
        if (health < pulseBelow)
        {
            float k = 1f - health / pulseBelow;
            beat += Time.unscaledDeltaTime / Mathf.Lerp(beatTime, fastBeatTime, k);
            pulse = Heartbeat(beat % 1f) * pulseDepth * Mathf.Lerp(0.5f, 1f, k);
        }
        BossInkBorder.SetAlpha(ink, shown * Mathf.Lerp(minAlpha, 1f, low));
        BossInkBorder.Cover(ink, cam, depth + pulse, ref block);
    }

    // a heartbeat over one beat: a strong thump, a softer second one, then rest; each thump eases in and out (a sine
    // squared), so the border swells rather than jerks
    private static float Heartbeat(float t)
    {
        float Thump(float at, float width) { float x = (t - at) / width; if (x < 0f || x > 1f) return 0f; float s = Mathf.Sin(x * Mathf.PI); return s * s; }
        return Mathf.Max(Thump(0f, 0.24f), 0.6f * Thump(0.22f, 0.26f));
    }

    private float PartyHealth()
    {
        foreach (CompanionInstance companion in CombatEntityManager.Instance.getCompanions())
        {
            if (companion == null || companion.combatInstance == null) continue;
            CombatInstance ci = companion.combatInstance;
            maxHealth[ci] = Mathf.Max(ci.combatStats.maxHealth, maxHealth.TryGetValue(ci, out int seen) ? seen : 0);
        }
        int current = 0, max = 0;
        foreach (KeyValuePair<CombatInstance, int> ally in maxHealth)
        {
            max += ally.Value;
            if (ally.Key != null) current += Mathf.Max(0, HitTiming.ShownHealth(ally.Key));   // a fallen ally counts as 0
        }
        return max > 0 ? Mathf.Clamp01((float)current / max) : 1f;
    }
}
