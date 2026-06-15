using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that adds a status of some kind to an entity

    Input: Entity to add the status to
    Output: NA
    Parameters:
        - StatusEffect: The status effect to add to the entity
        - Scale: The fixed scale if GetScaleFromKey is not enabled
        - GetScaleFromKey: If checked, the scale will be pulled from a previous step
        - InputScaleKey: The key from which to pull the scale integer from
*/
public class ApplyStatus : EffectStep, ITooltipProvider, IEffectStepCalculation
{
    [SerializeField]
    [Tooltip(
        "Designates what entity to add status to based on " +
        "the key the entity was saved to from a previous effect"
    )]
    private string inputKey = "";
    [SerializeField]
    private StatusEffectType statusEffect;
    [SerializeField]
    private int scale;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string inputScaleKey = "";
    [SerializeField]
    private bool multiplyByNumAuraStacks = false;
    [SerializeField]
    private int multiplicity = 1;
    [SerializeField]
    private bool getMultiplicityFromKey = false;
    [SerializeField]
    private string inputMultiplicityKey = "";
    [SerializeField]
    private bool toggleStatus = false; // if true, will remove the status if it's already present instead of adding to stacks


    private static Dictionary<StatusEffectType, DescriptionToken.DescriptionIconType> descriptionIconMapping = new Dictionary<StatusEffectType, DescriptionToken.DescriptionIconType>() {
        {StatusEffectType.Strength, DescriptionToken.DescriptionIconType.Strength},
        {StatusEffectType.Defended, DescriptionToken.DescriptionIconType.Block},
        {StatusEffectType.TemporaryStrength, DescriptionToken.DescriptionIconType.TemporaryStrength},
    };

    public ApplyStatus()
    {
        effectStepName = "ApplyStatus";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> combatInstances = document.map.GetList<CombatInstance>(inputKey);
        if (combatInstances.Count == 0) {
            EffectError("No input targets present for key " + inputKey);
            yield return null;
        }

        // Setup the scale
        int finalScale = getFinalScale(document);

        foreach (CombatInstance combatInstance in combatInstances) {
            if (toggleStatus) {
                combatInstance.ToggleStatusEffect(statusEffect);
                continue;
            }

            int personalizedScale = finalScale;
            // Certain companion abilities have effects like "give each
            // companion on the team 4 block for each aura stack they have".
            if (multiplyByNumAuraStacks) {
                personalizedScale *= combatInstance.GetStatus(StatusEffectType.Orb);
            }

            // setup multiplicity (we now have effets that care about each time block is applied so we should apply it individually)
            int finalMultiplicity = multiplicity;
            if (getMultiplicityFromKey && document.intMap.ContainsKey(inputMultiplicityKey)) {
                finalMultiplicity = document.intMap[inputMultiplicityKey];
                Debug.LogError(finalMultiplicity);
            }
            for (int i = 0; i < finalMultiplicity; i++)
            {
                combatInstance.ApplyStatusEffects(statusEffect, personalizedScale);
            }
        }
        yield return null;
    }

    private int getFinalScale(EffectDocument document) {
        PlayableCard originCard = null;
        CombatInstance originCombatInstance;
        // Determine whether origin of damage is from a card, companion ability, or enemy attack
        // and get that origin
        if (document.map.ContainsValueWithKey<PlayableCard>(EffectDocument.ORIGIN))
        {
            // Debug.Log("[ApplyStatus] Origin is a card");
            originCard = document.map.GetItem<PlayableCard>(EffectDocument.ORIGIN, 0);
            originCombatInstance = originCard.deckFrom.combatInstance;
        }
        else if (document.map.ContainsValueWithKey<CompanionInstance>(EffectDocument.ORIGIN))
        {
            // Debug.Log("[ApplyStatus] Origin is a companion");
            originCombatInstance = document.map.GetItem<CompanionInstance>(EffectDocument.ORIGIN, 0).combatInstance;
        }
        else if (document.map.ContainsValueWithKey<EnemyInstance>(EffectDocument.ORIGIN))
        {
            // Debug.Log("[ApplyStatus] Origin is an enemy");
            originCombatInstance = document.map.GetItem<EnemyInstance>(EffectDocument.ORIGIN, 0).combatInstance;
        }
        else
        {
            EffectError("No origin set in EffectDocument to pull stats from");
            return -1;
        }

        int finalScale = scale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            finalScale = document.intMap[inputScaleKey];
        }
        return UpdateScaleForEffect(finalScale, originCombatInstance, originCard);
    }

    private int UpdateScaleForEffect(
            int baseScale,
            CombatInstance origin = null,
            PlayableCard card = null)
    {
        int newScale = baseScale;
        switch (statusEffect)
        {
            case StatusEffectType.Defended:
                if (card != null)
                {
                    int val = card.card.GetBonusBlock();
                    Debug.Log($"[ApplyStatus] Card {card.card.name} has FixedBlockIncrease {val}");
                    Debug.Log("[ApplyStatus] Base scale: " + baseScale);
                    newScale += val;
                    Debug.Log("[ApplyStatus] New scale: " + newScale);
                }
                else
                {
                    Debug.Log("[ApplyStatus] No card origin, base scale: " + baseScale);
                }
                break;
        }

        return newScale;
    }

    public TooltipViewModel GetTooltip() {
        if (descriptionIconMapping.ContainsKey(statusEffect) && KeywordTooltipProvider.Instance.HasTooltip(descriptionIconMapping[statusEffect])){
            return KeywordTooltipProvider.Instance.GetTooltip(descriptionIconMapping[statusEffect]);
        }
         else if (KeywordTooltipProvider.Instance.HasTooltip(statusEffect)){
            return KeywordTooltipProvider.Instance.GetTooltip(statusEffect);
        }
         else return new TooltipViewModel(empty: true);
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        if (statusEffect == StatusEffectType.Defended)
        {
            int finalScale = getFinalScale(document);
            string blockRplKey = document.stringMap.GetValueOrDefault("card_calculation_rpl_block_key", "rpl_block");
            // Help subsequent combat effect steps store their damage under a different key.
            // If we have a card that does multiple combat effect steps with different values, we should have some way to access them.
            // The index will be local to the efect workflow.
            string numApplyStatusStepsKey = blockRplKey + "_num_applystatussteps";
            int numApplyStatusSteps = document.intMap.GetValueOrDefault(numApplyStatusStepsKey, 1);

            // For the second and third invocations, we add the suffix with the damage.
            string adjustedBlockRplKey = numApplyStatusSteps > 1 ? $"{blockRplKey}_{numApplyStatusSteps}" : blockRplKey;

            Debug.Log($"Block RPL key {adjustedBlockRplKey} has final scale {finalScale}");

            document.intMap[adjustedBlockRplKey] = finalScale;
            document.intMap[numApplyStatusStepsKey] = numApplyStatusSteps + 1;
        }
        yield return null;
    }
}
