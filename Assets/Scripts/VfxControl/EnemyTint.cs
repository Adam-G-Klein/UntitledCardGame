using UnityEngine;

/*
    Shared status effects (block absorb / break / expire) are authored in the ally colours. When one of
    them spawns on an enemy, this recolours it into the enemy language: red with black, like the
    "Enemy (Red)" effects. Each particle system's start colour is rebuilt from its own brightness, so
    light layers become red and dark layers stay dark.
*/
public class EnemyTint : MonoBehaviour
{
    [SerializeField] private Color enemyLight = new Color(1f, 0.25f, 0.2f, 1f);
    [SerializeField] private Color enemyCore = new Color(0.85f, 0.05f, 0.08f, 1f);
    [Tooltip("How close (world units) a combat entity must be to count as the one this effect is on.")]
    [SerializeField] private float matchRadius = 0.6f;

    private void Awake()
    {
        if (!OnEnemy()) return;
        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule m = ps.main;
            ParticleSystem.MinMaxGradient g = m.startColor;
            switch (g.mode)
            {
                case ParticleSystemGradientMode.Color: m.startColor = Recolour(g.color); break;
                case ParticleSystemGradientMode.TwoColors: m.startColor = new ParticleSystem.MinMaxGradient(Recolour(g.colorMin), Recolour(g.colorMax)); break;
            }
        }
    }

    private Color Recolour(Color c)
    {
        float lum = 0.3f * c.r + 0.59f * c.g + 0.11f * c.b;
        Color target = lum > 0.75f ? enemyLight : enemyCore;
        Color result = lum < 0.15f ? c : target * Mathf.Clamp(lum * 1.3f, 0.35f, 1.6f);
        result.a = c.a;
        return result;
    }

    private bool OnEnemy()
    {
        foreach (CombatInstance ci in FindObjectsOfType<CombatInstance>())
        {
            if ((ci.transform.position - transform.position).sqrMagnitude > matchRadius * matchRadius) continue;
            return ci.parentType == CombatInstance.CombatInstanceParent.ENEMY;
        }
        return false;
    }
}
