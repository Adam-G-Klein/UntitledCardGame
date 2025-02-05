using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
public abstract class EntityAbilityInstance
{
    protected EntityAbility ability;

    private List<TurnPhaseTrigger> turnPhaseTriggers = new List<TurnPhaseTrigger>();
    private List<CombatEntityTrigger> combatEntityTriggers = new List<CombatEntityTrigger>();

    protected abstract CombatInstance getCombatInstance();

    // Abstract because different implementations of the subclass will create different versions of the base effect document.
    protected abstract EffectDocument createEffectDocument();

    private IEnumerable setupAndInvokeAbility() {
        EffectDocument document = createEffectDocument();
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document, ability.effectSteps, null);
    }

    public void Setup() {
        getCombatInstance().onDeathHandler += OnDeath;
        registerTrigger();
    }

    private void registerTrigger() {
        switch (ability.abilityTrigger) {
            case EntityAbility.EntityAbilityTrigger.EnterTheBattlefield:
                setupForTurnPhaseTrigger(TurnPhase.START_ENCOUNTER);
            break;

            case EntityAbility.EntityAbilityTrigger.EndOfCombat:
                setupForTurnPhaseTrigger(TurnPhase.END_ENCOUNTER);
            break;

            case EntityAbility.EntityAbilityTrigger.EndOfPlayerTurn:
                setupForTurnPhaseTrigger(TurnPhase.BEFORE_END_PLAYER_TURN);
            break;

            case EntityAbility.EntityAbilityTrigger.EndOfEnemyTurn:
                setupForTurnPhaseTrigger(TurnPhase.END_ENEMY_TURN);
            break;

            case EntityAbility.EntityAbilityTrigger.StartOfPlayerTurn:
                setupForTurnPhaseTrigger(TurnPhase.START_PLAYER_TURN);
            break;

            case EntityAbility.EntityAbilityTrigger.OnFriendOrFoeDeath:
                CombatEntityTrigger companionDeathTrigger = new CombatEntityTrigger(
                    CombatEntityTriggerType.COMPANION_DIED,
                    setupAndInvokeAbility());
                CombatEntityTrigger enemyDeathTrigger = new CombatEntityTrigger(
                    CombatEntityTriggerType.ENEMY_DIED,
                    setupAndInvokeAbility());
                CombatEntityTrigger minionDeathTrigger = new CombatEntityTrigger(
                    CombatEntityTriggerType.MINION_DIED,
                    setupAndInvokeAbility());
                combatEntityTriggers.Add(companionDeathTrigger);
                combatEntityTriggers.Add(enemyDeathTrigger);
                combatEntityTriggers.Add(minionDeathTrigger);
                CombatEntityManager.Instance.registerTrigger(companionDeathTrigger);
                CombatEntityManager.Instance.registerTrigger(enemyDeathTrigger);
                CombatEntityManager.Instance.registerTrigger(minionDeathTrigger);
            break;

            case EntityAbility.EntityAbilityTrigger.OnFriendDeath:
                CombatEntityTrigger onFriendDeathTrigger = new CombatEntityTrigger(
                    CombatEntityTriggerType.COMPANION_DIED,
                    setupAndInvokeAbility());
                combatEntityTriggers.Add(onFriendDeathTrigger);
                CombatEntityManager.Instance.registerTrigger(onFriendDeathTrigger);
            break;

            case EntityAbility.EntityAbilityTrigger.OnFoeDeath:
                CombatEntityTrigger onFoeDeathTrigger = new CombatEntityTrigger(
                    CombatEntityTriggerType.ENEMY_DIED,
                    setupAndInvokeAbility());
                combatEntityTriggers.Add(onFoeDeathTrigger);
                CombatEntityManager.Instance.registerTrigger(onFoeDeathTrigger);
            break;

            // Experiment: let's try handling the OnDeath trigger by directly doing it here in the callback.
            case EntityAbility.EntityAbilityTrigger.OnDeath:
            //     this.companionInstance.SetCompanionAbilityDeathCallback(setupAndInvokeAbility());
                break;

            case EntityAbility.EntityAbilityTrigger.OnCardCast:
                PlayerHand.Instance.onCardCastHandler += OnCardCast;
            break;
            case EntityAbility.EntityAbilityTrigger.OnCombine:
                // This is handled in the Companion class's constructor.
                // It's messy, but CompanionInstance and therefore this class just never exist
                // in the shop as of this writing
                break;
            case EntityAbility.EntityAbilityTrigger.OnCardExhausted:
                PlayerHand.Instance.onCardExhaustHandler += OnCardExhaust;
                break;
            case EntityAbility.EntityAbilityTrigger.OnDeckShuffled:
                PlayerHand.Instance.onDeckShuffledHandler += OnDeckShuffled;
                break;
            case EntityAbility.EntityAbilityTrigger.OnEntityDamageTaken:
                CombatEntityManager.Instance.onCompanionDamageHandler += OnDamageTaken;
                break;
            case EntityAbility.EntityAbilityTrigger.OnHandEmpty:
                PlayerHand.Instance.onHandEmptyHandler += OnHandEmpty;
                break;
        }
    }

    public IEnumerator OnDeath(CombatInstance killer) {
        // Run the on death trigger directly.
        if (ability.abilityTrigger == EntityAbility.EntityAbilityTrigger.OnDeath) {
            yield return setupAndInvokeAbility().GetEnumerator();
        }

        foreach (TurnPhaseTrigger trigger in turnPhaseTriggers) {
            TurnManager.Instance.removeTurnPhaseTrigger(trigger);
        }

        foreach (CombatEntityTrigger trigger in combatEntityTriggers) {
            CombatEntityManager.Instance.unregisterTrigger(trigger);
        }

        if (ability.abilityTrigger == EntityAbility.EntityAbilityTrigger.OnCardCast) {
            PlayerHand.Instance.onCardCastHandler -= OnCardCast;
        }
        if (ability.abilityTrigger == EntityAbility.EntityAbilityTrigger.OnHandEmpty) {
            PlayerHand.Instance.onHandEmptyHandler -= OnHandEmpty;
        }


        // This way of unsubscribing is giga sketchy, because PlayerHand is a generic singleton
        // and persists longer than the "Instance" game objects.
        // When should we unsubscribe so that we do not get memory leaks?
        if (ability.abilityTrigger == EntityAbility.EntityAbilityTrigger.OnCardExhausted) {
            PlayerHand.Instance.onCardExhaustHandler -= OnCardExhaust;
        }
        if (ability.abilityTrigger == EntityAbility.EntityAbilityTrigger.OnDeckShuffled) {
            PlayerHand.Instance.onDeckShuffledHandler -= OnDeckShuffled;
        }
        if (ability.abilityTrigger == EntityAbility.EntityAbilityTrigger.OnEntityDamageTaken) {
            CombatEntityManager.Instance.onCompanionDamageHandler -= OnDamageTaken;
        }
    }

    private void setupForTurnPhaseTrigger(TurnPhase turnPhase) {
        TurnPhaseTrigger newTrigger = new TurnPhaseTrigger(turnPhase, setupAndInvokeAbility());
        turnPhaseTriggers.Add(newTrigger);
        TurnManager.Instance.addTurnPhaseTrigger(newTrigger);
    }

    // This is a bit of a hack, but I'm ok with it being here for now
    private IEnumerator OnCardCast(PlayableCard card) {
        EffectDocument document = createEffectDocument();
        document.map.AddItem<PlayableCard>("cardPlayed", card);
        if (card.deckFrom.TryGetComponent(out CompanionInstance companion)) {
            EffectUtils.AddCompanionToDocument(document, "companionCardPlayedFrom", companion);
        }
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document, ability.effectSteps, null);
    }

    private IEnumerator OnHandEmpty() {
        Debug.Log("OnHandEmpty ability invoked!!!");
        EffectManager.Instance.QueueEffectWorkflow(
            new EffectWorkflowClosure(createEffectDocument(), ability.effectWorkflow, null)
        );
        yield return null;
    }

    private IEnumerator OnCardExhaust(DeckInstance deckFrom, Card card) {
        EffectDocument document = createEffectDocument();
        if (deckFrom.TryGetComponent(out CompanionInstance companion)) {
            EffectUtils.AddCompanionToDocument(document, "companionExhaustedFrom", companion);
        }
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document, ability.effectSteps, null);
    }

    private IEnumerator OnDeckShuffled(DeckInstance deckFrom) {
        EffectDocument document = createEffectDocument();
        if (deckFrom.TryGetComponent(out CompanionInstance companion)) {
            document.map.AddItem<CompanionInstance>("companionDeckFrom", companion);
            document.map.AddItem<CombatInstance>("companionDeckFrom", companion.combatInstance);
            document.map.AddItem<DeckInstance>("companionDeckFrom", companion.deckInstance);
        }
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document, ability.effectSteps, null);
    }

    private IEnumerator OnDamageTaken(CombatInstance damagedInstance) {
        EffectDocument document = createEffectDocument();
        if (damagedInstance.parentType == CombatInstance.CombatInstanceParent.COMPANION) {
            CompanionInstance companion = CombatEntityManager.Instance.getCompanionInstanceForCombatInstance(damagedInstance);
            if (companion != null) {
                EffectUtils.AddCompanionToDocument(document, "damagedCompanion", companion);
            }
            if (document.originEntityType == EntityType.CompanionInstance) {
                CompanionInstance source = document.map.GetItem<CompanionInstance>(EffectDocument.ORIGIN, 0);
                document.boolMap.Add("selfDamaged", source == companion);
            }
        } else if (damagedInstance.parentType == CombatInstance.CombatInstanceParent.ENEMY) {
            EnemyInstance enemy = CombatEntityManager.Instance.getEnemyInstanceForCombatInstance(damagedInstance);
            if (enemy != null) {
                EffectUtils.AddEnemyToDocument(document, "damagedEnemy", enemy);
            }
        }
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document, ability.effectSteps, null);
    }
}

public class CompanionInstanceAbilityInstance : EntityAbilityInstance
{
    private CompanionInstance companionInstance;

    public CompanionInstanceAbilityInstance(EntityAbility ability, CompanionInstance companionInstance) {
        this.ability = ability;
        this.companionInstance = companionInstance;
    }

    protected override CombatInstance getCombatInstance() { return companionInstance.combatInstance; }

    protected override EffectDocument createEffectDocument()
    {
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this.companionInstance);
        document.originEntityType = EntityType.CompanionInstance;
        return document;
    }
}

public class EnemyInstanceAbilityInstance : EntityAbilityInstance
{
    private EnemyInstance enemyInstance;

    public EnemyInstanceAbilityInstance(EntityAbility ability, EnemyInstance enemyInstance) {
        this.ability = ability;
        this.enemyInstance = enemyInstance;
    }

    protected override CombatInstance getCombatInstance() { return enemyInstance.combatInstance; }

    protected override EffectDocument createEffectDocument()
    {
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this.enemyInstance);
        document.originEntityType = EntityType.Enemy;
        return document;
    }
}