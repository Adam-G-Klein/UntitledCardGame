using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class CompanionDebugMenu : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool isEnabled = false;
    [SerializeField] private GameObject buttonsGameObject;
    [SerializeField] private CompanionInstance companionInstance;
    [SerializeField] private CardViewUI cardViewUIPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (isEnabled == false) {
            gameObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        bool areButtonsActive = buttonsGameObject.activeSelf;
        buttonsGameObject.SetActive(!areButtonsActive);
    }

    public void ViewDrawPile() {
        List<Card> cardsToShow = companionInstance.deckInstance.drawPile;
        SpawnCardUI(cardsToShow);
    }

    public void ViewDiscardPile() {
        List<Card> cardsToShow = companionInstance.deckInstance.discardPile;
        SpawnCardUI(cardsToShow);
    }

    public void ViewExhaustPile() {
        List<Card> cardsToShow = companionInstance.deckInstance.exhaustPile;
        SpawnCardUI(cardsToShow);
    }

    public void ViewInHand() {
        List<Card> cardsToShow = companionInstance.deckInstance.inHand;
        SpawnCardUI(cardsToShow);
    }

    public void DrawCard() {
        companionInstance.deckInstance.DealCardsToPlayerHand(1);
    }

    private void SpawnCardUI(List<Card> cards) {
        CardViewUI cardViewUI = GameObject.Instantiate(
            cardViewUIPrefab,
            Vector3.zero,
            Quaternion.identity);
        cardViewUI.Setup(cards, 0, "");
    }
}
