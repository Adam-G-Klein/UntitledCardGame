using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UICard : MonoBehaviour, IPointerClickHandler {
    public UnityEvent<UICard> onclickEvent;
    public Card card;
    public GameObject selectionFrame;
    public bool selected = false;

    public void Start() {
        card = GetComponent<CardDisplay>().getCardInfo();
    }

    public void OnPointerClick(PointerEventData eventData) {
        onclickEvent.Invoke(this);
    }
}