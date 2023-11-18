using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 

[RequireComponent(typeof(CardDisplay))]
[RequireComponent(typeof(UIStateEventListener))]
[RequireComponent(typeof(Targetable))]
public class PlayableCard : MonoBehaviour,
    IPointerClickHandler,
    IDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Card card;
    public DeckInstance deckFrom;

    [SerializeField]
    private float hoverScale = 1.5f;
    [SerializeField]
    private float nonHoverScale = 1f;
    [SerializeField]
    private CardCastEvent cardCastEvent;
    public bool hovered = false;

    private UIState currentState;

    // Checked by PlayerHand when discarding the whole hand
    // set back to false there when it's checked
    public bool retained = false;

    public void Start()
    {
        card = GetComponent<CardDisplay>().cardInfo;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentState != UIState.DEFAULT) return; 

        if (card.cost > ManaManager.Instance.currentMana 
                || !card.cardType.playable) {
            // Theoretically we'd have some kind of indicator
            // to the player that they can't cast this
            return;
        }

        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Card;
        EffectManager.Instance.invokeEffectWorkflow(document, card.effectSteps, CardFinishCastingCallback);
    }

    private void CardFinishCastingCallback() {
        ManaManager.Instance.updateMana(-card.cost);
        StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(card)));
        DiscardCardFromHand();
    }

    public void DiscardCardFromHand() {
        PlayerHand.Instance.DiscardCard(this);
    }

    public void ExhaustCard() {
        deckFrom.ExhaustCard(card);
        Destroy(gameObject);
    }

    // Called by playerHand.discardCard
    public void DiscardFromDeck() {
        deckFrom.DiscardCards(new List<Card> { card });
    }

    // Keeping these here for reference as they will almost certainly
    // be needed for UI effects in the future
    public void OnDrag(PointerEventData eventData) { }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        transform.localScale = new Vector3(hoverScale, hoverScale, 1);
        PlayerHand.Instance.UpdateLayout();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!hovered) return;
        hovered = false;
        transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
        PlayerHand.Instance.UpdateLayout();
    }

    // Used when instantiating the card after Start has run
    // See PrefabInstantiator.cs
    public void SetCardInfo(Card card){
        this.card = card;
    }
    
    // Should pass by reference so that the values stay updated
    public void SetDeckFrom(DeckInstance deckFrom) {
        this.deckFrom = deckFrom;
    }

    public void uiStateEventHandler(UIStateEventInfo eventInfo) {
        currentState = eventInfo.newState;
    }
}
