using System;
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
    private List<Targetable.TargetType> validTargets;
    [Tooltip(
        "If either of the TargetAllValidTargets rules are set, number is ignored"
    )]
    [SerializeField]
    private int number = 1;
    [SerializeField]
    private SpecialTargetRule specialTargetRule = SpecialTargetRule.None;
    [SerializeField]
    private bool cantCancelTargetting = false;

    private List<Targetable> targetsList;
    private List<GameObject> limitOptions;
    private List<GameObject> disallowedTargets;
    private GameObject self;

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

        targetsList = new List<Targetable>();
        this.self = getSelf(document);
        disallowedTargets = new List<GameObject>();
        bool getAllValidTargets = false;
        if (specialTargetRule == SpecialTargetRule.TargetAllValidTargetsExceptSelf ||
                specialTargetRule == SpecialTargetRule.CantTargetSelf) {
            disallowedTargets.Add(self);
        }

        limitOptions = null;
        if (useInputToLimitOptions) {
            limitOptions = document.GetGameObjects(inputKey);
        }

        if (specialTargetRule == SpecialTargetRule.TargetAllValidTargets || 
                specialTargetRule == SpecialTargetRule.TargetAllValidTargetsExceptSelf) {
            getAllValidTargets = true;
        }

        if (getAllValidTargets) {
            GetAllValidTargets(document);
        } else {
            TargettingManager.Instance.targetSuppliedHandler += TargetSuppliedHandler;
            TargettingManager.Instance.cancelTargettingHandler += CancelHandler;
            TargettingArrowsController.Instance.createTargettingArrow(validTargets, self);
            UIStateManager.Instance.setState(UIState.EFFECT_TARGETTING);
            yield return new WaitUntil(() => targetsList.Count == number);
            TargettingManager.Instance.targetSuppliedHandler -= TargetSuppliedHandler;
            TargettingManager.Instance.cancelTargettingHandler -= CancelHandler;

            AddTargetsToDocument(document);
        }

        yield return null;
    }

    private void AddTargetsToDocument(EffectDocument document) {
        foreach (Targetable target in targetsList) {
            switch (target.targetType) {
                case Targetable.TargetType.Companion:
                    AddCompanionToDocument(document, outputKey, target.GetComponent<CompanionInstance>());
                break;

                case Targetable.TargetType.Minion:
                    AddMinionToDocument(document, outputKey, target.GetComponent<MinionInstance>());
                break;

                case Targetable.TargetType.Enemy:
                    AddEnemyToDocument(document, outputKey, target.GetComponent<EnemyInstance>());
                break;

                case Targetable.TargetType.Card:
                    AddPlayableCardToDocument(document, outputKey, target.GetComponent<PlayableCard>());
                break;
            }
        }
    }

    private void TargetSuppliedHandler(Targetable target) {
        // Check if the provided target is disallowed
        if (disallowedTargets != null && disallowedTargets.Contains(target.gameObject)) 
            return;

        // Check if we only want to pick from a specific list of targets
        if (limitOptions != null && !limitOptions.Contains(target.gameObject)) {
            return;
        }

        // Check if the entity type is correct
        if (!validTargets.Contains(target.targetType)) {
            return;
        }

        TargettingArrowsController.Instance.createTargettingArrow(validTargets, self);
        targetsList.Add(target);

        if (number > targetsList.Count) {
            TargettingArrowsController.Instance.createTargettingArrow(validTargets, self);
        } else {
            UIStateManager.Instance.setState(UIState.DEFAULT);
        }
    }

    private void CancelHandler(CancelContext context) {
        if (cantCancelTargetting) {
            context.canCancel = false;
        } else {
            context.canCancel = true;
            EffectManager.Instance.CancelEffectWorkflow();
            TargettingManager.Instance.targetSuppliedHandler -= TargetSuppliedHandler;
            TargettingManager.Instance.cancelTargettingHandler -= CancelHandler;
        }
    }

    private void GetAllValidTargets(EffectDocument document) {
        if (validTargets.Contains(Targetable.TargetType.Companion)) {
            List<CompanionInstance> companions = CombatEntityManager.Instance.getCompanions()
                .FindAll(instance => !disallowedTargets.Contains(instance.gameObject));
            companions.ForEach(companion => {
                AddCompanionToDocument(document, outputKey, companion);
            });
        }
        if (validTargets.Contains(Targetable.TargetType.Enemy)) {
            List<EnemyInstance> enemies = CombatEntityManager.Instance.getEnemies()
                .FindAll(instance => !disallowedTargets.Contains(instance.gameObject));
            enemies.ForEach(enemy => AddEnemyToDocument(document, outputKey, enemy));
        }
        if (validTargets.Contains(Targetable.TargetType.Minion)) {
            List<MinionInstance> minions = CombatEntityManager.Instance.getMinions()
                .FindAll(instance => !disallowedTargets.Contains(instance.gameObject));
            minions.ForEach(minion => {
                AddMinionToDocument(document, outputKey, minion);
            });
        }
        if (validTargets.Contains(Targetable.TargetType.Card)) {
            List<PlayableCard> playableCards = PlayerHand.Instance.cardsInHand
                .FindAll(card => !disallowedTargets.Contains(card.gameObject));
            playableCards.ForEach(playableCard => AddPlayableCardToDocument(document, outputKey, playableCard));
        }
    }

    private void AddCompanionToDocument(
            EffectDocument document,
            string key,
            CompanionInstance companion) {
        document.map.AddItem(key, companion);
        document.map.AddItem(key, companion.combatInstance);
        document.map.AddItem(key, companion.deckInstance);
    }

    private void AddMinionToDocument(
            EffectDocument document,
            string key,
            MinionInstance minion) {
        document.map.AddItem(key, minion);
        document.map.AddItem(key, minion.combatInstance);
        document.map.AddItem(key, minion.deckInstance);
    }

    private void AddEnemyToDocument(
            EffectDocument document,
            string key,
            EnemyInstance enemy) {
        document.map.AddItem(key, enemy);
        document.map.AddItem(key, enemy.combatInstance);
    }

    private void AddPlayableCardToDocument(
            EffectDocument document,
            string key,
            PlayableCard playableCard) {
        document.map.AddItem(key, playableCard);
        document.map.AddItem(key, playableCard.card);
    }

    public GameObject getSelf(EffectDocument document) {
        // A companion is the source of the effect
        if (document.originEntityType == EntityType.Companion) {
            CompanionInstance companion = document.map.GetItem<CompanionInstance>(
                EffectDocument.ORIGIN, 0);
            return companion.gameObject;
        // A playable card is the source of the effect
        } else if (document.originEntityType == EntityType.Card) {
            PlayableCard playableCard = document.map.GetItem<PlayableCard>(
                EffectDocument.ORIGIN, 0);
            return playableCard.gameObject;
        }

        EffectError("Origin entity was not specified");
        return null;
    }

    public void addOriginCardEntityFromToDocument(EffectDocument document) {
        if (document.originEntityType != EntityType.Card) {
            EffectError("TargetCompanionThatDeltCard" +
                " rule set, but origin of effect isn't a PlayableCard");
            return;
        }

        PlayableCard playableCard = document.map.GetItem<PlayableCard>(
            EffectDocument.ORIGIN, 0);
        DeckInstance deckFrom = playableCard.deckFrom;

        if (deckFrom.TryGetComponent(out MinionInstance minion)) {
            document.map.AddItem<MinionInstance>(outputKey, minion);
            document.map.AddItem<CombatInstance>(outputKey, minion.combatInstance);
            document.map.AddItem<DeckInstance>(outputKey, minion.deckInstance);
        } else if (deckFrom.TryGetComponent(out CompanionInstance companion)) {
            document.map.AddItem<CompanionInstance>(outputKey, companion);
            document.map.AddItem<CombatInstance>(outputKey, companion.combatInstance);
            document.map.AddItem<DeckInstance>(outputKey, companion.deckInstance);
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
            CompanionInstance companion = document.map.GetItem<CompanionInstance>(
                EffectDocument.ORIGIN, 0);
            document.map.AddItem<CompanionInstance>(outputKey, companion);
            document.map.AddItem<CombatInstance>(outputKey, companion.combatInstance);
            document.map.AddItem<DeckInstance>(outputKey, companion.deckInstance);
        // A playable card is the source of the effect
        } else if (document.originEntityType == EntityType.Card) {
            PlayableCard playableCard = document.map.GetItem<PlayableCard>(
                EffectDocument.ORIGIN, 0);
            document.map.AddItem<PlayableCard>(outputKey, playableCard);
            document.map.AddItem<Card>(outputKey, playableCard.card);
        } else if (document.originEntityType == EntityType.Enemy) {
            EnemyInstance enemy = document.map.GetItem<EnemyInstance>(
                EffectDocument.ORIGIN, 0);
            document.map.AddItem<EnemyInstance>(outputKey, enemy);
            document.map.AddItem<CombatInstance>(outputKey, enemy.combatInstance);
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