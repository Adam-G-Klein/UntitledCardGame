using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(UIDocument))]
public class MiniUIDocumentScreenspace : MonoBehaviour {

    [SerializeField]
    [Header("This script just holds a reference to the ui doc and sets up picking mode")]
    public UIDocument doc;
    [SerializeField]
    private Canvas canvas;

    void Awake() {
        if(!doc) {
            Debug.LogError("UIDocumentScreenspace: No UIDocument component set on this script. Please set it from the component attached to the gameobject so we load the scene 1 frame faster :)");
        }
        // Do this in awake so individual controllers can enable clicking on the elements they care about
        UIDocumentUtils.SetAllPickingMode(doc.rootVisualElement, PickingMode.Ignore);
        canvas.sortingLayerID = 7; // Above cards
    }

    public VisualElement GetVisualElement(string name){
        return doc.rootVisualElement.Q<VisualElement>(name);
    }

}