using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyBrain
{
    public List<EnemyBehavior> behaviors;
    [SerializeField]
    public EnemyBehaviorPattern behaviorType;
    public int behaviorIndex = 0;
    public int nextBehaviorIndex = 0;

    // To be revisited
    private float attackTime = 0.5f;

    public EnemyIntent ChooseIntent(EnemyInstance self) {
        Debug.LogError("ChooseIntent");
        if(behaviors.Count == 0) {
            Debug.LogError("No behaviors defined for enemy");
            return null;
        }
        behaviorIndex = 0;
        switch (behaviorType) {
            case EnemyBehaviorPattern.SequentialCycling:
                behaviorIndex = nextBehaviorIndex;
                nextBehaviorIndex = (nextBehaviorIndex + 1) % behaviors.Count;
                break;
            case EnemyBehaviorPattern.Random:
                behaviorIndex = UnityEngine.Random.Range(0, behaviors.Count);
                break;
            case EnemyBehaviorPattern.SequentialWithSinkAtLastElement:
                behaviorIndex = nextBehaviorIndex;
                // Advance until we reach the end of the defined behaviors.
                nextBehaviorIndex = Math.Min(nextBehaviorIndex + 1, behaviors.Count - 1);
                break;
        }
        EnemyBehavior action = behaviors[behaviorIndex];
        // Note: this only allows the enemies to target companions for now.
        // There is nothing that allows targeting other enemies, but this is not
        // an important behavior to support for now.
        CompanionInstance target = ChooseTargets(action.enemyTargetMethod, self);
        List<CompanionInstance> targetList = new();
        if (target != null) {
            targetList.Add(target);
        }
        return new EnemyIntent(
            self,
            // I'm aware this is bad, stick with me for a sec
            targetList,
            attackTime,
            action.intent,
            action.targetsKey,
            action.displayValue,
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

    public enum EnemyBehaviorPattern {
        // Cycle through the behaviors in sequence.
        SequentialCycling,
        // Choose a behavior at random.
        Random,
        // Terminate on the last behavior and repeat until combat is over.
        SequentialWithSinkAtLastElement,
    }
}