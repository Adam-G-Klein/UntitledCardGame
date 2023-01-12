using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBrainContext {
    public EnemyInstance enemyInstance;
    public CompanionManager companionManager;

    public EnemyBrainContext(EnemyInstance enemyInstance, CompanionManager companionManager) {
        this.enemyInstance = enemyInstance;
        this.companionManager = companionManager;
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


    // IEnumerable because the enemy may need to figure
    // other things out before it can choose a behavior to get an intent from
    public virtual IEnumerable chooseIntent(EnemyBrainContext context) {
        // Just random for now
        Debug.Log("Enemy " + context.enemyInstance.id + " is choosing an intent");
        context.enemyInstance.currentIntent = behaviors[UnityEngine.Random.Range(0, behaviors.Count)].getIntent(context);
        yield return null;
    }

    public virtual IEnumerable act(EnemyBrainContext context) {
        EnemyIntent chosenIntent = context.enemyInstance.currentIntent;
        int damage = chosenIntent.damage;
        context.enemyInstance.raiseEnemyEffectEvent(new EnemyEffectEventInfo(chosenIntent));
        Debug.Log("Enemy " + context.enemyInstance.id + " attacked companion " + chosenIntent.targets[0].id + " for " + damage + " damage");
        yield return new WaitForSeconds(chosenIntent.attackTime);
        chosenIntent = null;
    }

}
