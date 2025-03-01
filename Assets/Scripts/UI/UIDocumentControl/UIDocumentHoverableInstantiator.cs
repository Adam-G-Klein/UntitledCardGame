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