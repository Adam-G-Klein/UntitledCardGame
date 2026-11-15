using UnityEngine;

/*
    The exhaust experience dissolves the card it's bound to through the card shader's fade texture (its timeline
    animates the cutoff). The shared card material's fade texture is a generic noise that the cast, consume and
    create animations also use, so this swaps it on the exhausting card's own material instance only, so the
    card washes away in the game's watercolour. The card is parented under the experience before it starts.

    The removal experience (CardConsumeVFX, played for a card removed from its deck for good) and the discard
    experience (CardCastVFX, which every card going to its discard pile plays) are set up the same way, each in
    its own colours and with its own ink material instance. The discard's smoke streaks fly to the card's discard
    pile: the "Companion" location the card hands its experience (toPile).

    It also fits the burn (the sparks and ink blots rising off the card, built by VFXRetoolBuilder.CardBurn
    for a card drawn 14 units tall) to the size the card is actually drawn at, and lets the burn outlive the
    experience (which destroys itself when its timeline ends) so the ink already in the air finishes rising.
    The fit goes by the card's drawn height, not its scale: cards are sliced sprites whose drawn size is set
    separately from their transform (a hand card is at scale 2 but drawn 2.8 units tall).

    With burnEdge on (the exhaust), the card burns in from its edges: two copies of its face drawn behind it, a char
    band and a glowing rim, each dissolving a little after the face does, so a dark edge and a bright one follow the
    dissolve in. This is what tells the exhaust (destroyed, in place) from the discard (set aside, flying to its pile).
*/
public class ExhaustDissolveTexture : MonoBehaviour
{
    [SerializeField] private Texture fadeTexture;
    [SerializeField] private Transform burn;
    [Tooltip("Seconds the burn lingers after the exhaust ends, for the ink still in the air.")]
    [SerializeField] private float burnLinger = 1.5f;
    [Tooltip("The burn rides the card (a trail behind a card that's moving) instead of staying where it started.")]
    [SerializeField] private bool followCard = false;
    [Tooltip("Particles steered into the card's discard pile (the experience's \"Companion\" location). Every HomingParticles under the burn is aimed there.")]
    [SerializeField] private HomingParticles toPile;

    [Header("Burn edge (exhaust)")]
    [Tooltip("A char band and a glowing rim follow the dissolve just behind it, so the card burns in from its edges. The card shader has no edge colour of its own: these are copies of the card's face drawn behind it, dissolving a little later.")]
    [SerializeField] private bool burnEdge = false;
    [ColorUsage(false, true)] [SerializeField] private Color edgeGlow = new Color(4.5f, 1.6f, 0.25f);
    [SerializeField] private Color edgeChar = new Color(0.12f, 0.06f, 0.04f);
    [Tooltip("How far behind the card's own cutoff the char band dissolves.")]
    [SerializeField] private float charLag = 0.05f;
    [Tooltip("How far behind the card's own cutoff the glowing rim dissolves.")]
    [SerializeField] private float glowLag = 0.13f;

    private static readonly int FadeTex = Shader.PropertyToID("_Fade_Tex");
    private static readonly int CutoffId = Shader.PropertyToID("_Cutoff_Value");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private const float BurnCardHeight = 14f;   // the card height the burn is built for

    private SpriteRenderer face, charCopy, glowCopy;
    private MaterialPropertyBlock block;

    private void Start()
    {
        FXExperience experience = GetComponent<FXExperience>();
        if (experience != null && burn != null) experience.onExperienceOver += ReleaseBurn;
        // every layer flying to the pile (the smoke, and its flecks) aims at the card's discard pile
        if (burn != null)
            foreach (HomingParticles homing in burn.GetComponentsInChildren<HomingParticles>(true))
            {
                if (experience != null && experience.ContainsLocationForKey("Companion")) homing.target = experience.GetLocationFromKey("Companion");
                else homing.enabled = false;   // nowhere to go: they just drift
            }
        PlayableCard card = GetComponentInChildren<PlayableCard>();
        if (card == null) return;
        face = card.GetComponent<SpriteRenderer>();
        float fit = face != null && face.bounds.size.y > 0f ? face.bounds.size.y / BurnCardHeight : card.transform.lossyScale.x;
        if (burn != null && followCard)
        {
            // rides the card: under it, undoing the card's own scale
            burn.SetParent(card.transform, false);
            burn.localPosition = Vector3.zero;
            burn.localScale = Vector3.one * (fit / Mathf.Max(0.0001f, card.transform.lossyScale.x));
        }
        else if (burn != null) burn.localScale = Vector3.one * (fit / Mathf.Max(0.0001f, burn.parent != null ? burn.parent.lossyScale.x : 1f));
        if (fadeTexture != null && face != null && face.material.HasProperty(FadeTex)) face.material.SetTexture(FadeTex, fadeTexture);
        if (burnEdge && face != null)
        {
            block = new MaterialPropertyBlock();
            charCopy = EdgeCopy("Burn Char", -1);
            glowCopy = EdgeCopy("Burn Glow", -2);
        }
    }

    // a copy of the card's face, drawn just behind it with the same material instance (so the same fade texture)
    private SpriteRenderer EdgeCopy(string name, int orderOffset)
    {
        var go = new GameObject(name);
        go.transform.SetParent(face.transform, false);
        var copy = go.AddComponent<SpriteRenderer>();
        copy.sprite = face.sprite;
        // the card fronts are sliced sprites with tight meshes: setting that draw mode from code logs a "Sprite Tiling
        // might not appear correctly" warning on every exhaust, though the copy draws exactly as the face does
        bool logging = Debug.unityLogger.logEnabled;
        Debug.unityLogger.logEnabled = false;
        copy.drawMode = face.drawMode;
        if (face.drawMode != SpriteDrawMode.Simple) copy.size = face.size;
        Debug.unityLogger.logEnabled = logging;
        copy.sharedMaterial = face.sharedMaterial;
        copy.sortingLayerID = face.sortingLayerID;
        copy.sortingOrder = face.sortingOrder + orderOffset;
        copy.flipX = face.flipX; copy.flipY = face.flipY;
        return copy;
    }

    // the timeline animates the face through its property block: each copy takes the same values, its own colour,
    // and a cutoff a little behind the face's, so it shows only in a band just outside what's left of the card
    private void LateUpdate()
    {
        if (!burnEdge || face == null || charCopy == null) return;
        face.GetPropertyBlock(block);
        float cutoff = block.GetFloat(CutoffId);
        charCopy.sortingOrder = face.sortingOrder - 1; glowCopy.sortingOrder = face.sortingOrder - 2;
        charCopy.enabled = glowCopy.enabled = face.enabled;
        Edge(charCopy, edgeChar, cutoff - charLag);
        face.GetPropertyBlock(block);
        Edge(glowCopy, edgeGlow, cutoff - glowLag);
    }

    private void Edge(SpriteRenderer copy, Color colour, float cutoff)
    {
        block.SetColor(ColorId, colour);
        block.SetFloat(CutoffId, cutoff);
        copy.SetPropertyBlock(block);
    }

    private void ReleaseBurn()
    {
        if (burn == null) return;
        burn.SetParent(null, true);
        foreach (ParticleSystem ps in burn.GetComponentsInChildren<ParticleSystem>()) ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        Destroy(burn.gameObject, burnLinger);
    }
}
