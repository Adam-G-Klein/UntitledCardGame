using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInstance : MonoBehaviour, CombatEntityInstance {
    public Enemy enemy;
    public int currentHealth;
    public int baseAttackDamage;

    [SerializeField]
    private float attackTime = 0.5f;

    [Space(5)]
    public SpriteRenderer spriteRenderer;
    public string id;
    [SerializeField]
    private EnemyInstantiatedEvent enemyInstantiatedEvent;
    [SerializeField]
    private EnemyEffectEvent enemyEffectEvent;

    [SerializeField]
    private EnemyTurnFinishedEvent enemyTurnFinishedEvent;

    private CompanionManager companionManager;

    private CombatEntityInEncounterStats stats;
    
    // Start is called before the first frame update
    void Start() {
        this.stats = new CombatEntityInEncounterStats(enemy);
        this.spriteRenderer.sprite = enemy.enemyType.sprite;
        this.id = Id.newGuid();
        StartCoroutine(enemyInstantiatedEvent.RaiseAtEndOfFrameCoroutine(new EnemyInstantiatedEventInfo(this)));
        companionManager = GameObject.FindGameObjectWithTag("CompanionManager").GetComponent<CompanionManager>();
    }

    public void cardEffectEventHandler(CardEffectEventInfo item){
        if(!item.targets.Contains(this.id)){
            return;
        }
        switch(item.effectName) {
            case CardEffectName.Draw:
                Debug.LogWarning("omg an enemy is drawing cards what happened");
                break;
            case CardEffectName.Damage:
                stats.currentHealth -= item.scale;
                break;
            case CardEffectName.Buff:
                stats.currentAttackDamage += item.scale;
                break;

        }

    }

    public void turnStartEventHandler(){
        StartCoroutine("attackCoroutine");
        

    }

    IEnumerator attackCoroutine(){
        // TODO: determine beforehand so the player can see intents 
        string targetId = companionManager.getRandomCompanionId();
        int damage = Random.Range(1,5);
        StartCoroutine(enemyEffectEvent.RaiseAtEndOfFrameCoroutine(
            new EnemyEffectEventInfo(
                EnemyEffectName.Damage, 
                damage,
                new List<string> {targetId})));
        Debug.Log("Enemy " + id + " attacked companion " + targetId + " for " + damage + " damage");
        yield return new WaitForSeconds(attackTime);
        enemyTurnFinishedEvent.Raise(new EnemyTurnFinishedEventInfo(id));
        
    }


    public int getHealth(){
        return currentHealth;
    }

    public int getMaxHealth() {
        return enemy.enemyType.maxHealth;
    }

    public CombatEntityInEncounterStats getCombatEntityInEncounterStats(){
        return stats;
    }

}
