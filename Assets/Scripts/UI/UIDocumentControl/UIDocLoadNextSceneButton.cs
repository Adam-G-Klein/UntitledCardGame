using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class UIDocLoadNextSceneButton: MonoBehaviour
{
    public GameStateVariableSO gameState;
    private UIDocument uiDocument;
    private VisualElement nextSceneButton;

    public void loadNextScene() {
        gameState.LoadNextLocation();
    }
    void OnEnable() {
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart() {
        uiDocument = GetComponent<UIDocument>();

        yield return new WaitUntil(() => {
            nextSceneButton = uiDocument.rootVisualElement.Q<VisualElement>(name:"next-scene");
            return nextSceneButton != null;
            }
        );

        // make sure we get pointer events on this region of the screen
        nextSceneButton.pickingMode = PickingMode.Position;

        // so we get the nice default hover animation
        nextSceneButton.RegisterCallback<PointerEnterEvent>(nextEnter);

        nextSceneButton.RegisterCallback<PointerLeaveEvent>(nextLeave);

        nextSceneButton.RegisterCallback<ClickEvent>(nextClick);
        nextSceneButton.RegisterCallback<NavigationSubmitEvent>((evt) => nextClick(null));
        VisualElementFocusable nextSceneButtonFocusable = nextSceneButton.AsFocusable();
        nextSceneButtonFocusable.additionalFocusAction += () => nextEnter(null);
        nextSceneButtonFocusable.additionalUnfocusAction += () => nextLeave(null);
        FocusManager.Instance.RegisterFocusableTarget(nextSceneButtonFocusable);
        FocusManager.Instance.SetFocus(nextSceneButtonFocusable);
    }

    private void nextEnter(PointerEnterEvent evt) {
        nextSceneButton.AddToClassList("button-hover");
    }

    private void nextLeave(PointerLeaveEvent evt) {
        nextSceneButton.RemoveFromClassList("button-hover");
    }

    private void nextClick(ClickEvent evt) {
        loadNextScene();
        // Prevent double clicks because that will advance to the next scene!!!!
        nextSceneButton.SetEnabled(false);
    }

}