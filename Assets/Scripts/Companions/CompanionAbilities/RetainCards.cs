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
        turnPhaseTrigger = new TurnPhaseTrigger(TurnPhase.BEFORE_END_PLAYER_TURN, invoke(context));
        context.invoker.registerTurnPhaseTrigger(turnPhaseTrigger);
    }

    public override IEnumerable invoke(CompanionAbilityContext context) {
        PlayableCard lastCard = lastUnretainedCard(PlayerHand.Instance.cardsInHand);
        if(PlayerHand.Instance.cardsInHand.Count == 0)
        {
            Debug.Log("RetainCards found no cards in hand");
            yield return new WaitForEndOfFrame();
            resetAbilityState();
            yield break;
        }  else if (lastCard == null) {
            Debug.Log("RetainCards requested target");
            context.invoker.requestTarget(new List<EntityType> { EntityType.PlayableCard }, this);
        } else {
            currentTargets.Add(lastCard);
        }
        yield return new WaitUntil(() => currentTargets.Count > 0);
        PlayableCard retainedCard = (PlayableCard) currentTargets[0];
        retainedCard.retained = true;
        yield return new WaitForEndOfFrame();
        resetAbilityState();
    }

    private PlayableCard lastUnretainedCard(List<PlayableCard> cardsInHand)
    {
        PlayableCard lastUnretainedCard = null;
        int unretainedCount = 0;
        foreach (PlayableCard card in cardsInHand)
        {
            if (!card.retained)
            {
                unretainedCount += 1;
                lastUnretainedCard = card;
            } 
        }
        return unretainedCount == 1 ? lastUnretainedCard : null;
    }

    public override void onDeath(CompanionAbilityContext context)
    {
        context.turnPhaseManager.removeTurnPhaseTrigger(turnPhaseTrigger);
    }

}