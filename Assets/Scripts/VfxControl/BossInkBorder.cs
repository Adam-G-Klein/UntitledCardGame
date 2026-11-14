using UnityEngine;

/*
    The boss ink border: in every boss fight except Meothra's (the elite encounters, which end the acts; only
    Meothra has a boss controller, so isBoss is hers alone) the screen is framed by the preview's ink, bubbles and
    all, moving like pooled ink: its edge wobbles, and loose bubbles drift along the edges, merging into the ink and
    pinching off again (CS_InkPoolBorder). The whole border breathes a little.

    The party's health has its own border (LowHealthBorder), red and closer to the edges, which lies over this one
    when the party is low in a boss fight. Purely visual. CombatEntityManager.registerCompanion spawns it (once per
    fight) from the encounter constants; it hides itself outside boss fights.
*/
public class BossInkBorder : MonoBehaviour
{
    [Tooltip("Read for the active encounter: the border shows in elite encounters.")]
    [SerializeField] private GameStateVariableSO gameState;
    [Tooltip("The ink, a full-screen sprite drawn with CS_InkPoolBorder.")]
    [SerializeField] private SpriteRenderer ink;
    [Tooltip("How far in the ink's front sits, as a fraction of the screen's height.")]
    [SerializeField] private float depth = 0.045f;
    [Tooltip("The preview ink's average front depth in its texture, as a fraction of the screen's height; it's scaled out from the centre to sit at depth.")]
    [SerializeField] private float authoredDepth = 0.11f;
    [Tooltip("It breathes in and out by this much of the screen's height, every breatheTime seconds.")]
    [SerializeField] private float breatheDepth = 0.006f;
    [SerializeField] private float breatheTime = 6f;
    [SerializeField] private float fadeInTime = 1.5f;

    private static BossInkBorder instance;

    public static void Ensure(GameObject prefab)
    {
        if (prefab == null || instance != null) return;
        instance = Instantiate(prefab).GetComponent<BossInkBorder>();
    }

    private float alpha;
    private bool ruledOut;
    private MaterialPropertyBlock block;

