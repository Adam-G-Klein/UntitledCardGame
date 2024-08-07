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
        CompanionInstance target = ChooseTargets(action.enemyTargetMethod, self);
        return new EnemyIntent(
            // I'm aware this is bad, stick with me for a sec
            new List<CompanionInstance>() { target },
            attackTime,
            action.intent,
            action.targetsKey,
            UpdateDisplayValue(self, action.displayValue, action),
            action.effectSteps);
    }

    private CompanionInstance ChooseTargets(EnemyTargetMethod targetMethod, EnemyInstance self) {
        CompanionInstance target = null;
        List<CompanionInstance> possibleTargets = new List<CompanionInstance>();
        switch (targetMethod) {
            case EnemyTargetMethod.FirstCompanion:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(0);
            break;

            case EnemyTargetMethod.LastCompanion:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(-1);
            break;

            case EnemyTargetMethod.SecondFromFront:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(1);
            break;

            case EnemyTargetMethod.ThirdFromFront:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(2);
            break;

            case EnemyTargetMethod.RandomCompanion:
                CombatEntityManager.Instance.getCompanions()
                    .ForEach(companion => possibleTargets.Add(companion));
                target = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
            break;

            // case EnemyTargetMethod.RandomEnemyNotSelf:
            //     CombatEntityManager.Instance.getEnemies()
            //         .ForEach(enemy => {
            //             if (enemy != self) {
            //                 possibleTargets.Add(enemy.combatInstance);
            //             }
            //         });
            //     target = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
            // break;

            case EnemyTargetMethod.LowestHealth:
                List<CompanionInstance> companions = CombatEntityManager.Instance.getCompanions();
                target = companions[0];
                foreach (CompanionInstance instance in companions) {
                    if (instance.combatInstance.combatStats.getCurrentHealth() < target.combatInstance.combatStats.getCurrentHealth()) {
                        target = instance;
                    }
                }
            break;
        }
        return target;
    }

    private int UpdateDisplayValue(EnemyInstance instance, int value, EnemyBehavior behavior) {
        if (behavior.intent != EnemyIntentType.BigAttack && behavior.intent != EnemyIntentType.SmallAttack) {
            return value;
        }

        return value + instance.combatInstance.combatStats.baseAttackDamage + instance.combatInstance.statusEffects[StatusEffect.Strength];
    }
}