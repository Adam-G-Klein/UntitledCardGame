using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Hit timing: the hit feedback lands when the effect visibly strikes, not when it's spawned.

    The game applies a card's damage the moment the step runs, and used to play all the hit feedback with it:
    the hit sound and voice bark, damage number, hurt shake, screen shake, hit-stop, block absorb/break, and
    the health bar. Many effects wind up first (a sword swing, a thrown flask, a wave crossing the row, a
    precast trail), so you'd hear and see the hit before the effect got there.

    Put this component on an effect prefab and set impactAt to the second it strikes. When the effect is spawned
    on a target, that target's feedback waits until then. GAMEPLAY IS UNCHANGED: health, block, triggers,
    turn flow and death resolve exactly when they did. Only the feedback (and what the health bar shows) waits.

      - No HitTiming on the prefab, or impactAt 0: feedback plays immediately, exactly as before.
      - AreaEffect prefabs (hit every enemy) add their own wait per target, so each enemy reacts as the
        wave reaches it.
      - Precast trails (ally -> ally) add the trail's flight time.
      - A multi-hit from one effect keeps its rhythm: every hit shifts by the same amount as the first.
      - A target that dies waits for its pending hits before its death effect plays.
      - Off switch: Encounter Constants > Sync Hit Feedback To VFX.

    Hooks (each one line, marked "HitTiming"): CombatInstance (spawns, damage/heal/status feedback, death),
    InstantiatePrefab and CombatEffectStep (effect spawns), EnemyView / CompanionView (health bar).
*/
public class HitTiming : MonoBehaviour
{
    [Tooltip("Seconds after the effect spawns that it visibly hits its target. The hit sound, damage number, " +
             "shake and health bar wait until then.")]
    [SerializeField] private float impactAt = 0f;

    // A hit played this long after the first hit from the same effect keeps the first one's shift (multi-hits).
    private const float MultiHitWindow = 1.2f;
    // An effect nobody took feedback from is forgotten this long after it lands.
    private const float ForgetAfterLanding = 0.25f;
    // Safety cap so nothing waits forever (a dying target, a held health bar).
    private const float MaxWait = 3f;

    private class Expected { public float landsAt; public float shift = -1f; public float takenAt; }
    private class Held { public float landsAt; public int health; }

    private static readonly Dictionary<CombatInstance, List<Expected>> expected = new Dictionary<CombatInstance, List<Expected>>();
    private static readonly Dictionary<CombatInstance, List<Held>> heldHealth = new Dictionary<CombatInstance, List<Held>>();
    private static readonly Dictionary<CombatInstance, int> pending = new Dictionary<CombatInstance, int>();

    private static bool Enabled
    {
        get
        {
            // isDestroyed is also true before anything has asked for the manager, so look it up then
            // (Instance would create an empty one outside combat)
            CombatEntityManager manager = CombatEntityManager.isDestroyed
                ? FindObjectOfType<CombatEntityManager>() : CombatEntityManager.Instance;
            return manager != null && manager.encounterConstants != null && manager.encounterConstants.syncHitFeedbackToVfx;
        }
    }

    // Seconds until this effect strikes: its impactAt, plus the AreaEffect wait for this copy's target.
    // Works on a prefab asset too (no AreaEffect wait then, since that's worked out when it spawns).
    public static float LandsIn(GameObject effect)
    {
        if (effect == null) return 0f;
        float t = effect.TryGetComponent(out HitTiming timing) ? timing.impactAt : 0f;
        if (effect.TryGetComponent(out AreaEffect area)) t += area.TargetHitDelay;
        return Mathf.Max(0f, t);
    }

    // Called where an effect is spawned on a target. extraDelay covers anything before the spawn (a precast trail).
    public static void Expect(CombatInstance target, GameObject effect, float extraDelay = 0f)
    {
        if (target == null) return;
        if (expected.Count > 64) ForgetDestroyed();
        if (!expected.TryGetValue(target, out List<Expected> list)) expected[target] = list = new List<Expected>();
        Prune(list);
        list.Add(new Expected { landsAt = Time.time + extraDelay + LandsIn(effect) });
    }

