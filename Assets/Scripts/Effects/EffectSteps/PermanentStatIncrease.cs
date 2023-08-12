using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that permanently increase a unit's stats. Technically this can
    be used on enemies, but we shouldn't really ever be doing that since
    enemies just poof once an encounter is completed, so the temporary versions
    of stat increases suffice. Technically we could use this if we don't want
    the status on an enemy to eventually end.

    Input: The entity target(s) that we want to permanently increase stat of
    Output: NA
    Paramters:
        - StatIncreaseType: The type of permanent stat increase
        - Scale: The fixed scale if GetScaleFromKey is not enabled
        - GetScaleFromKey: If checked, the scale will be pulled from a previous step
        - InputScaleKey: The key from which to pull the scale integer from
*/
public class PermanentStatIncrease : EffectStep {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private StatIncreaseType statIncreaseType;
    [SerializeField]
    private int scale;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string inputScaleKey = "";

    public PermanentStatIncrease() {
        effectStepName = "PermanentStatIncrease";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> instances = document.GetCombatInstances(inputKey);
        if (instances.Count == 0) {
            EffectError("No valid inputs for increasing stats");
            yield return null;
        }

        int finalScale = scale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            finalScale = document.intMap[inputScaleKey];
        }

        switch(statIncreaseType) {
            case StatIncreaseType.AttackDamage:
                increaseAttackDamage(instances, finalScale);
            break;

            case StatIncreaseType.Health:
                increaseHealth(instances, finalScale);
            break;

            default:
                EffectError("StatIncreaseType " + statIncreaseType + 
                    " not yet supported");
            break;
        }
        yield return null;
    }

    private void increaseAttackDamage(List<CombatInstance> instances, int scale) {
        foreach (CombatInstance instance in instances) {
            instance.combatStats.IncreaseBaseAttackDamage(scale);
        }
    }

    private void increaseHealth(List<CombatInstance> instances, int scale) {
        foreach (CombatInstance instance in instances) {
            instance.combatStats.IncreaseMaxHealth(scale);
            // Choosing to immediately give the health increase to the entity
            // Example: Without this, if a entity health is 20/20 and we increase
            // max health by 1, then the entity would have 20/21 health and it would
            // visually look like it took damage even though it didn't.
            instance.combatStats.currentHealth += scale;
        }
    }

    public enum StatIncreaseType {
        AttackDamage,
        Health,
    }
}