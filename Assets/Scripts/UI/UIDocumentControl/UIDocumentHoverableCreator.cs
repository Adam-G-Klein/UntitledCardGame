using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[RequireComponent(typeof(UIDocument))]
public class UIDocumentHoverableCreator : MonoBehaviour {

    [SerializeField]
    [Header("This Script registers the listed visual elements as hoverables in screenspace")]
    public UIDocument doc;

    void Awake() {
        if(!doc) {
            Debug.LogError("UIDocumentScreenspace: No UIDocument component set on this script. Please set it from the component attached to the gameobject so we load the scene 1 frame faster :)");
        }
        // Do this in awake so individual controllers can enable clicking on the elements they care about
        UIDocumentUtils.SetAllPickingMode(doc.rootVisualElement, PickingMode.Ignore);

    }

    void OnEnable() {
    }

    void Start() {
        
    }

    public void SetStateDirty(){
        // no op now that gpu is updating the texture on every frame
    }

    public VisualElement GetVisualElement(string name){
        return doc.rootVisualElement.Q<VisualElement>(name);
    }

}