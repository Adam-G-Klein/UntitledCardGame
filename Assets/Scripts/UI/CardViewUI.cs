using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardViewUI : MonoBehaviour
{
    public GameObject cardPrefab;
    public int cardPadding;
    public int cardsPerRow;

    public void Setup(List<Card> cards) {
        int i = 0;
        foreach (Card card in cards) {
            createCard(card, i);
            i++;
        }
    }

    private void createCard(Card card, int i) {
        // Start at the upper left and move to the right
        // then move down, creating cards
        GameObject cardObj = GameObject.Instantiate(
            cardPrefab,
            Vector3.zero,
            Quaternion.identity,
            transform);

        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        cardDisplay.cardInfo = card;
        
        RectTransform parentTransform = gameObject.GetComponent<RectTransform>();
        RectTransform cardTransform = cardObj.GetComponent<RectTransform>();

        int numberInRow = i % cardsPerRow;
        float xPos = (parentTransform.rect.width / 2) * -1;
        xPos += (cardTransform.rect.width / 2);
        xPos += cardTransform.rect.width * numberInRow;
        xPos += cardPadding * numberInRow;
        float yPos = (parentTransform.rect.height / 2);
        yPos -= (cardTransform.rect.height / 2);
        yPos -= (cardTransform.rect.height * Mathf.FloorToInt(i / cardsPerRow));
        yPos -= cardPadding * Mathf.FloorToInt(i / cardsPerRow);
        cardTransform.localPosition = new Vector3(xPos, yPos, 0);
    }

    public void exitView() {
        Destroy(this.gameObject);
    }
}
