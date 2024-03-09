using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyBrain
{
    public List<EnemyBehavior> behaviors;
    public bool sequentialBehaviors = false;
    public int nextBehaviorIndex = 0;
    
    // To be revisited
    private float attackTime = 0.5f;

    public EnemyIntent ChooseIntent(EnemyInstance self) {
        // Just randomly choose behavior for now, but there's room to make this more smarter
        int behaviorIndex = sequentialBehaviors ? nextBehaviorIndex : UnityEngine.Random.Range(0, behaviors.Count);
        if(behaviors.Count == 0) {
            Debug.LogError("No behaviors defined for enemy");
            return null;
        }
        nextBehaviorIndex = (nextBehaviorIndex + 1) % behaviors.Count;
        EnemyBehavior action = behaviors[behaviorIndex];
        CombatInstance target = ChooseTargets(action.enemyTargetMethod, self);
        return new EnemyIntent(
            // I'm aware this is bad, stick with me for a sec
            new List<CombatInstance>() { target },
            attackTime, 
            action.intent,
            action.targetsKey,
            action.displayValue + self.combatInstance.combatStats.baseAttackDamage,
            action.effectSteps);
    } 

    private CombatInstance ChooseTargets(EnemyTargetMethod targetMethod, EnemyInstance self) {
        CombatInstance target = null;
        List<CombatInstance> possibleTargets = new List<CombatInstance>();
        switch (targetMethod) {
            case EnemyTargetMethod.FirstCompanion:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(0).combatInstance;
            break;

            case EnemyTargetMethod.LastCompanion:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(-1).combatInstance;
            break;

            case EnemyTargetMethod.SecondFromFront:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(1).combatInstance;
            break;

            case EnemyTargetMethod.ThirdFromFront:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(2).combatInstance;
            break;

            case EnemyTargetMethod.RandomCompanion:
                CombatEntityManager.Instance.getCompanions()
                    .ForEach(companion => possibleTargets.Add(companion.combatInstance));
                target = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
            break;

            case EnemyTargetMethod.RandomEnemyNotSelf:
                CombatEntityManager.Instance.getEnemies()
                    .ForEach(enemy => {
                        if (enemy != self) {
                            possibleTargets.Add(enemy.combatInstance);
                        }
                    });
                target = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
            break;

            case EnemyTargetMethod.LowestHealth:
                List<CompanionInstance> companions = CombatEntityManager.Instance.getCompanions();
                target = companions[0].combatInstance;
                foreach (CompanionInstance instance in companions) {
                    if (instance.combatInstance.combatStats.currentHealth < target.combatStats.currentHealth) {
                        target = instance.combatInstance;
                    }
                }
            break;
        }
        return target;
    }
}