using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems; 

public class CompanionInView : MonoBehaviour, IPointerClickHandler
{
    public Companion companion;
    public CompanionEvent companionClickedEvent;
    public Image image;
    public UnityEvent showCompanionActions;
    public UnityEvent hideCompanionActions;

    private bool isCompanionActionsVisible = false;

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            companionClickedEvent.Raise(companion);
        } else if (eventData.button == PointerEventData.InputButton.Right) {
            if (isCompanionActionsVisible) {
                hideCompanionActions.Invoke();
                isCompanionActionsVisible = false;
            } else {
                showCompanionActions.Invoke();
                isCompanionActionsVisible = true;
            }
        }
    }

    public void setup() {
        image.sprite = companion.companionType.sprite;
    }
}
