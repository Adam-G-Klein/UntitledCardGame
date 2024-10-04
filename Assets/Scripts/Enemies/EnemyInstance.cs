using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyPillarUIController))]
[RequireComponent(typeof(CombatInstance))]
[RequireComponent(typeof(Targetable))]
public class EnemyInstance : MonoBehaviour {
    public Enemy enemy;
    public CombatInstance combatInstance;
    [Header("Image or SpriteRenderer required in children")]
    public Image spriteImage;
    public SpriteRenderer spriteRenderer;

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

    public void Setup(WorldPositionVisualElement placement) {
        CombatEntityManager.Instance.registerEnemy(this);
        this.intentDisplay = GetComponentInChildren<EnemyIntentDisplay>();
        this.enemyPillarUIController = GetComponent<EnemyPillarUIController>();
        this.placement = placement;
        enemyPillarUIController.Setup(this);
        this.spriteImage = GetComponentInChildren<Image>();
        this.spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if(spriteImage) {
            spriteImage.sprite = enemy.enemyType.sprite;
        } else if (spriteRenderer) {
            spriteRenderer.sprite = enemy.enemyType.sprite;
        }
        combatInstance.parentType = CombatInstance.CombatInstanceParent.ENEMY;
        combatInstance.combatStats = enemy.combatStats;
        // Reset the behavior indices on the EnemyBrain to zero.
        enemy.enemyType.enemyPattern.nextBehaviorIndex = 0;
        enemy.enemyType.belowHalfHPEnemyPattern.nextBehaviorIndex = 0;
        Debug.Log("EnemyInstance Start for enemy " + enemy.id + " initialized with combat stats (health): " + combatInstance.combatStats.getCurrentHealth());
        combatInstance.SetId(enemy.id);
        combatInstance.onDeathHandler += OnDeath;
        foreach (InitialStatus status in enemy.enemyType.initialStatuses) {
            combatInstance.statusEffects[status.status] = status.scale;
        }
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
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Enemy;
        List<CombatInstance> combatInstanceTargets = currentIntent.targets.Select(x => x.combatInstance).ToList();
        List<DeckInstance> deckInstanceTargets = currentIntent.targets.Select(x => x.deckInstance).ToList();
        document.map.AddItems<CombatInstance>(currentIntent.targetsKey, combatInstanceTargets);
        document.map.AddItems<DeckInstance>(currentIntent.targetsKey, deckInstanceTargets);
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
