using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/*
    Slow, soft moving lights for a combat board. Each entry drives one point light along an eased path
    with eased intensity, so highlights travel across the board's masked ornaments and the cards in hand
    without anything popping. Everything fades up from zero when the board appears.

      Sway   - pendulum between two points (a hanging lamp).
      Glide  - travels from A to B, fading in at the start and out at the end, then rests before repeating.
      Orbit  - slow ellipse around a centre (centre = A, radii = B.xy).

    Positions are local to this transform. Times are in seconds of scaled time, so hit-stop and the
    game-speed setting carry through naturally.
*/
public class BoardLightRig : MonoBehaviour
{
    public enum Path { Sway, Glide, Orbit }

    [Serializable]
    public class MovingLight
    {
        public string name = "Light";
        public Path path = Path.Sway;
        public Vector3 a = new Vector3(-4f, 2.5f, -3.5f);
        public Vector3 b = new Vector3(4f, 2.5f, -3.5f);
        [Tooltip("Seconds for one full cycle. For Glide this is the travel time.")]
        public float period = 9f;
        [Tooltip("Glide only: seconds of darkness between passes.")]
        public float rest = 3f;
        [Tooltip("Offset into the cycle (0-1) so several lights don't line up.")]
        [Range(0f, 1f)] public float phase = 0f;
        public Color color = new Color(1f, 0.78f, 0.45f);
        public float intensity = 2f;
        [Tooltip("Slow breathing of the intensity, 0 = steady.")]
        [Range(0f, 1f)] public float breathDepth = 0.25f;
        public float breathPeriod = 5f;
        public float range = 8f;
        [Tooltip("Board glow under this light (first four lights only): radius in board heights, 0 = none.")]
        public float poolRadius = 0.25f;
        [Tooltip("Board glow strength at full intensity.")]
        public float poolStrength = 1f;
        [Tooltip("0 = glow only on the highlight mask (ornaments), 1 = glow on the whole surface (a broad wash).")]
        [Range(0f, 1f)] public float poolFill = 0f;
        [Tooltip("Blend the light colour toward a slowly drifting rainbow (0 = steady colour).")]
        [Range(0f, 1f)] public float rainbow = 0f;
        public float rainbowPeriod = 20f;
        [Tooltip("Light only the cards (the card layer), never the board, so it can't take the board's per-object light slots.")]
        public bool cardsOnly = false;

        [NonSerialized] public Light light;
    }

    [SerializeField] private List<MovingLight> lights = new List<MovingLight>();
    [Tooltip("Seconds for everything to fade up when the board appears.")]
    [SerializeField] private float fadeIn = 2f;

    [Header("Board glow (CS_BoardLighting)")]
    [Tooltip("The board sprite. Each of the first four lights paints a soft pool onto it.")]
    [SerializeField] private SpriteRenderer board;

    [Tooltip("Layer the worldspace cards render on; cards-only lights are culled to it.")]
    [SerializeField] private string cardLayer = "WorldCards";
    [Tooltip("Rendering layer the cards receive light on (WorldspaceCard's Rendering Layer Mask). Rig lights include it so the board's lights reach the cards; cards-only lights use only it.")]
    [SerializeField] private uint cardRenderingLayers = 2;

    [Header("Existing key light (optional)")]
    [Tooltip("The board's static key light. Breathes gently so the room never feels frozen.")]
    [SerializeField] private Light keyLight;
    [Range(0f, 0.5f)] [SerializeField] private float keyBreathDepth = 0.06f;
    [SerializeField] private float keyBreathPeriod = 6f;

    private float keyBase;
    private float startTime;
    private MaterialPropertyBlock block;
    private static readonly int[] PoolIds = { Shader.PropertyToID("_Pool0"), Shader.PropertyToID("_Pool1"), Shader.PropertyToID("_Pool2"), Shader.PropertyToID("_Pool3") };
    private static readonly int[] PoolColorIds = { Shader.PropertyToID("_Pool0_Color"), Shader.PropertyToID("_Pool1_Color"), Shader.PropertyToID("_Pool2_Color"), Shader.PropertyToID("_Pool3_Color") };

