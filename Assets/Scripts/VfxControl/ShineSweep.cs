using UnityEngine;

/*
    Sweeps a gloss band across a surface now and then, like light catching it. Moves the main texture's
    offset on this renderer's own material instance (the shared material is never touched): a quick
    eased pass from offStart to offEnd, then a pause before the next one.

    The default offsets suit CTX_PremiumShineBand at tiling 0.25: 0.22 is just off one edge and 0.56
    just past the other.
*/
public class ShineSweep : MonoBehaviour
{
    [SerializeField] private string textureProperty = "_MainTex";
    [SerializeField] private float offStart = 0.22f;
    [SerializeField] private float offEnd = 0.56f;
    [Tooltip("Seconds for one pass across.")]
    [SerializeField] private float sweepTime = 0.55f;
    [Tooltip("Random pause between passes (seconds).")]
    [SerializeField] private Vector2 pauseRange = new Vector2(3.5f, 5f);
    [SerializeField] private float firstDelay = 0.8f;

    private Renderer target;
    private Material instance;
    private float sweepStart;

    private void Awake()
    {
        target = GetComponent<Renderer>();
        if (target == null) { enabled = false; return; }
        instance = target.material;
        SetOffset(offStart);
        sweepStart = Time.time + firstDelay;
    }

    private void Update()
    {
        float t = (Time.time - sweepStart) / Mathf.Max(sweepTime, 0.01f);
        if (t < 0f) return;
        if (t >= 1f)
        {
            SetOffset(offStart);
            sweepStart = Time.time + Random.Range(pauseRange.x, pauseRange.y);
            return;
        }
        SetOffset(Mathf.Lerp(offStart, offEnd, t * t * (3f - 2f * t)));
    }

    private void SetOffset(float x)
    {
        Vector2 o = instance.GetTextureOffset(textureProperty);
        instance.SetTextureOffset(textureProperty, new Vector2(x, o.y));
    }

    private void OnDestroy()
    {
        if (instance != null) Destroy(instance);
    }
}
