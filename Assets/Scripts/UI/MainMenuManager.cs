using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject optionsUIPrefab;
    [SerializeField]
    private GameStateVariableSO gameState;
    [SerializeField]
    private UIDocument optionsUI;
    [SerializeField]
    private CanvasGroup canvasGroup;

    public void startButtonHandler() {
        SceneManager.LoadScene("GenerateMap");
    }

    public void optionsButtonHandler() {
        optionsUI.rootVisualElement.style.visibility = Visibility.Visible;
        canvasGroup.blocksRaycasts = true;
    }

    public void exitButtonHandler() {
        Application.Quit();
    }
}
