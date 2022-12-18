using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 


/* A card has a list of procedures
A procedure can be a list of effects (like draw, damage, or a status effect),
Or it can contain its own code. 
*/

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

public class GetTargetCoroutineArgs{

    public List<EntityType> validTargets;
    public EffectProcedure callbackProcedure;

    public GetTargetCoroutineArgs(List<EntityType> validTargets, EffectProcedure callbackProcedure) {
        this.validTargets = validTargets;
        this.callbackProcedure = callbackProcedure;
    }
}

[RequireComponent(typeof(EffectTargetSuppliedEventListener))]
[RequireComponent(typeof(UIStateEventListener))]
public class CardCaster : MonoBehaviour {
    // Handles casting cardInfo's  

    // A non-guid format string to designate no target
    // has been returned from a target request
    public static string NO_TARGET = "No target set";
    [SerializeField]
    private CardEffectEvent cardEffectEvent;
    // +1 to the above comment this is bad
    [SerializeField]
    private CardCastEvent cardCastEvent;

    [SerializeField]
    private EffectTargetRequestEvent effectTargetRequestEvent;

    //private Dictionary<CardEffectData, CombatEntityInstance> effectsToTargets = new Dictionary<CardEffectData, CombatEntityInstance>();
    // set to the empty string to designate no target set
    private string requestedTarget = NO_TARGET;
    private CompanionManager companionManager;
    private EnemyManager enemyManager;
    public Transform castingCardTransform;
    /// Don't rely on this being available, it's only set when a card is being cast
    private EffectProcedure currentProcedure;
    /// Don't rely on this being available, it's only set when a card is being cast
    private EffectProcedureContext currentContext;

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
        // TODO currentProcedure.cancel();
        castingCardTransform = arrowRoot;
        StartCoroutine("castingCoroutine", new CastingCoroutineArgs(info, args, arrowRoot));
        // StartCoroutine(procedure.invoke(new EffectProcedureContext(this, companionManager, enemyManager, args.casterStats)));
        
    }

    public void effectTargetSuppliedEventHandler(EffectTargetSuppliedEventInfo info){
        print("Caster got target: " + info.target.baseStats);//.getId());
        //effectsToTargets[info.effect] = info.target;
        requestedTarget = info.target.baseStats.getId();
    }

    public void uiStateEventHandler(UIStateEventInfo info) {
        if(info.newState == UIState.EFFECT_TARGETTING) {
            // no change to be applied
            return;
        }
        // If we're casting a card and the ui state changes, we need to cancel the cast
        StopCoroutine("castingCoroutine");
        //effectsToTargets.Clear();
        requestedTarget = NO_TARGET;
    }

/*
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
                // Create a copy of the target list because the event
                // just gets a reference to it
                new List<string>(targetList)
            )));
            targetList.Clear();

        }
    }
    */

    private List<string> getAllValidTargets(List<EntityType> validTargets) {
        List<string> returnList = new List<string>();
        if(validTargets.Contains(EntityType.Companion)){
            if(companionManager)
                returnList.AddRange(companionManager.getCompanionIds());
            else
                Debug.LogWarning("No companion manager in scene, couldn't cast effect that targets all companions");
        }
        if(validTargets.Contains(EntityType.Enemy)){
            if(enemyManager)
                returnList.AddRange(enemyManager.getEnemyIds());
            else
                Debug.LogWarning("No enemy manager in scene, couldn't cast effect that targets all enemies");
        }
        return returnList;

    }

    public int getEffectScale(SimpleEffectName effect, int baseScale) {
        if(currentContext == null) Debug.LogError("No current context set, can't get effect scale. Try calling the overload that provides casterStats");
        CombatEntityInEncounterStats casterStats = currentContext.stats;
        switch(effect) {
            case SimpleEffectName.Draw:
                return baseScale;
            case SimpleEffectName.Damage:
                return baseScale + casterStats.strength;
            case SimpleEffectName.Buff:
                return baseScale;
            default:
                return baseScale;
        }

    }
    public int getEffectScale(SimpleEffectName effect, int baseScale, CombatEntityInEncounterStats casterStats) {
        // Add effect increases here when we add them to CardCastArguments
        // Important to note thjat this is different from the strength of companions
        // which is handled in the EntityInEncounterStats class.
        // This math adds to the *card's* base attack damage, for example
        switch(effect) {
            case SimpleEffectName.Draw:
                return baseScale;
            case SimpleEffectName.Damage:
                return baseScale + casterStats.strength;
            case SimpleEffectName.Buff:
                return baseScale;
            default:
                return baseScale;
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
        currentContext = new EffectProcedureContext(
            this, 
            companionManager, 
            enemyManager, 
            args.castArgs.casterStats);
        print("EffectProcedures count: " + args.cardInfo.EffectProcedures.Count);
        foreach(EffectProcedure procedure in args.cardInfo.EffectProcedures){
            yield return StartCoroutine(procedure.invoke(currentContext));
        }
        yield return StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(args.cardInfo)));
    }

/*
    private IEnumerator castingCoroutine(CastingCoroutineArgs args){
        effectsToTargets = getEmptyTargetsList(args.cardInfo.EffectsList);
        foreach(CardEffectData effect in args.cardInfo.EffectsList) {
            if(effect.needsTargets) {
                print("Getting target for effect: " + effect.effectName);
                StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(
                    new EffectTargetRequestEventInfo(effect,
                    args.casterTransform)));
                yield return new WaitUntil(() => effectsToTargets[effect] != null);
            }
        }
        castCardWithTargets(args.cardInfo, args.castArgs, effectsToTargets);
    }
    */

    // Have to call this here to start it as a coroutine
    public void raiseSimpleEffect(SimpleEffectName effectName, int scale, List<string> targets){
        StartCoroutine(cardEffectEvent.RaiseAtEndOfFrameCoroutine(
            new CardEffectEventInfo(effectName, scale, targets)));
    }

    public void requestTarget(List<EntityType> validTargets, EffectProcedure procedure){
        StartCoroutine("getTargetCoroutine", new GetTargetCoroutineArgs(validTargets, procedure));
    }

    private IEnumerator getTargetCoroutine(GetTargetCoroutineArgs args) {
        requestedTarget = NO_TARGET;
        StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(
                new EffectTargetRequestEventInfo(args.validTargets, castingCardTransform)));
        yield return new WaitUntil(() => !requestedTarget.Equals(NO_TARGET));
        Debug.Log("Get target coroutine providing target: " + requestedTarget);
        args.callbackProcedure.targetsSupplied(new List<string>() { requestedTarget });
    }


}
