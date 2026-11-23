using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/*
    Full-screen "pick cards from a list" view: Scry / SelectCardsFromList, shop card removal, and the read-only
    deck viewer (no handler attached, nothing to select).

    Selection feedback:
      - Selected cards lift and get a gold frame.
      - Hovering/focusing a card shows a faint frame, so you can tell which card a click will toggle.
      - Once the maximum is picked, the other cards dim. When only one card can be picked, clicking another
        card moves the selection to it instead.
      - The footer shows the prompt next to Confirm, which is greyed out until the minimum is met.
*/
[RequireComponent(typeof(Canvas))]
public class CardSelectionView : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;
    [SerializeField] private GameObject tooltipPrefab;

    public delegate void CardsSelected(List<Card> cards, Companion companion);
    public event CardsSelected cardsSelectedHandler;

    private const string WrapperSelectedClass = "card-wrapper--selected";
    private const string WrapperUnavailableClass = "card-wrapper--unavailable";
    private const string WrapperFocusedClass = "card-wrapper--focused";
    private const string ButtonDisabledClass = "exit-button--disabled";
    private const string PromptWarningClass = "selection-prompt--warning";

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
    private TooltipController tooltipController;

    private List<CardView> cardViews;
    private readonly Dictionary<CardView, VisualElement> wrapperFor = new Dictionary<CardView, VisualElement>();

    // The read-only deck viewer passes no limits and attaches no handler.
    private bool IsSelectable => cardsSelectedHandler != null || minSelections > 0 || maxSelections > 0;
    private bool SingleSelect => maxSelections == 1;
    private bool AtMax => maxSelections != -1 && cardsSelected.Count >= maxSelections;

    public void Start() {
        if (uiDoc == null) {
            uiDoc = GetComponent<UIDocument>();
        }
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        // The handler is attached right after Setup, so refresh once it's there (e.g. "select any number" scry).
        if (cardsSelected != null) UpdateSelectionUI();
    }

    public void Setup(List<Card> cards, Companion companion, VisualTreeAsset companionViewTemplate) {
        this.companion = companion;
        this.Setup(cards, "", -1, -1, companion, companionViewTemplate);
    }

    public void Setup(
            List<Card> cards,
            string promptText = "",
            int minSelections = -1,
            int maxSelections = -1,
            Companion companion = null,
            VisualTreeAsset companionViewTemplate = null) {
        this.companion = companion;
        InitFields();
        this.minSelections = minSelections;
        this.maxSelections = maxSelections;
        this.promptText = promptText;
        cardViews = new List<CardView>();
        wrapperFor.Clear();
        tooltipController = new TooltipController(tooltipPrefab);

        FocusManager.Instance.StashFocusables(this.GetType().Name);

        if (string.IsNullOrEmpty(promptText)) {
            SetPromptVisible(false);
        }

        if (companion == null || companionViewTemplate == null) {
            this.companionArea.style.display = DisplayStyle.None;
        } else {
            CompanionView companionView = new CompanionView(companion, companionViewTemplate, 0, CompanionView.CARD_SELECTION_CONTEXT, null);
            companionView.ScaleView(0.70f);
            this.companionArea.Insert(0, companionView.container);

            Label companionText = uiDoc.rootVisualElement.Q<Label>("companion-description-text");
            companionText.text = companion.GetDescription();
            this.companionArea.Add(companionText);
        }

        foreach (Card card in cards) {
            CardView newCardView = new CardView(card, card.getCompanionFrom(), true);
            VisualElement cardWrapper = new VisualElement();
            cardWrapper.AddToClassList("card-wrapper");

            // Drawn over the card; never takes clicks away from it.
            VisualElement frame = new VisualElement();
            frame.AddToClassList("card-selection-frame");
            frame.pickingMode = PickingMode.Ignore;

            newCardView.cardContainer.RegisterOnSelected(() => CardViewClicked(newCardView));
            newCardView.cardContainer.RegisterOnFocused(() => {
                cardWrapper.AddToClassList(WrapperFocusedClass);
                tooltipController.DisplayTooltip(newCardView.cardContainer, card.cardType.GetTooltip(), TooltipContext.CardSelection);
            });
            newCardView.cardContainer.RegisterOnUnfocused(() => {
                cardWrapper.RemoveFromClassList(WrapperFocusedClass);
                tooltipController.DestroyTooltip(newCardView.cardContainer);
            });

            cardWrapper.Add(newCardView.cardContainer);
            cardWrapper.Add(frame);
            cardViews.Add(newCardView);
            wrapperFor[newCardView] = cardWrapper;
            this.cardContainer.Add(cardWrapper);
        }

        this.promptTextLabel.text = promptText;
        UpdateSelectionUI();
        FocusManager.Instance.RegisterFocusables(uiDoc);
        FocusManager.Instance.LockFocusables();
    }

    private void InitFields() {
        cardsSelected = new List<CardView>();
        VisualElement root = uiDoc.rootVisualElement;
        this.companionArea = root.Q("companion-area");
        this.cardContainer = root.Q("card-scroll-view-container");
        this.confirmExitButton = root.Q<Button>("confirm-exit-button");
        this.promptTextLabel = root.Q<Label>("prompt-text-label");
        this.confirmExitButton.RegisterOnSelected(() => ExitView());
    }

    private void CardViewClicked(CardView cardView) {
        if (!IsSelectable) return;

        if (cardsSelected.Contains(cardView)) {
            cardsSelected.Remove(cardView);
        } else if (!AtMax) {
            cardsSelected.Add(cardView);
        } else if (SingleSelect) {
            // Only one can be picked: move the pick to this card.
            cardsSelected.Clear();
            cardsSelected.Add(cardView);
        } else {
            ShowWarning("You can only select " + CardCount(maxSelections));
            return;
        }

        UpdateSelectionUI();
    }

    private void UpdateSelectionUI() {
        bool atMax = AtMax;
        foreach (CardView cardView in cardViews) {
            if (!wrapperFor.TryGetValue(cardView, out VisualElement wrapper)) continue;
            bool selected = cardsSelected.Contains(cardView);
            wrapper.EnableInClassList(WrapperSelectedClass, selected);
            wrapper.EnableInClassList(WrapperUnavailableClass, atMax && !selected && !SingleSelect);
        }

        int count = cardsSelected.Count;
        bool enoughSelected = count >= minSelections;

        if (!IsSelectable) {
            confirmExitButton.text = "Close";
        } else if (count == 0 && minSelections <= 0) {
            confirmExitButton.text = "Skip";
        } else {
            confirmExitButton.text = "Confirm";
        }
        // Still clickable when greyed out, so pressing it explains what's missing.
        confirmExitButton.EnableInClassList(ButtonDisabledClass, IsSelectable && !enoughSelected);
    }

    private void ExitView() {
        if (cardsSelected.Count < minSelections) {
            ShowWarning("Select " + CardCount(minSelections - cardsSelected.Count, "more "));
            return;
        }

        cardViews.Clear();
        tooltipController.DestroyAllTooltips();

        List<Card> outputCards = new List<Card>();
        foreach (CardView cardView in cardsSelected) {
            outputCards.Add(cardView.cardInstance);
        }
        if(cardsSelectedHandler != null) {
            // need to invoke the handler even if we have no cards,
            // in cases like scrying on an empty deck, so the targetting manager knows
            // that it can proceed
            cardsSelectedHandler.Invoke(outputCards, companion);
        }
        FocusManager.Instance.UnregisterFocusables(uiDoc);
        FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        FocusManager.Instance.UnlockFocusables();
        Destroy(this.gameObject);
    }

    private static string CardCount(int n, string adjective = "") {
        return n + " " + adjective + (n == 1 ? "card" : "cards");
    }

    private void SetPromptVisible(bool visible) {
        promptTextLabel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ShowWarning(string text) {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = changePromptText(text);
        StartCoroutine(currentCoroutine);
    }

    private IEnumerator changePromptText(string newText) {
        SetPromptVisible(true);
        promptTextLabel.text = newText;
        promptTextLabel.AddToClassList(PromptWarningClass);
        yield return new WaitForSeconds(1.5f);
        promptTextLabel.RemoveFromClassList(PromptWarningClass);
        promptTextLabel.text = promptText;
        if (string.IsNullOrEmpty(promptText)) SetPromptVisible(false);
        currentCoroutine = null;
    }
}
