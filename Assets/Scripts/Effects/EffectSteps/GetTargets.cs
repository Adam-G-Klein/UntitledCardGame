using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    The effect that is used to target anything.

    Input: (Optional) A specific list of targets to choose from
    Output: The targets acquired based on the parameters
    Parameters:
        - UseInputToLimitOptions: If checked, will limit the targetting system
            to only allow the player to target what was taken from a previous
            step, saved to the key designated by the InputKey field
        - ValidTargets: What type of entities can be targetted, 
            and will hence be stored under the output key
        - Number: If no special targetting rules, how many targets are required
        - SpecialTargetRule: An enum that influences targetting
        - CantCancelTargetting: Currently not implemented, but will eventually 
            prevent the player from cancelling this targetting effect if checked

    SpecialTargetRules:
        - TargetAllValidTargets: Get all targets of the given valid 
            targets that exist
        - TargetAllValidTargetsExceptSelf: Same as TargetAllValidTargets, 
            but excludes the originator of the effect (card or companion)
        - TargetSelf: Target saved is the originator of the effect
        - TargetEntityThatDeltCard: Only used for effects on cards, target
            that gets saved is whatever entity delt the card
        - CantTargetSelf: Prevents targetting from choosing originator of effect
*/

[System.Serializable]
public class GetTargets : EffectStep
{
    [SerializeField]
    private bool useInputToLimitOptions = false;
    [SerializeField]
    private string inputKey = "";
    [Tooltip(
        "Save target(s) under given key to be used by a later effect"
    )]
    [SerializeField]
    private string outputKey = "";
    [SerializeField]
    private List<EntityType> validTargets;
    [Tooltip(
        "If either of the TargetAllValidTargets rules are set, number is ignored"
    )]
    [SerializeField]
    private int number = 1;
    [SerializeField]
    private SpecialTargetRule specialTargetRule = SpecialTargetRule.None;
    [SerializeField]
    private bool cantCancelTargetting = false;

    public GetTargets() {
        effectStepName = "GetTargets";
    }

    public override IEnumerator invoke(EffectDocument document) {
        // Two specific special target rule handling
        if (specialTargetRule == SpecialTargetRule.TargetSelf) {
            addSelfToDocument(document);
            yield break;
        } else if (specialTargetRule == SpecialTargetRule.TargetEntityThatDeltCard) {
            addOriginCardEntityFromToDocument(document);
            yield break;
        }

        List<TargettableEntity> targetList = new List<TargettableEntity>();
        TargettableEntity self = getSelf(document);
        List<TargettableEntity> disallowedTargets = null;
        bool getAllValidTargets = false;
        if (specialTargetRule == SpecialTargetRule.TargetAllValidTargetsExceptSelf ||
                specialTargetRule == SpecialTargetRule.CantTargetSelf) {
            disallowedTargets = new List<TargettableEntity>() { self };
        }

        List<TargettableEntity> limitOptions = null;
        if (useInputToLimitOptions) {
            limitOptions = document.getTargettableEntities(inputKey);
        }

        if (specialTargetRule == SpecialTargetRule.TargetAllValidTargets || 
                specialTargetRule == SpecialTargetRule.TargetAllValidTargetsExceptSelf) {
            getAllValidTargets = true;
        }

        TargettingManager.Instance.requestTargets(
            targetList,
            self,
            validTargets,
            getAllValidTargets,
            number,
            disallowedTargets,
            limitOptions);

        if (getAllValidTargets) {
            yield return new WaitUntil(() => targetList.Count > 0);
        } else {
            yield return new WaitUntil(() => targetList.Count == number);
        }

        foreach (TargettableEntity entity in targetList) {
            document.addEntityToDocument(outputKey, entity);
        }

        yield return null;
    }

    public TargettableEntity getSelf(EffectDocument document) {
        // A companion is the source of the effect
        if (document.originEntityType == EntityType.Companion) {
            CompanionInstance companion = document.companionMap.getItem(
                EffectDocument.ORIGIN, 0);
            return companion;
        // A playable card is the source of the effect
        } else if (document.originEntityType == EntityType.PlayableCard) {
            PlayableCard playableCard = document.playableCardMap.getItem(
                EffectDocument.ORIGIN, 0);
            return playableCard;
        }

        EffectError("Origin entity was not specified");
        return null;
    }

    public void addOriginCardEntityFromToDocument(EffectDocument document) {
        if (document.originEntityType != EntityType.PlayableCard) {
            EffectError("TargetCompanionThatDeltCard" +
                " rule set, but origin of effect isn't a PlayableCard");
            return;
        }

        PlayableCard playableCard = document.playableCardMap.getItem(
            EffectDocument.ORIGIN, 0);
        CombatEntityWithDeckInstance entityFrom = playableCard.entityFrom;

        if (entityFrom is MinionInstance) {
            MinionInstance minion = entityFrom as MinionInstance;
            document.minionMap.addItem(outputKey, minion);
        } else if (entityFrom is CompanionInstance) {
            CompanionInstance companion = entityFrom as CompanionInstance;
            document.companionMap.addItem(outputKey, companion);
        }
    }

    public void addSelfToDocument(EffectDocument document) {
        if (document.originEntityType == EntityType.Unknown) {
            EffectError("TargetSelf rule checked but" +
                " the origin entity was not specified");
            return;
        }

        // A companion is the source of the effect
        if (document.originEntityType == EntityType.Companion) {
            CompanionInstance companion = document.companionMap.getItem(
                EffectDocument.ORIGIN, 0);
            document.companionMap.addItem(outputKey, companion);
        // A playable card is the source of the effect
        } else if (document.originEntityType == EntityType.PlayableCard) {
            PlayableCard playableCard = document.playableCardMap.getItem(
                EffectDocument.ORIGIN, 0);
            document.playableCardMap.addItem(outputKey, playableCard);
        }
    }

    public enum SpecialTargetRule {
        None,
        TargetAllValidTargets,
        TargetAllValidTargetsExceptSelf,
        TargetSelf,
        TargetEntityThatDeltCard,
        CantTargetSelf
    }
}