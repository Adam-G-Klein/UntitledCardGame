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
    private VisualElement backTutorialElement;
    
    private VisualElement backgroundElement; 
    private int currentTutorialElementIndex = 0;

    void Start() {
        StartCoroutine(LateStart());
        
    }

    private IEnumerator LateStart() {
        //yield return new WaitUntil(() => UIDocumentGameObjectPlacer.Instance.IsReady());
        screenspaceDoc = GetComponent<UIDocumentScreenspace>();

        nextTutorialElement = screenspaceDoc.GetVisualElement("next");      
        // make sure we get pointer events on this region of the screen
        nextTutorialElement.pickingMode = PickingMode.Position;

        // nextTutorialElement.RegisterCallback<ClickEvent>((evt) => {
        //     NextTutorialElementButtonHandler();
        // });
        VisualElementUtils.RegisterSelected(nextTutorialElement, NextTutorialElementButtonHandler);

        backTutorialElement = screenspaceDoc.GetVisualElement("back");
        backTutorialElement.AddToClassList("tutorial-button-disabled");
        backTutorialElement.pickingMode = PickingMode.Position;
        // backTutorialElement.RegisterCallback<ClickEvent>((evt) => {
        //     BackTutorialElementButtonHandler();
        // });
        VisualElementUtils.RegisterSelected(backTutorialElement, BackTutorialElementButtonHandler);

        FocusManager.Instance.RegisterFocusableTarget(nextTutorialElement.AsFocusable());
        FocusManager.Instance.RegisterFocusableTarget(backTutorialElement.AsFocusable());
        FocusManager.Instance.SetFocus(nextTutorialElement.AsFocusable());
        yield return null;
    }

    private void NextTutorialElementButtonHandler() {
        currentTutorialElementIndex++;
        backTutorialElement.RemoveFromClassList("tutorial-button-disabled");
        Debug.Log("omg we are so back");
        //if(nextTutorialButtonEnabled) {
            TutorialManager.Instance.TutorialButtonClicked();
        //}
    }
    private void BackTutorialElementButtonHandler() {
        if (currentTutorialElementIndex == 0) return;
        TutorialManager.Instance.TutorialBackButtonClicked();
        currentTutorialElementIndex--;
        if (currentTutorialElementIndex == 0) {
            backTutorialElement.AddToClassList("tutorial-button-disabled");
        }
    }
}