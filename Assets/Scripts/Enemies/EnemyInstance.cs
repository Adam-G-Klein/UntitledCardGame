using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CombatInstance))]
[RequireComponent(typeof(Targetable))]
public class EnemyInstance : MonoBehaviour {
    public Enemy enemy;
    public CombatInstance combatInstance;
    public Image spriteImage;

    public EnemyIntent currentIntent;
    public TurnPhaseTriggerEvent registerTurnPhaseTriggerEvent;
    public TurnPhaseTriggerEvent removeTurnPhaseTriggerEvent;

    // reference for resetting intent if the enemy is taunted
    private EnemyIntentDisplay intentDisplay;
    [SerializeField]
    private CombatEffectEvent combatEffectEvent;
    [HideInInspector]
    public List<TurnPhaseTrigger> turnPhaseTriggers = new List<TurnPhaseTrigger>();

    // Start is called before the first frame update
    public void Start() {
        CombatEntityManager.Instance.registerEnemy(this);
        this.intentDisplay = GetComponentInChildren<EnemyIntentDisplay>();
        spriteImage.sprite = enemy.enemyType.sprite;
        combatInstance.combatStats = enemy.combatStats;
        Debug.Log("EnemyInstance Start for enemy " + enemy.id + " initialized with combat stats (health): " + combatInstance.combatStats.getCurrentHealth());
        combatInstance.SetId(enemy.id);
        combatInstance.onDeathHandler += OnDeath;
        RegisterTurnPhaseTriggers();
    }


    private void RegisterTurnPhaseTriggers() {
        turnPhaseTriggers.Add(new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, DeclareIntent()));
        turnPhaseTriggers.Add(new TurnPhaseTrigger(TurnPhase.ENEMIES_TURN, EnactIntent()));
        turnPhaseTriggers.Add(new TurnPhaseTrigger(TurnPhase.END_PLAYER_TURN, ClearBlock()));
        foreach(TurnPhaseTrigger trigger in turnPhaseTriggers) {
            registerTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(trigger));
        }
    }

    private void UnregisterTurnPhaseTriggers() {
        foreach(TurnPhaseTrigger trigger in turnPhaseTriggers) {
            removeTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(trigger));
        }
    }

    private IEnumerable DeclareIntent() {
        currentIntent = enemy.ChooseIntent(this);
        yield return null;
    }

    private IEnumerable EnactIntent() {
        yield return new WaitForSeconds(currentIntent.attackTime);
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Enemy;
        document.map.AddItems<CombatInstance>(currentIntent.targetsKey, currentIntent.targets);
        EffectManager.Instance.invokeEffectWorkflow(document, currentIntent.effectSteps, null);
        yield return null;
    }

    private IEnumerable ClearBlock() {
        combatInstance.statusEffects[StatusEffect.Defended] = 0;
        yield return null;
    }
    
    public void SetTauntedTarget(CombatInstance target){
        throw new NotImplementedException();
    }

    private IEnumerator OnDeath(CombatInstance killer) {
        Debug.Log("EnemyInstance OnDeath handler");
        UnregisterTurnPhaseTriggers();
        CombatEntityManager.Instance.EnemyDied(this);
        yield return null;
    }
}
