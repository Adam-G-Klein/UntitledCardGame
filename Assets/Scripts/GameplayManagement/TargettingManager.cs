using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargettingManager : GenericSingleton<TargettingManager>
{
    [SerializeField]
    private CardEffectEvent cardEffectEvent;
    [SerializeField]
    private CardSelectionRequestEvent cardSelectionRequestEvent;

    private List<TargettableEntity> targetList;
    private Entity origin;
    private List<EntityType> validTargets;
    private int numberOfTargets;
    private List<TargettableEntity> disallowedTargets;
    private List<TargettableEntity> specificTargetOptions;
    private bool lookingForTarget;

    // old function
    public void requestTargets(
            TargetRequester requester,
            Entity source,
            List<EntityType> validTargets, 
            List<TargettableEntity> disallowedTargets = null) {
        UIStateManager.Instance.setState(UIState.EFFECT_TARGETTING);
        lookingForTarget = true;
        this.validTargets = validTargets;
        // this.requester = requester;
        this.disallowedTargets = disallowedTargets;
        TargettingArrowsController.Instance.createTargettingArrow(validTargets, source);
    }

    public void raiseCardEffect(CardEffectEventInfo info) {
        StartCoroutine(cardEffectEvent.RaiseAtEndOfFrameCoroutine(info));
    }

    public void raiseCardSelectionRequest(CardSelectionRequestEventInfo info) {
        StartCoroutine(cardSelectionRequestEvent.RaiseAtEndOfFrameCoroutine(info));
    }

    // ===================================================================== //

    public void requestTargets(
            List<TargettableEntity> targetListToAddTo,
            Entity origin,
            List<EntityType> validTargets,
            bool targetAllValidTargets,
            int numberOfTargets,
            List<TargettableEntity> disallowedTargets = null,
            List<TargettableEntity> specificTargetOptions = null) {
        if (targetAllValidTargets) {
            targetListToAddTo.AddRange(getAllValidTargets(validTargets, disallowedTargets));
            return;
        }

        UIStateManager.Instance.setState(UIState.EFFECT_TARGETTING);
        lookingForTarget = true;
        this.targetList = targetListToAddTo;
        this.origin = origin;
        this.validTargets = validTargets;
        this.numberOfTargets = numberOfTargets;
        this.disallowedTargets = disallowedTargets;
        this.specificTargetOptions = specificTargetOptions;
        TargettingArrowsController.Instance.createTargettingArrow(validTargets, origin);
    }

    public List<TargettableEntity> getAllValidTargets(
            List<EntityType> validTargets,
            List<TargettableEntity> disallowedTargets = null) {
        List<TargettableEntity> returnList = new List<TargettableEntity>();
        if (validTargets.Contains(EntityType.Companion)) {
            returnList.AddRange(CombatEntityManager.Instance.getCompanions());
        }
        if (validTargets.Contains(EntityType.Enemy)) {
            returnList.AddRange(CombatEntityManager.Instance.getEnemies());
        }
        if (validTargets.Contains(EntityType.Minion)) {
            returnList.AddRange(CombatEntityManager.Instance.getMinions());
        }
        if (validTargets.Contains(EntityType.PlayableCard)) {
            returnList.AddRange(PlayerHand.Instance.cardsInHand);
        }
        
        if (disallowedTargets != null) {
            foreach (TargettableEntity entity in disallowedTargets) {
                returnList.Remove(entity);
            }
        }

        return returnList;
    }

    public void attemptToTarget(TargettableEntity target) {
        // Not actively looking for a target, so do nothing
        if (!lookingForTarget) return;

        // Check if the provided target is disallowed
        if (disallowedTargets != null && disallowedTargets.Contains(target)) 
            return;

        // Check if the entity type is correct
        if (!validTargets.Contains(target.entityType)) return;

        // Check if we only want to pick from a specific list of targets
        if (specificTargetOptions != null && !specificTargetOptions.Contains(target)) {
            return;
        }

        // Passed above checks, so we're good to set the target
        Debug.Log(target);
        Debug.Log(targetList);
        targetList.Add(target);
        TargettingArrowsController.Instance.freezeArrow(target);

        if (numberOfTargets > targetList.Count) {
            TargettingArrowsController.Instance.createTargettingArrow(validTargets, origin);
        } else {
            // Reset targetting state
            lookingForTarget = false;
            validTargets = null;
            numberOfTargets = -1;
            disallowedTargets = null;
            specificTargetOptions = null;
            targetList = null;
            UIStateManager.Instance.setState(UIState.DEFAULT);
        }
    }
}
