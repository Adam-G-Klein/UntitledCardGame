using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ExitGameViewController : MonoBehaviour
{
    public UIDocument uIDocument;
    public string buttonSadText;
    public string buttonHappyText;
    public Sprite neutralSprite;
    public Sprite happySprite;
    public Sprite cryingSprite;

    private VisualElement ratElement;
    private Label mainLabel;
    private Button steamPageButton;
    private Button exitButton;

    private bool visitedSteamPage = false;


    // Start is called before the first frame update
    void Start() {
        ratElement = uIDocument.rootVisualElement.Q<VisualElement>("rat-element");
        mainLabel = uIDocument.rootVisualElement.Q<Label>("main-label");
        steamPageButton = uIDocument.rootVisualElement.Q<Button>("steam-page-button");
        exitButton = uIDocument.rootVisualElement.Q<Button>("exit-button");

        steamPageButton.clicked += VisitSteamPageHandler;
        exitButton.clicked += ExitButtonHandler;

        exitButton.RegisterCallback<PointerEnterEvent>(ExitButtonPointerEnterHandler);
        exitButton.RegisterCallback<PointerLeaveEvent>(ExitButtonPointerExitHandler);
    }

    private void VisitSteamPageHandler() {
        Application.OpenURL("https://store.steampowered.com/app/3931980/Rite_of_the_Dealer/");
        visitedSteamPage = true;
        exitButton.text = buttonHappyText;
        ratElement.style.backgroundImage = new StyleBackground(happySprite);
    }

    private void ExitButtonPointerEnterHandler(PointerEnterEvent evt) {
        if (!visitedSteamPage) {
            ratElement.style.backgroundImage = new StyleBackground(cryingSprite);
        }
    }

    private void ExitButtonPointerExitHandler(PointerLeaveEvent evt) {
        if (!visitedSteamPage) {
            ratElement.style.backgroundImage = new StyleBackground(neutralSprite);
        }
    }

    private void ExitButtonHandler() {
        Application.Quit();
    }
}
