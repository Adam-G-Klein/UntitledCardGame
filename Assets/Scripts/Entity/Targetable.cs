using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Targetable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TargetType targetType;
    
    public void OnPointerClick(PointerEventData eventData)
    {
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetType == TargetType.Companion) {
            EnemyEncounterManager.Instance.gameState.UpdateHoveredCompanion(GetComponent<CompanionInstance>());
        }
    }

        public void OnPointerExit(PointerEventData eventData)
    {
        if (targetType == TargetType.Companion) {
            EnemyEncounterManager.Instance.gameState.UpdateHoveredCompanion(null);
        }
    }

    public enum TargetType {
        Companion,
        Minion,
        Enemy,
        Card
    }
}