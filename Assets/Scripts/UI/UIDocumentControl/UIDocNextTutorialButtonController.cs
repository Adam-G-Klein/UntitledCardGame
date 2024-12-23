using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[RequireComponent(typeof(UIDocumentScreenspace))]
public class UIDocNextTutorialButtonController : MonoBehaviour {

    private UIDocumentScreenspace screenspaceDoc;

    private bool nextTutorialButtonEnabled = true;

    private VisualElement nextTutorialElement;
    
    private VisualElement backgroundElement; 

    void Start() {
        StartCoroutine(LateStart());
        
    }

    private IEnumerator LateStart() {
        //yield return new WaitUntil(() => UIDocumentGameObjectPlacer.Instance.IsReady());
        screenspaceDoc = GetComponent<UIDocumentScreenspace>();

        nextTutorialElement = screenspaceDoc.GetVisualElement("next");
        Debug.Log("HEREE");
        Debug.Log(nextTutorialElement);
        
        // make sure we get pointer events on this region of the screen
        nextTutorialElement.pickingMode = PickingMode.Position;

        nextTutorialElement.RegisterCallback<ClickEvent>((evt) => {
            NextTutorialElementButtonHandler();
        });

        backgroundElement = screenspaceDoc.GetVisualElement("tutorial-container");
        
        backgroundElement.RegisterCallback<ClickEvent>((evt) => {
            // preventDefault (avoid clicking through to UI documents behind this one)
            // does not work lol
        });
        yield return null;
    }

    private void NextTutorialElementButtonHandler() {
        Debug.Log("omg we are so back");
        //if(nextTutorialButtonEnabled) {
            TutorialManager.Instance.TutorialButtonClicked();
        //}
    }
}