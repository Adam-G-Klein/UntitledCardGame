using System;
using System.Collections;
using UnityEngine;

/*
    Plays a "precast trail" that arcs from a caster to a target BEFORE the target's
    payload VFX fires. Used for "do something to another friendly" effects, i.e.
    ally -> ally and enemy -> enemy support (block, buff, strength, heal, etc.).

    Only the *visual* is deferred: the gameplay (block granted, hp healed) has already
    been applied by the effect step. This controller just delays the impact prefab
    until the trail lands.

    Hook points:
      - CombatEffectStep (heal to another ally, etc.)
      - InstantiatePrefab  (block / buff / strength status VFX)
    Both call ShouldTrail(caster, target); when true they hand their payload spawn
    to PlayTrailThenSpawn as an onArrival callback instead of spawning immediately.

    TIMING NOTE (important when tuning): the trail prefabs are particle systems with
    their own start delay, emission rate and particle lifetime. If the travel is faster
    than the prefab takes to become visible, the trail only "appears" near the target
    and reads as if it started in the wrong place. leadInDelay holds the trail at the
    caster so its intro plays there, and arcDuration should be long enough that the
    system is emitting for the whole journey.

    Setup: drop this component on a GameObject in the combat scene and assign the
    Ally / Enemy Precast Trail prefabs. If it is absent, ShouldTrail returns false
    everywhere and effects spawn their VFX immediately, exactly as before.
*/
public class PrecastTrailController : MonoBehaviour
{
    public static PrecastTrailController Instance { get; private set; }

    [Header("Trail prefabs (picked by the caster's team)")]
    [Tooltip("Plays when a COMPANION affects another companion.")]
    [SerializeField] private GameObject allyPrecastTrail;
    [Tooltip("Plays when an ENEMY affects another enemy.")]
    [SerializeField] private GameObject enemyPrecastTrail;

    [Header("Timing")]
    [Tooltip("Seconds the trail sits at the caster before travelling, so the prefab's " +
             "intro/start-delay plays AT the caster. Raise this to match the prefab's particle start delay.")]
    [SerializeField] private float leadInDelay = 0.3f;
    [Tooltip("Seconds for the trail to travel from caster to target. Too fast and a low " +
             "emission-rate prefab will not draw anything until it has nearly arrived.")]
    [SerializeField] private float arcDuration = 0.8f;
    [Tooltip("Seconds the trail lingers after arrival before it is destroyed (lets particles fade).")]
    [SerializeField] private float trailLinger = 0.4f;

    [Header("Arc shape")]
    [Tooltip("World-space height of the arc apex above the caster/target midpoint. Higher = more lofted toward screen center.")]
    [SerializeField] private float arcHeight = 2f;
    [Tooltip("Eases travel along the arc. X = normalized time [0,1], Y = progress along the path.")]
    [SerializeField] private AnimationCurve travelCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Rotate the trail so its local 'right' points along its direction of travel.")]
    [SerializeField] private bool orientToDirection = true;

    [Header("Placement")]
    [Tooltip("Pulls the trail toward the camera so it is not occluded by the board or the played card.")]
    [SerializeField] private float zOffset = -1f;
    [Tooltip("Uniform scale applied to the spawned trail. Other combat VFX are spawned at 5x by " +
             "InstantiatePrefab, so raise this if the trail looks small next to them.")]
    [SerializeField] private float trailScale = 1f;

    [Header("Debug")]
    [Tooltip("Logs the resolved caster/target and their world positions on every trail.")]
    [SerializeField] private bool logTrails = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // True when an effect should route through a precast trail: a controller exists,
    // both parties are combat instances on the same team, they are not the same
    // instance, and a trail prefab is assigned for that team.
    public static bool ShouldTrail(CombatInstance caster, CombatInstance target)
    {
        if (Instance == null || caster == null || target == null) return false;
        if (caster == target) return false;
        if (caster.parentType != target.parentType) return false;
        return Instance.TrailPrefabFor(caster) != null;
    }

