using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Targetable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TargetType targetType;
    /*
    * This might not be the best place for these, but with recent UI doc changes
    * the UI doc eats pointer events on game objects. This works around it
    * by registering all relevant pointer events with this script since this
    * script was updated to work with UI document. 
    */
    public UnityEvent<PointerEventData> pointerEnterEvent;
    public UnityEvent<PointerEventData> pointerExitEvent;
    
    public void OnPointerClick(PointerEventData eventData) {
        TargetableClicked();
    }

    // This comes from UI Document based events
    public void OnPointerClickUI(ClickEvent evt) {
        TargetableClicked();
    }

    private void TargetableClicked() {
        Debug.Log("Targetable: Clicked on targetable");
        if(targetType == TargetType.Companion) {
            Debug.Log("Targetable: Clicked on companion");
        } else if(targetType == TargetType.Minion) {
            Debug.Log("Targetable: Clicked on minion");
        } else if(targetType == TargetType.Enemy) {
            Debug.Log("Targetable: Clicked on enemy");
        } else if(targetType == TargetType.Card) {
            Debug.Log("Targetable: Clicked on card");
        }
        TargettingManager.Instance.InvokeTargetSuppliedHandler(this);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        TargetableEntered();
    }
    
    public void OnPointerEnterUI(PointerEnterEvent evt) {
        TargetableEntered();
    }

    private void TargetableEntered() {
        if (targetType == TargetType.Companion) {
            EnemyEncounterManager.Instance.gameState.UpdateHoveredCompanion(GetComponent<CompanionInstance>());
        }
        pointerEnterEvent.Invoke(null);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TargetableExited();
    }

    public void OnPointerLeaveUI(PointerLeaveEvent evt) {
        TargetableExited();
    }

    private void TargetableExited() {
        if (targetType == TargetType.Companion) {
            EnemyEncounterManager.Instance.gameState.UpdateHoveredCompanion(null);
        }
        pointerExitEvent.Invoke(null);
    }

    public enum TargetType {
        Companion,
        Minion,
        Enemy,
        Card,
        None
    }
}