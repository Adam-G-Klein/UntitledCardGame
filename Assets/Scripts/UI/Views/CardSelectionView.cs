using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class CardSelectionView : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;

    private VisualElement companionArea;
    private VisualElement cardContainer;
    private Button confirmExitButton;
    private Label promptTextLabel;

    private List<CardView> cardsSelected;
    private int minSelections;
    private int maxSelections;
    private string promptText;
    private IEnumerator currentCoroutine = null;

    public void Start() {
        if (uiDoc == null) {
            uiDoc = GetComponent<UIDocument>();
        }
    }

    public void Setup(
            List<Card> cards,
            string promptText = "",
            int minSelections = -1,
            int maxSelections = -1) {
        InitFields();
        this.minSelections = minSelections;
        this.maxSelections = maxSelections;
        this.promptText = promptText;

        if (minSelections <= 0) {
            this.confirmExitButton.text = "Exit";
        } else {
            this.confirmExitButton.text = "Confirm";
        }

        foreach (Card card in cards) {
            CardView newCardView = new CardView(card, card.getCompanionFrom(), true);
            VisualElement cardWrapper = new VisualElement();
            cardWrapper.AddToClassList("card-wrapper");
            newCardView.cardContainer.RegisterCallback<ClickEvent>(evt => CardViewClicked(evt, newCardView));
            cardWrapper.Add(newCardView.cardContainer);
            this.cardContainer.Add(cardWrapper);
        }
    }

    private void InitFields() {
        cardsSelected = new List<CardView>();
        this.companionArea = uiDoc.rootVisualElement.Q("companion-area");
        this.cardContainer = uiDoc.rootVisualElement.Q("card-scroll-view-container");
        this.confirmExitButton = uiDoc.rootVisualElement.Q<Button>("confirm-exit-button");
        this.confirmExitButton.clicked += ExitView;
    }

    private void CardViewClicked(ClickEvent evt, CardView cardView) {
        if (cardsSelected.Contains(cardView)) {
            cardsSelected.Remove(cardView);
            // Undo styling for having card selected
            return;
        }

        if (maxSelections != -1 && cardsSelected.Count >= maxSelections) {
            return;
        }

        cardsSelected.Add(cardView);
        // Setup styling for having card selected
    }

    private void ExitView() {
        if (this.minSelections <= 0) {
            Destroy(this.gameObject);
            return;
        }

        if (cardsSelected.Count < minSelections) {
            if (currentCoroutine == null) {
                currentCoroutine = changePromptText("Please select " + 
                    (minSelections - cardsSelected.Count) + " more cards");
                StartCoroutine(currentCoroutine);
            }
            return;
        }
        List<Card> outputCards = new List<Card>();
        foreach (CardView cardView in cardsSelected) {
            outputCards.Add(cardView.cardInstance);
        }
        // Call the callback
        Destroy(this.gameObject);
    }

    private IEnumerator changePromptText(string newText) {
        promptTextLabel.text = newText;
        yield return new WaitForSeconds(1.5f);
        promptTextLabel.text = promptText;
        currentCoroutine = null;
        yield return null;
    }
}