    // How long this target's feedback should wait. Called by CombatInstance as it applies a hit.
    public static float Take(CombatInstance target)
    {
        if (!Enabled || target == null || !expected.TryGetValue(target, out List<Expected> list)) return 0f;
        Prune(list);
        Expected next = list.Find(e => e.shift < 0f);
        if (next != null)
        {
            next.shift = Mathf.Min(MaxWait, Mathf.Max(0f, next.landsAt - Time.time));
            next.takenAt = Time.time;
            return next.shift;
        }
        Expected last = list.FindLast(e => e.shift >= 0f);
        return last != null ? last.shift : 0f;
    }

    // The wait of the hit just applied to this target (from Take), for extra feedback played after it, e.g. the
    // per-card attack sound in CombatEffectStep. 0 when nothing is pending.
    public static float CurrentDelay(CombatInstance target)
    {
        if (!Enabled || target == null || !expected.TryGetValue(target, out List<Expected> list)) return 0f;
        Prune(list);
        Expected last = list.FindLast(e => e.shift >= 0f);
        return last != null ? last.shift : 0f;
    }

    private static void Prune(List<Expected> list)
    {
        float now = Time.time;
        list.RemoveAll(e => e.shift < 0f ? now > e.landsAt + ForgetAfterLanding : now > e.takenAt + MultiHitWindow);
    }

    // Keeps the health bar showing the target's current health until delay passes. Call it just before
    // the health changes; the feedback passed to AtImpact refreshes the bar when the hit lands.
    public static void HoldHealthBar(CombatInstance target, float delay)
    {
        if (target == null || delay <= 0f || !target.isActiveAndEnabled) return;
        if (!heldHealth.TryGetValue(target, out List<Held> holds)) heldHealth[target] = holds = new List<Held>();
        holds.Add(new Held { landsAt = Time.time + delay, health = target.combatStats.getCurrentHealth() });
        holds.Sort((a, b) => a.landsAt.CompareTo(b.landsAt));
    }

    // Plays feedback on the target after delay (now, if 0 or the feature is off).
    public static void AtImpact(CombatInstance target, float delay, Action feedback)
    {
        if (target == null) return;
        if (delay <= 0f || !target.isActiveAndEnabled)
        {
            feedback?.Invoke();
            return;
        }
        pending[target] = pending.TryGetValue(target, out int n) ? n + 1 : 1;
        target.StartCoroutine(Land(target, delay, feedback));
    }

    private static IEnumerator Land(CombatInstance target, float delay, Action feedback)
    {
        yield return new WaitForSeconds(delay);
        if (pending.TryGetValue(target, out int n)) { if (n <= 1) pending.Remove(target); else pending[target] = n - 1; }
        feedback?.Invoke();
    }

    // What the health bar should show: the health from before the earliest hit that hasn't landed yet.
    public static int ShownHealth(CombatInstance target)
    {
        if (target == null) return 0;
        if (heldHealth.TryGetValue(target, out List<Held> holds))
        {
            holds.RemoveAll(h => Time.time >= h.landsAt - 0.02f);   // a frame's rounding early is fine
            if (holds.Count > 0) return holds[0].health;
        }
        return target.combatStats.getCurrentHealth();
    }

    // For death: wait for this target's hits to land before its death effect plays.
    public static IEnumerator WaitForPendingHits(CombatInstance target)
    {
        float until = Time.time + MaxWait;
        while (target != null && pending.ContainsKey(target) && Time.time < until) yield return null;
    }

    // Drops targets that have been destroyed (their coroutines stopped with them).
    private static void ForgetDestroyed()
    {
        foreach (CombatInstance gone in new List<CombatInstance>(expected.Keys)) if (gone == null) expected.Remove(gone);
        foreach (CombatInstance gone in new List<CombatInstance>(heldHealth.Keys)) if (gone == null) heldHealth.Remove(gone);
        foreach (CombatInstance gone in new List<CombatInstance>(pending.Keys)) if (gone == null) pending.Remove(gone);
    }
}