    private void OnEnable()
    {
        startTime = Time.time;
        if (keyLight != null) keyBase = keyLight.intensity;
        int cardLayerIndex = LayerMask.NameToLayer(cardLayer);
        foreach (MovingLight m in lights)
        {
            if (m.light == null)
            {
                GameObject go = new GameObject("Rig " + m.name);
                go.transform.SetParent(transform, false);
                m.light = go.AddComponent<Light>();
                m.light.type = LightType.Point;
                m.light.shadows = LightShadows.None;
                m.light.renderMode = LightRenderMode.ForcePixel;
            }
            m.light.color = m.color;
            m.light.range = m.range;
            m.light.intensity = 0f;
            m.light.cullingMask = m.cardsOnly && cardLayerIndex >= 0 ? 1 << cardLayerIndex : ~0;
            m.light.GetUniversalAdditionalLightData().renderingLayers = m.cardsOnly ? cardRenderingLayers : ~0u;
        }
    }

    private void OnDisable()
    {
        if (keyLight != null) keyLight.intensity = keyBase;
        if (board != null && block != null)
        {
            foreach (int id in PoolIds) block.SetVector(id, Vector4.zero);
            board.SetPropertyBlock(block);
        }
    }

    private void Update()
    {
        float t = Time.time - startTime;
        float fade = Ease(Mathf.Clamp01(t / Mathf.Max(fadeIn, 0.01f)));

        if (keyLight != null)
            keyLight.intensity = keyBase * (1f - keyBreathDepth * 0.5f * (1f - Mathf.Cos(t * 2f * Mathf.PI / Mathf.Max(keyBreathPeriod, 0.01f))));

        foreach (MovingLight m in lights)
        {
            if (m.light == null) continue;
            float envelope = 1f;
            Vector3 p;
            switch (m.path)
            {
                case Path.Glide:
                {
                    float cycle = m.period + m.rest;
                    float u = Mathf.Repeat(t / cycle + m.phase, 1f) * cycle / Mathf.Max(m.period, 0.01f);
                    if (u > 1f) { envelope = 0f; u = 1f; }
                    else envelope = Mathf.Sin(u * Mathf.PI);            // soft in and out over the pass
                    p = Vector3.Lerp(m.a, m.b, Ease(u));
                    break;
                }
                case Path.Orbit:
                {
                    float ang = (t / Mathf.Max(m.period, 0.01f) + m.phase) * 2f * Mathf.PI;
                    p = m.a + new Vector3(Mathf.Cos(ang) * m.b.x, Mathf.Sin(ang) * m.b.y, 0f);
                    break;
                }
                default:
                {
                    float s = 0.5f - 0.5f * Mathf.Cos((t / Mathf.Max(m.period, 0.01f) + m.phase) * 2f * Mathf.PI);
                    p = Vector3.Lerp(m.a, m.b, s);
                    break;
                }
            }
            float breath = 1f - m.breathDepth * 0.5f * (1f - Mathf.Cos((t / Mathf.Max(m.breathPeriod, 0.01f) + m.phase) * 2f * Mathf.PI));
            m.light.transform.localPosition = p;
            m.light.intensity = m.intensity * envelope * breath * fade;
            m.light.color = CurrentColor(m, t);
        }
        UpdateBoardPools();
    }

    private void UpdateBoardPools()
    {
        if (board == null) return;
        if (block == null) block = new MaterialPropertyBlock();
        board.GetPropertyBlock(block);
        Bounds b = board.bounds;
        for (int i = 0; i < PoolIds.Length; i++)
        {
            Vector4 pool = Vector4.zero;
            if (i < lights.Count && lights[i].light != null && lights[i].poolRadius > 0f && !lights[i].cardsOnly)
            {
                MovingLight m = lights[i];
                Vector3 wp = m.light.transform.position;
                float u = Mathf.InverseLerp(b.min.x, b.max.x, wp.x);
                float v = Mathf.InverseLerp(b.min.y, b.max.y, wp.y);
                float strength = m.intensity > 0f ? m.light.intensity / m.intensity : 0f;   // follows envelope, breath and fade
                pool = new Vector4(u, v, m.poolRadius, strength * m.poolStrength);
                Color c = m.light.color; c.a = m.poolFill;
                block.SetColor(PoolColorIds[i], c);
            }
            block.SetVector(PoolIds[i], pool);
        }
        board.SetPropertyBlock(block);
    }

    private static float Ease(float x) => x * x * (3f - 2f * x);

    private static Color CurrentColor(MovingLight m, float t)
    {
        if (m.rainbow <= 0f) return m.color;
        float h = Mathf.Repeat(t / Mathf.Max(m.rainbowPeriod, 0.01f) + m.phase, 1f);
        Color.RGBToHSV(m.color, out _, out float s, out float v);
        Color hue = Color.HSVToRGB(h, Mathf.Max(s, 0.35f), v);
        return Color.Lerp(m.color, hue, m.rainbow);
    }
}
