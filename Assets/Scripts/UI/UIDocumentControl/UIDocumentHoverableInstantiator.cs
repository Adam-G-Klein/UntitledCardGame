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

    public void InstantiateHoverable(VisualElement element, Action callback){
        // get the position from 
        Vector3 position = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(element);
        // shouldn't need to care about z position because the hover indicator should render on top regardless
        GameObject hoverable = Instantiate(hoverablePrefab, position, Quaternion.identity);
        hoverable.name = element.name + "-Hoverable";
        UIDocumentHoverableCallbackRegistry.Instance.RegisterCallback(element.name, callback);
    }
}