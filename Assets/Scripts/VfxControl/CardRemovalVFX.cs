using System;
using UnityEngine;
using UnityEngine.UIElements;

/*
    Card removal in the shop: once the player picks the card to remove, it's shown in the middle of the shop and
    consumed, in the same look as the removal experience in combat (CardConsumeVFX): it goes red, stays red, and
    burns away out from its middle, while its embers, ash and ink rise off it.

    The shop has no world-space card renderer (combat's card face pool isn't in the shop), so the card is a UI copy
    on the shop's own document, and the sparks and blots are CardConsumeVFX's own "Removal Burn" layers, played in
    world space over it. Purely visual: the card is already removed. ShopViewController.ProcessCardRemoval calls
    Play(); settings live on ShopViewController (ShopManager prefab) under "Card Removal VFX".
*/
[Serializable]
public class CardRemovalVFX
{
    [Tooltip("The removal experience (CardConsumeVFX); its \"Removal Burn\" layers play over the card.")]
    public GameObject consumeVFX;
    [Tooltip("The card's height as a fraction of the shop's height.")]
    public float cardHeight = 0.46f;

    private const float CardAspect = 0.714f;        // width / height of a card view
    private const float BurnCardHeight = 14f;       // the burn layers are built for a card 14 units tall

    private static readonly Color Crimson = new Color(1f, 0.1f, 0.04f), Char = new Color(0.11f, 0.03f, 0.02f);

    public void Play(VisualElement root, Card card)
    {
        if (root == null || card == null) return;
        float h = root.resolvedStyle.height * cardHeight, w = h * CardAspect;
        if (float.IsNaN(h) || h <= 0f) return;

        // the card in two halves, each clipped by a frame that shrinks toward its own edge, so a gap opens across the
        // middle of the card and burns outward (the UI has no dissolve shader to do it with)
        VisualElement frame = new VisualElement { pickingMode = PickingMode.Ignore };
        frame.style.position = Position.Absolute;
        frame.style.left = (root.resolvedStyle.width - w) / 2f;
        frame.style.top = (root.resolvedStyle.height - h) / 2f;
        frame.style.width = w; frame.style.height = h;
        frame.style.opacity = 0f;
        var tints = new System.Collections.Generic.List<VisualElement>();
        VisualElement upper = Half(card, w, h, true, tints), lower = Half(card, w, h, false, tints);
        frame.Add(upper); frame.Add(lower);
        root.Add(frame);

        GameObject burn = null;
        Transform burnSource = consumeVFX != null ? consumeVFX.transform.Find("Removal Burn") : null;

        // 0-0.12 it appears, already red; it stays red, then chars to black over the end (like the consume in combat);
        // 0.3-0.95 it burns out from the middle
        const float total = 1.15f;
        LeanTween.value(0f, total, total).setOnUpdate(t =>
        {
            float appear = Mathf.SmoothStep(0f, 1f, t / 0.12f);
            float s = Mathf.Lerp(0.9f, 1f, appear);
            frame.style.scale = new Scale(new Vector3(s, s, 1f));
            float gone = Mathf.SmoothStep(0f, 1f, (t - 0.3f) / 0.65f);
            upper.style.height = lower.style.height = h * 0.5f * (1f - gone);
            frame.style.opacity = appear * (1f - Mathf.SmoothStep(0f, 1f, (t - 0.9f) / 0.25f));
            float charred = Mathf.SmoothStep(0f, 1f, (t - 0.55f) / 0.4f);
            Color tint = Color.Lerp(Crimson, Char, charred);
            foreach (VisualElement layer in tints) layer.style.backgroundColor = new Color(tint.r, tint.g, tint.b, Mathf.Lerp(0.6f, 0.88f, charred));

            if (burn == null && burnSource != null && t >= 0.15f) burn = SpawnBurn(burnSource, frame, h);
        }).setOnComplete(() => frame.RemoveFromHierarchy());
    }

    // half of the card under a red tint, clipped by a frame pinned to the card's top or bottom edge
    private static VisualElement Half(Card card, float w, float h, bool top, System.Collections.Generic.List<VisualElement> tints)
    {
        VisualElement clip = new VisualElement { pickingMode = PickingMode.Ignore };
        clip.style.position = Position.Absolute;
        clip.style.left = 0; clip.style.width = w; clip.style.height = h * 0.5f;
        if (top) clip.style.top = 0; else clip.style.bottom = 0;
        clip.style.overflow = Overflow.Hidden;
        CardView view = new CardView(card, card.getCompanionFrom(), true);
        view.cardContainer.pickingMode = PickingMode.Ignore;
        foreach (VisualElement layer in new[] { view.cardContainer, new VisualElement { pickingMode = PickingMode.Ignore } })
        {
            layer.style.position = Position.Absolute;
            layer.style.left = 0; layer.style.width = w; layer.style.height = h;
            if (top) layer.style.top = 0; else layer.style.bottom = 0;
            if (layer != view.cardContainer) { layer.style.backgroundColor = new Color(Crimson.r, Crimson.g, Crimson.b, 0.6f); tints.Add(layer); }
            clip.Add(layer);
        }
        return clip;
    }

    // The consume experience's sparks and blots, over the card and sized to it.
    private static GameObject SpawnBurn(Transform source, VisualElement frame, float panelHeight)
    {
        Camera cam = Camera.main;
        if (cam == null) return null;
        Rect b = frame.worldBound;
        Vector3 top = cam.ScreenToWorldPoint(new Vector3(b.center.x, Screen.height - b.yMin, 0f));
        Vector3 bottom = cam.ScreenToWorldPoint(new Vector3(b.center.x, Screen.height - (b.yMin + panelHeight), 0f));
        GameObject burn = UnityEngine.Object.Instantiate(source.gameObject);
        burn.transform.position = new Vector3((top.x + bottom.x) / 2f, (top.y + bottom.y) / 2f, UIDocumentGameObjectPlacer.zPlane);
        // wider than the card, so the blots rise past its edges and read against the shop rather than the dark card
        burn.transform.localScale = Vector3.one * (Mathf.Abs(top.y - bottom.y) / BurnCardHeight) * 1.5f;
        foreach (ParticleSystem ps in burn.GetComponentsInChildren<ParticleSystem>(true)) ps.Play(false);
        UnityEngine.Object.Destroy(burn, 2.5f);
        return burn;
    }
}
