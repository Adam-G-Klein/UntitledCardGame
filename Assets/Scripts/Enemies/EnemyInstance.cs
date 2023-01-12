using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIntent {
    public List<TargettableEntity> targets;
    public int damage;
    public float attackTime;
    public Dictionary<StatusEffect, int> statusEffects;

    public EnemyIntent(List<TargettableEntity> targets, int damage, float attackTime, Dictionary<StatusEffect, int> statusEffects) {
        this.targets = targets;
        this.damage = damage;
        this.attackTime = attackTime;
        this.statusEffects = statusEffects;
    }

}

public class EnemyInstance : CombatEntityInstance {
    public Enemy enemy;

    [SerializeField]
    private float attackTime = 0.5f;

    [Space(5)]
    [SerializeField]
    private EnemyEffectEvent enemyEffectEvent;

    private CompanionManager companionManager;
    private TurnManager turnManager;
    private TurnPhaseTrigger chooseIntentTrigger;
    private TurnPhaseTrigger actTrigger;
    private EnemyBrainContext brainContext;
    public EnemyIntent currentIntent;

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        this.baseStats = enemy;
        this.id = enemy.id; // Crucial (and forgettable)
        this.stats = new CombatEntityInEncounterStats(enemy);
        GameObject turnManagerObject = GameObject.Find("TurnManager");
        if(turnManagerObject != null)  turnManager = turnManagerObject.GetComponent<TurnManager>();
        else Debug.LogError("No TurnManager found in scene, won't have the turn cycle occurring");
        companionManager = GameObject.FindGameObjectWithTag("CompanionManager").GetComponent<CompanionManager>();

        brainContext = new EnemyBrainContext(this, companionManager);
        registerTurnPhaseTriggers(brainContext);
    }

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
                    die();
                }
                break;
            case SimpleEffectName.Buff:
                stats.strength += item.scale;
                break;
            case SimpleEffectName.Discard:
                Debug.LogWarning("omg an enemy being discarded what happened");
                break;

        }

    }

    private void registerTurnPhaseTriggers(EnemyBrainContext brainContext) {
        chooseIntentTrigger = new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, enemy.chooseIntent(brainContext));
        turnManager.addTurnPhaseTrigger(chooseIntentTrigger);
        actTrigger = new TurnPhaseTrigger(TurnPhase.ENEMIES_TURN, enemy.act(brainContext));
        turnManager.addTurnPhaseTrigger(actTrigger);
    }

    private void die() {
        Debug.Log("Enemy " + id + " died");
        turnManager.removeTurnPhaseTrigger(chooseIntentTrigger);
        turnManager.removeTurnPhaseTrigger(actTrigger);
        StartCoroutine(deathEvent.RaiseAtEndOfFrameCoroutine(new CombatEntityDeathEventInfo(this)));
    }

    public void turnStartEventHandler(){
        StartCoroutine("attackCoroutine");
    }


    public void raiseEnemyEffectEvent(EnemyEffectEventInfo info){
        StartCoroutine(enemyEffectEvent.RaiseAtEndOfFrameCoroutine(info));
    }

    public override CombatEntityInEncounterStats getCombatEntityInEncounterStats(){
        return stats;
    }

}
