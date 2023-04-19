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
    private PlayerHand playerHand;
    private CombatEntityWithDeckInstance entityFrom;

    [SerializeField]
    private float hoverScale = 1.5f;
    [SerializeField]
    private float nonHoverScale = 1f;
    [SerializeField]
    public float hoverYDiff = 185f;
    private int preHoverSiblingIndex;
    // Start is called before the first frame update
    public bool hovered = false;

    private CardCastManager caster;

    private UIState currentState;

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
        GameObject companionManagerGO = GameObject.Find("CompanionManager");
        GameObject cardCasterGO = GameObject.Find("CardCaster");
        GameObject playerHandGO = GameObject.Find("PlayerHand");
        // My attempt at null safing. We should def talk about how we want to 
        // do this generally because it'll happen a lot with the modular scenes
        if(cardCasterGO) caster = cardCasterGO.GetComponent<CardCastManager>();
        else Debug.LogError("CardCaster not found by card, won't be able to cast cards");
        if(playerHandGO) playerHand = playerHandGO.GetComponent<PlayerHand>();
        else Debug.LogError("PlayerHand not found by card, won't be able to discard cards");
    }

    public override void onPointerClickChildImpl(PointerEventData eventData) 
    {
        // TargettableEntity Onclick impl handles anything outside of the player clicking on the card to cast it
        // (like if we're about to discard it)
        if (currentState != UIState.DEFAULT) return; 

        CardCastArguments args = new CardCastArguments(entityFrom);
        // Cast event handler in PlayerHand.cs will handle the card 
        // being removed from the hand
        caster.cardClickHandler(card, args, this);
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

    public override void uiStageChangeEventHandlerChildImpl(UIStateEventInfo eventInfo)
    {
        currentState = eventInfo.newState;
    }

    public void cardEffectEventHandler(CardEffectEventInfo info){
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
                playerHand.discardCard(this);
                break;
        }
    }

    // Called by playerHand.discardCard
    public void discardFromDeck() {
        entityFrom.inCombatDeck.discardCards(new List<Card>{card});
    }


    // Keeping these here for reference as they will almost certainly
    // be needed for UI effects in the future
    public void OnDrag(PointerEventData eventData)
    {
    }

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
