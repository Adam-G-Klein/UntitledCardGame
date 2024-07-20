using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class UIDocGOMapping {
    public string UIDocumentElementName;
    public GameObject gameObject;
    
    public UIDocGOMapping(GameObject gameObject, string UIDocumentElementName) {
        this.UIDocumentElementName = UIDocumentElementName;
        this.gameObject = gameObject;
    }
}
[RequireComponent(typeof(UIDocument))]
// Only works for screenspace right now
[RequireComponent(typeof(UIDocumentScreenspace))]
public class UIDocumentGameObjectPlacer : MonoBehaviour {

    public List<UIDocGOMapping> mappings;

    void Update() {

        // Set the gameobject to the position of the element in the ui document
        foreach(UIDocGOMapping mapping in mappings) {
            VisualElement element = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(mapping.UIDocumentElementName);
            if (element != null) {
                RectTransform rectTransform = mapping.gameObject.GetComponent<RectTransform>();
                Debug.Log("element.localBound.center.x: " + element.localBound.center.x);
                Debug.Log("element.localBound.center.y: " + element.localBound.center.y);
                Vector3 screenPosition =  new Vector3(
                    element.localBound.center.x,
                    element.localBound.center.y,
                    0
                );
                rectTransform.position = screenPosition;
            }
        }
    }


}