using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatEntityManager : GenericSingleton<CombatEntityManager>
{
    public EncounterConstantsSO encounterConstants;
    public TurnPhaseEvent turnPhaseEvent;

    // This list assumes ordering
    [SerializeField]
    private List<CompanionInstance> companions = new List<CompanionInstance>();
    [SerializeField]
    private List<EnemyInstance> enemies = new List<EnemyInstance>();

    [SerializeField]
    private bool IS_DEVELOPMENT_MODE = false;

    private Dictionary<CombatEntityTriggerType, List<CombatEntityTrigger>> combatEntityTriggers =
            new Dictionary<CombatEntityTriggerType, List<CombatEntityTrigger>>() {
                {CombatEntityTriggerType.COMPANION_DIED, new List<CombatEntityTrigger>()},
                {CombatEntityTriggerType.ENEMY_DIED, new List<CombatEntityTrigger>()},
                {CombatEntityTriggerType.MINION_DIED, new List<CombatEntityTrigger>()}
            };

    public delegate IEnumerator OnEntityDamage(CombatInstance combatInstance);
    public PriorityEventDispatcher<OnEntityDamage> onEntityDamageDispatcher = new();

    public delegate IEnumerator OnEntityHealed(CombatInstance combatInstance);
    public PriorityEventDispatcher<OnEntityHealed> onEntityHealedDispatcher = new();

    public delegate IEnumerator OnCompanionGainedBlock(CombatInstance combatInstance);
    public PriorityEventDispatcher<OnCompanionGainedBlock> onBlockGainedDispatcher = new();

    private bool encounterEnded = false;

    private Dictionary<Companion, GameObject> companionToDeathVFXMap = new Dictionary<Companion, GameObject>();

    void Update()
    {
        if (IS_DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(EndCombatAfterEffectsResolve());
        }
    }

    public void registerCompanion(CompanionInstance companion)
    {
        companions.Add(companion);
    }

    public void registerEnemy(EnemyInstance enemy)
    {
        enemies.Add(enemy);
    }

    public List<CompanionInstance> getCompanions()
    {
        return new List<CompanionInstance>(companions);
    }

    // position 0 is front
    // position -1 is back
    public CompanionInstance GetCompanionInstanceAtPosition(int position)
    {
        if (companions.Count <= 0) return null;
        if (position == -1 || position >= companions.Count)
        {
            return companions[companions.Count - 1];
        }

        return companions[position];
    }

    public CompanionInstance getCompanionInstanceById(string id)
    {
        foreach (CompanionInstance instance in companions)
        {
            if (instance.companion.id.Equals(id))
            {
                return instance;
            }
        }
        Debug.LogWarning("No companion found by id: " + id);
        return null;
    }

    public CompanionInstance getCompanionInstanceForCombatInstance(CombatInstance combatInstance)
    {
        foreach (CompanionInstance instance in companions)
        {
            if (instance.combatInstance == combatInstance)
            {
                return instance;
            }
        }
        return null;
    }

    public EnemyInstance getEnemyInstanceForCombatInstance(CombatInstance combatInstance)
    {
        foreach (EnemyInstance instance in enemies)
        {
            if (instance.combatInstance == combatInstance)
            {
                return instance;
            }
        }
        return null;
    }

    public List<CombatInstance> getEnemyTargets()
    {
        List<CombatInstance> retList = new List<CombatInstance>();
        foreach (CompanionInstance companionInstance in companions)
        {
            retList.Add(companionInstance.combatInstance);
        }
        return retList;
    }

    public List<EnemyInstance> getEnemies()
    {
        return enemies;
    }

    public void EnemyDied(EnemyInstance enemyInstance)
    {
        enemies.Remove(enemyInstance);
        executeTriggers(CombatEntityTriggerType.ENEMY_DIED);
        Debug.Log("EnemyDied: " + enemies.Count);
        if (enemies.Count == 0)
        {
            StartCoroutine(EndCombatAfterEffectsResolve());
            encounterEnded = true;
        }
    }

    public void CompanionDied(CompanionInstance companionInstance)
    {
        companions.Remove(companionInstance);
        executeTriggers(CombatEntityTriggerType.COMPANION_DIED);
        if (companions.Count == 0)
        {
            StartCoroutine(
                turnPhaseEvent.RaiseAtEndOfFrameCoroutine(
                    new TurnPhaseEventInfo(TurnPhase.END_ENCOUNTER)));
            encounterEnded = true;
        }
    }

    public void registerTrigger(CombatEntityTrigger trigger)
    {
        combatEntityTriggers[trigger.type].Add(trigger);
    }

    public void unregisterTrigger(CombatEntityTrigger trigger)
    {
        combatEntityTriggers[trigger.type].Remove(trigger);
    }

    public void executeTriggers(CombatEntityTriggerType triggerType)
    {
        foreach (CombatEntityTrigger trigger in combatEntityTriggers[triggerType])
        {
            StartCoroutine(trigger.callback.GetEnumerator());
        }
    }

    private IEnumerator EndCombatAfterEffectsResolve()
    {
        Debug.Log("Waiting for all effects running to resolve");
        yield return new WaitUntil(() => EffectManager.Instance.IsEffectRunning() == false);
        Debug.Log("All effects resolved");
        this.encounterEnded = true;
        StartCoroutine(
            turnPhaseEvent.RaiseAtEndOfFrameCoroutine(
                new TurnPhaseEventInfo(TurnPhase.END_ENCOUNTER)));
    }

    public IEnumerator OnDamageTaken(CombatInstance combatInstance)
    {
        yield return StartCoroutine(onEntityDamageDispatcher.Invoke(combatInstance).GetEnumerator());
    }

    public IEnumerator OnHeal(CombatInstance combatInstance)
    {
        yield return StartCoroutine(onEntityHealedDispatcher.Invoke(combatInstance).GetEnumerator());
    }

    public IEnumerator OnBlockGained(CombatInstance combatInstance)
    {
        yield return StartCoroutine(onBlockGainedDispatcher.Invoke(combatInstance).GetEnumerator());
    }

    public bool IsEncounterEnded()
    {
        return this.encounterEnded;
    }

    public void SpawnEntityOnDeathVfx(CombatInstance combatInstance)
    {
        if (combatInstance.TryGetComponent<CompanionInstance>(out CompanionInstance companionInstance))
        {
            GameObject deathVFX = Instantiate(encounterConstants.companionDeathPrefab, combatInstance.transform.position, Quaternion.identity);
            companionToDeathVFXMap[companionInstance.companion] = deathVFX;
            deathVFX.ScaleSelfAndChildren(1.1f);
        }
        else if (combatInstance.TryGetComponent<EnemyInstance>(out EnemyInstance enemyInstance))
        {
            if (enemyInstance.enemy.enemyType.enemyDisplayType == DisplayType.MEOTHRA) return; // Don't spawn death VFX for Boss
            GameObject deathVFX = Instantiate(encounterConstants.enemyDeathPrefab, combatInstance.transform.position, Quaternion.identity);
            deathVFX.ScaleSelfAndChildren(1.1f);
        }
    }

    public IEnumerator ReviveCompanions(List<Companion> companionsToRevive, GameObject vfxPrefab, float waitTime)
    {
        foreach (Companion companion in companionsToRevive)
        {
            if (companionToDeathVFXMap.ContainsKey(companion))
            {
                GameObject deathVFX = companionToDeathVFXMap[companion];
                Vector3 pos = deathVFX.transform.position;
                Destroy(deathVFX.gameObject);
                companionToDeathVFXMap.Remove(companion);
                Instantiate(vfxPrefab, pos, Quaternion.identity);
                yield return new WaitForSeconds(waitTime);
            }
        }
        yield return null;
    }

    public IEnumerator HealAliveCompanions(GameObject vfxPrefab, float waitTime)
    {
        foreach (CompanionInstance companion in getCompanions())
        {
            Instantiate(vfxPrefab, companion.gameObject.transform.position, Quaternion.identity);
            yield return new WaitForSeconds(waitTime);
        }
        yield return null;
    }
}

public class CombatEntityTrigger {
    public CombatEntityTriggerType type;
    public IEnumerable callback;

    public CombatEntityTrigger(CombatEntityTriggerType type, IEnumerable callback) {
        this.type = type;
        this.callback = callback;
    }
}

public enum CombatEntityTriggerType {
    COMPANION_DIED,
    ENEMY_DIED,
    MINION_DIED
}