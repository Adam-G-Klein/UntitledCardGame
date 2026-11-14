using System;
using UnityEngine;
using UnityEngine.UIElements;

/*
    Booster pack opening: the beat between buying a pack in the shop and its cards flying out.

    The screen darkens and the envelope flies from its slot to the centre of the screen, growing as it goes. It
    trembles there while curved trails sweep into it, then it bursts and the cards are thrown out of the centre.

      Basic   - one wave of gold trails, a halo and a small burst (about 0.8s).
      Premium - three colourful waves, halos pulling in on the seal as it throbs, then a big burst (about 1.3s).

    ShopViewController.AnimateBoosterPackOpening calls Play() and holds the cards back until it returns, starting
    them from StageCentre. Purely visual: the purchase and the cards are already decided. The shop's UI is drawn into
    a render texture, which can't be dimmed around one element, so for the opening the envelope is lifted out of the
    UI: it's redrawn as a world-space sprite (the shop clears the slot itself), with a world-space dim between it and
    the shop. The effect prefabs (VFXRetoolBuilder: VFX Retool / Build pack opening) sort around the two.
    Settings live on ShopViewController (ShopManager prefab) under "Booster Pack Opening".
*/
[Serializable]
public class BoosterPackOpeningVFX
{
    [Tooltip("Played from the start of a basic opening (it bursts at basicBurstAt).")]
    public GameObject basicBurstVFX;
    [Tooltip("Played from the start of a premium opening (it bursts at premiumBurstAt).")]
    public GameObject premiumOpenVFX;
    [Tooltip("The envelope height (world units) the effect prefabs are built for; they scale to the envelope at the centre of the screen.")]
    public float designEnvelopeHeight = 4f;
    public float basicBurstAt = 0.8f;
    public float premiumBurstAt = 1.3f;
    [Range(0f, 1f)] public float basicDim = 0.55f;
    [Range(0f, 1f)] public float premiumDim = 0.85f;
    [Tooltip("Seconds the envelope takes to fly from its slot to the centre of the screen.")]
    public float stageFlyTime = 0.3f;
    [Tooltip("How much bigger than its slot the envelope is at the centre of the screen.")]
    public float basicStageScale = 1.4f;
    public float premiumStageScale = 1.6f;

    private const string SortingLayer = "Above Primary Scene UI";
    private const int DimOrder = 40, EnvelopeOrder = 42;   // the effect prefabs sort at 41 (behind) and 43+ (in front)
    private static Sprite white;

    // The centre of the screen in panel pixels, where the envelope bursts and the cards start.
    public Vector2 StageCentre => new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

