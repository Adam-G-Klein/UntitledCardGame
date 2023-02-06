using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawnETB : CompanionAbility {

    private TurnPhaseTrigger turnPhaseTrigger;
    public int numMinions = 1;
    public MinionTypeSO minionType;

    public MinionSpawnETB()
    {
        abilityName = "GrotesqueETB";
    }
    
    public override void setupAbility(CompanionAbilityContext context)
    {
        turnPhaseTrigger = new TurnPhaseTrigger(TurnPhase.START_ENCOUNTER, invoke(context));
        context.turnPhaseManager.addTurnPhaseTrigger(turnPhaseTrigger);
    }

    public override IEnumerable invoke(CompanionAbilityContext context) {
        for(int i = 0; i < numMinions; i++) {
            PrefabInstantiator.instantiateMinion(
                context.companionManager.encounterConstants.minionPrefab,
                new Minion(minionType),
                // Temp
                context.companionInstance.getNextMinionSpawnPosition()
            );
        }
        yield return null;
    }
    
    public override void onDeath(CompanionAbilityContext context)
    {
        context.turnPhaseManager.removeTurnPhaseTrigger(turnPhaseTrigger);
    }

}