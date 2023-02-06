using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// for displaying the image above the enemy
public enum EnemyIntentType {
    BigAttack,
    SmallAttack,
    Buff,
    Debuff,
    // Possible later inclusions
    // Defend,
    // Heal,
    // None
}

public class EnemyIntent {
    public TargettableEntity attacker;
    public List<TargettableEntity> targets;
    public float attackTime;
    public Dictionary<CombatEffect, int> combatEffects;
    public EnemyIntentType intentType;

    public EnemyIntent(List<TargettableEntity> targets, float attackTime, Dictionary<CombatEffect, int> statusEffects, EnemyIntentType intentType, TargettableEntity attacker) {
        this.targets = targets;
        this.attackTime = attackTime;
        this.combatEffects = statusEffects;
        this.intentType = intentType;
        this.attacker = attacker;
    }

}

public class EnemyInstance : CombatEntityInstance {
    public Enemy enemy;

    [Space(5)]

    private CompanionManager companionManager;
    private TurnManager turnManager;
    private TurnPhaseTrigger chooseIntentTrigger;
    private TurnPhaseTrigger actTrigger;
    private TurnPhaseTrigger clearBlockTrigger;
    private EnemyBrainContext brainContext;
    public EnemyIntent currentIntent;

    private EnemyManager enemyManager;
    // reference for resetting intent if the enemy is taunted
    private EnemyIntentDisplay intentDisplay;
    [SerializeField]
    private CombatEffectEvent combatEffectEvent;

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        GameObject turnManagerObject = GameObject.Find("TurnManager");
        if(turnManagerObject != null)  turnManager = turnManagerObject.GetComponent<TurnManager>();
        else Debug.LogError("No TurnManager found in scene, won't have the turn cycle occurring");

        GameObject companionManagerObject = GameObject.Find("CompanionManager");
        if(companionManagerObject != null)  companionManager = companionManagerObject.GetComponent<CompanionManager>();
        else Debug.LogError("No CompanionManager found in scene, enemies won't be able to find companions to target");

        GameObject enemyManagerObject = GameObject.Find("EnemyManager");
        if(enemyManagerObject != null)  enemyManager = enemyManagerObject.GetComponent<EnemyManager>();
        else Debug.LogError("No EnemyManager found in scene, enemies won't be able to find other enemies to target");

        brainContext = new EnemyBrainContext(this, companionManager, enemyManager);
        intentDisplay = GetComponentInChildren<EnemyIntentDisplay>();
        registerTurnPhaseTriggers(brainContext);
    }


    private void registerTurnPhaseTriggers(EnemyBrainContext brainContext) {
        chooseIntentTrigger = new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, enemy.chooseIntent(brainContext));
        turnManager.addTurnPhaseTrigger(chooseIntentTrigger);
        actTrigger = new TurnPhaseTrigger(TurnPhase.ENEMIES_TURN, enemy.act(brainContext));
        turnManager.addTurnPhaseTrigger(actTrigger);
        clearBlockTrigger = new TurnPhaseTrigger(TurnPhase.END_PLAYER_TURN, clearBlock());
    }

    private IEnumerable clearBlock() {
        stats.statusEffects[StatusEffect.Defended] = 0;
        yield return null;
    }

    protected override IEnumerator onDeath(CombatEntityInstance killer)
    {
        Debug.Log("Enemy " + id + " died");
        turnManager.removeTurnPhaseTrigger(chooseIntentTrigger);
        turnManager.removeTurnPhaseTrigger(actTrigger);
        turnManager.removeTurnPhaseTrigger(intentDisplay.displayIntentTrigger);
        turnManager.removeTurnPhaseTrigger(clearBlockTrigger);
        return base.onDeath(killer);
    }

    public void turnStartEventHandler(){
        StartCoroutine("attackCoroutine");
    }

    public void setTauntedTarget(TargettableEntity target){
        if(currentIntent.intentType == EnemyIntentType.Buff){
            // not gonna overcomplicate this edge case right now
            currentIntent = new EnemyIntent(new List<TargettableEntity>(){target},
                currentIntent.attackTime, 
                new Dictionary<CombatEffect, int>(), 
                EnemyIntentType.SmallAttack, this);
        } else {
            currentIntent = new EnemyIntent(new List<TargettableEntity>(){target},
                currentIntent.attackTime, 
                currentIntent.combatEffects, 
                currentIntent.intentType, this);
        }
        intentDisplay.clearIntent();
        StartCoroutine(intentDisplay.displayIntent(this).GetEnumerator());

    }

    public void raiseEnemyEffectEvent(EnemyIntent intent){
        StartCoroutine(combatEffectEvent.RaiseAtEndOfFrameCoroutine(new CombatEffectEventInfo(intent)));
    }

}
