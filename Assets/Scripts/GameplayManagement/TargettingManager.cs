using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardListEventListener))]
public class TargettingManager : GenericSingleton<TargettingManager>
{
    [SerializeField]
    private GameObject cardSelectionUIPrefab;
    [SerializeField]
    private TargettingArrowsController targettingArrows;

    // ========== Normal Targetting ========== //
    private List<TargettableEntity> targetList;
    private Entity origin;
    private List<EntityType> validTargets;
    private int numberOfTargets;
    private List<TargettableEntity> disallowedTargets;
    private List<TargettableEntity> specificTargetOptions;
    private bool lookingForTarget;

    // ========== Card Selecting ========== //
    private bool lookingForCardSelections;
    private List<Card> cardSelectionsList;

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
            // Make targetting not break if there are no valid targets but we accidentally
            // force targetting. Revisit this later
            if (targetListToAddTo.Count == 0) {
                targetListToAddTo.Add(null);
            }
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
        targettingArrows.createTargettingArrow(validTargets, origin);
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
        targettingArrows.freezeArrow(target);

        if (numberOfTargets > targetList.Count) {
            targettingArrows.createTargettingArrow(validTargets, origin);
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

    public void selectCards(
            List<Card> options,
            string promptText,
            int cardsToSelect,
            List<Card> output) {
        GameObject gameObject = GameObject.Instantiate(
            cardSelectionUIPrefab,
            Vector3.zero,
            Quaternion.identity);
        CardViewUI cardViewUI = gameObject.GetComponent<CardViewUI>();
        cardViewUI.Setup(options, cardsToSelect, promptText);
        cardSelectionsList = output;
        lookingForCardSelections = true;
    }

    public void cardsSelectedEventHandler(CardListEventInfo eventInfo) {
        if (!lookingForCardSelections) {
            Debug.LogError("TargettingManager: Cards selected event raised but" +
            " not looking for card selections!");
            return;
        }
        cardSelectionsList.AddRange(eventInfo.cards);
        lookingForCardSelections = false;
        cardSelectionsList = null;
    }
}
