using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/****


DEPRECATED

IN FAVOR OF CARDSELECTIONVIEW.CS


*****/ 
public class CardViewUI : MonoBehaviour
{
    [SerializeField]
    private GameObject cardPrefab;
    [SerializeField]
    private RectTransform cardPrefabParent;
    [SerializeField]
    private TMP_Text prompt;
    [SerializeField]
    private CardListEvent cardsSelectedEvent;
    private string promptText;
    private IEnumerator currentCoroutine = null;

    private int minSelections;
    private int maxSelections;
    private List<UICard> selectedCards = new List<UICard>();
    [SerializeField]
    private GameObject button;

    private void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 10;   
        canvas.sortingLayerName = "Tutorial";
    }

    public void Setup(List<Card> cards, int minSelections, string promptText, int maxSelections = -1, CompanionInstance companionFrom = null) {
        EnemyEncounterManager.Instance.SetInToolTip(true);
        foreach (Card card in cards) {
            createCard(card);
        }
        this.promptText = promptText;
        this.prompt.text = promptText;
        this.minSelections = minSelections;
        this.maxSelections = maxSelections;
        button.GetComponent<RectTransform>().localScale = new Vector3((minSelections > 0 || maxSelections > 0) ? -1 : 1, 1, 1);
        EnemyEncounterViewModel.Instance.SetInDeckView(true);
    }

    // Can replace with just requesting a mapping and instantiating at that location
    private void createCard(Card card) {
        GameObject cardObj = GameObject.Instantiate(
            cardPrefab,
            Vector3.zero,
            Quaternion.identity,
            cardPrefabParent);

        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        cardDisplay.Initialize(card);
        UICard uiCard = cardObj.GetComponent<UICard>();
        uiCard.onclickEvent.AddListener(uiCardSelected);
    }

    private void uiCardSelected(UICard card) {
        if (card.selected) {
            card.selected = false;
            card.selectionFrame.SetActive(false);
            selectedCards.Remove(card);
            return;
        }

        if (maxSelections != -1 && selectedCards.Count >= maxSelections) {
            return;
        }

        card.selected = true;
        card.selectionFrame.SetActive(true);
        selectedCards.Add(card);
    }

    public void exitView() {
        if (selectedCards.Count < minSelections) {
            if (currentCoroutine == null) {
                currentCoroutine = changePromptText("Please select " + 
                    (minSelections - selectedCards.Count) + " more cards");
                StartCoroutine(currentCoroutine);
            }
            return;
        }
        List<Card> outputCards = new List<Card>();
        foreach (UICard uiCard in selectedCards) {
            outputCards.Add(uiCard.card);
        }
        cardsSelectedEvent.Raise(new CardListEventInfo(outputCards));
        Destroy(this.gameObject);
        EnemyEncounterViewModel.Instance.SetInDeckView(false);
        EnemyEncounterManager.Instance.SetInToolTip(false);
    }

    private IEnumerator changePromptText(string newText) {
        prompt.text = newText;
        yield return new WaitForSeconds(1.5f);
        prompt.text = promptText;
        currentCoroutine = null;
        yield return null;
    }
}
