using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class CardCastArguments {

    public CombatEntityWithDeckInstance caster;
    public CombatEntityInEncounterStats casterStats;
    // Can put stuff like increased draw or other effects here
    // Also see CardInfo.cs for how the effects get added to the 
    // effect events

    public CardCastArguments(CombatEntityWithDeckInstance caster){
        this.caster = caster;
        this.casterStats = caster.stats;
    }
}
