using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.U2D.IK;

public class TooltipOnHover : MonoBehaviour,
    IPointerClickHandler,
    IDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{


    [Header("Populate this with the Tooltip prefab found at\nAssets/Prefabs/UI")]
    public GameObject tooltipPrefab;

    [SerializeReference]
    public TooltipViewModel tooltip;

    [Header("Where, in the object's local space, the tooltip will be instantiated.\nDon't touch the Z coord unless you know what you're doing :)")]
    public Vector3 positionOffset;

    [Header("TODO: get this from a global config?\nHave TooltipType enum with different wait times set in global config?")]
    [SerializeField]
    private float displayWaitTime = 1.0f;

    // private TooltipView currentView = null;
    private List<TooltipView> instantaitedViews = new List<TooltipView>();
    private IEnumerator currentCoroutine = null;
    private bool coroutineIsRunning = false;
    [SerializeField]
    private bool instantiateInWorldspace = false;
    public CompanionInstance companionInstance = null;
    private bool destroyed = false;


    private bool Active() {
        return tooltip != null && !tooltip.empty;
    }

    public void OnPointerEnterVoid() {
        OnPointerEnter(null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(Active() && !destroyed) {
            Debug.Log("Tooltip: Enter");
            ResetCoroutine();
            currentCoroutine = DisplayTooltip();
            StartCoroutine(currentCoroutine);
        }
    }

    public void OnDestroy() {
        if(Active()) {
            Debug.Log("Tooltip: Exit");
            ResetCoroutine();
            // if(currentView == null) return;
            if(instantaitedViews.Count == 0) return;
            Debug.Log("Tooltip: hiding current view");
            // currentView.Hide();
            DestroyAllInstantiatedViews();
        }
    }

    public void OnPointerExitVoid() {
        OnPointerExit(null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(Active()) {
            Debug.Log("Tooltip: Exit");
            ResetCoroutine();
            // if(currentView == null) return;
            if(instantaitedViews.Count == 0) return;
            Debug.Log("Tooltip: hiding current view");
            // currentView.Hide();
            DestroyAllInstantiatedViews();
        }
    }

    public void Destroy() {
        ResetCoroutine();
        DestroyAllInstantiatedViews();
        destroyed = true;
    }

    public void Clear() {
        ResetCoroutine();
        DestroyAllInstantiatedViews();
    }

    /*
    * There is only intended to ever be a single view in this list, but we've
    * experienced bugs where we end up with phantom views due to quick movements
    * or transition from mouse controls to keyboard/controller controls or vice versa.
    * This list tracking attempts to prevent this from happening (essentially we should only
    * ever have a single tooltip on the screen at once)
    */
    private void DestroyAllInstantiatedViews() {
        // Copy to new list so no list modification during foreach
        List<TooltipView> viewsToDestroy = new List<TooltipView>(instantaitedViews);
        foreach (TooltipView view in viewsToDestroy) {
            instantaitedViews.Remove(view);
            view.Hide();
        }
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
        TooltipView instantiatedView;
        if(instantiateInWorldspace) {
            instantiatedView = PrefabInstantiator.instantiateTooltipView(
                tooltipPrefab,
                tooltip,
                transform.position + positionOffset, //this is in world space for some reason
                null);
            instantaitedViews.Add(instantiatedView);
        } else {
            instantiatedView = PrefabInstantiator.instantiateTooltipView(
                tooltipPrefab,
                tooltip,
                transform.position + positionOffset, //this is in world space for some reason
                transform);
            instantaitedViews.Add(instantiatedView);
        }
        TooltipController.ClampTooltipToScreen(instantiatedView.gameObject, Camera.main);
        coroutineIsRunning = false;
    }
}