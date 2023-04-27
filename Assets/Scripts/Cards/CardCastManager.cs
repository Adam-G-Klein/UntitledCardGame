using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 


[RequireComponent(typeof(UIStateEventListener))]
public class CardCastManager : TargetProvider {
    // Handles casting cardInfo's  

    [SerializeField]
    private CombatEffectEvent combatEffectEvent;
    [SerializeField]
    private CardEffectEvent cardEffectEvent;
    [SerializeField]
    private CardCastEvent cardCastEvent;
    [SerializeField]
    private IntGameEvent manaChangeEvent;

    //private Dictionary<CardEffectData, CombatEntityInstance> effectsToTargets = new Dictionary<CardEffectData, CombatEntityInstance>();

    /// Don't rely on this being available, it's only set when a card is being cast
    private IEnumerator currentCastingProcedure;
    private IEnumerator currentEffectProcedure;
    /// Don't rely on this being available outside of effectProcedure invokations, 
    /// it's only set when a card is being cast
    private EffectProcedureContext currentContext;
    // There's gotta be a better place for this,
    // but this is where I'm putting it for now
    // Tough to decide where an authoritative figure like this should go
    // But I figure here - where it's being checked - is probably a decent place?

    void Start() {
    }

    
    public bool isValidCast(Card info, CardCastArguments args) {
        if(info.cost > ManaManager.Instance.currentMana || !info.cardType.playable) return false;
        // theoretically we could check for other things here
        return true;
    }
    public void cardClickHandler(Card info, CardCastArguments args, Entity callingCard) {
        if(isValidCast(info, args)) {
            resetCastingState();
            targetRequestingEntity = callingCard;
            currentCastingProcedure = castingCoroutine(info, args);
            StartCoroutine(currentCastingProcedure);
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

    public List<TargettableEntity> getAllValidTargets(List<EntityType> validTargets) {
        List<TargettableEntity> returnList = new List<TargettableEntity>();
        if(validTargets.Contains(EntityType.Companion)){
            returnList.AddRange(CombatEntityManager.Instance.getCompanions());
        }
        if(validTargets.Contains(EntityType.Enemy)){
            returnList.AddRange(CombatEntityManager.Instance.getEnemies());
        }
        if(validTargets.Contains(EntityType.Minion)){
            returnList.AddRange(CombatEntityManager.Instance.getEnemies());
        }
        if(validTargets.Contains(EntityType.PlayableCard)){
            returnList.AddRange(PlayerHand.Instance.cardsInHand);
        }
        return returnList;
    }


    private IEnumerator castingCoroutine(Card card, CardCastArguments args) {
        // tracking who's already been targetted for situations like discarding multiple
        // cards to a discard effect. Tech designer can select whether they want unique targets
        // in a card type using a boolean in the ui
        List<TargettableEntity> alreadyTargetted = new List<TargettableEntity>();
        // an area for improvement, shouldn't be passing object references for direct function
        // calls, should be using events instead
        currentContext = new EffectProcedureContext(
            args.caster,
            alreadyTargetted,
            combatEffectEvent,
            card,
            args.origin);
        foreach(EffectProcedure procedure in card.effectProcedures){
            // Track current procedure for casting cancellation
            currentEffectProcedure = procedure.prepare(currentContext);
            yield return StartCoroutine(currentEffectProcedure);
        }
        foreach(EffectProcedure procedure in card.effectProcedures){
            // Track current procedure for casting cancellation
            currentEffectProcedure = procedure.invoke(currentContext);
            yield return StartCoroutine(currentEffectProcedure);
        }
        yield return StartCoroutine(manaChangeEvent.RaiseAtEndOfFrameCoroutine( -card.cost));
        yield return StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(card)));
        // instantiate card's VFX prefab
        // PrefabInstantiator.InstantiatePrefab(card.vfxPrefab, args.caster.transform.position, Quaternion.identity);
    }

    public void raiseCombatEffect(CombatEffectEventInfo info){
        StartCoroutine(combatEffectEvent.RaiseAtEndOfFrameCoroutine(info));
    }

    public void raiseCombatEffect(CombatEffect effect, int scale, List<TargettableEntity> targets, CombatEntityInstance caster) {
        raiseCombatEffect(
            new CombatEffectEventInfo(
                new Dictionary<CombatEffect, int> {
                    {effect, caster.stats.getEffectScale(effect, scale)}
                },
                targets,
                caster
            )
        );
    }

    public void raiseCardEffect(CardEffectEventInfo info){
        StartCoroutine(cardEffectEvent.RaiseAtEndOfFrameCoroutine(info));
    }

    public void raiseIntEvent(IntGameEvent gameEvent, int value){
        StartCoroutine(gameEvent.RaiseAtEndOfFrameCoroutine(value));
    }


    private void resetCastingState(){
        resetTargettingState();
        if(currentCastingProcedure != null) {
            StopCoroutine(currentCastingProcedure);
            currentCastingProcedure = null;
        }
        if(currentEffectProcedure != null) {
            StopCoroutine(currentEffectProcedure);
            currentEffectProcedure = null;
            // Shouldn't be relying on this existing anyways, but just in case
            currentContext = null; 
        }
    }   


}
