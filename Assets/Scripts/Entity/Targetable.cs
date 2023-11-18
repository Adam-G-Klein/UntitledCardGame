using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Targetable : MonoBehaviour, IPointerClickHandler
{
    public TargetType targetType;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        TargettingManager.Instance.InvokeTargetSuppliedHandler(this);
    }

    public enum TargetType {
        Companion,
        Minion,
        Enemy,
        Card
    }
}