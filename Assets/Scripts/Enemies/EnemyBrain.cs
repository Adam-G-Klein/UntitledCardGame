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

    public EnemyIntent ChooseIntent() {
        // Just randomly choose behavior for now, but there's room to make this more smarter
        int behaviorIndex = sequentialBehaviors ? nextBehaviorIndex : UnityEngine.Random.Range(0, behaviors.Count);
        nextBehaviorIndex = (nextBehaviorIndex + 1) % behaviors.Count;
        EnemyBehavior action = behaviors[behaviorIndex];
        CombatInstance target = ChooseTargets(action.enemyTargetMethod);
        return new EnemyIntent(
            // I'm aware this is bad, stick with me for a sec
            new List<CombatInstance>() { target },
            attackTime, 
            action.intent,
            action.targetsKey,
            action.effectSteps);
    } 

    private CombatInstance ChooseTargets(EnemyTargetMethod targetMethod) {
        List<CombatInstance> possibleTargets = new List<CombatInstance>();
        switch (targetMethod) {
            // TODO: For now this is just going to be random because we don't have the companion
            // ordering implemented yet.
            case EnemyTargetMethod.FirstCompanion:
            case EnemyTargetMethod.LastCompanion:
                possibleTargets.AddRange(CombatEntityManager.Instance.getEnemyTargets());
            break;

            case EnemyTargetMethod.RandomEnemy:
                CombatEntityManager.Instance.getEnemies()
                    .ForEach(enemy => possibleTargets.Add(enemy.combatInstance));
            break;
        }
        return possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
    }
}