    private void Start()
    {
        SetAlpha(ink, 0f);
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        bool show = cam != null && !CombatEntityManager.isDestroyed && !ruledOut && InBossFight();
        ink.enabled = show;
        if (!show) return;

        alpha = Mathf.MoveTowards(alpha, 1f, Time.unscaledDeltaTime / fadeInTime);
        SetAlpha(ink, alpha);
        float reach = depth + Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / breatheTime) * breatheDepth;
        Cover(ink, cam, reach, ref block);
        // the preview's ink, scaled out from the centre so its front sits `reach` in from every edge
        float screenH = cam.orthographicSize * 2f, screenW = screenH * cam.aspect;
        float ay = authoredDepth, ax = authoredDepth / TextureAspect, vy = Mathf.Min(reach, authoredDepth), vx = vy * screenH / screenW;
        ink.GetPropertyBlock(block);
        block.SetVector(FieldScaleId, new Vector4((0.5f - ax) / (0.5f - vx), (0.5f - ay) / (0.5f - vy), 0f, 0f));
        WorkOutBubbles(reach, cam.aspect);
        block.SetVectorArray(BubblesId, bubbles);
        block.SetFloat(BubbleReachId, bubbleReach);
        ink.SetPropertyBlock(block);
    }

    private const float TextureAspect = 16f / 9f;   // the preview ink's width / height (1920 x 1080)
    private static readonly int FieldScaleId = Shader.PropertyToID("_FieldScale"), BubblesId = Shader.PropertyToID("_Bubbles"), BubbleReachId = Shader.PropertyToID("_BubbleReach");
    private const int BubbleCount = 14;   // = BUBBLES in CS_InkPoolBorder
    private readonly Vector4[] bubbles = new Vector4[BubbleCount];
    private float bubbleReach;

    // The loose bubbles drifting along the edges, some each way, bobbing in and out of the ink: worked out here once a
    // frame so the full-screen shader only measures how close each pixel is to them.
    private void WorkOutBubbles(float reach, float aspect)
    {
        Material m = ink.sharedMaterial;
        Vector4 size = m.GetVector(BubbleSizeId), speed = m.GetVector(BubbleSpeedId), inDepth = m.GetVector(BubbleDepthId);
        float bob = m.GetFloat(BubbleBobId), t = Time.timeSinceLevelLoad;
        float deepest = 0f;
        for (int i = 0; i < BubbleCount; i++)
        {
            float h1 = Hash(i * 12.9898f + 1.3f), h2 = Hash(i * 78.233f + 2.1f), h3 = Hash(i * 39.425f + 3.7f), h4 = Hash(i * 23.17f + 4.9f);
            float s = Mathf.Repeat(h2 + t * (h1 < 0.5f ? -1f : 1f) * Mathf.Lerp(speed.x, speed.y, h3), 1f);
            float radius = Mathf.Lerp(size.x, size.y, h4 * h4);
            float inset = reach * Mathf.Lerp(inDepth.x, inDepth.y, h3) + Mathf.Sin(t * Mathf.Lerp(0.12f, 0.3f, h1) + h2 * 6.283f) * bob;
            Vector2 at = AroundEdge(s, inset, aspect);
            bubbles[i] = new Vector4(at.x, at.y, radius, 0f);
            deepest = Mathf.Max(deepest, inset + radius);
        }
        bubbleReach = deepest + m.GetFloat(RangeId) * (0.5f + m.GetFloat(GooId));   // past this, no bubble can touch a pixel
    }

    private static float Hash(float n) { float v = Mathf.Sin(n) * 43758.5453f; return v - Mathf.Floor(v); }

    // the point s (0..1) of the way round a rectangle inset from the screen's edges, in screen heights
    private static Vector2 AroundEdge(float s, float inset, float aspect)
    {
        float w = aspect - 2f * inset, h = 1f - 2f * inset, d = s * 2f * (w + h);
        if (d < w) return new Vector2(inset + d, inset);
        d -= w;
        if (d < h) return new Vector2(inset + w, inset + d);
        d -= h;
        if (d < w) return new Vector2(inset + w - d, 1f - inset);
        return new Vector2(inset, 1f - inset - (d - w));
    }

    private static readonly int BubbleSizeId = Shader.PropertyToID("_BubbleSize"), BubbleSpeedId = Shader.PropertyToID("_BubbleSpeed"), BubbleDepthId = Shader.PropertyToID("_BubbleDepth"),
        BubbleBobId = Shader.PropertyToID("_BubbleBob"), RangeId = Shader.PropertyToID("_Range"), GooId = Shader.PropertyToID("_Goo");

    // fits a full-screen ink layer over the camera and sets its front's depth
    public static void Cover(SpriteRenderer layer, Camera cam, float reach, ref MaterialPropertyBlock block)
    {
        if (block == null) block = new MaterialPropertyBlock();
        layer.transform.parent.position = new Vector3(cam.transform.position.x, cam.transform.position.y, UIDocumentGameObjectPlacer.zPlane);
        float screenH = cam.orthographicSize * 2f, screenW = screenH * cam.aspect;
        Vector3 size = layer.sprite.bounds.size;
        layer.transform.localScale = new Vector3(screenW / size.x, screenH / size.y, 1f);
        layer.GetPropertyBlock(block);
        block.SetFloat(ReachId, reach);
        block.SetFloat(AspectId, cam.aspect);
        layer.SetPropertyBlock(block);
    }

    public static void SetAlpha(SpriteRenderer layer, float a)
    {
        Color c = layer.color; c.a = a; layer.color = c;
    }

    private static readonly int ReachId = Shader.PropertyToID("_Reach"), AspectId = Shader.PropertyToID("_Aspect");

    private bool InBossFight()
    {
        if (gameState == null || !(gameState.activeEncounter.GetValue() is EnemyEncounter encounter) || !encounter.isEliteEncounter) return false;
        foreach (EnemyInstance enemy in CombatEntityManager.Instance.getEnemies())
        {
            if (enemy != null && enemy.enemy.enemyType.enemyDisplayType == DisplayType.MEOTHRA)
            {
                ruledOut = true;   // Meothra's fight has its own staging
                return false;
            }
        }
        return true;
    }
}
