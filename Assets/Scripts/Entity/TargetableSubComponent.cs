using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetableSubComponent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Targetable targetable;

    public void OnPointerClick(PointerEventData eventData)
    {
        targetable.TargetableClicked();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetable.TargetableEntered();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetable.TargetableExited();
    }
}
