using UnityEngine;

/*
    Hot Streak's flame: each discard this turn adds to the rat's damage, so the flame that plays on each discard is
    bigger than the last. The effect's layers scale with their root. Purely visual.
*/
public class GrowWithDiscards : MonoBehaviour
{
    [Tooltip("How much bigger the flame is for each discard after the first.")]
    [SerializeField] private float perDiscard = 0.18f;
    [Tooltip("It stops growing at this scale.")]
    [SerializeField] private float maxScale = 2f;

    private void Start()
    {
        CombatEncounterState state = EnemyEncounterManager.Instance != null ? EnemyEncounterManager.Instance.combatEncounterState : null;
        if (state == null) return;
        float scale = 1f + perDiscard * Mathf.Max(0, state.cardsDiscardedThisTurn.Count - 1);
        transform.localScale *= Mathf.Min(scale, maxScale);
    }
}
