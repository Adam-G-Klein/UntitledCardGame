using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

[RequireComponent(typeof(UIDocumentScreenspace))]
public class UIDocReturnToMainMenuButton: MonoBehaviour
{
    private UIDocumentScreenspace screenspaceDoc;
    private VisualElement nextSceneButton;

    public void ReturnToMainMenu() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    void OnEnable() {
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart() {
        screenspaceDoc = GetComponent<UIDocumentScreenspace>();

        yield return new WaitUntil(() => {
            nextSceneButton = screenspaceDoc.GetVisualElement("main-menu");
            return nextSceneButton != null;
            }
        );

        // make sure we get pointer events on this region of the screen
        nextSceneButton.pickingMode = PickingMode.Position;

        // so we get the nice default hover animation
        nextSceneButton.RegisterCallback<PointerEnterEvent>((evt) => {
            screenspaceDoc.SetStateDirty();
            nextSceneButton.AddToClassList("button-hover");
        });

        nextSceneButton.RegisterCallback<PointerLeaveEvent>((evt) => {
            screenspaceDoc.SetStateDirty();
            nextSceneButton.RemoveFromClassList("button-hover");
        });

        nextSceneButton.RegisterCallback<ClickEvent>((evt) => {
            ReturnToMainMenu();
            // Prevent double clicks because that will advance to the next scene!!!!
            nextSceneButton.SetEnabled(false);
        });
    }

}