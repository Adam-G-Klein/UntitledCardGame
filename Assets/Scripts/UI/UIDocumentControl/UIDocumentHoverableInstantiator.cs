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


    public void InstantiateHoverableWhenUIElementReady(VisualElement element, Action selectCallback = null, Action hoverCallback = null, Action unhoverCallback = null){
        StartCoroutine(InstantiateHoverableWhenUIElementReadyCorout(element, selectCallback, hoverCallback, unhoverCallback));
    }

    private IEnumerator InstantiateHoverableWhenUIElementReadyCorout(VisualElement element, Action selectCallback = null, Action hoverCallback = null, Action unhoverCallback = null){
        // wait for the element to be ready
        while(!UIDocumentUtils.ElementIsReady(element)){
            Debug.Log("[HoverableInstantiation] Element: " + element.name + " is not ready yet, waiting...");
            yield return null;
        }
        InstantiateHoverable(element, selectCallback, hoverCallback, unhoverCallback);
    }

    public void InstantiateHoverable(VisualElement element, Action selectCallback = null, Action hoverCallback = null, Action unhoverCallback = null){
        Debug.Log("[HoverableInstantiation] Instantiating hoverable for element: " + element.name);
        // get the position from ui doc
        Vector3 position = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(element);
        // shouldn't need to care about z position because the hover indicator should render on top regardless
        GameObject hoverableGO = Instantiate(hoverablePrefab, position, Quaternion.identity);
        hoverableGO.name = element.name + "-Hoverable";
        Hoverable hoverable = hoverableGO.GetComponent<Hoverable>();
        hoverable.associatedUIDocElement = element;
        UIDocumentHoverableCallbackRegistry.Instance.RegisterCallback(element, InputActionType.Select, selectCallback);
        UIDocumentHoverableCallbackRegistry.Instance.RegisterCallback(element, InputActionType.Hover, hoverCallback);
        UIDocumentHoverableCallbackRegistry.Instance.RegisterCallback(element, InputActionType.Unhover, unhoverCallback);
        hoverablesByElement.Add(element, hoverableGO);
    }

    public void CleanupHoverable(VisualElement element){
        if(hoverablesByElement.ContainsKey(element)){
            GameObject hoverableGO = hoverablesByElement[element];
            Destroy(hoverableGO);
            hoverablesByElement.Remove(element);
        }
    }
}