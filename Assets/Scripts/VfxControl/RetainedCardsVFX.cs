using UnityEngine;

/*
    When the player's turn ends, the cards still in hand were retained: give each one a soft shimmer so it's
    clear which cards are staying. It only watches the turn phase, so it needs no hooks in the hand code.
*/
public class RetainedCardsVFX : MonoBehaviour
{
    [SerializeField] private GameObject retainedVFX;
    private TurnPhase lastPhase = TurnPhase.START_ENCOUNTER;
    private float nextLookup;
    private TurnManager turns;

    private void Update()
    {
        if (turns == null)
        {
            if (Time.unscaledTime < nextLookup) return;
            nextLookup = Time.unscaledTime + 1f;
            turns = FindObjectOfType<TurnManager>();
            if (turns == null) return;
        }
        TurnPhase phase = turns.GetTurnPhase();
        if (phase == lastPhase) return;
        // by the start of the enemy turn the hand has been discarded; what's left was retained
        if (phase == TurnPhase.START_ENEMY_TURN && PlayerHand.Instance != null)
            foreach (PlayableCard card in PlayerHand.Instance.GetCardsOrdered())
                CardVFX.Play(retainedVFX, card, follow: true);
        lastPhase = phase;
    }
}
