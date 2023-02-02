using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBrainContext {
    public EnemyInstance enemyInstance;
    public CompanionManager companionManager;
    public EnemyManager enemyManager;

    public EnemyBrainContext(EnemyInstance enemyInstance, CompanionManager companionManager, EnemyManager enemyManager) {
        this.enemyInstance = enemyInstance;
        this.companionManager = companionManager;
        this.enemyManager = enemyManager;
    }
}

// Contains the list of enemy behaviors
// and the logic to choose between them
// The default will just be choosing one at random

// The idea being that multiple enemies could share the same brain
// but have different stats

[CreateAssetMenu(
    fileName = "EnemyBrain",
    menuName = "Enemies/Enemy Brain")]
public class EnemyBrain: ScriptableObject {

    [SerializeReference]
    public List<EnemyBehavior> behaviors;

    private EnemyBrainContext context;
    public bool sequentialBehaviors = false;
    public int nextBehaviorIndex = 0;


    // IEnumerable because the enemy may need to figure
    // other things out before it can choose a behavior to get an intent from
    public virtual IEnumerable chooseIntent(EnemyBrainContext context) {
        // Just random for now
        Debug.Log("Enemy " + context.enemyInstance.id + " is choosing an intent");
        int behaviorIndex = sequentialBehaviors ? nextBehaviorIndex : UnityEngine.Random.Range(0, behaviors.Count);
        context.enemyInstance.currentIntent = behaviors[behaviorIndex].getIntent(context);
        yield return null;
        nextBehaviorIndex = (nextBehaviorIndex + 1) % behaviors.Count;
    }

    public virtual IEnumerable act(EnemyBrainContext context) {
        EnemyIntent chosenIntent = context.enemyInstance.currentIntent;
        context.enemyInstance.raiseEnemyEffectEvent(chosenIntent);
        yield return new WaitForSeconds(chosenIntent.attackTime);
        context.enemyInstance.currentIntent = null;
    }

}
