using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public enum CompanionActionType {
    SELECT,
    VIEW_DECK
}

public class UICompanion : MonoBehaviour, IPointerClickHandler
{
    public Companion companion;
    public CompanionEvent companionSelectedEvent;
    public Image image;
    public GameObject companionActionsObject;
    public GameObject buttonPrefab;
    public GameObject deckViewUIPrefab;
    public int buttonPadding;
    [Header("Companion Action Events")]
    public string selectButtonText;
    public string viewDeckButtonText;

    private bool isCompanionActionsVisible = false;

    public void OnPointerClick(PointerEventData eventData) {
        if (isCompanionActionsVisible) {
            companionActionsObject.SetActive(false);
            isCompanionActionsVisible = false;
        } else {
            companionActionsObject.SetActive(true);
            isCompanionActionsVisible = true;
        }
    }

    public void Setup(List<CompanionActionType> actions) {
        image.sprite = companion.companionType.sprite;
        int i = 0;
        foreach (CompanionActionType action in actions) {
            setupActionButton(action, i);
            i++;
        }
    }

    private void setupActionButton(CompanionActionType action, int i) {
        Button button = createButton(i);
        switch(action) {
            case CompanionActionType.SELECT:
                button.GetComponentInChildren<TMP_Text>().text = selectButtonText;
                button.onClick.AddListener(selectButtonOnClick);
            break;

            case CompanionActionType.VIEW_DECK:
                button.GetComponentInChildren<TMP_Text>().text = viewDeckButtonText;
                button.onClick.AddListener(viewDeckButtonOnClick);
            break;
        }
    }

    private Button createButton(int i) {
        GameObject buttonObj = GameObject.Instantiate(
            buttonPrefab,
            companionActionsObject.transform.position,
            Quaternion.identity,
            companionActionsObject.transform);
        RectTransform parentTransform = companionActionsObject
            .GetComponent<RectTransform>();
        RectTransform buttonTransform = buttonObj.GetComponent<RectTransform>();
        // Buttons start at the top of the parent rect, then move down
        float yPos = (parentTransform.rect.height / 2) - (buttonTransform.rect.height / 2);
        yPos -= buttonTransform.rect.height * i;
        if (i != 0) {
            yPos -= buttonPadding * i;
        }
        buttonTransform.localPosition = new Vector3(0, yPos, 0);
        Button button = buttonObj.GetComponent<Button>();
        return button;
    }

    public void selectButtonOnClick() {
        companionSelectedEvent.Raise(this.companion);
    }

    public void viewDeckButtonOnClick() {
        GameObject deckViewUI = GameObject.Instantiate(
                        deckViewUIPrefab,
                        new Vector3(Screen.width / 2, Screen.height / 2, 0),
                        Quaternion.identity);
        CardViewUI cardView = deckViewUI.GetComponent<CardViewUI>();
        cardView.Setup(companion.deck.cards);
    }
}
