using UnityEngine;

/*
    Delays every particle system in this effect by where it spawned across the screen, so an effect that
    the game spawns on each target at once (e.g. an attack that hits all enemies) rolls across the row as
    one sweep instead of popping everywhere together. Purely visual: the effect still spawns when it did.
*/
public class SweepDelayByPosition : MonoBehaviour
{
    [Tooltip("World X where the sweep starts (no delay) and ends (full delay).")]
    [SerializeField] private float fromX = -6.5f;
    [SerializeField] private float toX = 6.5f;
    [Tooltip("Seconds between the first and the last tile.")]
    [SerializeField] private float sweepSeconds = 0.45f;

    private void Awake()
    {
        float t = Mathf.Clamp01(Mathf.InverseLerp(fromX, toX, transform.position.x));
        float delay = t * sweepSeconds;
        if (delay <= 0f) return;
        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = ps.main;
            ParticleSystem.MinMaxCurve d = main.startDelay;
            if (d.mode == ParticleSystemCurveMode.TwoConstants) { d.constantMin += delay; d.constantMax += delay; }
            else d = new ParticleSystem.MinMaxCurve(d.constant + delay);
            main.startDelay = d;
        }
        DestroyAfterSeconds destroy = GetComponent<DestroyAfterSeconds>();
        if (destroy != null) destroy.seconds += Mathf.CeilToInt(delay);
    }
}
