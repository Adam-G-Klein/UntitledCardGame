using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public List<TargettableEntity> targets;
    public int damage;
    public float attackTime;
    public Dictionary<StatusEffect, int> statusEffects;
    public EnemyIntentType intentType;

    public EnemyIntent(List<TargettableEntity> targets, int damage, float attackTime, Dictionary<StatusEffect, int> statusEffects, EnemyIntentType intentType) {
        this.targets = targets;
        this.damage = damage;
        this.attackTime = attackTime;
        this.statusEffects = statusEffects;
        this.intentType = intentType;
    }

}

public class EnemyInstance : CombatEntityInstance {
    public Enemy enemy;

    [Space(5)]
    [SerializeField]
    private EnemyEffectEvent enemyEffectEvent;

    private CompanionManager companionManager;
    private TurnManager turnManager;
    private TurnPhaseTrigger chooseIntentTrigger;
    private TurnPhaseTrigger actTrigger;
    private EnemyBrainContext brainContext;
    public EnemyIntent currentIntent;

    private EnemyManager enemyManager;
    // reference for resetting intent if the enemy is taunted
    private EnemyIntentDisplay intentDisplay;

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

    // gotta put this into the base class somehow.
    // problem is that enemies and companions handle this stuff differently.
    // so idk
    public void cardEffectEventHandler(CardEffectEventInfo item){
        if(!item.targets.Contains(this)){
            return;
        }
        print("Enemy " + id + " processing card effect event");
        switch(item.effectName) {
            case SimpleEffectName.Draw:
                Debug.LogWarning("omg an enemy is drawing cards what happened");
                break;
            case SimpleEffectName.Damage:
                stats.currentHealth = Mathf.Max(stats.currentHealth - item.scale, 0);
                if(stats.currentHealth == 0) {
                    onDeath();
                }
                break;
            case SimpleEffectName.Buff:
                base.applyStatusEffect(StatusEffect.Strength, item.scale);
                break;
            case SimpleEffectName.Weaken:
                base.applyStatusEffect(StatusEffect.Weakness, item.scale);
                break;
            case SimpleEffectName.Discard:
                Debug.LogWarning("omg an enemy is being discarded what happened");
                break;
        }

    }

    private void registerTurnPhaseTriggers(EnemyBrainContext brainContext) {
        chooseIntentTrigger = new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, enemy.chooseIntent(brainContext));
        turnManager.addTurnPhaseTrigger(chooseIntentTrigger);
        actTrigger = new TurnPhaseTrigger(TurnPhase.ENEMIES_TURN, enemy.act(brainContext));
        turnManager.addTurnPhaseTrigger(actTrigger);
    }

    protected override IEnumerator onDeath()
    {
        Debug.Log("Enemy " + id + " died");
        turnManager.removeTurnPhaseTrigger(chooseIntentTrigger);
        turnManager.removeTurnPhaseTrigger(actTrigger);
        return base.onDeath();
    }

    public void turnStartEventHandler(){
        StartCoroutine("attackCoroutine");
    }

    public void setTauntedTarget(TargettableEntity target){
        if(currentIntent.intentType == EnemyIntentType.Buff){
            // not gonna overcomplicate this edge case right now
            currentIntent = new EnemyIntent(new List<TargettableEntity>(){target},
                currentIntent.damage, 
                currentIntent.attackTime, 
                new Dictionary<StatusEffect, int>(), 
                EnemyIntentType.SmallAttack);
        } else {
            currentIntent = new EnemyIntent(new List<TargettableEntity>(){target},
                currentIntent.damage, 
                currentIntent.attackTime, 
                currentIntent.statusEffects, 
                currentIntent.intentType);
        }
        intentDisplay.clearIntent();
        StartCoroutine(intentDisplay.displayIntent(this).GetEnumerator());

    }


    public void raiseEnemyEffectEvent(EnemyIntent intent){
        StartCoroutine(enemyEffectEvent.RaiseAtEndOfFrameCoroutine(new EnemyEffectEventInfo(this, intent)));
    }

}
