using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CompanionAbility
{
    [SerializeField]
    private CompanionAbilityTrigger companionAbilityTrigger;
    [SerializeReference]
    public List<EffectStep> effectSteps;

    private CompanionInstance companionInstance;

    private List<TurnPhaseTrigger> turnPhaseTriggers = new List<TurnPhaseTrigger>();
    private List<CombatEntityTrigger> combatEntityTriggers = new List<CombatEntityTrigger>();

    public void Setup(CompanionInstance companionInstance) {
        this.companionInstance = companionInstance;
        this.companionInstance.combatInstance.onDeathHandler += OnDeath;
        registerTrigger();
    }

    private void registerTrigger() {
        switch (companionAbilityTrigger) {
            case CompanionAbilityTrigger.EnterTheBattlefield:
                setupForTurnPhaseTrigger(TurnPhase.START_ENCOUNTER);
            break;

            case CompanionAbilityTrigger.EndOfCombat:
                setupForTurnPhaseTrigger(TurnPhase.END_ENCOUNTER);
            break;

            case CompanionAbilityTrigger.EndOfPlayerTurn:
                setupForTurnPhaseTrigger(TurnPhase.BEFORE_END_PLAYER_TURN);
            break;

            case CompanionAbilityTrigger.EndOfEnemyTurn:
                setupForTurnPhaseTrigger(TurnPhase.END_ENEMY_TURN);
            break;

            case CompanionAbilityTrigger.OnFriendOrFoeDeath:
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

            case CompanionAbilityTrigger.OnDeath:
                this.companionInstance.SetCompanionAbilityDeathCallback(setupAndInvokeAbility());
            break;

            case CompanionAbilityTrigger.OnAttackCardPlayed:
                this.companionInstance.deckInstance.onCardCastHandler += CheckAttackCardPlayed;
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

        if (companionAbilityTrigger == CompanionAbilityTrigger.OnAttackCardPlayed) {
            this.companionInstance.deckInstance.onCardCastHandler -= CheckAttackCardPlayed;
        }
        
        yield return null;
    }

    private IEnumerable setupAndInvokeAbility() {
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this.companionInstance);
        document.originEntityType = EntityType.Companion;
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document,  effectSteps, null);
    }

    // This is a bit of a hack, but I'm ok with it being here for now
    private IEnumerator CheckAttackCardPlayed(PlayableCard card) {
        if (card.card.cardType.cardCategory == CardCategory.Attack) {
            yield return companionInstance.StartCoroutine(setupAndInvokeAbility().GetEnumerator());
        }
    }


    public enum CompanionAbilityTrigger {
        EnterTheBattlefield,
        EndOfCombat,
        EndOfPlayerTurn,
        EndOfEnemyTurn,
        OnFriendOrFoeDeath,
        OnDeath,
        OnAttackCardPlayed
    }
}