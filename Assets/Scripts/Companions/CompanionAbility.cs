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

    public void Setup(CompanionInstance companionInstance) {
        this.companionInstance = companionInstance;
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

            case CompanionAbilityTrigger.OnFriendOrFoeDeath:
                CombatEntityManager.Instance.registerTrigger(
                    CombatEntityTrigger.COMPANION_DIED, setupAndInvokeAbility());
                CombatEntityManager.Instance.registerTrigger(
                    CombatEntityTrigger.ENEMY_DIED, setupAndInvokeAbility());  
                CombatEntityManager.Instance.registerTrigger(
                    CombatEntityTrigger.MINION_DIED, setupAndInvokeAbility());
            break;

            case CompanionAbilityTrigger.OnDeath:
                this.companionInstance.setOnDeath(setupAndInvokeAbility());
            break;
        }
    }

    private void setupForTurnPhaseTrigger(TurnPhase turnPhase) {
        TurnManager.Instance.addTurnPhaseTrigger(
            new TurnPhaseTrigger(
                turnPhase, 
                setupAndInvokeAbility()));
    }

    private IEnumerable setupAndInvokeAbility() {
        EffectDocument document = new EffectDocument();
        document.companionMap.addItem(EffectDocument.ORIGIN, this.companionInstance);
        document.originEntityType = EntityType.Companion;
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document,  effectSteps, () => {});
    }

    public enum CompanionAbilityTrigger {
        EnterTheBattlefield,
        EndOfCombat,
        EndOfPlayerTurn,
        OnFriendOrFoeDeath,
        OnDeath
    }
}