    // Plays the opening from the bought envelope (bounds in panel pixels) and returns the seconds until it bursts,
    // which is when the cards should fly out. Returns 0 (cards fly at once, as before) if it can't play.
    public float Play(Rect envelopeBounds, StyleBackground envelopeImage, bool premium)
    {
        Camera cam = Camera.main;
        Sprite envelopeSprite = envelopeImage.value.sprite;
        if (cam == null || envelopeSprite == null || float.IsNaN(envelopeBounds.width) || envelopeBounds.width <= 0f) return 0f;

        // the envelope's place on screen, in world space
        Vector3 bottomLeft = UIToWorld(cam, envelopeBounds.xMin, envelopeBounds.yMax);
        Vector3 topRight = UIToWorld(cam, envelopeBounds.xMax, envelopeBounds.yMin);
        Vector3 slot = (bottomLeft + topRight) / 2f;
        Vector3 stage = UIToWorld(cam, StageCentre.x, StageCentre.y);
        Vector2 size = new Vector2(Mathf.Abs(topRight.x - bottomLeft.x), Mathf.Abs(topRight.y - bottomLeft.y));
        float worldPerPixel = size.y / envelopeBounds.height;

        float burstAt = premium ? premiumBurstAt : basicBurstAt;
        float dim = premium ? premiumDim : basicDim;
        float stageScale = premium ? premiumStageScale : basicStageScale;

        SpriteRenderer veil = MakeDim(cam);
        SpriteRenderer envelope = new GameObject("Opening Envelope").AddComponent<SpriteRenderer>();
        envelope.sprite = envelopeSprite;
        envelope.sortingLayerName = SortingLayer;
        envelope.sortingOrder = EnvelopeOrder;
        Vector3 spriteSize = envelopeSprite.bounds.size;
        float fit = Mathf.Min(size.x / spriteSize.x, size.y / spriteSize.y);   // scale-to-fit, like the UI
        // posed now, not on the tween's first update: a new sprite is at scale 1 (screen-filling) until it's posed
        Pose(envelope.transform, slot, stage, fit, stageScale, worldPerPixel, 0f, burstAt, premium);

        GameObject vfxPrefab = premium ? premiumOpenVFX : basicBurstVFX;
        if (vfxPrefab != null)
        {
            GameObject vfx = UnityEngine.Object.Instantiate(vfxPrefab, stage, Quaternion.identity);
            vfx.transform.localScale = Vector3.one * (size.y * stageScale / Mathf.Max(0.01f, designEnvelopeHeight));
        }

        LeanTween.value(0f, burstAt, burstAt)
            .setOnUpdate(t =>
            {
                veil.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t / 0.25f) * dim);
                Pose(envelope.transform, slot, stage, fit, stageScale, worldPerPixel, t, burstAt, premium);
            })
            .setOnComplete(() =>
            {
                // the envelope empties: it squashes and fades as the cards come out, and the dim lifts so they show
                Vector3 from = envelope.transform.localScale;
                LeanTween.value(0f, 1f, 0.16f).setOnUpdate(p =>
                {
                    envelope.transform.localScale = new Vector3(from.x * (1f + 0.15f * p), from.y * (1f - 0.35f * p), 1f);
                    envelope.color = new Color(1f, 1f, 1f, 1f - p);
                }).setOnComplete(() => UnityEngine.Object.Destroy(envelope.gameObject));
                LeanTween.value(dim, 0f, 0.3f).setDelay(0.03f)
                    .setOnUpdate(a => veil.color = new Color(0f, 0f, 0f, a))
                    .setOnComplete(() => UnityEngine.Object.Destroy(veil.gameObject));
            });
        return burstAt;
    }

    // The envelope's pose at time t before the burst (offsets in panel pixels, as the UI version had them).
    private void Pose(Transform envelope, Vector3 slot, Vector3 stage, float fit, float stageScale, float worldPerPixel, float t, float burstAt, bool premium)
    {
        float fly = Mathf.Clamp01(t / stageFlyTime);
        float eased = 1f - (1f - fly) * (1f - fly) * (1f - fly);        // ease out
        float rotate, scale = Mathf.Lerp(1f, stageScale, eased), jitter = 0f;
        float k = Mathf.Clamp01((t - stageFlyTime) / Mathf.Max(0.01f, burstAt - stageFlyTime));
        if (premium)
        {
            // trembles harder up to the burst
            rotate = Mathf.Sin(t * 2f * Mathf.PI * 16f) * Mathf.Lerp(1f, 7f, k * k) * (fly >= 1f ? 1f : 0f);
            jitter = Mathf.Sin(t * 2f * Mathf.PI * 23f) * 6f * k * k;
            scale *= 1f + 0.05f * k;
        }
        else
        {
            rotate = Mathf.Sin(t * 2f * Mathf.PI * 7f) * 6f * (1f - k * 0.5f) * eased;
            scale *= 1f + 0.06f * k;
        }
        envelope.position = Vector3.Lerp(slot, stage, eased) + new Vector3(jitter, 0f, 0f) * worldPerPixel;
        envelope.rotation = Quaternion.Euler(0f, 0f, -rotate);
        envelope.localScale = new Vector3(fit * scale, fit * scale, 1f);
    }

    // A black sprite over the whole view, under the envelope.
    private static SpriteRenderer MakeDim(Camera cam)
    {
        if (white == null)
            white = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(0.5f, 0.5f), Texture2D.whiteTexture.width);
        SpriteRenderer veil = new GameObject("Opening Dim").AddComponent<SpriteRenderer>();
        veil.sprite = white;
        veil.sortingLayerName = SortingLayer;
        veil.sortingOrder = DimOrder;
        veil.color = new Color(0f, 0f, 0f, 0f);
        float h = cam.orthographic ? cam.orthographicSize * 2f : 20f;
        veil.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, UIDocumentGameObjectPlacer.zPlane);
        veil.transform.localScale = new Vector3(h * cam.aspect * 1.1f, h * 1.1f, 1f);
        return veil;
    }

    // Panel pixels to world, as FollowVisualElementBounds maps the shop's UI.
    private static Vector3 UIToWorld(Camera cam, float x, float y)
    {
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(x, Screen.height - y, 0f));
        return new Vector3(world.x, world.y, UIDocumentGameObjectPlacer.zPlane);
    }
}
