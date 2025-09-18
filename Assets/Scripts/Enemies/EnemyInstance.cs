using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(CombatInstance))]
[RequireComponent(typeof(Targetable))]
public class EnemyInstance : MonoBehaviour, IUIEntity {
    public Enemy enemy;
    public CombatInstance combatInstance;

    public EnemyIntent currentIntent;
    public TurnPhaseTriggerEvent registerTurnPhaseTriggerEvent;
    public TurnPhaseTriggerEvent removeTurnPhaseTriggerEvent;

    [SerializeField]
    private CombatEffectEvent combatEffectEvent;

    [HideInInspector]
    public List<TurnPhaseTrigger> turnPhaseTriggers = new List<TurnPhaseTrigger>();

    public WorldPositionVisualElement placement;
    public bool dead = false;
    public EnemyView enemyView;

    private Dictionary<EnemyBrain, ValueTuple<int, int>> behaviorIndices = new();

    public void Setup(WorldPositionVisualElement placement, Enemy enemy, float leftRightScreenPlacementPercent) {
        this.enemy = enemy;
        gameObject.name = enemy.enemyType.name;
        CombatEntityManager.Instance.registerEnemy(this);
        this.placement = placement;
        dead = false;
        combatInstance.Setup(enemy.combatStats, enemy, CombatInstance.CombatInstanceParent.ENEMY, placement, this.enemy.enemyType.cacheValueConfigs);
        Debug.Log("EnemyInstance Start for enemy " + enemy.id + " initialized with combat stats (health): " + combatInstance.combatStats.getCurrentHealth());
        combatInstance.SetId(enemy.id);

        // ---- set up abilities ----
        // We cannot perform "Setup" on the ability itself, because that is global on the
        // EnemyTypeSO.
        // If you have multiple copies of the same entity type on the team, they would
        // all try to write state to the same Ability class.
        foreach (EntityAbility ability in enemy.enemyType.abilities) {
            EnemyInstanceAbilityInstance abilityInstance = new(ability, this);
            abilityInstance.Setup();
        }

        combatInstance.onDeathHandler += OnDeath;
        foreach (InitialStatus status in enemy.enemyType.initialStatuses) {
            combatInstance.SetStatusEffect(status.status, status.scale);
        }
        RegisterTurnPhaseTriggers();
    }

    public EnemyIntent ChooseIntent(EnemyBrain brain) {
        Debug.Log("ChooseIntent");
        List<EnemyBehavior> behaviors = brain.behaviors;
        if(behaviors.Count == 0) {
            Debug.LogError("No behaviors defined for enemy");
            return null;
        }
        ValueTuple<int, int> idxs = behaviorIndices.GetValueOrDefault(brain, new ValueTuple<int, int>(0, 0));

        int behaviorIndex = idxs.Item1;
        int nextBehaviorIndex = idxs.Item2;
        switch (brain.behaviorType) {
            case EnemyBrain.EnemyBehaviorPattern.SequentialCycling:
                behaviorIndex = nextBehaviorIndex;
                nextBehaviorIndex = (nextBehaviorIndex + 1) % behaviors.Count;
                break;
            case EnemyBrain.EnemyBehaviorPattern.Random:
                behaviorIndex = UnityEngine.Random.Range(0, behaviors.Count);
                break;
            case EnemyBrain.EnemyBehaviorPattern.SequentialWithSinkAtLastElement:
                behaviorIndex = nextBehaviorIndex;
                // Advance until we reach the end of the defined behaviors.
                nextBehaviorIndex = Math.Min(nextBehaviorIndex + 1, behaviors.Count - 1);
                break;
        }
        EnemyBehavior action = behaviors[behaviorIndex];
        // Note: this only allows the enemies to target companions for now.
        // There is nothing that allows targeting other enemies, but this is not
        // an important behavior to support for now.
        CompanionInstance target = ChooseTargets(action.enemyTargetMethod);
        List<CompanionInstance> targetList = new();
        if (target != null) {
            targetList.Add(target);
        }

        behaviorIndices[brain] = new ValueTuple<int, int>(behaviorIndex, nextBehaviorIndex);

        return new EnemyIntent(
            this,
            // I'm aware this is bad, stick with me for a sec
            targetList,
            0.05f,
            action.intent,
            action.targetsKey,
            action.displayValue,
            action.effectSteps);
    }

