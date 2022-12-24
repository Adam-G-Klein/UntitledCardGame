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
    private IEnumerator currentProcedure;
    private PlayerHand playerHand;
    private ManaManager manaManager;
    /// Don't rely on this being available outside of effectProcedure invokations, 
    /// it's only set when a card is being cast
    private EffectProcedureContext currentContext;
    // There's gotta be a better place for this,
    // but this is where I'm putting it for now
    // Tough to decide where an authoritative figure like this should go
    // But I figure here - where it's being checked - is probably a decent place?

    void Start() {
        // This may cause me to consider combining these into an EntityInstanceManager
        GameObject companionManagerGO = GameObject.Find("CompanionManager");
        if(!companionManagerGO) Debug.LogWarning("Card caster couldn't find companion manager, won't be able to cast effects that target all companions");
        else companionManager = companionManagerGO.GetComponent<CompanionManager>();

        GameObject enemyManagerGO = GameObject.Find("EnemyManager");
        if(!enemyManagerGO) Debug.LogWarning("Card caster couldn't find enemy manager, won't be able to cast effects that target all enemies");
        else enemyManager = enemyManagerGO.GetComponent<EnemyManager>();

        GameObject playerHandGO = GameObject.Find("PlayerHand");
        if(!playerHandGO) Debug.LogWarning("Card caster couldn't find player hand, won't be able to cast effects interacting with the player's hand (card counting, discard effects). Can add this manager by adding the EnemyEncounterCanvas prefab");
        else playerHand = playerHandGO.GetComponent<PlayerHand>();

        GameObject manaManagerGO = GameObject.Find("ManaManager");
        if(!manaManagerGO) Debug.LogWarning("Card caster couldn't find mana manager, won't be checking card costs before casting. Can add this manager by adding the EnemyEncounterCanvas prefab");
        else manaManager = manaManagerGO.GetComponent<ManaManager>();
    }

    
    public bool isValidCast(CardInfo info, CardCastArguments args) {
        if(info.Cost > manaManager.currentMana) return false;
        return true;
    }
    public void cardClickHandler(CardInfo info, CardCastArguments args, Transform arrowRoot) {
        if(isValidCast(info, args)) {
            resetCastingState();
            castingCardTransform = arrowRoot;
            StartCoroutine("castingCoroutine", new CastingCoroutineArgs(info, args, arrowRoot));
            // StartCoroutine(procedure.invoke(new EffectProcedureContext(this, companionManager, enemyManager, args.casterStats)));
        } else {
            Debug.Log("Not casting card because we have insufficient mana");
        }
    }


    public void uiStateEventHandler(UIStateEventInfo info) {
        if(info.newState == UIState.EFFECT_TARGETTING) {
            // no change to be applied
            return;
        }
        // If we're casting a card and the ui state changes, we need to cancel the cast
        // if we want to make casts uninterruptable, we'll need to check for a flag 
        // on the caster from the UIStateManager before we change the state. If the 
        // state is changed here, it means we're good to cancel.
        resetCastingState();
    }

    public List<string> getAllValidTargets(List<EntityType> validTargets) {
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
        CombatEntityInEncounterStats casterStats = currentContext.casterStats;
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

    private IEnumerator castingCoroutine(CastingCoroutineArgs args){
        currentContext = new EffectProcedureContext(
            this, 
            companionManager, 
            enemyManager, 
            args.castArgs.casterStats,
            playerHand);
        foreach(EffectProcedure procedure in args.cardInfo.EffectProcedures){
            // Track current procedure for casting cancellation
            currentProcedure = procedure.invoke(currentContext);
            yield return StartCoroutine(currentProcedure);
        }
        yield return StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(args.cardInfo)));
    }

    // Have to call this here to start the effect as a coroutine
    public void raiseSimpleEffect(SimpleEffectName effectName, int scale, List<string> targets){
        StartCoroutine(cardEffectEvent.RaiseAtEndOfFrameCoroutine(
            new CardEffectEventInfo(effectName, scale, targets)));
    }

    public void requestTarget(List<EntityType> validTargets, EffectProcedure procedure){
        StartCoroutine("getTargetCoroutine", new GetTargetCoroutineArgs(validTargets, procedure));
    }

    public void effectTargetSuppliedEventHandler(EffectTargetSuppliedEventInfo info){
        print("Caster got target: " + info.target.id);
        requestedTarget = info.target.id;
    }

    private IEnumerator getTargetCoroutine(GetTargetCoroutineArgs args) {
        requestedTarget = NO_TARGET;
        StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(
                new EffectTargetRequestEventInfo(args.validTargets, castingCardTransform)));
        // Waits until the effectTargetSuppliedHandler is called
        yield return new WaitUntil(() => !requestedTarget.Equals(NO_TARGET));
        Debug.Log("Get target coroutine providing target: " + requestedTarget);
        args.callbackProcedure.targetsSupplied(new List<string>() { requestedTarget });
    }

    private void resetCastingState(){
        requestedTarget = NO_TARGET;
        StopCoroutine("castingCoroutine");
        if(currentProcedure != null) {
            StopCoroutine(currentProcedure);
            currentProcedure = null;
            // Shouldn't be relying on this existing anyways, but just in case
            currentContext = null; 
        }
    }


}
