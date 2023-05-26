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
    public CombatEntityWithDeckInstance entityFrom;

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

        if (card.cost > ManaManager.Instance.currentMana 
                || !card.cardType.playable) {
            // Theoretically we'd have some kind of indicator
            // to the player that they can't cast this
            return;
        }

        EffectDocument document = new EffectDocument();
        document.playableCardMap.addItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.PlayableCard;
        EffectManager.Instance.invokeEffectWorkflow(document, card.cardType.effectSteps, cardFinishCastingCallback);
    }

    private void cardFinishCastingCallback() {
        ManaManager.Instance.updateMana(-card.cost);
        StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(card)));
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

    private void applyCardEffects(Dictionary<CardEffectType, int> effects) {
        foreach (KeyValuePair<CardEffectType, int> effect in effects) {
            applyCardEffect(effect.Key, effect.Value);
        }
    }
    public void applyCardEffect(CardEffectType effect, int value) {
        switch(effect) {
            case CardEffectType.Discard:
                PlayerHand.Instance.discardCard(this);
                break;
        }
    }

    public void discardCardFromHand() {
        PlayerHand.Instance.discardCard(this);
    }

    public void exhaustCard() {
        entityFrom.inCombatDeck.exhaustCard(card);
        Destroy(gameObject);
    }

    // Called by playerHand.discardCard
    public void discardFromDeck() {
        entityFrom.inCombatDeck.discardCards(new List<Card> { card });
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
