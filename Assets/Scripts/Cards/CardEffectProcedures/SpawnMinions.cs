using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnMinions: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public string procedureClass;
    public int baseScale = 1;
    public MinionTypeSO minionType;

    public SpawnMinions() {
        procedureClass = "SpawnMinions";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        yield return null;
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        
        for(int i = 0; i < baseScale; i++) {
            PrefabInstantiator.instantiateMinion(
                context.companionManager.encounterConstants.minionPrefab,
                new Minion(minionType),
                // Temp
                context.cardCaster.getNextMinionSpawnPosition()
            );
        }
        
        yield return null;
    }

}