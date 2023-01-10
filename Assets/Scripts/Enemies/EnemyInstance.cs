using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Setting up for when we want to display this
public class EnemyIntent {
    public string target;
    public int damage;
    public Dictionary<StatusEffect, int> statusEffects;

    public EnemyIntent(string target, int damage, Dictionary<StatusEffect, int> statusEffects) {
        this.target = target;
        this.damage = damage;
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
    private TurnPhaseTrigger turnPhaseTrigger;

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        this.baseStats = enemy;
        this.id = enemy.id; // Crucial (and forgettable)
        this.stats = new CombatEntityInEncounterStats(enemy);
        GameObject turnManagerObject = GameObject.Find("TurnManager");
        if(turnManagerObject != null)  turnManager = turnManagerObject.GetComponent<TurnManager>();
        else Debug.LogError("No TurnManager found in scene, won't have the turn cycle occurring");
        turnPhaseTrigger = new TurnPhaseTrigger(TurnPhase.ENEMIES_TURN, attackCoroutine());
        turnManager.addTurnPhaseTrigger(turnPhaseTrigger);
        companionManager = GameObject.FindGameObjectWithTag("CompanionManager").GetComponent<CompanionManager>();
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

    private void die() {
        Debug.Log("Enemy " + id + " died");
        turnManager.removeTurnPhaseTrigger(turnPhaseTrigger);
        StartCoroutine(deathEvent.RaiseAtEndOfFrameCoroutine(new CombatEntityDeathEventInfo(this)));
    }

    public void turnStartEventHandler(){
        StartCoroutine("attackCoroutine");
    }


    IEnumerable attackCoroutine(){
        // TODO: determine beforehand so the player can see intents 
        EnemyIntent intent = enemy.getNewEnemyIntent(companionManager.getCompanionIds(), stats);
        string targetId = intent.target;
        int damage = intent.damage;
        StartCoroutine(enemyEffectEvent.RaiseAtEndOfFrameCoroutine(
            new EnemyEffectEventInfo(
                damage,
                new List<string> {targetId},
                new Dictionary<StatusEffect, int> { {StatusEffect.Weakness, 1} })));
        Debug.Log("Enemy " + id + " attacked companion " + targetId + " for " + damage + " damage");
        yield return new WaitForSeconds(attackTime);
    }

    public override CombatEntityInEncounterStats getCombatEntityInEncounterStats(){
        return stats;
    }

}
