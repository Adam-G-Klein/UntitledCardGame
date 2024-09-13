using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

[RequireComponent(typeof(UIDocumentScreenspace))]
public class UIDocLoadNextSceneButton: MonoBehaviour
{
    public GameStateVariableSO gameState;
    private UIDocumentScreenspace screenspaceDoc;
    private VisualElement nextSceneButton;

    public void loadNextScene() {
        gameState.LoadNextLocation();
    }
    void OnEnable() {
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart() {
        screenspaceDoc = GetComponent<UIDocumentScreenspace>();

        yield return new WaitUntil(() => {
            nextSceneButton = screenspaceDoc.GetVisualElement("next-scene");
            return nextSceneButton != null;
            }
        );

        // make sure we get pointer events on this region of the screen
        nextSceneButton.pickingMode = PickingMode.Position;

        // so we get the nice default hover animation
        nextSceneButton.RegisterCallback<PointerEnterEvent>((evt) => {
            screenspaceDoc.SetStateDirty();
        });

        nextSceneButton.RegisterCallback<PointerLeaveEvent>((evt) => {
            screenspaceDoc.SetStateDirty();
        });

        nextSceneButton.RegisterCallback<ClickEvent>((evt) => {
            loadNextScene();
        });
    }

}