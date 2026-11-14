using UnityEngine;

/*
    Mirrors every other spawn of this effect left-to-right, so a multi-hit card (Double Slash) cuts one way
    and then back the other way instead of repeating the same stroke. With alwaysMirror on it mirrors every spawn
    (Dance of Steel: the single cut always comes from the right).
*/
public class AlternateFlip : MonoBehaviour
{
    [Tooltip("Spawns further apart than this start the pattern over (seconds).")]
    [SerializeField] private float resetAfter = 1.5f;
    [Tooltip("Mirror every spawn instead of every other one.")]
    [SerializeField] private bool alwaysMirror = false;

    private static float lastSpawn = -10f;
    private static bool flipNext;

    private void Awake()
    {
        bool mirror;
        if (alwaysMirror) mirror = true;   // leaves the alternating pattern of the other effects alone
        else
        {
            if (Time.time - lastSpawn > resetAfter) flipNext = false;
            lastSpawn = Time.time;
            mirror = flipNext;
            flipNext = !flipNext;
        }
        if (mirror)
        {
            Vector3 s = transform.localScale; s.x = -s.x; transform.localScale = s;
            foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>(true))
            {
                // billboard particles ignore negative scale, so flip their rotation and sideways motion too
                ParticleSystem.MainModule m = ps.main;
                if (!m.startRotation3D)
                {
                    // the multiplier only scales one end of a min-max pair, which would make a fixed angle random
                    ParticleSystem.MinMaxCurve sr = m.startRotation;
                    if (sr.mode == ParticleSystemCurveMode.TwoConstants) { float lo = sr.constantMin; sr.constantMin = -sr.constantMax; sr.constantMax = -lo; }
                    else if (sr.mode == ParticleSystemCurveMode.Constant) sr.constant = -sr.constant;
                    else sr.curveMultiplier = -sr.curveMultiplier;
                    m.startRotation = sr;
                }
                ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
                if (r != null) r.flip = new Vector3(1f - r.flip.x, r.flip.y, r.flip.z);
                // motion set in world space isn't mirrored by the scale (e.g. the blade's lunge across the
                // target); without this the mirrored hit starts on the far side and flies off past the target
                ParticleSystem.VelocityOverLifetimeModule v = ps.velocityOverLifetime;
                if (v.enabled && v.space == ParticleSystemSimulationSpace.World) v.xMultiplier = -v.xMultiplier;
                ParticleSystem.ForceOverLifetimeModule f = ps.forceOverLifetime;
                if (f.enabled && f.space == ParticleSystemSimulationSpace.World) f.xMultiplier = -f.xMultiplier;
                // a swung weapon (VFXRetoolBuilder.SwingLayer) turns about its grip: mirror the turn and the grip too,
                // so the backhand swings the other way about the same hand
                ParticleSystem.RotationOverLifetimeModule rot = ps.rotationOverLifetime;
                if (rot.enabled && !rot.separateAxes) rot.zMultiplier = -rot.zMultiplier;
                if (r != null && r.pivot.x != 0f) r.pivot = new Vector3(-r.pivot.x, r.pivot.y, r.pivot.z);
            }
        }
    }
}
