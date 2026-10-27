using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/*
    Sits on the "Card" particle of the shuffle-into-deck VFX. That particle is a blank card shape, so on
    its own it reads as a box dropping into the deck. When AddCardsToDeck has just added cards, this
    renders the first added card into a pooled card texture (the same way UIDocumentCard does), draws it
    with a runtime copy of the worldspace card's material, follows the particle's authored motion and
    size, and erodes it with the card shader's own cutoff.

    The face arrives washed in the shuffler's colour (red for an enemy, blue for an ally: the particle's
    start colour) and fades to the card itself, keeping a light faceTint. The blank particle is hidden.

    If anything it needs is missing (no recent add, no free card texture, no card look to copy) it does
    nothing and the original particle plays as authored.
*/
[RequireComponent(typeof(ParticleSystem))]
public class ShuffleCardFace : MonoBehaviour
{
    private struct RecentAdd
    {
        public DeckInstance deck;
        public Card card;
        public float time;
    }

    private static readonly List<RecentAdd> recentAdds = new List<RecentAdd>();
    // enemy adds fly the card over before this VFX spawns, so allow a few seconds
    private const float RecentWindow = 4f;

    // Called by AddCardsToDeck after it adds cards to a deck.
    public static void NotifyCardsAdded(DeckInstance deck, List<Card> cards)
    {
        if (deck == null || cards == null || cards.Count == 0) return;
        recentAdds.RemoveAll(a => a.deck == null || Time.time - a.time > RecentWindow);
        recentAdds.Add(new RecentAdd { deck = deck, card = cards[0], time = Time.time });
    }

    [Tooltip("How much of the particle's height the card shape fills in its texture (T_CardHand: 0.86).")]
    [SerializeField] private float cardHeightInParticle = 0.86f;
    [Tooltip("WorldCardShader cutoff when fully visible and fully eroded.")]
    [SerializeField] private float visibleCutoff = -0.25f;
    [SerializeField] private float erodedCutoff = 0.99f;
    [Tooltip("The worldspace card's renderer, copied for the face. Without it the look is copied from a card in hand, which is empty on the enemy turn.")]
    [SerializeField] private SpriteRenderer cardTemplate;
    [Tooltip("Face size relative to the particle's card shape.")]
    [SerializeField] private float faceInset = 1f;
    [Tooltip("Tint the face settles to (white = untouched).")]
    [SerializeField] private Color faceTint = Color.white;
    [Tooltip("Fraction of the particle's life over which the shuffler's colour fades into the card.")]
    [SerializeField] private float colourFadeEnd = 0.45f;

    private Color washColour = Color.white;

