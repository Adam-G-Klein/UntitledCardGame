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
public class GetTargets : EffectStep, IEffectStepCalculation
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
    [SerializeField] private string cardSelectionHelperText;
    private bool cancelled = false;

    private List<Targetable> targetsList;
    private List<GameObject> limitOptions;
    private List<GameObject> disallowedTargets;
    private GameObject self;
    private PlayableCard originCard = null;

    public GetTargets() {
        effectStepName = "GetTargets";
    }

    public override IEnumerator invoke(EffectDocument document) {
        // Two specific special target rule handling
        if (document.map.ContainsValueWithKey<PlayableCard>(EffectDocument.ORIGIN)) {
            originCard = document.map.TryGetItem<PlayableCard>(EffectDocument.ORIGIN, 0);
        }
        if (specialTargetRule == SpecialTargetRule.TargetSelf) {
            addSelfToDocument(document);
            yield break;
        } else if (specialTargetRule == SpecialTargetRule.TargetEntityThatDeltCard) {
            addOriginCardEntityFromToDocument(document);
            yield break;
        } else if (specialTargetRule == SpecialTargetRule.TargetAllEntitiesExceptEntityThatDealtCard) {
            addAllEntitiesExceptOriginCardEntityToDocument(document);
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
        } else if (specialTargetRule == SpecialTargetRule.TargetLeftmost) {
            GetTargetAtPosition(document, 0);
        } else if (specialTargetRule == SpecialTargetRule.TargetRandom) {
            GetTargetsRandomly(document);
        } else {
            cancelled = false;
            // if we're here then we need to user to click something on the screen
            // FocusManager.Instance.StashFocusablesNotOfTargetType(validTargets, this.GetType().Name);
            // UIStateManager.Instance.setState(UIState.EFFECT_TARGETTING);
            if (validTargets.Count == 1 && validTargets.Contains(Targetable.TargetType.Card)) {
                FocusManager.Instance.StashFocusablesNotOfTargetType(validTargets, this.GetType().Name);
                UIStateManager.Instance.setState(UIState.EFFECT_TARGETTING);
                bool selected = false;
                PlayerHand.Instance.SelectCardsFromHand(number, disallowedTargets, limitOptions, CancelHandler, (List<PlayableCard> selectedCards) => {
                    foreach (PlayableCard card in selectedCards) {
                        targetsList.Add(card.GetComponent<Targetable>());
                    }
                    selected = true;
                }, originCard, !cantCancelTargetting, cardSelectionHelperText);
                yield return new WaitUntil(() => selected == true || (!cantCancelTargetting && cancelled));
            } else {
                TargettingManager.Instance.targetSuppliedHandler += TargetSuppliedHandler;
                TargettingManager.Instance.cancelTargettingHandler += CancelHandler;
                TargettingArrowsController.Instance.createTargettingArrow(validTargets, self);
                FocusManager.Instance.StashFocusablesNotOfTargetType(validTargets, this.GetType().Name);
                UIStateManager.Instance.setState(UIState.EFFECT_TARGETTING);
                yield return new WaitUntil(() => targetsList.Count == number || (!cantCancelTargetting && cancelled));
                TargettingManager.Instance.targetSuppliedHandler -= TargetSuppliedHandler;
                TargettingManager.Instance.cancelTargettingHandler -= CancelHandler;
            }

            FocusManager.Instance.UnstashFocusables(this.GetType().Name);
            // If the user cancelled the effect workflow, we should interrupt it.
            // Also, disable the callback so the card doesn't finish casting :)
            if (!cantCancelTargetting && cancelled) {
                document.Interrupt(disableCallback: true);
            }

            AddTargetsToDocument(document);
        }

        yield return null;
    }

    private void AddTargetsToDocument(EffectDocument document) {
        foreach (Targetable target in targetsList) {
            switch (target.targetType) {
                case Targetable.TargetType.Companion:
                    EffectUtils.AddCompanionToDocument(document, outputKey, target.GetComponent<CompanionInstance>());
                break;

                case Targetable.TargetType.Enemy:
                    EffectUtils.AddEnemyToDocument(document, outputKey, target.GetComponent<EnemyInstance>());
                break;

                case Targetable.TargetType.Card:
                    EffectUtils.AddPlayableCardToDocument(document, outputKey, target.GetComponent<PlayableCard>());
                break;
            }
        }
    }

    private void TargetSuppliedHandler(Targetable target) {
        Debug.Log("GetTargets Effet Step Target supplied: " + target.gameObject.name);
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
            Debug.Log("Cancelling the GetTargets effect step");
            context.canCancel = true;
            cancelled = true;
            FocusManager.Instance.UnstashFocusables(this.GetType().Name);
            PlayerHand.Instance.SetHoverable(true);
            if (originCard != null) {
                if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.KeyboardController)
                {
                    originCard.ResetCardScale(true);
                }
                else
                {
                    originCard.ResetCardScale(false);
                }
                EnemyEncounterManager.Instance.SetCastingCard(false);
                PlayerHand.Instance.UpdateCardPositions();
            }
        }
    }

    private void GetAllValidTargets(EffectDocument document) {
        if (validTargets.Contains(Targetable.TargetType.Companion)) {
            List<CompanionInstance> companions = CombatEntityManager.Instance.getCompanions()
                .FindAll(instance => !disallowedTargets.Contains(instance.gameObject));
            companions.ForEach(companion => {
                EffectUtils.AddCompanionToDocument(document, outputKey, companion);
            });
        }
        if (validTargets.Contains(Targetable.TargetType.Enemy)) {
            List<EnemyInstance> enemies = CombatEntityManager.Instance.getEnemies()
                .FindAll(instance => !disallowedTargets.Contains(instance.gameObject));
            enemies.ForEach(enemy => EffectUtils.AddEnemyToDocument(document, outputKey, enemy));
        }
        if (validTargets.Contains(Targetable.TargetType.Card)) {
            List<PlayableCard> playableCards = PlayerHand.Instance.GetCardsOrdered()
                .FindAll(card => !disallowedTargets.Contains(card.gameObject));
            playableCards.ForEach(playableCard => EffectUtils.AddPlayableCardToDocument(document, outputKey, playableCard));
        }
    }

    private void GetTargetAtPosition(EffectDocument document, int pos = 0) {
        if (validTargets.Contains(Targetable.TargetType.Companion)) {
            List<CompanionInstance> companions = CombatEntityManager.Instance.getCompanions()
                .FindAll(instance => !disallowedTargets.Contains(instance.gameObject));
            int index = Mathf.Clamp(pos, 0, companions.Count - 1);
            EffectUtils.AddCompanionToDocument(document, outputKey, companions[index]);
        }
        if (validTargets.Contains(Targetable.TargetType.Enemy)) {
            List<EnemyInstance> enemies = CombatEntityManager.Instance.getEnemies()
                .FindAll(instance => !disallowedTargets.Contains(instance.gameObject));
            int index = Mathf.Clamp(pos, 0, enemies.Count - 1);
            EffectUtils.AddEnemyToDocument(document, outputKey, enemies[index]);
        }
        if (validTargets.Contains(Targetable.TargetType.Card)) {
            List<PlayableCard> playableCards = PlayerHand.Instance.GetCardsOrdered()
                .FindAll(card => !disallowedTargets.Contains(card.gameObject));
            int index = Mathf.Clamp(pos, 0, playableCards.Count - 1);
            EffectUtils.AddPlayableCardToDocument(document, outputKey, playableCards[index]);
        }
    }

    private void GetTargetsRandomly(EffectDocument document) {
        for (int i = 0; i < number ; i++) {
            if (validTargets.Contains(Targetable.TargetType.Companion)) {
                List<CompanionInstance> companions = CombatEntityManager.Instance.getCompanions()
                    .FindAll(instance => !disallowedTargets.Contains(instance.gameObject));
                CompanionInstance target = companions[UnityEngine.Random.Range(0, companions.Count)];
                EffectUtils.AddCompanionToDocument(document, outputKey, target);
            }
            if (validTargets.Contains(Targetable.TargetType.Enemy)) {
                List<EnemyInstance> enemies = CombatEntityManager.Instance.getEnemies()
                    .FindAll(instance => !disallowedTargets.Contains(instance.gameObject));
                EnemyInstance target = enemies[UnityEngine.Random.Range(0, enemies.Count)];
                EffectUtils.AddEnemyToDocument(document, outputKey, target);
            }
            if (validTargets.Contains(Targetable.TargetType.Card)) {
                List<PlayableCard> playableCards = PlayerHand.Instance.GetCardsOrdered()
                    .FindAll(card => !disallowedTargets.Contains(card.gameObject));
                PlayableCard target = playableCards[UnityEngine.Random.Range(0, playableCards.Count)];
                EffectUtils.AddPlayableCardToDocument(document, outputKey, target);
            }
        }
    }

    public GameObject getSelf(EffectDocument document) {
        // A companion is the source of the effect
        if (document.originEntityType == EntityType.CompanionInstance) {
            CompanionInstance companion = document.map.GetItem<CompanionInstance>(
                EffectDocument.ORIGIN, 0);
            return companion.gameObject;
        // A playable card is the source of the effect
        } else if (document.originEntityType == EntityType.Card) {
            PlayableCard playableCard = document.map.GetItem<PlayableCard>(
                EffectDocument.ORIGIN, 0);
            return playableCard.gameObject;
        } else if (document.originEntityType == EntityType.Enemy) {
            EnemyInstance enemy = document.map.GetItem<EnemyInstance>(
                EffectDocument.ORIGIN, 0);
            return enemy.gameObject;
        } else {
            // This is a hack to get around targetting in the case a card has an
            // effect that triggers on exhaust. The card goes away, but the targetting arrows
            // wont have a game object to position the base of the arrow at.
            return new GameObject();
        }
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

        if (deckFrom.TryGetComponent(out CompanionInstance companion)) {
            EffectUtils.AddCompanionToDocument(document, outputKey, companion);
        }
    }

    public void addAllEntitiesExceptOriginCardEntityToDocument(EffectDocument document) {
        if (document.originEntityType != EntityType.Card) {
            EffectError("addAllEntitiesExceptOriginCardEntityToDocument" +
                " called, but origin of effect isn't a PlayableCard");
            return;
        }

        PlayableCard playableCard = document.map.GetItem<PlayableCard>(
            EffectDocument.ORIGIN, 0);
        DeckInstance deckFrom = playableCard.deckFrom;

        List<CompanionInstance> companions = CombatEntityManager.Instance.getCompanions();
        companions.RemoveAll(companion => companion == deckFrom.GetComponent<CompanionInstance>());

        EffectUtils.AddCompanionsToDocument(document, outputKey, companions);
    }

    public void addSelfToDocument(EffectDocument document) {
        if (document.originEntityType == EntityType.Unknown) {
            EffectError("TargetSelf rule checked but" +
                " the origin entity was not specified");
            return;
        }

        // A companion is the source of the effect
        if (document.originEntityType == EntityType.CompanionInstance) {
            CompanionInstance companion = document.map.GetItem<CompanionInstance>(
                EffectDocument.ORIGIN, 0);
            EffectUtils.AddCompanionToDocument(document, outputKey, companion);
        // A playable card is the source of the effect
        } else if (document.originEntityType == EntityType.Card) {
            PlayableCard playableCard = document.map.GetItem<PlayableCard>(
                EffectDocument.ORIGIN, 0);
            EffectUtils.AddPlayableCardToDocument(document, outputKey, playableCard);
        } else if (document.originEntityType == EntityType.Enemy) {
            EnemyInstance enemy = document.map.GetItem<EnemyInstance>(
                EffectDocument.ORIGIN, 0);
            EffectUtils.AddEnemyToDocument(document, outputKey, enemy);
        }
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        if (specialTargetRule == SpecialTargetRule.CantTargetSelf) {
            yield return null;
        } else if (specialTargetRule == SpecialTargetRule.None) {
            if (!validTargets.Contains(Targetable.TargetType.Companion)) yield return null;
            CompanionInstance companionInstance = EnemyEncounterManager.Instance.gameState.hoveredCompanion;
            if (companionInstance != null) {
                EffectUtils.AddCompanionToDocument(document, outputKey, companionInstance);
            } else {
                yield return null;
            }
        } else {
            yield return invoke(document);
        }
    }

    public enum SpecialTargetRule {
        None,
        TargetAllValidTargets,
        TargetAllValidTargetsExceptSelf,
        TargetSelf,
        TargetEntityThatDeltCard,
        TargetAllEntitiesExceptEntityThatDealtCard,
        CantTargetSelf,
        TargetRandom,
        TargetLeftmost,
    }
}