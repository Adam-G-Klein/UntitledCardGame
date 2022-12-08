using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 


public class CardCaster : MonoBehaviour {
    // Handles casting cardInfo's  

    [SerializeField]
    private CardEffectEvent cardEffectEvent;
    // +1 to the above comment this is bad
    [SerializeField]
    private CardCastEvent cardCastEvent;

    [SerializeField]
    private EffectTargetRequestEvent effectTargetRequestEvent;
    private Dictionary<CardEffectData, CombatEntityInstance> effectsToTargets = new Dictionary<CardEffectData, CombatEntityInstance>();
    public Transform defaultArrowRoot = null;
    private CardInfo cardCasting;
    private CardCastArguments argsCasting;

    public void Cast(CardInfo info, CardCastArguments args, Transform arrowRoot) {
        // This could be a lot better
        // this class is just about the opposite of 
        // idempotent, I think a lot more events will be necessary to change this
        // We could do this all with coroutines to maintain state too but
        // I worry that have a bunch of threads in flight will only complicate things
        // At least everything is synchronous here
        cardCasting = info;
        argsCasting = args;
        populateTargetsList(info, args, arrowRoot);
        
    }

    private void populateTargetsList(CardInfo info, CardCastArguments args, Transform arrowRoot){
        // The effectTargetSuppliedEventHandler will count the targets that have been supplied
        // and we'll cast with the final targets list 
        foreach(CardEffectData effect in info.EffectsList) {
            // We need to know what the effect is so we can know what kind of target to request
            // but we don't want to request the target until we know what the effect is
            // so we'll just add the effect to the dictionary and then request the target
            // and then when the target is supplied we'll know what effect it's for
            effectsToTargets.Add(effect, null);
        }
        print("raising effectTargetRequestEvent");
        StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(
            new EffectTargetRequestEventInfo(info.EffectsList[0],
            args.casterId, arrowRoot)));
    }

    public void effectTargetSuppliedEventHandler(EffectTargetSuppliedEventInfo info){
        effectsToTargets[info.effect] = info.target;
        if(effectsListFull(effectsToTargets)){
            castCardWithTargets();
        } 
    }

    private void castCardWithTargets(){
        // We have all the targets we need, so we can cast the card
        cardCastEvent.Raise(new CardCastEventInfo(cardCasting));
        for(int i = 0; i < cardCasting.EffectsList.Count; i++) {
            // like, we need to raise the event, but I think the only way to get it here
            // is with a reference on the class unless we use Resources.Load 
            // plz lmk if you have a better idea :/
            // WAIT I BET ITS A CUSTOM EDITOR
            // I don't know if I'll do that as a part of this commit just comment "yep" 
            // on this line of code if I'm right
            print("target: " + effectsToTargets[cardCasting.EffectsList[i]].baseStats.getId());

            cardEffectEvent.Raise(
                new CardEffectEventInfo(cardCasting.EffectsList[i].effectName, 
                getEffectScale(cardCasting.EffectsList[i], argsCasting),
                effectsToTargets[cardCasting.EffectsList[i]].baseStats.getId() // Lost the ability to target multiple things here, TODO put it back
            ));

        }
    }


    private int getEffectScale(CardEffectData effect, CardCastArguments args) {
        // Add effect increases here when we add them to CardCastArguments
        switch(effect.effectName) {
            case CardEffectName.Draw:
                return effect.scale;
            case CardEffectName.Damage:
                return effect.scale + args.damageIncrease;
            case CardEffectName.Buff:
                return effect.scale;
            default:
                return effect.scale;
        }

    }

    // not generalizable because C# can't look into the dict
    private bool effectsListFull(Dictionary<CardEffectData, CombatEntityInstance> dict){
        foreach(CardEffectData key in dict.Keys){
            if(dict[key] == null){
                return false;
            }
        }
        return true;
    }


}
