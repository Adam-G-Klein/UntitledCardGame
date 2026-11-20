using System.Collections;
using UnityEngine;

/*
    VFX: the headliner of an elite encounter gets a grander flair in the smoke behind it than the 2- and 3-star rats
    get (CompanionLevelFlair), in black with small bright purple bits, in its own smoke (left as it is), and the
    fight gets a purple screen vignette rising from the bottom of the screen (the bottom half of Meothra's
    vignette, redrawn for elites), once per fight, on the camera behind the characters. Meothra keeps her own look. It reads the encounter itself, so EnemyInstance is
    unchanged. Purely visual. Set on WorldspaceEnemy's Combat Screen Highlight (VFX Retool / Wire smoke looks); the
    flair prefab comes from VFX Retool / Build smoke flair.
*/
public class EliteEnemyFlair : MonoBehaviour
{
    [SerializeField] private GameStateVariableSO gameState;
    [SerializeField] private GameObject eliteFlair;
    [Tooltip("On: only the elite fight's headliner (the enemy with the most max health, ties included) gets the flair. Off: every enemy in it.")]
    [SerializeField] private bool headlinerOnly = true;
    [Tooltip("Full-screen vignette spawned once per elite fight on the main camera (Elite Screen Vignette).")]
    [SerializeField] private GameObject screenVignette;
    [Tooltip("Height of the vignette's quad in world units at scale 1 (Meothra's screen quads are 11.5 tall).")]
    [SerializeField] private float vignetteHeight = 11.5f;

    private static GameObject vignetteInstance;

    private static bool IsHeadliner(EnemyInstance enemy)
    {
        if (CombatEntityManager.isDestroyed) return true;
        int mine = enemy.enemy.combatStats != null ? enemy.enemy.combatStats.maxHealth : 0, most = mine;
        foreach (EnemyInstance other in CombatEntityManager.Instance.getEnemies())
            if (other != null && other.enemy != null && other.enemy.combatStats != null) most = Mathf.Max(most, other.enemy.combatStats.maxHealth);
        return mine >= most;
    }

    [Tooltip("Big enemies (EnemyTypeSO.big, the elites) are drawn wider (EnemyView: 18% of the screen, not 16%): their smoke and flair grow by the same.")]
    [SerializeField] private float bigEnemyScale = 0.18f / 0.16f;

    private IEnumerator Start()
    {
        if (gameState == null || eliteFlair == null) yield break;
        // the enemy is set up by whoever spawned it; give it a few frames
        EnemyInstance enemy = null;
        for (int tries = 0; tries < 30; tries++)
        {
            enemy = GetComponentInParent<EnemyInstance>();
            if (enemy != null && enemy.enemy != null && enemy.enemy.enemyType != null) break;
            yield return null;
        }
        if (enemy == null || enemy.enemy == null || enemy.enemy.enemyType == null) yield break;
        bool big = enemy.enemy.enemyType.big && enemy.enemy.enemyType.enemyDisplayType != DisplayType.MEOTHRA;
        // the smoke behind a big enemy fits its bigger token
        if (big) ScaleSmoke(gameObject, bigEnemyScale);
        if (!(gameState.activeEncounter.GetValue() is EnemyEncounter encounter) || !encounter.isEliteEncounter) yield break;
        if (enemy.enemy.enemyType.enemyDisplayType == DisplayType.MEOTHRA) yield break;
        if (headlinerOnly && !IsHeadliner(enemy)) yield break;
        GameObject instance = Instantiate(eliteFlair, transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        // the flair's layers scale with their hierarchy, so its root takes the big enemy's size
        if (big) instance.transform.localScale *= bigEnemyScale;
        SpawnScreenVignette();
    }

    // this smoke highlight's own particles (not the flair, spawned after): their sizes and emission areas
    private static void ScaleSmoke(GameObject smoke, float by)
    {
        foreach (ParticleSystem ps in smoke.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule m = ps.main;
            if (m.startSize3D) { m.startSizeXMultiplier *= by; m.startSizeYMultiplier *= by; m.startSizeZMultiplier *= by; }
            else m.startSizeMultiplier *= by;
            ParticleSystem.ShapeModule shape = ps.shape;
            if (shape.enabled) { shape.scale *= by; shape.radius *= by; }
        }
    }

    private void SpawnScreenVignette()
    {
        if (screenVignette == null || vignetteInstance != null) return;
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;
        vignetteInstance = Instantiate(screenVignette, cam.transform);
        vignetteInstance.transform.localPosition = new Vector3(0f, 0f, 10f);
        vignetteInstance.transform.localRotation = Quaternion.identity;
        // fill the screen with a little overscan so the edges never show
        float scale = cam.orthographicSize * 2f * 1.03f / Mathf.Max(vignetteHeight, 0.01f);
        vignetteInstance.transform.localScale = Vector3.one * scale;
    }
}