    public int GetBehaviorIndexForBrain(EnemyBrain brain) {
        return behaviorIndices.GetValueOrDefault(brain, new (0, 0)).Item1;
    }

    private CompanionInstance ChooseTargets(EnemyTargetMethod targetMethod) {
        CompanionInstance target = null;
        List<CompanionInstance> possibleTargets = new List<CompanionInstance>();
        switch (targetMethod) {
            case EnemyTargetMethod.FirstCompanion:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(0);
            break;

            case EnemyTargetMethod.LastCompanion:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(-1);
            break;

            case EnemyTargetMethod.SecondFromFront:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(1);
            break;

            case EnemyTargetMethod.ThirdFromFront:
                target = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(2);
            break;

            case EnemyTargetMethod.RandomCompanion:
                CombatEntityManager.Instance.getCompanions()
                    .ForEach(companion => possibleTargets.Add(companion));
                target = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
            break;

            // case EnemyTargetMethod.RandomEnemyNotSelf:
            //     CombatEntityManager.Instance.getEnemies()
            //         .ForEach(enemy => {
            //             if (enemy != self) {
            //                 possibleTargets.Add(enemy.combatInstance);
            //             }
            //         });
            //     target = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
            // break;

            case EnemyTargetMethod.LowestHealth:
                List<CompanionInstance> companions = CombatEntityManager.Instance.getCompanions();
                target = companions[0];
                foreach (CompanionInstance instance in companions) {
                    if (instance.combatInstance.combatStats.getCurrentHealth() < target.combatInstance.combatStats.getCurrentHealth()) {
                        target = instance;
                    }
                }
            break;
        }
        return target;
    }

    private void RegisterTurnPhaseTriggers() {
        // Don't need this for now, portraits are covering
        turnPhaseTriggers.Add(new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, DeclareIntent()));
        turnPhaseTriggers.Add(new TurnPhaseTrigger(
            TurnPhase.START_ENEMY_TURN,
            combatInstance.UpdateStatusEffects(new List<StatusEffectType> {
                StatusEffectType.Burn
            })
        ));
        turnPhaseTriggers.Add(new TurnPhaseTrigger(TurnPhase.ENEMIES_TURN, EnactIntent()));
        turnPhaseTriggers.Add(new TurnPhaseTrigger(TurnPhase.END_PLAYER_TURN, ClearBlock()));
        turnPhaseTriggers.Add(new TurnPhaseTrigger(TurnPhase.END_ENEMY_TURN, ClearTemporaryStrength()));
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
    }

    private IEnumerable EnactIntent() {
        yield return new WaitForSeconds(currentIntent.attackTime);
        if(dead) yield break;
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Enemy;
        List<CombatInstance> combatInstanceTargets = currentIntent.targets.Select(x => x.combatInstance).ToList();
        List<DeckInstance> deckInstanceTargets = currentIntent.targets.Select(x => x.deckInstance).ToList();
        List<GameObject> gameObjectTargets = currentIntent.targets.Select(x => x.gameObject).ToList();
        List<VisualElement> visualElementTargets = currentIntent.targets.Select(x => x.combatInstance.GetVisualElement()).ToList();
        document.map.AddItems<CombatInstance>(currentIntent.targetsKey, combatInstanceTargets);
        document.map.AddItems<DeckInstance>(currentIntent.targetsKey, deckInstanceTargets);
        document.map.AddItems<GameObject>(currentIntent.targetsKey, gameObjectTargets);
        document.map.AddItems<VisualElement>(currentIntent.targetsKey, visualElementTargets);
        EffectManager.Instance.invokeEffectWorkflow(document, currentIntent.effectSteps, null);
        yield return null;
    }

    private IEnumerable ClearBlock() {
        combatInstance.SetStatusEffect(StatusEffectType.Defended, 0);
        yield return null;
    }

    private IEnumerable ClearTemporaryStrength() {
        combatInstance.SetStatusEffect(StatusEffectType.TemporaryStrength, 0);
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
        return enemy.GetName();
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

    public Targetable GetTargetable() {
        return GetComponent<Targetable>();
    }

    public Sprite GetBackgroundImage() {
        return enemy.enemyType.backgroundImage;
    }

    public Sprite GetEntityFrame() {
        return enemy.enemyType.entityFrame;
    }
}