using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class CardCastArguments {

    public List<string> targets;
    public int damageIncrease;
    // Can put stuff like increased draw or other effects here
    // Also see CardInfo.cs for how the effects get added to the 
    // effect events

    public CardCastArguments (List<string> targets, int damageIncrease) { 
        this.targets = targets;
        this.damageIncrease = damageIncrease;
    }
}
