using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurgeCardOnEncounterEnd : CompanionAbility {

    private TurnPhaseTrigger turnPhaseTrigger;

    public PurgeCardOnEncounterEnd()
    {
        abilityName = "PurgeCardOnEncounterEnd";
    }
    
    public override void setupAbility(CompanionAbilityContext context)
    {
        turnPhaseTrigger = new TurnPhaseTrigger(TurnPhase.END_ENCOUNTER, invoke(context));
        context.invoker.registerTurnPhaseTrigger(turnPhaseTrigger);
    }

    public override IEnumerable invoke(CompanionAbilityContext context) {
        resetAbilityState(); // adding this here because of the hacks we're doing to stall on card selection
        context.invoker.requestTarget(new List<EntityType> { EntityType.Companion}, this);
        yield return new WaitUntil(() => currentTargets.Count > 0);
        CompanionInstance target = (CompanionInstance) currentTargets[0];
        context.invoker.raiseCardSelectionRequest(new CardSelectionRequestEventInfo(target.inCombatDeck.getAllCards(), CardEffect.Purge, CardEffect.Discard, 1, 1));
        currentTargets.Clear();
        // hack: invoker will give us a dummy target when the selection is complete
        yield return new WaitUntil(() => currentTargets.Count > 0);
        Debug.Log("purge card on encounter end complete");
        resetAbilityState();
    }

    public override void onDeath(CompanionAbilityContext context)
    {
        context.turnPhaseManager.removeTurnPhaseTrigger(turnPhaseTrigger);
    }

}