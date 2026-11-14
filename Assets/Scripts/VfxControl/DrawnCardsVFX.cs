using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    A card an effect draws mid-turn (not the start-of-turn deal) glows gold for a moment once it lands in the hand,
    so it's clear which cards just arrived: the card itself warms to gold and brightens, then settles back (its own
    material's colour and emission, the way the exhaust colours a card), rather than a separate effect around it. Like RetainedCardsVFX it only watches game state, so it needs no
    hooks in the draw or hand code: the encounter counts every extra draw, and the cards drawn are the PlayableCards
    that appeared with it (they're created when queued, before they reach the hand). Each one gets the glow when
    PlayerHand has it.
*/
public class DrawnCardsVFX : MonoBehaviour
{
    [Tooltip("The colour the drawn card warms to (multiplied over its face).")]
    [SerializeField] private Color glowTint = new Color(1f, 0.86f, 0.52f, 1f);
    [Tooltip("The card's emission at the height of the glow (its own is 1).")]
    [SerializeField] private float glowEmission = 1.8f;
    [Tooltip("Seconds of the glow: it warms up quickly, holds a beat and settles back.")]
    [SerializeField] private float glowTime = 1.3f;
    [Tooltip("A drawn card that hasn't reached the hand by then (hand full, discarded) is let go.")]
    [SerializeField] private float waitForHand = 5f;

    private readonly HashSet<PlayableCard> known = new HashSet<PlayableCard>();
    private readonly Dictionary<PlayableCard, float> pending = new Dictionary<PlayableCard, float>();
    private int lastExtraDraws = -1;
    private float nextRefresh;

    private void Update()
    {
        // isDestroyed only reads the singletons (Instance would create them outside combat)
        if (EnemyEncounterManager.isDestroyed || PlayerHand.isDestroyed) return;
        CombatEncounterState state = EnemyEncounterManager.Instance.combatEncounterState;
        if (state == null) return;

        int extraDraws = state.GetNumExtraCardsDrawnThisTurn();
        if (lastExtraDraws < 0 || extraDraws < lastExtraDraws)
        {
            // first frame of the encounter, or the count reset for a new turn: nothing was just drawn
            RememberAllCards();
        }
        else if (extraDraws > lastExtraDraws)
        {
            foreach (PlayableCard card in FindObjectsOfType<PlayableCard>(true))
                if (known.Add(card)) pending[card] = Time.time + waitForHand;
        }
        else if (Time.time >= nextRefresh)
        {
            // cards dealt or created without a draw (turn start, generated cards) aren't "just drawn"
            RememberAllCards();
        }
        lastExtraDraws = extraDraws;

        if (pending.Count == 0) return;
        List<PlayableCard> done = null;
        foreach (KeyValuePair<PlayableCard, float> entry in pending)
        {
            PlayableCard card = entry.Key;
            bool inHand = card != null && PlayerHand.Instance.ContainsPlayableCard(card);
            if (inHand) StartCoroutine(GoldGlow(card));
            if (inHand || card == null || Time.time > entry.Value) (done ??= new List<PlayableCard>()).Add(card);
        }
        if (done != null) foreach (PlayableCard card in done) pending.Remove(card);
    }

    private static readonly int ColorId = Shader.PropertyToID("_Color"), EmissionId = Shader.PropertyToID("_Emission_Power");

    private IEnumerator GoldGlow(PlayableCard card)
    {
        SpriteRenderer face = card.GetComponent<SpriteRenderer>();
        if (face == null) yield break;
        Material m = face.material;   // the card's own instance
        Color baseColour = m.GetColor(ColorId);
        float baseEmission = m.HasProperty(EmissionId) ? m.GetFloat(EmissionId) : 1f;
        for (float t = 0f; t < glowTime; t += Time.deltaTime)
        {
            if (card == null || face == null) yield break;
            float u = t / glowTime;
            // up over the first 15%, held to 35%, then eased back down
            float k = u < 0.15f ? u / 0.15f : u < 0.35f ? 1f : 1f - Mathf.SmoothStep(0f, 1f, (u - 0.35f) / 0.65f);
            m.SetColor(ColorId, Color.Lerp(baseColour, baseColour * glowTint, k));
            if (m.HasProperty(EmissionId)) m.SetFloat(EmissionId, Mathf.Lerp(baseEmission, glowEmission, k));
            yield return null;
        }
        if (face == null) yield break;
        m.SetColor(ColorId, baseColour);
        if (m.HasProperty(EmissionId)) m.SetFloat(EmissionId, baseEmission);
    }

    private void RememberAllCards()
    {
        known.RemoveWhere(card => card == null);
        foreach (PlayableCard card in FindObjectsOfType<PlayableCard>(true)) known.Add(card);
        nextRefresh = Time.time + 0.25f;
    }
}
