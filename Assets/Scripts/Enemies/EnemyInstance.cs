using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyPillarUIController))]
[RequireComponent(typeof(CombatInstance))]
[RequireComponent(typeof(Targetable))]
public class EnemyInstance : MonoBehaviour, IUIEntity {
    public Enemy enemy;
    public CombatInstance combatInstance;

    public EnemyIntent currentIntent;
    public TurnPhaseTriggerEvent registerTurnPhaseTriggerEvent;
    public TurnPhaseTriggerEvent removeTurnPhaseTriggerEvent;

    // reference for resetting intent if the enemy is taunted
    private EnemyIntentDisplay intentDisplay;
    [SerializeField]
    private CombatEffectEvent combatEffectEvent;

    private EnemyPillarUIController enemyPillarUIController;
    [HideInInspector]
    public List<TurnPhaseTrigger> turnPhaseTriggers = new List<TurnPhaseTrigger>();

    public WorldPositionVisualElement placement;
    public bool dead = false;

    public void Setup(WorldPositionVisualElement placement, Enemy enemy) {
        this.enemy = enemy;
        gameObject.name = enemy.enemyType.name;
        CombatEntityManager.Instance.registerEnemy(this);
        this.intentDisplay = GetComponentInChildren<EnemyIntentDisplay>();
        this.enemyPillarUIController = GetComponent<EnemyPillarUIController>();
        this.placement = placement;
        dead = false;
        enemyPillarUIController.Setup(this, placement);
        combatInstance.Setup(enemy.combatStats, enemy, CombatInstance.CombatInstanceParent.ENEMY, placement);
        // Reset the behavior indices on the EnemyBrain to zero.
        enemy.enemyType.enemyPattern.nextBehaviorIndex = 0;
        enemy.enemyType.belowHalfHPEnemyPattern.nextBehaviorIndex = 0;
        Debug.Log("EnemyInstance Start for enemy " + enemy.id + " initialized with combat stats (health): " + combatInstance.combatStats.getCurrentHealth());
        combatInstance.SetId(enemy.id);
        combatInstance.onDeathHandler += OnDeath;
        foreach (InitialStatus status in enemy.enemyType.initialStatuses) {
            combatInstance.SetStatusEffect(status.status, status.scale);
        }
        GetComponentInChildren<CombatInstanceDisplayWorldspace>().Setup(combatInstance, placement);
        RegisterTurnPhaseTriggers();
    }


    private void RegisterTurnPhaseTriggers() {
        // Don't need this for now, portraits are covering
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
        Debug.Log("EnemyInstance: UpdateView");
        EnemyEncounterViewModel.Instance.SetStateDirty();
        yield return null;
        // yield return intentDisplay.displayIntent(this);
    }

    private IEnumerable EnactIntent() {
        yield return new WaitForSeconds(currentIntent.attackTime);
        if(dead) yield break;
        intentDisplay.clearIntent();
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Enemy;
        List<CombatInstance> combatInstanceTargets = currentIntent.targets.Select(x => x.combatInstance).ToList();
        List<DeckInstance> deckInstanceTargets = currentIntent.targets.Select(x => x.deckInstance).ToList();
        List<GameObject> gameObjectTargets = currentIntent.targets.Select(x => x.gameObject).ToList();
        document.map.AddItems<CombatInstance>(currentIntent.targetsKey, combatInstanceTargets);
        document.map.AddItems<DeckInstance>(currentIntent.targetsKey, deckInstanceTargets);
        document.map.AddItems<GameObject>(currentIntent.targetsKey, gameObjectTargets);
        yield return StartCoroutine(EffectManager.Instance.invokeEffectWorkflowCoroutine(document, currentIntent.effectSteps, null));
        yield return null;
    }

    private IEnumerable ClearBlock() {
        combatInstance.SetStatusEffect(StatusEffectType.Defended, 0);
        yield return null;
    }

    public void SetTauntedTarget(CombatInstance target){
        throw new NotImplementedException();
    }

    private IEnumerator OnDeath(CombatInstance killer) {
        Debug.Log("EnemyInstance OnDeath handler");
        UnregisterTurnPhaseTriggers();
        dead = true;
        CombatEntityManager.Instance.EnemyDied(this);
        yield return null;
    }

    public string GetName() {
        return enemy.enemyType.name;
    }

    public int GetCurrentHealth() {
        return combatInstance.combatStats.getCurrentHealth();
    }

    public string GetDescription() {
        if(currentIntent == null) {
            return "";
        }
        return currentIntent.intentType.ToString();
    }

    public CombatStats GetCombatStats() {
        return combatInstance.combatStats;
    }

    public CombatInstance GetCombatInstance() {
        return combatInstance;
    }

    public EnemyInstance GetEnemyInstance() {
        return this;
    }

    public DeckInstance GetDeckInstance() {
        return null;
    }
}
