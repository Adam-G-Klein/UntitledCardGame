using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class CardCastArguments {

    public int damageIncrease;
    public string casterId;
    // Can put stuff like increased draw or other effects here
    // Also see CardInfo.cs for how the effects get added to the 
    // effect events

    public CardCastArguments (string casterId, int damageIncrease ) { 
        this.damageIncrease = damageIncrease;
        this.casterId = casterId;
    }
}
