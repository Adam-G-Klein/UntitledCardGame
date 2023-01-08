using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetainCards : CompanionAbility {

    private TurnPhaseTrigger turnPhaseTrigger;

    private IEnumerator retainCards;

    private CompanionAbilityContext context;

    public RetainCards()
    {
        abilityName = "RetainCards";
        turnPhaseTrigger = new TurnPhaseTrigger(TurnPhase.BEFORE_END_PLAYER_TURN);
        turnPhaseTrigger.isFinished = false;
    }
    
    public override void setupAbility(CompanionAbilityContext context)
    {
        context.invoker.turnPhaseEventAbilities.Add(TurnPhase.BEFORE_END_PLAYER_TURN, this);
        context.turnPhaseManager.addTurnPhaseTrigger(turnPhaseTrigger);
    }

    public override IEnumerator invoke(CompanionAbilityContext context) {
        if(context.playerHand.cardsInHand.Count == 0)
        {
            turnPhaseTrigger.isFinished = true;
            yield return new WaitForEndOfFrame();
            resetAbilityState();
            yield break;
        } else if (context.playerHand.cardsInHand.Count == 1)
        {
            currentAbilityTargets.Add(context.playerHand.cardsInHand[0]);
        } else {
            context.invoker.requestTarget(new List<EntityType> { EntityType.Card }, this);
        }
        yield return new WaitUntil(() => currentAbilityTargets.Count > 0);
        PlayableCard retainedCard = (PlayableCard) currentAbilityTargets[0];
        retainedCard.retained = true;
        turnPhaseTrigger.isFinished = true;
        yield return new WaitForEndOfFrame();
        resetAbilityState();
    }

    public override void resetAbilityState()
    {
        base.resetAbilityState();
        turnPhaseTrigger.isFinished = false;
    }
}