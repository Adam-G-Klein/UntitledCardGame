using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DemoEndOfRunView : MonoBehaviour
{
    public UIDocument uiDoc;
    public string URL;
    [TextArea]
    public string defeatText;
    [TextArea]
    public string victoryText;
    public GameStateVariableSO gameState;

    private Label mainLabel;
    private Button enterButton;
    private Button mainMenuButton;


    // Start is called before the first frame update
    void Start()
    {
        mainLabel = uiDoc.rootVisualElement.Q<Label>("main-label");
        enterButton = uiDoc.rootVisualElement.Q<Button>("enterButton");
        mainMenuButton = uiDoc.rootVisualElement.Q<Button>("mainMenuButton");

        enterButton.RegisterOnSelected(EnterButtonHandler);
        mainMenuButton.RegisterOnSelected(MainMenuButtonHandler);

        if (gameState.currentRunOutcome == RunOutcome.Defeat) {
            mainLabel.text = defeatText;
            enterButton.style.display = DisplayStyle.None;
        } else {
            mainLabel.text = victoryText;
        }

        FocusManager.Instance.RegisterFocusableTarget(enterButton.AsFocusable());
        FocusManager.Instance.RegisterFocusableTarget(mainMenuButton.AsFocusable());
    }

    private void MainMenuButtonHandler() {
        MusicController.Instance.PrepareForGoingBackToMainMenu();
        SceneTransitionManager.LoadScene("MainMenu");
    }

    private void EnterButtonHandler() {
        Application.OpenURL(URL);
    }
}
