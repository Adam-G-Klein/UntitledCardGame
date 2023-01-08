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
    }
    
    public override void setupAbility(CompanionAbilityContext context)
    {
        context.invoker.turnPhaseEventAbilities.Add(TurnPhase.BEFORE_END_PLAYER_TURN, this);
        turnPhaseTrigger = new TurnPhaseTrigger(TurnPhase.BEFORE_END_PLAYER_TURN, invoke(context));
        context.turnPhaseManager.addTurnPhaseTrigger(turnPhaseTrigger);
    }

    public override IEnumerable invoke(CompanionAbilityContext context) {
        if(context.playerHand.cardsInHand.Count == 0)
        {
            Debug.Log("RetainCards found no cards in hand");
            yield return new WaitForEndOfFrame();
            resetAbilityState();
            yield break;
        }  else {
            Debug.Log("RetainCards requested target");
            context.invoker.requestTarget(new List<EntityType> { EntityType.Card }, this);
        }
        yield return new WaitUntil(() => currentTargets.Count > 0);
        PlayableCard retainedCard = (PlayableCard) currentTargets[0];
        retainedCard.retained = true;
        yield return new WaitForEndOfFrame();
        resetAbilityState();
    }

}