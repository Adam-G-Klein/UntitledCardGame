using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 

[RequireComponent(typeof(CardDisplay))]
[RequireComponent(typeof(CardEffectEventListener))]
[RequireComponent(typeof(UIStateEventListener))]
public class PlayableCard : TargettableEntity
    , IPointerClickHandler 
    , IDragHandler
    , IPointerEnterHandler
    , IPointerExitHandler
{
    public Card card;
    private CombatEntityWithDeckInstance entityFrom;

    [SerializeField]
    private float hoverScale = 1.5f;
    [SerializeField]
    private float nonHoverScale = 1f;
    [SerializeField]
    private CombatEffectEvent combatEffectEvent;
    [SerializeField]
    private CardCastEvent cardCastEvent;
    public bool hovered = false;

    private UIState currentState;
    private IEnumerator currentCastingProcedure;
    private IEnumerator currentEffectProcedure;
    private EffectProcedureContext currentContext;

    // Checked by PlayerHand when discarding the whole hand
    // set back to false there when it's checked
    public bool retained = false;

    protected override void Start()
    {
        base.Start();
        card = GetComponent<CardDisplay>().cardInfo;
        // IMPORTANT, will end up with duplicate IDs if we ever 
        // forget to do this on an Entity
        id = card.id;
        entityType = EntityType.PlayableCard;
    }

    public override void onPointerClickChildImpl(PointerEventData eventData) 
    {
        // TargettableEntity Onclick impl handles anything outside of the player clicking on the card to cast it
        // (like if we're about to discard it)
        if (currentState != UIState.DEFAULT) return; 

        CardCastArguments args = new CardCastArguments(entityFrom, this);
        // Cast event handler in PlayerHand.cs will handle the card 
        // being removed from the hand
        // caster.cardClickHandler(card, args, this);
        castCard();
    }

    private void castCard() {
        // Check for valid cast
        if (card.cost > ManaManager.Instance.currentMana 
                || !card.cardType.playable) {
            // Theoretically we'd have some kind of indicator
            // to the player that they can't cast this
            return;
        }

        resetCastingState();
        CardCastArguments args = new CardCastArguments(entityFrom, this);
        currentCastingProcedure = castingCoroutine(card, args);
        StartCoroutine(currentCastingProcedure);
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
        ManaManager.Instance.updateMana(-card.cost);
        yield return StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(card)));
        // instantiate card's VFX prefab
        // PrefabInstantiator.InstantiatePrefab(card.vfxPrefab, args.caster.transform.position, Quaternion.identity);
    }

    private void resetCastingState() {
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

    public override bool isTargetableByChildImpl(EffectTargetRequestEventInfo eventInfo)
    {
        // parent function checks that this is targetting us, currently just making sure 
        // that discard effects can't target the card that's discarding
        // if we want a card to be able to target itself, we'll need to change this and 
        // somehow check the effect name during the target request
        bool retval = eventInfo.source.id != id;
        return retval;
    }

    public override void uiStageChangeEventHandlerChildImpl(UIStateEventInfo eventInfo) {
        currentState = eventInfo.newState;
    }

    public void cardEffectEventHandler(CardEffectEventInfo info) {
        if (!info.targets.Contains(this)) return;
        applyCardEffects(info.cardEffects);
    }

    private void applyCardEffects(Dictionary<CardEffect, int> effects) {
        foreach (KeyValuePair<CardEffect, int> effect in effects) {
            applyCardEffect(effect.Key, effect.Value);
        }
    }
    private void applyCardEffect(CardEffect effect, int value) {
        switch(effect) {
            case CardEffect.Discard:
                PlayerHand.Instance.discardCard(this);
                break;
        }
    }

    // Called by playerHand.discardCard
    public void discardFromDeck() {
        entityFrom.inCombatDeck.discardCards(new List<Card>{card});
    }

    // Keeping these here for reference as they will almost certainly
    // be needed for UI effects in the future
    public void OnDrag(PointerEventData eventData) { }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        transform.localScale = new Vector3(hoverScale, hoverScale, 1);
        PlayerHand.Instance.updateLayout();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!hovered) return;
        hovered = false;
        transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
        PlayerHand.Instance.updateLayout();
    }

    // Used when instantiating the card after Start has run
    // See PrefabInstantiator.cs
    public void setCardInfo(Card card){
        this.card = card;
    }
    
    // Should pass by reference so that the values stay updated
    public void setEntityFrom(CombatEntityWithDeckInstance entityFrom) {
        this.entityFrom = entityFrom;
    }

}
