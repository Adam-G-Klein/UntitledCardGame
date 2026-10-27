using System.Collections.Generic;
using UnityEngine;

/*
    A card an effect draws mid-turn (not the start-of-turn deal) glows gold for a few seconds once it lands in the
    hand, so it's clear which cards just arrived. Like RetainedCardsVFX it only watches game state, so it needs no
    hooks in the draw or hand code: the encounter counts every extra draw, and the cards drawn are the PlayableCards
    that appeared with it (they're created when queued, before they reach the hand). Each one gets the glow when
    PlayerHand has it.
*/
public class DrawnCardsVFX : MonoBehaviour
{
    [Tooltip("Played on each drawn card when it reaches the hand; it rides the card and ends by itself.")]
    [SerializeField] private GameObject drawnGlowVFX;
    [Tooltip("A drawn card that hasn't reached the hand by then (hand full, discarded) is let go.")]
    [SerializeField] private float waitForHand = 5f;

    private readonly HashSet<PlayableCard> known = new HashSet<PlayableCard>();
    private readonly Dictionary<PlayableCard, float> pending = new Dictionary<PlayableCard, float>();
    private int lastExtraDraws = -1;
    private float nextRefresh;

    private void Update()
    {
        // isDestroyed only reads the singletons (Instance would create them outside combat)
        if (drawnGlowVFX == null || EnemyEncounterManager.isDestroyed || PlayerHand.isDestroyed) return;
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
            if (inHand) CardVFX.Play(drawnGlowVFX, card, follow: true);
            if (inHand || card == null || Time.time > entry.Value) (done ??= new List<PlayableCard>()).Add(card);
        }
        if (done != null) foreach (PlayableCard card in done) pending.Remove(card);
    }

    private void RememberAllCards()
    {
        known.RemoveWhere(card => card == null);
        foreach (PlayableCard card in FindObjectsOfType<PlayableCard>(true)) known.Add(card);
        nextRefresh = Time.time + 0.25f;
    }
}
