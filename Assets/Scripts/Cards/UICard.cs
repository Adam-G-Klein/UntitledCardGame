using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CardListEventListener))]
public class UICard : TargettableEntity {
    public Card card;
    // hot take having these be separate events,
    // but it allows us to reuse the event *shrug*
    [SerializeField]
    private CardListEvent cardSelectedEvent;
    [SerializeField]
    private CardListEvent cardDeselectedEvent;
    public GameObject selectionFrame;
    public bool selected = false;

    void Start() {
        card = GetComponent<CardDisplay>().cardInfo;
    }

    public override void onPointerClickChildImpl(PointerEventData eventData)
    {
        if(selected) {
            Debug.Log("Deselecting card " + card.name);
            StartCoroutine(cardDeselectedEvent.RaiseAtEndOfFrameCoroutine(new CardListEventInfo(new List<Card> {card})));
            selected = false;
            selectionFrame.SetActive(false);
        } else {
            Debug.Log("selecting card " + card.name);
            StartCoroutine(cardSelectedEvent.RaiseAtEndOfFrameCoroutine(new CardListEventInfo(new List<Card> {card})));
            selected = true;
            selectionFrame.SetActive(true);
        }
    }

    public void onCardDeselectedEvent(CardListEventInfo info) {
        if(info.cards.Contains(card)) {
            selected = false;
            selectionFrame.SetActive(false);
        }
    }

}