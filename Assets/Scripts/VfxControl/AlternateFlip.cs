using UnityEngine;

/*
    Mirrors every other spawn of this effect left-to-right, so a multi-hit card (Double Slash) cuts one way
    and then back the other way instead of repeating the same stroke.
*/
public class AlternateFlip : MonoBehaviour
{
    [Tooltip("Spawns further apart than this start the pattern over (seconds).")]
    [SerializeField] private float resetAfter = 1.5f;

    private static float lastSpawn = -10f;
    private static bool flipNext;

    private void Awake()
    {
        if (Time.time - lastSpawn > resetAfter) flipNext = false;
        lastSpawn = Time.time;
        if (flipNext)
        {
            Vector3 s = transform.localScale; s.x = -s.x; transform.localScale = s;
            foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>(true))
            {
                // billboard particles ignore negative scale, so flip their rotation and sideways motion too
                ParticleSystem.MainModule m = ps.main;
                if (!m.startRotation3D) m.startRotationMultiplier = -m.startRotationMultiplier;
                ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
                if (r != null) r.flip = new Vector3(1f - r.flip.x, r.flip.y, r.flip.z);
                // motion set in world space isn't mirrored by the scale (e.g. the blade's lunge across the
                // target); without this the mirrored hit starts on the far side and flies off past the target
                ParticleSystem.VelocityOverLifetimeModule v = ps.velocityOverLifetime;
                if (v.enabled && v.space == ParticleSystemSimulationSpace.World) v.xMultiplier = -v.xMultiplier;
                ParticleSystem.ForceOverLifetimeModule f = ps.forceOverLifetime;
                if (f.enabled && f.space == ParticleSystemSimulationSpace.World) f.xMultiplier = -f.xMultiplier;
            }
        }
        flipNext = !flipNext;
    }
}
