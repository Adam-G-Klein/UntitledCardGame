using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIDocMainMenuButton : MonoBehaviour
{
    public GameStateVariableSO gameState;
    private UIDocument uiDocument;
    private VisualElement nextSceneButton;

    public void loadMainMenu() {
        Application.Quit();
    }
    void OnEnable() {
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart() {
        uiDocument = GetComponent<UIDocument>();

        yield return new WaitUntil(() => {
            nextSceneButton = uiDocument.rootVisualElement.Q(name: "main-menu");
            return nextSceneButton != null;
            }
        );

        // make sure we get pointer events on this region of the screen
        nextSceneButton.pickingMode = PickingMode.Position;

        // so we get the nice default hover animation
        nextSceneButton.RegisterCallback<PointerEnterEvent>((evt) => {
            nextSceneButton.AddToClassList("button-hover");
        });

        nextSceneButton.RegisterCallback<PointerLeaveEvent>((evt) => {
            nextSceneButton.RemoveFromClassList("button-hover");
        });

        nextSceneButton.RegisterCallback<ClickEvent>((evt) => {
            loadMainMenu();
        });
    }

}