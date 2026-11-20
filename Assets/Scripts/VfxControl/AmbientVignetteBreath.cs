using UnityEngine;

/*
    VFX: an ambient screen vignette (the elite fight's Elite Screen Vignette) eases in when it's spawned and then
    breathes slowly, its layers' opacity rising and falling a little, so it reads as mood rather than a fixed overlay.
    Each layer is one long-lived particle, so this sets that particle's alpha (the master shader's lerp path ignores
    the material's _Color). Purely visual; the vignette is spawned by EliteEnemyFlair.
*/
public class AmbientVignetteBreath : MonoBehaviour
{
    [Tooltip("Seconds to ease in from nothing when the vignette appears.")]
    [SerializeField] private float fadeInTime = 2.5f;
    [Tooltip("Seconds for one slow breath (out and back).")]
    [SerializeField] private float breathTime = 9f;
    [Tooltip("How far the opacity dips at the bottom of a breath (0.2 = to 80%).")]
    [Range(0f, 1f)] [SerializeField] private float breathDepth = 0.25f;

    private ParticleSystem[] layers;
    private float[] baseAlpha;
    private ParticleSystem.Particle[] buffer = new ParticleSystem.Particle[4];
    private float startTime;

    private void Start()
    {
        layers = GetComponentsInChildren<ParticleSystem>();
        baseAlpha = new float[layers.Length];
        for (int i = 0; i < layers.Length; i++) baseAlpha[i] = layers[i].main.startColor.color.a;
        startTime = Time.time;
    }

    private void LateUpdate()
    {
        if (layers == null) return;
        float t = Time.time - startTime;
        float fade = fadeInTime > 0f ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / fadeInTime)) : 1f;
        float breath = 1f - breathDepth * 0.5f * (1f - Mathf.Cos(t * Mathf.PI * 2f / Mathf.Max(0.1f, breathTime)));
        for (int i = 0; i < layers.Length; i++)
        {
            ParticleSystem ps = layers[i];
            if (ps == null) continue;
            int n = ps.GetParticles(buffer);
            if (n == 0) continue;
            for (int p = 0; p < n; p++)
            {
                Color32 c = buffer[p].startColor;
                c.a = (byte)Mathf.Clamp(Mathf.RoundToInt(baseAlpha[i] * fade * breath * 255f), 0, 255);
                buffer[p].startColor = c;
            }
            ps.SetParticles(buffer, n);
        }
    }
}
