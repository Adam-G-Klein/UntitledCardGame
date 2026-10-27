using UnityEngine;

/*
    Sits on the mana emblem in the corner of the board. When the player's mana drops to zero it plays a
    small "out of mana" burst at the emblem. It only watches the mana value, so it needs no hooks in the
    mana code, and does nothing in scenes without a ManaManager (it never creates one).
*/
public class ManaDisplayVFX : MonoBehaviour
{
    [Tooltip("One-shot burst played on the emblem when mana runs out.")]
    [SerializeField] private GameObject outOfManaVFX;
    [Tooltip("One-shot burst played on the emblem when a card or effect gives Élan during the player's turn.")]
    [SerializeField] private GameObject gainVFX;
    [Tooltip("Ink trails that fly from where the Élan came from into the emblem (HomingParticles); the gain burst waits for them.")]
    [SerializeField] private GameObject gainTrailsVFX;
    [SerializeField] private float trailTravelTime = 0.5f;

    private static ManaDisplayVFX active;
    private float suppressBurstUntil = -1f;

    private void OnEnable() { active = this; }
    private void OnDisable() { if (active == this) active = null; }

    // Called by the Élan change step with where the Élan came from (the card played, or the rat).
    public static void PlayGainFrom(Vector3 from)
    {
        if (active == null || active.gainTrailsVFX == null) return;
        GameObject trails = Instantiate(active.gainTrailsVFX, new Vector3(from.x, from.y, active.transform.position.z), Quaternion.identity);
        foreach (HomingParticles h in trails.GetComponentsInChildren<HomingParticles>(true)) h.target = active.transform.position;
        // the emblem bursts when the ink lands, not when the value changes
        active.suppressBurstUntil = Time.time + 0.1f;
        active.StartCoroutine(active.BurstAfter(active.trailTravelTime));
    }

    private System.Collections.IEnumerator BurstAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (gainVFX != null) Instantiate(gainVFX, transform.position, Quaternion.identity);
    }

    private ManaManager mana;
    private int lastMana = -1;
    private float nextLookup;

    private void Update()
    {
        if (mana == null)
        {
            if (Time.unscaledTime < nextLookup) return;
            nextLookup = Time.unscaledTime + 1f;
            mana = FindObjectOfType<ManaManager>();
            if (mana == null) return;
            lastMana = mana.currentMana;
        }

        int now = mana.currentMana;
        if (now == 0 && lastMana > 0 && outOfManaVFX != null)
            Instantiate(outOfManaVFX, transform.position, Quaternion.identity);
        // a gain mid-turn (not the refill at the start of the turn)
        if (now > lastMana && lastMana >= 0 && gainVFX != null && Time.time > suppressBurstUntil && IsPlayerTurn())
            Instantiate(gainVFX, transform.position, Quaternion.identity);
        lastMana = now;
    }

    // looked up only on a gain (TurnManager.Instance would create one in scenes without it)
    private static bool IsPlayerTurn()
    {
        TurnManager turns = FindObjectOfType<TurnManager>();
        return turns != null && turns.GetTurnPhase() == TurnPhase.PLAYER_TURN;
    }
}
