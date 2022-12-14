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

[RequireComponent(typeof(EffectTargetSuppliedEventListener))]
[RequireComponent(typeof(UIStateEventListener))]
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
    private CompanionManager companionManager;
    private EnemyManager enemyManager;
    public Transform defaultArrowRoot = null;

    void Start() {
        // This may cause me to consider combining these into an EntityInstanceManager
        GameObject companionManagerGO = GameObject.Find("CompanionManager");
        if(!companionManagerGO) Debug.LogWarning("Card caster couldn't find companion manager, won't be able to cast effects that target all companions");
        else companionManager = companionManagerGO.GetComponent<CompanionManager>();

        GameObject enemyManagerGO = GameObject.Find("EnemyManager");
        if(!enemyManagerGO) Debug.LogWarning("Card caster couldn't find enemy manager, won't be able to cast effects that target all enemies");
        else enemyManager = enemyManagerGO.GetComponent<EnemyManager>();



    }
    public void newCastRequest(CardInfo info, CardCastArguments args, Transform arrowRoot) {
        StopCoroutine("castingCoroutine");
        StartCoroutine("castingCoroutine", new CastingCoroutineArgs(info, args, arrowRoot));
        
    }

    public void effectTargetSuppliedEventHandler(EffectTargetSuppliedEventInfo info){
        print("Got target for effect: " + info.effect.effectName + " target was: " + info.target.baseStats);//.getId());
        effectsToTargets[info.effect] = info.target;
    }

    public void uiStateEventHandler(UIStateEventInfo info) {
        if(info.newState == UIState.EFFECT_TARGETTING) {
            // no change to be applied
            return;
        }
        // If we're casting a card and the ui state changes, we need to cancel the cast
        StopCoroutine("castingCoroutine");
        effectsToTargets.Clear();
    }

    private void castCardWithTargets(CardInfo info, CardCastArguments args, Dictionary<CardEffectData, CombatEntityInstance> targets){
        // We have all the targets we need, so we can cast the card
        StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(info)));
        List<string> targetList = new List<string>();
        foreach(CardEffectData effect in info.EffectsList) {
            if(effect.needsTargets) {
                print("casting effect: " + effect.effectName + " with target: " + effectsToTargets[effect].baseStats.getId());
                targetList.Add(effectsToTargets[effect].baseStats.getId());
            } else {
                print("casting effect: " + effect.effectName + " targetting all valid targets");
                targetList = getAllValidTargets(effect);
            }

            StartCoroutine(cardEffectEvent.RaiseAtEndOfFrameCoroutine(
                new CardEffectEventInfo(effect.effectName, 
                getEffectScale(effect, args),
                targetList
            )));

        }
    }

    private List<string> getAllValidTargets(CardEffectData effect) {
        List<string> returnList = new List<string>();
        if(effect.validTargets.Contains(EntityType.Companion)){
            if(companionManager)
                returnList.AddRange(companionManager.getCompanionIds());
            else
                Debug.LogWarning("No companion manager in scene, couldn't cast effect that targets all companions");
        }
        if(effect.validTargets.Contains(EntityType.Enemy)){
            if(enemyManager)
                returnList.AddRange(enemyManager.getEnemyIds());
            else
                Debug.LogWarning("No enemy manager in scene, couldn't cast effect that targets all enemies");
        }
        return returnList;

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
            if(effect.needsTargets) {
                print("Getting target for effect: " + effect.effectName);
                StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(
                    new EffectTargetRequestEventInfo(effect,
                    args.castArgs.casterId, args.casterTransform)));
                yield return new WaitUntil(() => effectsToTargets[effect] != null);
            }
        }
        castCardWithTargets(args.cardInfo, args.castArgs, effectsToTargets);
    }


}
