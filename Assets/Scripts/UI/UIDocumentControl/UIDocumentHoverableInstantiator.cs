using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

// Used by views to store the hoverable instantiation for execution after the ui doc has finished drawing
public class StoredHoverableInstantiation {
    public VisualElement element;
    public Action selectCallback;
    public Action hoverCallback;
    public Action unhoverCallback;

    public StoredHoverableInstantiation(VisualElement element, Action selectCallback, Action hoverCallback, Action unhoverCallback){
        this.element = element;
        this.selectCallback = selectCallback;
        this.hoverCallback = hoverCallback;
        this.unhoverCallback = unhoverCallback;
    }

    public void ExecuteInstantiation(){
        UIDocumentHoverableInstantiator.Instance.InstantiateHoverable(element, selectCallback, hoverCallback, unhoverCallback);
    }
}

public class UIDocumentHoverableInstantiator : GenericSingleton<UIDocumentHoverableInstantiator>{
    [Header("This script handles the instantiation of hoverable elements in screenspace\n" +
    "It's called from view classes when they create visual elements that they want to be hoverable\n"+
    "using non-mouse controls")]
    [SerializeField]
    private GameObject hoverablePrefab;

    public IEnumerator InstantiateHoverableWhenUIElementReady(VisualElement element, Action selectCallback = null, Action hoverCallback = null, Action unhoverCallback = null){
        // wait for the element to be ready
        while(!UIDocumentUtils.ElementIsReady(element)){
            yield return null;
        }
        InstantiateHoverable(element, selectCallback, hoverCallback, unhoverCallback);
    }

    public void InstantiateHoverable(VisualElement element, Action selectCallback = null, Action hoverCallback = null, Action unhoverCallback = null){
        // get the position from 
        Vector3 position = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(element);
        // shouldn't need to care about z position because the hover indicator should render on top regardless
        GameObject hoverableGO = Instantiate(hoverablePrefab, position, Quaternion.identity);
        hoverableGO.name = element.name + "-Hoverable";
        Hoverable hoverable = hoverableGO.GetComponent<Hoverable>();
        hoverable.associatedUIDocElement = element;
        UIDocumentHoverableCallbackRegistry.Instance.RegisterCallback(element.name, InputActionType.Select, selectCallback);
        UIDocumentHoverableCallbackRegistry.Instance.RegisterCallback(element.name, InputActionType.Hover, hoverCallback);
        UIDocumentHoverableCallbackRegistry.Instance.RegisterCallback(element.name, InputActionType.Unhover, unhoverCallback);
    }
}