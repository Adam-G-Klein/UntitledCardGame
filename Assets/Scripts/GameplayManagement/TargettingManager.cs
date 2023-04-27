using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargettingManager : GenericSingleton<TargettingManager>
{
    [SerializeField]
    private CardEffectEvent cardEffectEvent;
    [SerializeField]
    private CardSelectionRequestEvent cardSelectionRequestEvent;
    
    private TargetRequester currentTargetRequester;
    private bool lookingForTarget;
    private List<EntityType> validTargets;
    private TargetRequester requester;
    private List<TargettableEntity> disallowedTargets;

    public void requestTargets(
            TargetRequester requester,
            Entity source,
            List<EntityType> validTargets, 
            List<TargettableEntity> disallowedTargets = null) {
        UIStateManager.Instance.setState(UIState.EFFECT_TARGETTING);
        lookingForTarget = true;
        this.validTargets = validTargets;
        this.requester = requester;
        this.disallowedTargets = disallowedTargets;
        TargettingArrowsController.Instance.createTargettingArrow(validTargets, source);
    }

    public void attemptToTarget(TargettableEntity target) {
        // Not actively looking for a target, so do nothing
        if (!lookingForTarget) return;

        // Check if the provided target is disallowed
        if (disallowedTargets != null && disallowedTargets.Contains(target)) 
            return;

        // Check if the entity type is correct
        if (!validTargets.Contains(target.entityType)) return;

        // Passed above checks, so we're good to set the target
        requester.targetsSupplied(new List<TargettableEntity>() { target });

        // Reset targetting state
        lookingForTarget = false;
        validTargets = null;
        requester = null;
        disallowedTargets = null;
    }

    public List<TargettableEntity> getAllValidTargets(List<EntityType> validTargets) {
        List<TargettableEntity> returnList = new List<TargettableEntity>();
        if(validTargets.Contains(EntityType.Companion)){
            returnList.AddRange(CombatEntityManager.Instance.getCompanions());
        }
        if(validTargets.Contains(EntityType.Enemy)){
            returnList.AddRange(CombatEntityManager.Instance.getEnemies());
        }
        if(validTargets.Contains(EntityType.Minion)){
            returnList.AddRange(CombatEntityManager.Instance.getEnemies());
        }
        if(validTargets.Contains(EntityType.PlayableCard)){
            returnList.AddRange(PlayerHand.Instance.cardsInHand);
        }
        return returnList;
    }

    public void raiseCardEffect(CardEffectEventInfo info){
        StartCoroutine(cardEffectEvent.RaiseAtEndOfFrameCoroutine(info));
    }

    public void raiseCardSelectionRequest(CardSelectionRequestEventInfo info){
        StartCoroutine(cardSelectionRequestEvent.RaiseAtEndOfFrameCoroutine(info));
    }
}