    // Spawn the trail at the caster, arc it to the target, then invoke onArrival
    // (which spawns the actual payload VFX). Falls back to invoking onArrival
    // immediately if anything required is missing.
    public void PlayTrailThenSpawn(CombatInstance caster, CombatInstance target, Action onArrival)
    {
        GameObject trailPrefab = TrailPrefabFor(caster);
        if (trailPrefab == null || caster == null || target == null)
        {
            onArrival?.Invoke();
            return;
        }

        if (logTrails)
        {
            Debug.Log($"[PrecastTrail] {caster.gameObject.name} {caster.transform.position} -> " +
                      $"{target.gameObject.name} {target.transform.position}");
        }

        StartCoroutine(TrailRoutine(trailPrefab, caster.transform, target.transform, onArrival));
    }

    private GameObject TrailPrefabFor(CombatInstance caster)
    {
        if (caster == null) return null;
        return caster.parentType == CombatInstance.CombatInstanceParent.COMPANION
            ? allyPrecastTrail
            : enemyPrecastTrail;
    }

    private IEnumerator TrailRoutine(GameObject trailPrefab, Transform casterTf, Transform targetTf, Action onArrival)
    {
        Vector3 start = WithZOffset(casterTf.position);
        GameObject trail = Instantiate(trailPrefab, start, Quaternion.identity);
        if (!Mathf.Approximately(trailScale, 1f))
        {
            trail.transform.localScale *= trailScale;
        }

        // Hold at the caster so the prefab's start delay / intro burst reads as
        // originating from the caster rather than appearing mid-flight.
        float held = 0f;
        while (held < leadInDelay)
        {
            held += Time.deltaTime;
            if (trail == null) yield break;
            trail.transform.position = WithZOffset(casterTf != null ? casterTf.position : start);
            yield return null;
        }

        // Re-read the endpoints now, in case anything shifted during the lead-in.
        start = WithZOffset(casterTf != null ? casterTf.position : start);
        Vector3 end = WithZOffset(targetTf != null ? targetTf.position : start);

        // Quadratic bezier control point: lofted above the midpoint so the path
        // arcs up toward the center of the screen instead of travelling in a straight line.
        Vector3 control = (start + end) * 0.5f + Vector3.up * arcHeight;

        float t = 0f;
        Vector3 prev = start;
        while (t < 1f)
        {
            t += (arcDuration > 0f) ? Time.deltaTime / arcDuration : 1f;
            float progress = travelCurve.Evaluate(Mathf.Clamp01(t));
            Vector3 pos = QuadraticBezier(start, control, end, progress);

            if (trail == null) yield break;
            trail.transform.position = pos;
            if (orientToDirection)
            {
                Vector3 dir = pos - prev;
                dir.z = 0f;
                if (dir.sqrMagnitude > 1e-7f) trail.transform.right = dir.normalized;
            }
            prev = pos;
            yield return null;
        }

        if (trail != null) trail.transform.position = end;

        onArrival?.Invoke();

        if (trail != null) Destroy(trail, trailLinger);
    }

    private Vector3 WithZOffset(Vector3 p)
    {
        return new Vector3(p.x, p.y, p.z + zOffset);
    }

    private static Vector3 QuadraticBezier(Vector3 p0, Vector3 control, Vector3 p1, float t)
    {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * control) + (t * t * p1);
    }

    // Resolve the caster CombatInstance from an effect's ORIGIN (card / companion / enemy).
    // Mirrors the origin resolution used by CombatEffectStep / ApplyStatus.
    public static CombatInstance GetCasterFromDocument(EffectDocument document)
    {
        if (document == null) return null;

        if (document.map.ContainsValueWithKey<PlayableCard>(EffectDocument.ORIGIN))
        {
            PlayableCard card = document.map.GetItem<PlayableCard>(EffectDocument.ORIGIN, 0);
            return (card != null && card.deckFrom != null) ? card.deckFrom.combatInstance : null;
        }
        if (document.map.ContainsValueWithKey<CompanionInstance>(EffectDocument.ORIGIN))
        {
            CompanionInstance companion = document.map.GetItem<CompanionInstance>(EffectDocument.ORIGIN, 0);
            return companion != null ? companion.combatInstance : null;
        }
        if (document.map.ContainsValueWithKey<EnemyInstance>(EffectDocument.ORIGIN))
        {
            EnemyInstance enemy = document.map.GetItem<EnemyInstance>(EffectDocument.ORIGIN, 0);
            return enemy != null ? enemy.combatInstance : null;
        }
        return null;
    }
}
