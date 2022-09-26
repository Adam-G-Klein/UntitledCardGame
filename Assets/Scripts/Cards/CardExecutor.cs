using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class CardExecutor {
    public static void executeCard(CardEffectData card, CardCastArguments args) {
        switch(card.effectName) {
            case CardEffectName.Draw:
                CardEffect_Draw.Execute(card, args);
                break;
            case CardEffectName.Damage:
                CardEffect_Damage.Execute(card, args);
                break;
            case CardEffectName.Buff:
                CardEffect_Buff.Execute(card,args);
                break;

        }
    }
}