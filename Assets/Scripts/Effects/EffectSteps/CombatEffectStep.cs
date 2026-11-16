using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*
    Effect that deals with common combat effects (non status effects) such as
    dealing damage, healing, etc...

    Input: The entities that are the targets of the effect
    Output: NA
    Parameters:
        - CombatEffect: The effect to use in the step
        - Scale: The fixed scale if GetScaleFromKey is not enabled
        - GetScaleFromKey: If checked, the scale will be pulled from a previous step
        - InputScaleKey: The key from which to pull the scale integer from
*/
public class CombatEffectStep : EffectStep, IEffectStepCalculation
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private CombatEffect combatEffect;
    [SerializeField]
    private int scale;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string inputScaleKey = "";
    [SerializeField]
    private int multiplicity = 1;
    [SerializeField]
    private bool getMultiplicityFromKey = false;
    [SerializeField]
    private string inputMultiplicityKey = "";
    [SerializeField]
    private GameObject vfxPrefab = null;
    [Tooltip("VFX: flies from the caster to each target just before vfxPrefab plays, so a strike across the board reads as coming from its caster. Purely visual.")]
    [SerializeField]
    private GameObject casterTrail = null;
    private CombatInstance originCombatInstance;
    private int baseMultiplicity;
    [SerializeField]
    private string damageDealtOutputKey = "";
    public CombatEffectStep()
    {
        effectStepName = "CombatEffectStep";
    }

    public override IEnumerator invoke(EffectDocument document)
    {
        List<CombatInstance> instances = document.map.GetList<CombatInstance>(inputKey);
        if (instances.Count == 0)
        {
            EffectError("No input targets present for key " + inputKey);
            yield break;
        }
        int finalScale = getFinalScale(document);

        if (finalScale == -1) yield break;

        const float delayIncrement = 1f;

        int totalEffectiveScaleApplied = 0;
        // Update loop to do each instance of damage as the outer loop so that they can be timed across companions
        // taking damage from the same effect
        for (int i = 0; i < baseMultiplicity; i++)
        {
            foreach (CombatInstance instance in instances)
            {
                if (instance != null)
                {
                    // "Do something to another friendly" effects (e.g. heal an ally):
                    // arc a precast trail from the caster to the target and defer the
                    // impact VFX until it lands. Gameplay still applies immediately.
                    bool trail = PrecastTrailController.ShouldTrail(originCombatInstance, instance);
                    // VFX: an ability striking across the board flies its own quick trail from the caster
                    bool quickTrail = casterTrail != null && vfxPrefab != null && PrecastTrailController.CanQuickTrail(originCombatInstance, instance);
                    GameObject immediateVfx = trail || quickTrail ? null : vfxPrefab;
                    // HitTiming: with a trail, the feedback waits for the flight plus the effect's own wind-up
                    if (quickTrail) HitTiming.Expect(instance, vfxPrefab, PrecastTrailController.QuickFlightTime);
                    else if (trail && vfxPrefab != null) HitTiming.Expect(instance, vfxPrefab, PrecastTrailController.Instance.FlightTime);

                    int scaleApplied = instance.ApplyNonStatusCombatEffect(combatEffect,
                        finalScale,
                        originCombatInstance,
                        immediateVfx,
                        i == 0,
                        i / 4.0f); // TODO, elevate this logic for ethan to tune
                        PlayableCard originPlayableCard = document.map.TryGetItem<PlayableCard>(EffectDocument.ORIGIN, 0);
                        // the attack sound: attack cards always; other cards (the damaging Charms: Greek Fire, Bezoar,
                        // Boomflask) and rat abilities (Hold'em's gun) only when they have a sound of their own
                        string attackName = null;
                        if (originPlayableCard != null && originPlayableCard.card != null)
                        {
                            Card originCard = originPlayableCard.card;
                            if (originCard.cardType.cardCategory == CardCategory.Attack || MusicController.HasOwnAttackSound(originCard.cardType.Name))
                                attackName = originCard.cardType.Name;
                        }
                        else if (originCombatInstance != null)
                        {
                            CompanionInstance rat = originCombatInstance.GetComponentInParent<CompanionInstance>();
                            string ratName = rat != null && rat.companion != null && rat.companion.companionType != null ? rat.companion.companionType.companionName : null;
                            if (MusicController.HasOwnAttackSound(ratName)) attackName = ratName;
                        }
                        if (attackName != null)
                        {
                            // HitTiming: the attack sound plays when the effect strikes this target, with the rest of the hit
                            string sound = attackName;
                            HitTiming.AtImpact(instance, HitTiming.CurrentDelay(instance),
                                () => MusicController.Instance.PlayCorrectAttackSound(sound));
                        }

                    if (quickTrail)
                    {
                        CombatInstance target = instance;
                        PrecastTrailController.Instance.PlayQuickTrail(casterTrail, originCombatInstance, target,
                            () => { if (target != null) GameObject.Instantiate(vfxPrefab, target.transform.position, Quaternion.identity); });
                    }
                    else if (trail && vfxPrefab != null)
                    {
                        CombatInstance target = instance;
                        PrecastTrailController.Instance.PlayTrailThenSpawn(
                            originCombatInstance,
                            target,
                            () => { if (target != null) GameObject.Instantiate(vfxPrefab, target.transform.position, Quaternion.identity); });
                    }
                    totalEffectiveScaleApplied += scaleApplied;
                }
            }

            if (vfxPrefab && i < baseMultiplicity - 1)
            {
                yield return new WaitForSeconds(delayIncrement / baseMultiplicity);
            }
        }

        if (damageDealtOutputKey != "")
            document.intMap[damageDealtOutputKey] = totalEffectiveScaleApplied;
    }

    private int getFinalScale(EffectDocument document)
    {
        PlayableCard originCard = null;
        // Determine whether origin of damage is from a card, companion ability, or enemy attack
        // and get that origin
        if (document.map.ContainsValueWithKey<PlayableCard>(EffectDocument.ORIGIN))
        {
            originCard = document.map.GetItem<PlayableCard>(EffectDocument.ORIGIN, 0);
            originCombatInstance = originCard.deckFrom.combatInstance;
        }
        else if (document.map.ContainsValueWithKey<CompanionInstance>(EffectDocument.ORIGIN))
        {
            originCombatInstance = document.map.GetItem<CompanionInstance>(EffectDocument.ORIGIN, 0).combatInstance;
        }
        else if (document.map.ContainsValueWithKey<EnemyInstance>(EffectDocument.ORIGIN))
        {
            originCombatInstance = document.map.GetItem<EnemyInstance>(EffectDocument.ORIGIN, 0).combatInstance;
        }
        else
        {
            EffectError("No origin set in EffectDocument to pull stats from");
            return -1;
        }

        int baseScale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey))
        {
            baseScale = document.intMap[inputScaleKey];
        }
        else
        {
            baseScale = scale;
        }

        if (getMultiplicityFromKey && document.intMap.ContainsKey(inputMultiplicityKey))
        {
            baseMultiplicity = document.intMap[inputMultiplicityKey];
        }
        else
        {
            baseMultiplicity = multiplicity;
        }

        return UpdateScaleForEffect(baseScale, originCombatInstance, originCard);
    }

    private int UpdateScaleForEffect(
            int baseScale,
            CombatInstance origin,
            PlayableCard card = null)
    {
        int newScale = baseScale;
        switch (combatEffect)
        {
            case CombatEffect.Damage:
                newScale = baseScale + origin.GetCurrentDamage();
                if (card != null)
                {
                    newScale = card.card.UpdateScaleForCardModificationsAndPassives(newScale, origin);
                }
                break;
            case CombatEffect.FixedDamageWithCardModifications:
                if (card != null)
                {
                    newScale = card.card.UpdateScaleForCardModificationsAndPassives(newScale, origin);
                }
                break;
        }

        return newScale;
    }
    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        if (combatEffect == CombatEffect.Damage || combatEffect == CombatEffect.FixedDamageWithCardModifications)
        {
            int finalScale = getFinalScale(document);
            string damageRplKey = document.stringMap.GetValueOrDefault("card_calculation_rpl_damage_key", "rpl_damage");
            // Help subsequent combat effect steps store their damage under a different key.
            // If we have a card that does multiple combat effect steps with different values, we should have some way to access them.
            // The index will be local to the efect workflow.
            string numCombatStepInvocationsKey = damageRplKey + "_num_combateffectsteps";
            int numCombatEffectStepInvocations = document.intMap.GetValueOrDefault(numCombatStepInvocationsKey, 1);

            // For the second and third invocations, we add the suffix with the damage.
            string adjustedDamageRplKey = numCombatEffectStepInvocations > 1 ? $"{damageRplKey}_{numCombatEffectStepInvocations}" : damageRplKey;

            Debug.Log($"Damage RPL key {adjustedDamageRplKey} has final scale {finalScale}");

            document.intMap[adjustedDamageRplKey] = finalScale;
            document.intMap[numCombatStepInvocationsKey] = numCombatEffectStepInvocations + 1;
        }
        yield return null;
    }
}
