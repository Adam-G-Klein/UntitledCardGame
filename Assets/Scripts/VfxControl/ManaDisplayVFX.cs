using System.Collections.Generic;
using UnityEngine;

/*
    Sits on the mana emblem in the corner of the board. When the player's mana drops to zero it plays a
    small "out of mana" burst at the emblem, and its ambient glow (the looping halo, motes and glints
    under it) stops and fades out; when mana is refilled for the turn it plays the opposite, a "mana
    refill" bloom, and the glow comes back. Gains during the turn play the gain burst. It only watches
    the mana value, so it needs no hooks in the mana code, and does nothing in scenes without a
    ManaManager (it never creates one).
*/
public class ManaDisplayVFX : MonoBehaviour
{
    [Tooltip("One-shot burst played on the emblem when mana runs out.")]
    [SerializeField] private GameObject outOfManaVFX;
    [Tooltip("One-shot bloom played on the emblem when mana is refilled for the turn (the opposite of the out-of-mana burst).")]
    [SerializeField] private GameObject refillVFX;
    [Tooltip("One-shot burst played on the emblem when a card or effect gives Élan during the player's turn.")]
    [SerializeField] private GameObject gainVFX;
    [Tooltip("Ink trails that fly from where the Élan came from into the emblem (HomingParticles); the gain burst waits for them.")]
    [SerializeField] private GameObject gainTrailsVFX;
    [SerializeField] private float trailTravelTime = 0.5f;
    [Tooltip("The emblem's looping glow, shown while there's mana and faded out at zero. Empty: every looping particle system under this object.")]
    [SerializeField] private List<ParticleSystem> ambient = new List<ParticleSystem>();

    private static ManaDisplayVFX active;
    private float suppressBurstUntil = -1f;

    private void OnEnable() { active = this; }
    private void OnDisable() { if (active == this) active = null; }

    private void Awake()
    {
        if (ambient.Count == 0)
            foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>(true))
                if (ps.main.loop && ps.gameObject != gameObject) ambient.Add(ps);
    }

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
    private bool glowOn = true;

    private void Update()
    {
        if (mana == null)
        {
            if (Time.unscaledTime < nextLookup) return;
            nextLookup = Time.unscaledTime + 1f;
            mana = FindObjectOfType<ManaManager>();
            if (mana == null) return;
            lastMana = mana.currentMana;
            SetGlow(lastMana > 0);
        }

        int now = mana.currentMana;
        if (now == 0 && lastMana > 0 && outOfManaVFX != null)
            Instantiate(outOfManaVFX, transform.position, Quaternion.identity);
        if (now > lastMana && lastMana >= 0)
        {
            bool midTurn = IsPlayerTurn();
            // a gain mid-turn (a card or effect), or the refill for the turn
            if (midTurn && gainVFX != null && Time.time > suppressBurstUntil)
                Instantiate(gainVFX, transform.position, Quaternion.identity);
            else if (!midTurn && refillVFX != null)
                Instantiate(refillVFX, transform.position, Quaternion.identity);
        }
        if ((now > 0) != glowOn) SetGlow(now > 0);
        lastMana = now;
    }

    // the glow stops emitting at zero (what's showing fades out on its own) and starts again with mana
    private void SetGlow(bool on)
    {
        glowOn = on;
        foreach (ParticleSystem ps in ambient)
        {
            if (ps == null) continue;
            if (on) { if (!ps.isEmitting) ps.Play(false); }
            else ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    // looked up only on a gain (TurnManager.Instance would create one in scenes without it)
    private static bool IsPlayerTurn()
    {
        TurnManager turns = FindObjectOfType<TurnManager>();
        return turns != null && turns.GetTurnPhase() == TurnPhase.PLAYER_TURN;
    }
}