    private ParticleSystem ps;
    private ParticleSystemRenderer psRenderer;
    private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1];
    private GameObject faceObject;
    private SpriteRenderer face;
    private Material faceMaterial;
    private GameObject docObject;
    private PanelSettings panelSettings;
    private bool seenParticle;
    private static readonly int CutoffId = Shader.PropertyToID("_Cutoff_Value");
    private static readonly int UseCutoffId = Shader.PropertyToID("_Use_Cutoff");
    private static readonly int SecondTexId = Shader.PropertyToID("_SecondTex");

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        psRenderer = GetComponent<ParticleSystemRenderer>();
        RecentAdd? add = TakeRecentAdd();
        if (add == null) return;
        SpriteRenderer template = cardTemplate != null ? cardTemplate : FindHandCardRenderer();
        if (template == null || CardPanelSettingsPooler.Instance == null) return;
        panelSettings = CardPanelSettingsPooler.Instance.GetPanelSettings();
        if (panelSettings == null) return;

        // render the added card into the pooled texture
        docObject = new GameObject("ShuffleCardFaceDoc");
        docObject.SetActive(false);
        UIDocument doc = docObject.AddComponent<UIDocument>();
        doc.panelSettings = panelSettings;
        docObject.SetActive(true);
        CardView cardView = new CardView(add.Value.card, add.Value.deck.GetCompanionTypeSO(), false);
        doc.rootVisualElement.Add(cardView.cardContainer);
        UIDocumentUtils.SetAllPickingMode(doc.rootVisualElement, PickingMode.Ignore);

        // draw it like a hand card
        faceObject = new GameObject("ShuffleCardFace");
        faceObject.layer = template.gameObject.layer;   // so card-only lights reach it too
        face = faceObject.AddComponent<SpriteRenderer>();
        face.sprite = template.sprite;
        face.drawMode = template.drawMode;
        face.size = template.size;
        faceMaterial = new Material(template.sharedMaterial);
        faceMaterial.SetTexture(SecondTexId, panelSettings.targetTexture);
        faceMaterial.SetFloat(UseCutoffId, 1f);
        faceMaterial.SetFloat(CutoffId, erodedCutoff);
        face.sharedMaterial = faceMaterial;
        ParticleSystem.MinMaxGradient start = ps.main.startColor;
        washColour = start.mode == ParticleSystemGradientMode.Color ? start.color : start.colorMax;
        washColour.a = 1f;
        face.color = washColour;
        psRenderer.enabled = false;
        face.sortingLayerID = psRenderer.sortingLayerID;
        face.sortingOrder = psRenderer.sortingOrder + 1;
        face.enabled = false;
    }

    private void LateUpdate()
    {
        if (face == null) return;
        int count = ps.GetParticles(particles);
        if (count == 0)
        {
            face.enabled = false;
            if (seenParticle) Cleanup();
            return;
        }
        seenParticle = true;
        ParticleSystem.Particle p = particles[0];
        float life = 1f - p.remainingLifetime / Mathf.Max(p.startLifetime, 0.0001f);

        Vector3 pos = ps.main.simulationSpace == ParticleSystemSimulationSpace.World ? p.position : transform.TransformPoint(p.position);
        faceObject.transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        Vector3 size = p.GetCurrentSize3D(ps);
        float height = size.y * transform.lossyScale.y * cardHeightInParticle * faceInset;
        float scale = height / Mathf.Max(face.size.y, 0.0001f);
        faceObject.transform.localScale = new Vector3(scale, scale, 1f);
        faceObject.transform.rotation = Quaternion.Euler(0f, 0f, -p.rotation);

        // the particle's custom data x is its authored dissolve: <= 0 fully shown, >= 1 gone
        float dissolve = 0f;
        ParticleSystem.CustomDataModule cd = ps.customData;
        if (cd.enabled)
        {
            ParticleSystem.MinMaxCurve c = cd.GetVector(ParticleSystemCustomData.Custom1, 0);
            dissolve = c.mode == ParticleSystemCurveMode.Curve ? c.curve.Evaluate(life) * c.curveMultiplier : c.constant;
        }
        faceMaterial.SetFloat(CutoffId, Mathf.Lerp(visibleCutoff, erodedCutoff, Mathf.Clamp01(dissolve)));
        face.enabled = true;
        // the rim particle darkens as it dissolves; drop it once the face starts eroding so the gaps stay clean
        float fade = Mathf.Clamp01(life / Mathf.Max(colourFadeEnd, 0.01f));
        face.color = Color.Lerp(washColour, faceTint, fade * fade * (3f - 2f * fade));
    }

    private RecentAdd? TakeRecentAdd()
    {
        recentAdds.RemoveAll(a => a.deck == null || Time.time - a.time > RecentWindow);
        int best = -1;
        float bestDist = float.MaxValue;
        for (int i = 0; i < recentAdds.Count; i++)
        {
            float d = (recentAdds[i].deck.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = i; }
        }
        if (best < 0) return null;
        RecentAdd add = recentAdds[best];
        recentAdds.RemoveAt(best);
        return add;
    }

    private static SpriteRenderer FindHandCardRenderer()
    {
        foreach (PlayableCard card in FindObjectsOfType<PlayableCard>())
        {
            SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sharedMaterial != null && sr.sharedMaterial.HasProperty(SecondTexId)) return sr;
        }
        return null;
    }

    private void Cleanup()
    {
        if (faceObject != null) Destroy(faceObject);
        if (docObject != null) Destroy(docObject);
        if (faceMaterial != null) Destroy(faceMaterial);
        if (panelSettings != null && CardPanelSettingsPooler.Instance != null) CardPanelSettingsPooler.Instance.ReturnPanelSettings(panelSettings);
        faceObject = null; docObject = null; faceMaterial = null; panelSettings = null; face = null;
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
