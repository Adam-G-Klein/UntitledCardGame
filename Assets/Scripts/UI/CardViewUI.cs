using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    private int numberOfSelections;
    private List<UICard> selectedCards = new List<UICard>();

    public void Setup(List<Card> cards, int numSelections, string promptText) {
        foreach (Card card in cards) {
            createCard(card);
        }
        this.promptText = promptText;
        this.prompt.text = promptText;
        this.numberOfSelections = numSelections;
    }

    private void createCard(Card card) {
        GameObject cardObj = GameObject.Instantiate(
            cardPrefab,
            Vector3.zero,
            Quaternion.identity,
            cardPrefabParent);

        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        cardDisplay.cardInfo = card;
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

        if (selectedCards.Count >= numberOfSelections) {
            return;
        }

        card.selected = true;
        card.selectionFrame.SetActive(true);
        selectedCards.Add(card);
    }

    public void exitView() {
        if (selectedCards.Count < numberOfSelections) {
            if (currentCoroutine == null) {
                currentCoroutine = changePromptText("Please select " + 
                    (numberOfSelections - selectedCards.Count) + " more cards");
                StartCoroutine(currentCoroutine);
            }
            return;
        }
        List<Card> outputCards = new List<Card>();
        foreach (UICard uiCard in selectedCards) {
            outputCards.Add(uiCard.card);
        }
        if (numberOfSelections != 0) {
            cardsSelectedEvent.Raise(new CardListEventInfo(outputCards));
        }
        Destroy(this.gameObject);
    }

    private IEnumerator changePromptText(string newText) {
        prompt.text = newText;
        yield return new WaitForSeconds(1.5f);
        prompt.text = promptText;
        currentCoroutine = null;
        yield return null;
    }
}
