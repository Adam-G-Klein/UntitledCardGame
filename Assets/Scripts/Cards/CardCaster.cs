using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 

public class CastingCoroutineArgs{
    public CardInfo cardInfo;
    public CardCastArguments castArgs;
    public Transform casterTransform;

    public CastingCoroutineArgs(CardInfo cardInfo, CardCastArguments args, Transform casterTransform){
        this.cardInfo = cardInfo;
        this.castArgs = args;
        this.casterTransform = casterTransform;
    }
}

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

    public void newCastRequest(CardInfo info, CardCastArguments args, Transform arrowRoot) {
        // This could be a lot better
        // this class is just about the opposite of 
        // idempotent, I think a lot more events will be necessary to change this
        // We could do this all with coroutines to maintain state too but
        // I worry that have a bunch of threads in flight will only complicate things
        // At least everything is synchronous here
        StartCoroutine("castingCoroutine", new CastingCoroutineArgs(info, args, arrowRoot));
        
    }

    private void populateTargetsList(CardInfo info, CardCastArguments args, Transform arrowRoot){
        // The effectTargetSuppliedEventHandler will count the targets that have been supplied
        // and we'll cast with the final targets list 
        foreach(CardEffectData effect in info.EffectsList) {
            // A somewhat hackey way of knowing when we have all the targets we need
            effectsToTargets.Add(effect, null);
        }
        print("raising effectTargetRequestEvent");
        StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(
            new EffectTargetRequestEventInfo(info.EffectsList[0],
            args.casterId, arrowRoot)));
    }

    public void effectTargetSuppliedEventHandler(EffectTargetSuppliedEventInfo info){
        print("Got target for effect: " + info.effect.effectName + " target was: " + info.target.baseStats);//.getId());
        effectsToTargets[info.effect] = info.target;
        /*
        if(effectsListFull(effectsToTargets)){
            castCardWithTargets();
        } 
        */
    }

    private void castCardWithTargets(CardInfo info, CardCastArguments args, Dictionary<CardEffectData, CombatEntityInstance> targets){
        // We have all the targets we need, so we can cast the card
        StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(info)));
        for(int i = 0; i < info.EffectsList.Count; i++) {
            print("target: " + effectsToTargets[info.EffectsList[i]].baseStats.getId());

            StartCoroutine(cardEffectEvent.RaiseAtEndOfFrameCoroutine(
                new CardEffectEventInfo(info.EffectsList[i].effectName, 
                getEffectScale(info.EffectsList[i], args),
                effectsToTargets[info.EffectsList[i]].baseStats.getId() // Lost the ability to target multiple things here, TODO put it back
            )));

        }
    }


    private int getEffectScale(CardEffectData effect, CardCastArguments args) {
        // Add effect increases here when we add them to CardCastArguments
        // Important to note thjat this is different from the strength of companions
        // which is handled in the EntityInEncounterStats class.
        // This math adds to the *card's* base attack damage, for example
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

    private Dictionary<CardEffectData, CombatEntityInstance> getEmptyTargetsList(List<CardEffectData> effectsList){
        Dictionary<CardEffectData, CombatEntityInstance> dict = new Dictionary<CardEffectData, CombatEntityInstance>();
        foreach(CardEffectData effect in effectsList){
            dict.Add(effect, null);
        }
        return dict;
    }

    private IEnumerator castingCoroutine(CastingCoroutineArgs args){
        effectsToTargets = getEmptyTargetsList(args.cardInfo.EffectsList);
        foreach(CardEffectData effect in args.cardInfo.EffectsList) {
            StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(
                new EffectTargetRequestEventInfo(effect,
                args.castArgs.casterId, args.casterTransform)));
            yield return new WaitUntil(() => effectsToTargets[effect] != null);
            print("continuing casting coroutine");
        }
        castCardWithTargets(args.cardInfo, args.castArgs, effectsToTargets);
    }


}
