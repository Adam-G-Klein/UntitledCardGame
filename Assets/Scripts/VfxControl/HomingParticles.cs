using UnityEngine;

/*
    Steers this effect's particles into a target point (ink trails flying from where Élan came from into the
    Élan emblem). Particles burst out as authored, then curve in faster and faster, and are retired on arrival
    so their trails finish at the target. The particle system must simulate in world space.
*/
[RequireComponent(typeof(ParticleSystem))]
public class HomingParticles : MonoBehaviour
{
    [Tooltip("Roughly how long a particle takes to arrive.")]
    [SerializeField] private float travelTime = 0.5f;
    [Tooltip("How sharply particles turn toward the target.")]
    [SerializeField] private float steer = 9f;

    public Vector3 target;
    private ParticleSystem ps;
    private ParticleSystem.Particle[] buffer;

    private void Awake() { ps = GetComponent<ParticleSystem>(); }

    private void LateUpdate()
    {
        if (buffer == null || buffer.Length < ps.main.maxParticles) buffer = new ParticleSystem.Particle[ps.main.maxParticles];
        int n = ps.GetParticles(buffer);
        float dt = Time.deltaTime;
        for (int i = 0; i < n; i++)
        {
            ParticleSystem.Particle p = buffer[i];
            Vector3 to = target - p.position; to.z = 0f;
            float dist = to.magnitude;
            if (dist < 0.25f) { p.remainingLifetime = 0f; buffer[i] = p; continue; }
            float age = p.startLifetime - p.remainingLifetime;
            float urge = Mathf.Clamp01(age / Mathf.Max(travelTime, 0.05f));
            float speed = Mathf.Lerp(4f, dist / Mathf.Max(travelTime * 0.35f, 0.05f), urge);
            p.velocity = Vector3.Lerp(p.velocity, to / dist * speed, Mathf.Clamp01(steer * (0.3f + urge) * dt));
            buffer[i] = p;
        }
        ps.SetParticles(buffer, n);
    }
}
