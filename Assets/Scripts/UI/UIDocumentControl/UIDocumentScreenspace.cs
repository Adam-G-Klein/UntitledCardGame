using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(UIDocument))]
public class UIDocumentScreenspace : MonoBehaviour {

    [SerializeField]
    [Header("This script just holds a reference to the ui doc and sets up picking mode")]
    public UIDocument doc;

    void Awake() {
        if(!doc) {
            Debug.LogError("UIDocumentScreenspace: No UIDocument component set on this script. Please set it from the component attached to the gameobject so we load the scene 1 frame faster :)");
        }
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