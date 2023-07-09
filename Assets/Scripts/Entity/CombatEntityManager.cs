using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEntityManager : GenericSingleton<CombatEntityManager>
{
    public EncounterConstantsSO encounterConstants;
    public TurnPhaseEvent turnPhaseEvent;

    private List<CompanionInstance> companions = new List<CompanionInstance>();
    private List<MinionInstance> minions = new List<MinionInstance>();
    private List<EnemyInstance> enemies = new List<EnemyInstance>();

    private Dictionary<CombatEntityTrigger, List<IEnumerable>> combatEntityTriggers =
            new Dictionary<CombatEntityTrigger, List<IEnumerable>>() {
                {CombatEntityTrigger.COMPANION_DIED, new List<IEnumerable>()},
                {CombatEntityTrigger.ENEMY_DIED, new List<IEnumerable>()},
                {CombatEntityTrigger.MINION_DIED, new List<IEnumerable>()}
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

    public CompanionInstance getCompanionInstanceById(string id) {
        foreach (CompanionInstance instance in companions) {
            if (instance.id.Equals(id)){
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
            if (instance.id.Equals(id)) {
                return instance;
            }
        }
        Debug.LogWarning("No minion found by id: " + id);
        return null;
    }

    public CombatEntityWithDeckInstance getEntityWithDeckById(string id) {
        CombatEntityWithDeckInstance instance = getCompanionInstanceById(id);
        if (instance != null) {
            return instance;
        }
        instance = getMinionInstanceById(id);
        if (instance == null) {
            Debug.LogError("CombatEntityManager: getEntityWithDeckById can't find entity with id " + id);
        }
        return instance;
    }

    public List<TargettableEntity> getEnemyTargets() {
        List<TargettableEntity> retList = new List<TargettableEntity>(minions);
        retList.AddRange(companions);
        return retList;
    }

    public List<EnemyInstance> getEnemies() {
        return enemies;
    }
    
    public EnemyInstance getEnemyInstanceById(string id) {
        foreach (EnemyInstance instance in enemies) {
            if (instance.id.Equals(id)) {
                return instance;
            }
        }
        Debug.LogWarning("No enemy found by id: " + id);
        return null;
    }

    public void handleCombatEffect(CombatEffectEventInfo info) {
        foreach (CompanionInstance companion in companions) {
            if (info.targets.Contains(companion)) {
                companion.combatEffectEventHandler(info);
            }
        }

        foreach (EnemyInstance enemy in enemies) {
            if (info.targets.Contains(enemy)) {
                enemy.combatEffectEventHandler(info);
            }
        }

        foreach (MinionInstance minion in minions) {
            if (info.targets.Contains(minion)) {
                minion.combatEffectEventHandler(info);
            }
        }
    }

    public void combatEntityDied(CombatEntityInstance instance) {
        switch (instance.entityType) {
            case EntityType.Companion:
                CompanionInstance companion = getCompanionInstanceById(instance.id);
                companions.Remove(companion);
                executeTriggers(CombatEntityTrigger.COMPANION_DIED);
                if (companions.Count == 0) {
                    StartCoroutine(
                        turnPhaseEvent.RaiseAtEndOfFrameCoroutine(
                            new TurnPhaseEventInfo(TurnPhase.END_ENCOUNTER)));
                }
            break;

            case EntityType.Minion:
                MinionInstance minion = getMinionInstanceById(instance.id);
                minions.Remove(minion);
                executeTriggers(CombatEntityTrigger.MINION_DIED);
            break;

            case EntityType.Enemy:
                EnemyInstance enemy = getEnemyInstanceById(instance.id);
                enemies.Remove(enemy);
                executeTriggers(CombatEntityTrigger.ENEMY_DIED);
                if (enemies.Count == 0) {
                    StartCoroutine(
                        turnPhaseEvent.RaiseAtEndOfFrameCoroutine(
                            new TurnPhaseEventInfo(TurnPhase.END_ENCOUNTER)));
                }
            break;
        }
    }

    public void registerTrigger(CombatEntityTrigger trigger, IEnumerable callback) {
        combatEntityTriggers[trigger].Add(callback);
    }

    public void executeTriggers(CombatEntityTrigger trigger) {
        foreach (IEnumerable ienumerable in combatEntityTriggers[trigger]) {
            StartCoroutine(ienumerable.GetEnumerator());
        }
    }
}

public enum CombatEntityTrigger {
    COMPANION_DIED,
    ENEMY_DIED,
    MINION_DIED
}