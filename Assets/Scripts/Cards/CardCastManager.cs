using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 


public class CastingCoroutineArgs{
    public Card cardInfo;
    public CardCastArguments castArgs;

    public CastingCoroutineArgs(Card cardInfo, CardCastArguments args){
        this.cardInfo = cardInfo;
        this.castArgs = args;
    }
}

public class GetTargetCoroutineArgs{

    public List<EntityType> validTargets;
    public EffectProcedure callbackProcedure;
    public List<TargettableEntity> disallowedTargets;

    public GetTargetCoroutineArgs(List<EntityType> validTargets, EffectProcedure callbackProcedure, List<TargettableEntity> disallowedTargets = null){
        this.validTargets = validTargets;
        this.callbackProcedure = callbackProcedure;
        this.disallowedTargets = disallowedTargets;
    }
}

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
    // set to the empty string to designate no target set
    private CompanionManager companionManager;
    private EnemyManager enemyManager;
    private CardSelectionManager cardSelectionManager;
    /// Don't rely on this being available, it's only set when a card is being cast
    private IEnumerator currentCastingProcedure;
    private IEnumerator currentEffectProcedure;
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
        
        GameObject cardSelectionManagerGO = GameObject.Find("CardSelectionManager");
        if(!cardSelectionManagerGO) Debug.LogWarning("Card caster couldn't find card selection manager, won't be able to cast effects that require selecting cards from a group of cards displayed through the cardViewUI overlay. Can add this manager by adding the EnemyEncounterCanvas prefab");
        else cardSelectionManager = cardSelectionManagerGO.GetComponent<CardSelectionManager>();
    }

    
    public bool isValidCast(Card info, CardCastArguments args) {
        if(info.cost > manaManager.currentMana) return false;
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
            if(companionManager)
                returnList.AddRange(companionManager.getCompanions());
            else
                Debug.LogWarning("No companion manager in scene, couldn't cast effect that targets all companions");
        }
        if(validTargets.Contains(EntityType.Enemy)){
            if(enemyManager)
                returnList.AddRange(enemyManager.getEnemies());
            else
                Debug.LogWarning("No enemy manager in scene, couldn't cast effect that targets all enemies");
        }
        if(validTargets.Contains(EntityType.Minion)){
            if(companionManager)
                returnList.AddRange(companionManager.getMinions());
            else
                Debug.LogWarning("No player hand in scene, couldn't cast effect that targets the player");
        }
        if(validTargets.Contains(EntityType.Card)){
            if(playerHand)
                returnList.AddRange(playerHand.cardsInHand);
            else
                Debug.LogWarning("No player hand in scene, couldn't cast effect that targets the player");
        }
        return returnList;

    }


    private IEnumerator castingCoroutine(Card card, CardCastArguments args) {
        List<TargettableEntity> alreadyTargetted = new List<TargettableEntity>();
        currentContext = new EffectProcedureContext(
            this, 
            companionManager, 
            enemyManager, 
            args.caster,
            playerHand,
            alreadyTargetted,
            combatEffectEvent,
            cardSelectionManager);
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
    }

    public void raiseCombatEffect(CombatEffectEventInfo info){
        StartCoroutine(combatEffectEvent.RaiseAtEndOfFrameCoroutine(info));
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
