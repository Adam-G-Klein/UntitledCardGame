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

    private static readonly int FadeTex = Shader.PropertyToID("_Fade_Tex");
    private const float BurnCardHeight = 14f;   // the card height the burn is built for

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
        SpriteRenderer face = card.GetComponent<SpriteRenderer>();
        float fit = face != null && face.bounds.size.y > 0f ? face.bounds.size.y / BurnCardHeight : card.transform.lossyScale.x;
        if (burn != null && followCard)
        {
            // rides the card: under it, undoing the card's own scale
            burn.SetParent(card.transform, false);
            burn.localPosition = Vector3.zero;
            burn.localScale = Vector3.one * (fit / Mathf.Max(0.0001f, card.transform.lossyScale.x));
        }
        else if (burn != null) burn.localScale = Vector3.one * (fit / Mathf.Max(0.0001f, burn.parent != null ? burn.parent.lossyScale.x : 1f));
        if (fadeTexture == null) return;
        if (face != null && face.material.HasProperty(FadeTex)) face.material.SetTexture(FadeTex, fadeTexture);
    }

    private void ReleaseBurn()
    {
        if (burn == null) return;
        burn.SetParent(null, true);
        foreach (ParticleSystem ps in burn.GetComponentsInChildren<ParticleSystem>()) ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        Destroy(burn.gameObject, burnLinger);
    }
}
