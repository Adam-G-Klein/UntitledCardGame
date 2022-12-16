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
    public int currentHealth;
    public int baseAttackDamage;

    [SerializeField]
    private float attackTime = 0.5f;

    [Space(5)]
    public SpriteRenderer spriteRenderer;
    [SerializeField]
    private EnemyInstantiatedEvent enemyInstantiatedEvent;
    [SerializeField]
    private EnemyEffectEvent enemyEffectEvent;

    [SerializeField]
    private EnemyTurnFinishedEvent enemyTurnFinishedEvent;

    private CompanionManager companionManager;

    // Start is called before the first frame update
    void Start() {
        this.baseStats = enemy;
        this.stats = new CombatEntityInEncounterStats(enemy);
        this.spriteRenderer.sprite = enemy.enemyType.sprite;
        StartCoroutine(enemyInstantiatedEvent.RaiseAtEndOfFrameCoroutine(new EnemyInstantiatedEventInfo(this)));
        companionManager = GameObject.FindGameObjectWithTag("CompanionManager").GetComponent<CompanionManager>();
    }

    public void cardEffectEventHandler(CardEffectEventInfo item){
        if(!item.targets.Contains(baseStats.getId())){
            return;
        }
        print("Enemy " + baseStats.getId() + " processing card effect event");
        switch(item.effectName) {
            case SimpleEffectName.Draw:
                Debug.LogWarning("omg an enemy is drawing cards what happened");
                break;
            case SimpleEffectName.Damage:
                print("Enemy " + baseStats.getId() + " took " + item.scale + " damage");
                stats.currentHealth -= item.scale;
                break;
            case SimpleEffectName.Buff:
                stats.strength += item.scale;
                break;

        }

    }

    public void turnStartEventHandler(){
        StartCoroutine("attackCoroutine");
    }


    IEnumerator attackCoroutine(){
        // TODO: determine beforehand so the player can see intents 
        EnemyIntent intent = enemy.getNewEnemyIntent(companionManager.getCompanionIds(), stats);
        string targetId = intent.target;
        int damage = intent.damage;
        StartCoroutine(enemyEffectEvent.RaiseAtEndOfFrameCoroutine(
            new EnemyEffectEventInfo(
                damage,
                new List<string> {targetId},
                new Dictionary<StatusEffect, int> { {StatusEffect.Weakness, 1} })));
        Debug.Log("Enemy " + baseStats.getId() + " attacked companion " + targetId + " for " + damage + " damage");
        yield return new WaitForSeconds(attackTime);
        enemyTurnFinishedEvent.Raise(new EnemyTurnFinishedEventInfo(baseStats.getId()));
        
    }


    public int getHealth(){
        return currentHealth;
    }

    public int getMaxHealth() {
        return enemy.enemyType.maxHealth;
    }

    public override CombatEntityInEncounterStats getCombatEntityInEncounterStats(){
        return stats;
    }

}
