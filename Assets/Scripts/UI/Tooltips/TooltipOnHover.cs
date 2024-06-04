using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 

public class TooltipOnHover : MonoBehaviour,
    IPointerClickHandler,
    IDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{


    [Header("Populate this with the Tooltip prefab found at\nAssets/Prefabs/UI")]
    public GameObject tooltipPrefab;

    [SerializeReference]
    public Tooltip tooltip;

    [Header("Where, in the object's local space, the tooltip will be instantiated.\nDon't touch the Z coord unless you know what you're doing :)")]
    public Vector3 positionOffset;

    [Header("TODO: get this from a global config?\nHave TooltipType enum with different wait times set in global config?")]
    [SerializeField]
    private float displayWaitTime = 1.0f;

    private TooltipView currentView = null;
    private IEnumerator currentCoroutine = null;
    private bool coroutineIsRunning = false;



    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Tooltip: Enter");
        ResetCoroutine();
        currentCoroutine = DisplayTooltip();
        StartCoroutine(currentCoroutine);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Tooltip: Exit");
        ResetCoroutine();
        if(currentView == null) return;
        Debug.Log("Tooltip: hiding current view");
        currentView.Hide();
    }

    public void OnPointerClick(PointerEventData eventData) {}
    public void OnDrag(PointerEventData eventData) {}

    private void ResetCoroutine() {
        if(coroutineIsRunning && currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
            coroutineIsRunning = false;
        }
    }

    private IEnumerator DisplayTooltip() {
        coroutineIsRunning = true;
        yield return new WaitForSeconds(displayWaitTime);
        currentView = PrefabInstantiator.instantiateTooltipView(
            tooltipPrefab,
            tooltip,
            transform.position + positionOffset, //this is in world space for some reason
            transform);
        coroutineIsRunning = false;
    }
}