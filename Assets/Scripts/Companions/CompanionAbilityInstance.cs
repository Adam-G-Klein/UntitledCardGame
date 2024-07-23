using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CompanionAbilityInstance
{
    private CompanionAbility ability;
    private CompanionInstance companionInstance;

    private List<TurnPhaseTrigger> turnPhaseTriggers = new List<TurnPhaseTrigger>();
    private List<CombatEntityTrigger> combatEntityTriggers = new List<CombatEntityTrigger>();

    public CompanionAbilityInstance(CompanionAbility ability, CompanionInstance companionInstance) {
        this.ability = ability;
        this.companionInstance = companionInstance;
    }

    public void Setup() {
        companionInstance.combatInstance.onDeathHandler += OnDeath;
        registerTrigger();
    }

    private void registerTrigger() {
        switch (ability.companionAbilityTrigger) {
            case CompanionAbility.CompanionAbilityTrigger.EnterTheBattlefield:
                setupForTurnPhaseTrigger(TurnPhase.START_ENCOUNTER);
            break;

            case CompanionAbility.CompanionAbilityTrigger.EndOfCombat:
                setupForTurnPhaseTrigger(TurnPhase.END_ENCOUNTER);
            break;

            case CompanionAbility.CompanionAbilityTrigger.EndOfPlayerTurn:
                setupForTurnPhaseTrigger(TurnPhase.BEFORE_END_PLAYER_TURN);
            break;

            case CompanionAbility.CompanionAbilityTrigger.EndOfEnemyTurn:
                setupForTurnPhaseTrigger(TurnPhase.END_ENEMY_TURN);
            break;

            case CompanionAbility.CompanionAbilityTrigger.OnFriendOrFoeDeath:
                CombatEntityTrigger companionDeathTrigger = new CombatEntityTrigger(
                    CombatEntityTriggerType.COMPANION_DIED,
                    setupAndInvokeAbility());
                CombatEntityTrigger enemyDeathTrigger = new CombatEntityTrigger(
                    CombatEntityTriggerType.ENEMY_DIED,
                    setupAndInvokeAbility());
                CombatEntityTrigger minionDeathTrigger = new CombatEntityTrigger(
                    CombatEntityTriggerType.MINION_DIED,
                    setupAndInvokeAbility());
                CombatEntityManager.Instance.registerTrigger(companionDeathTrigger);
                CombatEntityManager.Instance.registerTrigger(enemyDeathTrigger);
                CombatEntityManager.Instance.registerTrigger(minionDeathTrigger);
            break;

            case CompanionAbility.CompanionAbilityTrigger.OnFriendDeath:
                CombatEntityManager.Instance.registerTrigger(new CombatEntityTrigger(
                    CombatEntityTriggerType.COMPANION_DIED,
                    setupAndInvokeAbility()));
            break;

            case CompanionAbility.CompanionAbilityTrigger.OnDeath:
                this.companionInstance.SetCompanionAbilityDeathCallback(setupAndInvokeAbility());
            break;

            case CompanionAbility.CompanionAbilityTrigger.OnCardCast:
                PlayerHand.Instance.onCardCastHandler += OnCardCast;
            break;
            case CompanionAbility.CompanionAbilityTrigger.OnCombine:
                // This is handled in the Companion class's constructor.
                // It's messy, but CompanionInstance and therefore this class just never exist
                // in the shop as of this writing
                break;
            case CompanionAbility.CompanionAbilityTrigger.OnCardExhausted:
                PlayerHand.Instance.onCardExhaustHandler += OnCardExhaust;
                break;
            case CompanionAbility.CompanionAbilityTrigger.OnDeckShuffled:
                PlayerHand.Instance.onDeckShuffledHandler += OnDeckShuffled;
                break;
            case CompanionAbility.CompanionAbilityTrigger.OnFriendDamageTaken:
                CombatEntityManager.Instance.onCompanionDamageHandler += OnDamageTaken;
                break;
        }
    }

    private void setupForTurnPhaseTrigger(TurnPhase turnPhase) {
        TurnPhaseTrigger newTrigger = new TurnPhaseTrigger(turnPhase, setupAndInvokeAbility());
        turnPhaseTriggers.Add(newTrigger);
        TurnManager.Instance.addTurnPhaseTrigger(newTrigger);
    }

    private IEnumerator OnDeath(CombatInstance killer) {
        foreach (TurnPhaseTrigger trigger in turnPhaseTriggers) {
            TurnManager.Instance.removeTurnPhaseTrigger(trigger);
        }

        foreach (CombatEntityTrigger trigger in combatEntityTriggers) {
            CombatEntityManager.Instance.unregisterTrigger(trigger);
        }

        if (ability.companionAbilityTrigger == CompanionAbility.CompanionAbilityTrigger.OnCardCast) {
            PlayerHand.Instance.onCardCastHandler -= OnCardCast;
        }

        // This way of unsubscribing is giga sketchy, because PlayerHand is a generic singleton
        // and persists longer than the "Instance" game objects.
        // When should we unsubscribe so that we do not get memory leaks?
        if (ability.companionAbilityTrigger == CompanionAbility.CompanionAbilityTrigger.OnCardExhausted) {
            PlayerHand.Instance.onCardExhaustHandler -= OnCardExhaust;
        }
        if (ability.companionAbilityTrigger == CompanionAbility.CompanionAbilityTrigger.OnDeckShuffled) {
            PlayerHand.Instance.onDeckShuffledHandler -= OnDeckShuffled;
        }
        if (ability.companionAbilityTrigger == CompanionAbility.CompanionAbilityTrigger.OnFriendDamageTaken) {
            CombatEntityManager.Instance.onCompanionDamageHandler -= OnDamageTaken;
        }

        yield return null;
    }

    private IEnumerable setupAndInvokeAbility() {
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this.companionInstance);
        document.originEntityType = EntityType.CompanionInstance;
        Debug.Log("Ability has " + ability.effectSteps.Count.ToString() + " steps");
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document, ability.effectSteps, null);
    }

    // This is a bit of a hack, but I'm ok with it being here for now
    private IEnumerator OnCardCast(PlayableCard card) {
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this.companionInstance);
        document.originEntityType = EntityType.CompanionInstance;
        document.map.AddItem<PlayableCard>("cardPlayed", card);
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document, ability.effectSteps, null);
    }

    private IEnumerator OnCardExhaust(DeckInstance deckFrom, Card card) {
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this.companionInstance);
        document.originEntityType = EntityType.CompanionInstance;
        if (deckFrom.TryGetComponent(out CompanionInstance companion)) {
            document.map.AddItem<CompanionInstance>("companionExhaustedFrom", companion);
            document.map.AddItem<CombatInstance>("companionExhaustedFrom", companion.combatInstance);
            document.map.AddItem<DeckInstance>("companionExhaustedFrom", companion.deckInstance);
        }
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document, ability.effectSteps, null);
    }

    private IEnumerator OnDeckShuffled(DeckInstance deckFrom) {
        Debug.Log("Activating deck shuffled ability for companion " + this.companionInstance.companion.companionType.name);
        yield return setupAndInvokeAbility().GetEnumerator();
    }

    private IEnumerator OnDamageTaken(CombatInstance damagedInstance) {
        Debug.Log("Activating on damage taken ability for companion " + this.companionInstance.companion.companionType.name);
        yield return setupAndInvokeAbility().GetEnumerator();
    }
}
