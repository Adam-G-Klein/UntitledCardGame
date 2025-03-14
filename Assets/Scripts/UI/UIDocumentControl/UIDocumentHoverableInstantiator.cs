using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIDocumentHoverableInstantiator : GenericSingleton<UIDocumentHoverableInstantiator>{
    [Header("This script handles the instantiation of hoverable elements in screenspace\n" +
    "It's called from view classes when they create visual elements that they want to be hoverable\n"+
    "using non-mouse controls")]
    [SerializeField]
    private GameObject hoverablePrefab;

    private Dictionary<VisualElement, GameObject> hoverablesByElement = new Dictionary<VisualElement, GameObject>();
    List<VisualElement> visualElementsWaitingOn = new List<VisualElement>();

    public void InstantiateHoverableWhenUIElementReady(VisualElement element, 
        Action selectCallback = null, 
        Action hoverCallback = null, 
        Action unhoverCallback = null, 
        HoverableType hoverableType = HoverableType.DefaultShop,
        CompanionTypeSO companionTypeSO = null){
        if(visualElementsWaitingOn.Contains(element)){
            return;
        }
        StartCoroutine(InstantiateHoverableWhenUIElementReadyCorout(element, selectCallback, hoverCallback, unhoverCallback, hoverableType, companionTypeSO));
        visualElementsWaitingOn.Add(element);
    }

    private IEnumerator InstantiateHoverableWhenUIElementReadyCorout(VisualElement element, Action selectCallback = null, Action hoverCallback = null, Action unhoverCallback = null, HoverableType hoverableType = HoverableType.DefaultShop, CompanionTypeSO companionTypeSO = null){
        // wait for the element to be ready
        while(!UIDocumentUtils.ElementIsReady(element)){
            Debug.Log("[HoverableInstantiation] Element: " + element.name + " is not ready yet, waiting...");
            yield return null;
        }
        InstantiateHoverable(element, selectCallback, hoverCallback, unhoverCallback, hoverableType, companionTypeSO);
    }

    public void InstantiateHoverable(VisualElement element, Action selectCallback = null, Action hoverCallback = null, Action unhoverCallback = null, HoverableType hoverableType = HoverableType.DefaultShop, CompanionTypeSO companionTypeSO = null){
        Debug.Log("[HoverableInstantiation] Instantiating hoverable for element: " + element.name);
        // get the position from ui doc
        Vector3 position = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(element);
        // shouldn't need to care about z position because the hover indicator should render on top regardless
        GameObject hoverableGO = Instantiate(hoverablePrefab, position, Quaternion.identity);
        hoverableGO.name = element.name + "-Hoverable";
        Hoverable hoverable = hoverableGO.GetComponent<Hoverable>();
        hoverable.associatedUIDocElement = element;
        hoverable.hoverableType = hoverableType;
        hoverable.companionTypeSO = companionTypeSO;
        UIDocumentHoverableCallbackRegistry.Instance.RegisterCallback(element, InputActionType.Select, selectCallback);
        UIDocumentHoverableCallbackRegistry.Instance.RegisterCallback(element, InputActionType.Hover, hoverCallback);
        UIDocumentHoverableCallbackRegistry.Instance.RegisterCallback(element, InputActionType.Unhover, unhoverCallback);
        hoverablesByElement.Add(element, hoverableGO);
        visualElementsWaitingOn.Remove(element);
    }

    public void CleanupHoverable(VisualElement element, bool removeFromDict = true){
        if(visualElementsWaitingOn.Contains(element)){
            visualElementsWaitingOn.Remove(element);
        }
        if(hoverablesByElement.ContainsKey(element)){
            Debug.Log("[HoverableInstantiation] Cleaning up hoverable for element: " + element.name);
            GameObject hoverableGO = hoverablesByElement[element];
            UIDocumentHoverableCallbackRegistry.Instance.UnregisterAllCallbacks(element);
            Destroy(hoverableGO);
            if(removeFromDict)
                hoverablesByElement.Remove(element);
        }
    }

    public void UpdateHoverablesPosition() {
        foreach (var kvp in hoverablesByElement) {
            VisualElement element = kvp.Key;
            GameObject hoverableGO = kvp.Value;
            hoverableGO.transform.position = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(element);
        }
    }

    public void CleanupAllHoverables(){
        foreach (var kvp in hoverablesByElement) {
            CleanupHoverable(kvp.Key, false);
        }
        hoverablesByElement.Clear();
        NonMouseInputManager.Instance.ClearHoverState();
    }
    public void CallIfNextHoverableNotInElemListWhenReady(Action action, List<VisualElement> elements) {
        StartCoroutine(CallIfNextHoverableNotInElemListWhenReadyCorout(action, elements));
    }

    private IEnumerator CallIfNextHoverableNotInElemListWhenReadyCorout(Action action, List<VisualElement> elements) {
        List<Hoverable> hoverables = new List<Hoverable>(); 
        foreach (var elem in elements) {
            if(elem == null) continue; // shop sell and view deck buttons can be null
            while (!hoverablesByElement.ContainsKey(elem)) {
                Debug.Log("[HoverableInstantiation] Element is not ready yet for calling method when it's hovered, waiting...");
                yield return null;
            }
            foreach (var kvp in hoverablesByElement) {
                Debug.Log($"Checking if element {elem.name} is equal to key {kvp.Key.name} and value is not null: {elem != null && kvp.Key == elem && kvp.Value != null}");
                if (elem != null && kvp.Key == elem && kvp.Value != null) {
                    hoverables.Add(kvp.Value.GetComponent<Hoverable>());
                }
            }
        }
        NonMouseInputManager.Instance.CallIfHoverableNotNextHovered(action, hoverables);
    }

}