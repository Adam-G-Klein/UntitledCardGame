using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UIDocumentCard))]
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
    public AudioClip cardHover;
    public float hoverSFXVolume = 0.1f;

    private UIDocumentCard docCard;

    public void Start()
    {
        docCard = GetComponent<UIDocumentCard>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Card clicked");
        if (currentState != UIState.DEFAULT) return;

        if (card.GetManaCost() > ManaManager.Instance.currentMana
                || !card.cardType.playable) {
            // Theoretically we'd have some kind of indicator
            // to the player that they can't cast this
            return;
        }

        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Card;
        EffectManager.Instance.invokeEffectWorkflow(document, card.effectSteps, CardFinishCastingCallback());
    }

    private IEnumerator CardFinishCastingCallback() {
        ManaManager.Instance.updateMana(-card.GetManaCost());
        StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(card)));
        IncrementCastCount();
        EnemyEncounterManager.Instance.combatEncounterState.cardsCastThisTurn.Add(card);
        yield return StartCoroutine(PlayerHand.Instance.OnCardCast(this));
        if (card.cardType.exhaustsWhenPlayed) {
            ExhaustCard();
        } else {
            DiscardCardFromHand();
        }
    }

    private void IncrementCastCount(){
        card.castCount += 1;
    }

    public void DiscardCardFromHand() {
        // PlayerHand calls discard from deck when done discarding from hand
        PlayerHand.Instance.DiscardCard(this);
    }

    public void ExhaustCard() {
        deckFrom.ExhaustCard(card);
        cleanupAndDestroy();
    }

    // Called by playerHand.discardCard
    public void DiscardToDeck() {
        deckFrom.DiscardCards(new List<Card> { card });
        cleanupAndDestroy();
    }

    public void cleanupAndDestroy() {
        docCard.Cleanup(() => {
            Destroy(this.gameObject);
        });
    }

    // Keeping these here for reference as they will almost certainly
    // be needed for UI effects in the future
    public void OnDrag(PointerEventData eventData) { }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        MusicController.Instance.PlaySFX(cardHover, hoverSFXVolume);
        transform.localScale = new Vector3(hoverScale, hoverScale, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!hovered) return;
        hovered = false;
        transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
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
