using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEntityManager : GenericSingleton<CombatEntityManager>
{
    public EncounterConstantsSO encounterConstants;
    public TurnPhaseEvent turnPhaseEvent;

    // This list assumes ordering
    private List<CompanionInstance> companions = new List<CompanionInstance>();
    private List<MinionInstance> minions = new List<MinionInstance>();
    private List<EnemyInstance> enemies = new List<EnemyInstance>();

    private Dictionary<CombatEntityTriggerType, List<CombatEntityTrigger>> combatEntityTriggers =
            new Dictionary<CombatEntityTriggerType, List<CombatEntityTrigger>>() {
                {CombatEntityTriggerType.COMPANION_DIED, new List<CombatEntityTrigger>()},
                {CombatEntityTriggerType.ENEMY_DIED, new List<CombatEntityTrigger>()},
                {CombatEntityTriggerType.MINION_DIED, new List<CombatEntityTrigger>()}
            };

    public void registerCompanion(CompanionInstance companion) {
        companions.Add(companion);
    }

    public void registerMinion(MinionInstance minion) {
        minions.Add(minion);
    }

    public void registerEnemy(EnemyInstance enemy) {
        enemies.Add(enemy);
    }

    public List<CompanionInstance> getCompanions() {
        return companions;
    }

    // position 0 is front
    // position -1 is back
    public CompanionInstance GetCompanionInstanceAtPosition(int position) {
        if (position == -1 || position >= companions.Count) {
            return companions[companions.Count - 1];
        }

        return companions[position];
    }

    public CompanionInstance getCompanionInstanceById(string id) {
        foreach (CompanionInstance instance in companions) {
            if (instance.companion.id.Equals(id)){
                return instance;
            }
        }
        Debug.LogWarning("No companion found by id: " + id);
        return null;
    }
    
    public List<MinionInstance> getMinions() {
        return minions;
    }

    public MinionInstance getMinionInstanceById(string id) {
        foreach (MinionInstance instance in minions) {
            if (instance.minion.id.Equals(id)) {
                return instance;
            }
        }
        Debug.LogWarning("No minion found by id: " + id);
        return null;
    }

    public List<CombatInstance> getEnemyTargets() {
        List<CombatInstance> retList = new List<CombatInstance>();
        foreach (CompanionInstance companionInstance in companions) {
            retList.Add(companionInstance.combatInstance);
        }
        foreach (MinionInstance minionInstance in minions) {
            retList.Add(minionInstance.combatInstance);
        }
        return retList;
    }

    public List<EnemyInstance> getEnemies() {
        return enemies;
    }

    public void EnemyDied(EnemyInstance enemyInstance) {
        enemies.Remove(enemyInstance);
        executeTriggers(CombatEntityTriggerType.ENEMY_DIED);
        Debug.Log("EnemyDied: " + enemies.Count);
        if (enemies.Count == 0) {
            StartCoroutine(EndCombatAfterEffectsResolve());
        }
    }

    public void CompanionDied(CompanionInstance companionInstance) {
        companions.Remove(companionInstance);
        executeTriggers(CombatEntityTriggerType.COMPANION_DIED);
        if (companions.Count == 0) {
            StartCoroutine(
                turnPhaseEvent.RaiseAtEndOfFrameCoroutine(
                    new TurnPhaseEventInfo(TurnPhase.END_ENCOUNTER)));
        }
    }

    public void MinionDied(MinionInstance minionInstance) {
        minions.Remove(minionInstance);
        executeTriggers(CombatEntityTriggerType.MINION_DIED);
    }

    public void registerTrigger(CombatEntityTrigger trigger) {
        combatEntityTriggers[trigger.type].Add(trigger);
    }

    public void unregisterTrigger(CombatEntityTrigger trigger) {
        combatEntityTriggers[trigger.type].Remove(trigger);
    }

    public void executeTriggers(CombatEntityTriggerType triggerType) {
        foreach (CombatEntityTrigger trigger in combatEntityTriggers[triggerType]) {
            StartCoroutine(trigger.callback.GetEnumerator());
        }
    }

    private IEnumerator EndCombatAfterEffectsResolve() {
        Debug.Log("Waiting for all effects running to resolve");
        yield return new WaitUntil(() => EffectManager.Instance.IsEffectRunning() == false);
        Debug.Log("All effects resolved");
        StartCoroutine(
            turnPhaseEvent.RaiseAtEndOfFrameCoroutine(
                new TurnPhaseEventInfo(TurnPhase.END_ENCOUNTER)));
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