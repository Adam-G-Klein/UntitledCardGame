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
    public Card outOfCombatCard;
    private PlayerHand playerHand;
    private CombatEntityInEncounterStats companionFromStats;
    private InCombatDeck deckFrom; 

    [SerializeField]
    private float hoverScale = 30f;
    [SerializeField]
    private float nonHoverScale = 20f;
    [SerializeField]
    public float hoverYDiff = 185f;
    private int preHoverSiblingIndex;
    // Start is called before the first frame update
    public bool hovered = false;

    private CardCaster caster;

    private UIState currentState;

    void Start()
    {
        outOfCombatCard = GetComponent<CardDisplay>().cardInfo;
        // IMPORTANT, will end up with duplicate IDs if we ever 
        // forget to do this on an Entity
        id = outOfCombatCard.id;
        entityType = EntityType.Card;
        GameObject companionManagerGO = GameObject.Find("CompanionManager");
        GameObject cardCasterGO = GameObject.Find("CardCaster");
        GameObject playerHandGO = GameObject.Find("PlayerHand");
        // My attempt at null safing. We should def talk about how we want to 
        // do this generally because it'll happen a lot with the modular scenes
        if(cardCasterGO) caster = cardCasterGO.GetComponent<CardCaster>();
        else Debug.LogError("CardCaster not found by card, won't be able to cast cards");
        if(playerHandGO) playerHand = playerHandGO.GetComponent<PlayerHand>();
        else Debug.LogError("PlayerHand not found by card, won't be able to discard cards");
    }

    public override void onPointerClickChildImpl(PointerEventData eventData) 
    {
        // TargettableEntity Onclick impl handles anything outside of the player clicking on the card to cast it
        // (like if we're about to discard it)
        if (currentState != UIState.DEFAULT) return; 

        CardCastArguments args = new CardCastArguments(companionFromStats);
        // Cast event handler in PlayerHand.cs will handle the card 
        // being removed from the hand
        caster.cardClickHandler(outOfCombatCard, args, this);
    }

    public override bool isTargetableByChildImpl(EffectTargetRequestEventInfo eventInfo)
    {
        // parent function checks that this is targetting us, currently just making sure 
        // that discard effects can't target the card that's discarding
        // if we want a card to be able to target itself, we'll need to change this and 
        // somehow check the effect name during the target request
        return eventInfo.source.id != id;
    }

    public void uiStageChangeEventHandler(UIStateEventInfo info) {
        currentState = info.newState;
    }

    public void cardEffectEventHandler(CardEffectEventInfo info){
        if (!info.targets.Contains(id)) return;
        switch(info.effectName){
            case SimpleEffectName.Discard:
                playerHand.discardCard(this);
                break;
        }
    }

    // Called by playerHand when the card is discarded from the hand
    public void discardFromDeck() {
        deckFrom.discardCards(new List<Card>{outOfCombatCard});
    }


    // Keeping these here for reference as they will almost certainly
    // be needed for UI effects in the future
    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        preHoverSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
        transform.localScale = new Vector3(hoverScale, hoverScale, 1);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + hoverYDiff, transform.localPosition.z);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if(!hovered) return;
        hovered = false;
        transform.SetSiblingIndex(preHoverSiblingIndex);
        transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - hoverYDiff, transform.localPosition.z);
    }

    

    // Used when instantiating the card after Start has run
    // See PrefabInstantiator.cs
    public void setCardInfo(Card card){
        this.outOfCombatCard = card;
    }
    
    // Should pass by reference so that the values stay updated
    public void setCompanionFrom(CombatEntityInEncounterStats companionStats){
        this.companionFromStats = companionStats;
    }

    public void setDeckFrom(InCombatDeck deck){
        this.deckFrom = deck;
    }

}
