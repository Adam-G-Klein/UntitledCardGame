using UnityEngine;

/*
    Steers this effect's particles into a target point (ink trails flying from where Élan came from into the
    Élan emblem). Particles burst out as authored, then curve in faster and faster, and are retired on arrival
    so their trails finish at the target. The particle system must simulate in world space.
    On arrival a particle stops on the target and lives a moment longer (settleTime), so its trail flows all the way in
    rather than vanishing with it short of the target.
*/
[RequireComponent(typeof(ParticleSystem))]
public class HomingParticles : MonoBehaviour
{
    [Tooltip("Roughly how long a particle takes to arrive.")]
    [SerializeField] private float travelTime = 0.5f;
    [Tooltip("How sharply particles turn toward the target.")]
    [SerializeField] private float steer = 9f;
    [Tooltip("Seconds an arrived particle sits on the target, so its trail catches up into it.")]
    [SerializeField] private float settleTime = 0.3f;

    public Vector3 target;
    private ParticleSystem ps;
    private static ParticleSystem.Particle[] buffer;   // only used within one LateUpdate, so they can all share it

    private void Awake() { ps = GetComponent<ParticleSystem>(); }

    private void LateUpdate()
    {
        if (ps.particleCount == 0) return;
        if (buffer == null || buffer.Length < ps.particleCount) buffer = new ParticleSystem.Particle[Mathf.Max(32, ps.particleCount)];
        int n = ps.GetParticles(buffer);
        float dt = Time.deltaTime;
        for (int i = 0; i < n; i++)
        {
            ParticleSystem.Particle p = buffer[i];
            Vector3 to = target - p.position; to.z = 0f;
            float dist = to.magnitude;
            if (dist < 0.25f)
            {
                p.position = new Vector3(target.x, target.y, p.position.z);
                p.velocity = Vector3.zero;
                p.remainingLifetime = Mathf.Min(p.remainingLifetime, settleTime);
                buffer[i] = p;
                continue;
            }
            float age = p.startLifetime - p.remainingLifetime;
            float urge = Mathf.Clamp01(age / Mathf.Max(travelTime, 0.05f));
            float speed = Mathf.Lerp(4f, dist / Mathf.Max(travelTime * 0.35f, 0.05f), urge);
            p.velocity = Vector3.Lerp(p.velocity, to / dist * speed, Mathf.Clamp01(steer * (0.3f + urge) * dt));
            buffer[i] = p;
        }
        ps.SetParticles(buffer, n);
    }
}
