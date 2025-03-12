using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class CardSelectionView : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;

    public delegate void CardsSelected(List<Card> cards, Companion companion);
    public event CardsSelected cardsSelectedHandler;

    private VisualElement companionArea;
    private VisualElement cardContainer;
    private Button confirmExitButton;
    private Label promptTextLabel;

    private List<CardView> cardsSelected;
    private int minSelections;
    private int maxSelections;
    private string promptText;
    private IEnumerator currentCoroutine = null;
    private Companion companion;

    public void Start() {
        if (uiDoc == null) {
            uiDoc = GetComponent<UIDocument>();
        }
    }

    public void Setup(List<Card> cards, Companion companion) {
        this.companion = companion;
        this.Setup(cards, "", -1, -1, companion);
    }

    public void Setup(
            List<Card> cards,
            string promptText = "",
            int minSelections = -1,
            int maxSelections = -1,
            Companion companion = null) {
        this.companion = companion;
        InitFields();
        this.minSelections = minSelections;
        this.maxSelections = maxSelections;
        this.promptText = promptText;

        if (minSelections <= 0) {
            this.confirmExitButton.text = "Exit";
        } else {
            this.confirmExitButton.text = "Confirm";
        }

        if (promptText == "") {
            this.promptTextLabel.style.display = DisplayStyle.None;
            this.confirmExitButton.style.marginLeft = 0;
        }

        if (companion == null) {
            this.companionArea.style.display = DisplayStyle.None;
        } else {
            EntityView entityView = new EntityView(companion, 0, false);
            entityView.UpdateWidthAndHeight();
            VisualElement portraitContainer = entityView.entityContainer.Q(className: "entity-portrait");
            portraitContainer.style.backgroundImage = new StyleBackground(companion.companionType.sprite);
            this.companionArea.Add(entityView.entityContainer);
        }

        foreach (Card card in cards) {
            CardView newCardView = new CardView(card, card.getCompanionFrom(), true);
            VisualElement cardWrapper = new VisualElement();
            cardWrapper.AddToClassList("card-wrapper");
            newCardView.cardContainer.RegisterCallback<ClickEvent>(evt => CardViewClicked(evt, newCardView));
            cardWrapper.Add(newCardView.cardContainer);
            this.cardContainer.Add(cardWrapper);
        }
        
        this.promptTextLabel.text = promptText;
    }

    private void InitFields() {
        cardsSelected = new List<CardView>();
        this.companionArea = uiDoc.rootVisualElement.Q("companion-area");
        this.cardContainer = uiDoc.rootVisualElement.Q("card-scroll-view-container");
        this.confirmExitButton = uiDoc.rootVisualElement.Q<Button>("confirm-exit-button");
        this.promptTextLabel = uiDoc.rootVisualElement.Q<Label>("prompt-text-label");
        this.confirmExitButton.clicked += ExitView;
    }

    private void CardViewClicked(ClickEvent evt, CardView cardView) {
        if (cardsSelected.Contains(cardView)) {
            cardsSelected.Remove(cardView);
            // Undo styling for having card selected
            cardView.cardContainer.parent.RemoveFromClassList("card-selected");
            return;
        }

        if (maxSelections != -1 && cardsSelected.Count >= maxSelections) {
            return;
        }

        cardsSelected.Add(cardView);
        // Setup styling for having card selected
        cardView.cardContainer.parent.AddToClassList("card-selected");
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
        cardsSelectedHandler.Invoke(outputCards, companion);
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
