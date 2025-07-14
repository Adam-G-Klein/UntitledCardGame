using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class VictoryView : MonoBehaviour
{
    private UIDocument doc;
    private UIDocumentScreenspace docRenderer;

    private Material mat;
    private CanvasGroup canvasGroup;
    private Button button;

    [SerializeField]
    private float fadeTime = .1f;


    void OnEnable()
    {
        doc = GetComponent<UIDocument>();
        docRenderer = GetComponent<UIDocumentScreenspace>();
        // Set initial alpha to 0
        mat = GetComponent<RawImage>().material;
        mat.SetFloat("_alpha", 0);

        // Ensure the CanvasGroup alpha is set to 0 and blocks raycasts
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;
        button = doc.rootVisualElement.Q<Button>();
        button.RegisterOnSelected(() => {
            button.SetEnabled(false);
            // go to end of run progress scene if there are any achievements to display
            if (ProgressManager.Instance.achievementSOList.Exists(x => x.lockedInProgress < x.target)) {
                SceneManager.LoadScene("EndOfRunProgressScene");
                return;
            }
            // otherwise go to main menu
            SceneManager.LoadScene("MainMenu");
        });
        FocusManager.Instance.RegisterFocusableTarget(button.AsFocusable());
    }

    public void Setup(List<Companion> companions) {
        VisualElement container = doc.rootVisualElement.Q(name: "victory-companions");
        for (int i = 0; i < companions.Count; i++) {
            Companion companion = companions[i];
            VisualElement companionContainer = new VisualElement();
            companionContainer.AddToClassList("victory-companion-container");
            CompanionTypeSO companionType = companions[i].companionType;
            EntityView entityView = new EntityView(companions[i], 0, false);
            //entityView.entityContainer.AddToClassList("compendium-item-container");
            VisualElement portraitContainer = entityView.entityContainer.Q(className: "entity-portrait");
            portraitContainer.style.backgroundImage = new StyleBackground(companionType.sprite);
            companionContainer.Add(entityView.entityContainer);
            
            List<Card> cards = new List<Card>();
            companion.deck.cards.Sort((x, y) => {
                if (x.shopRarity == y.shopRarity) return 0;
                if (x.shopRarity == Card.CardRarity.RARE) return -1;
                if (y.shopRarity == Card.CardRarity.RARE) return 1;
                if (x.shopRarity == Card.CardRarity.UNCOMMON) return -1;
                if (y.shopRarity == Card.CardRarity.UNCOMMON) return 1;
                return 0;
            });
            companion.deck.cards.Reverse();

            VisualElement cardsContainer = new VisualElement();
            cardsContainer.AddToClassList("victory-companion-cards-container");
            
            List<Card> deckToDisplay = companion.deck.cards;
            for (int j = 0; j < deckToDisplay.Count; j++) {
                VisualElement cardContainer = new CardView(deckToDisplay[j], companionType, true).cardContainer;
                cardContainer.style.position = Position.Absolute;
                cardContainer.style.top = cardContainer.style.height.value.value / 30 * j;
                cardsContainer.Add(cardContainer);
            }
            companionContainer.Add(cardsContainer);
            container.Add(companionContainer);
        }
    }

    public void Show() {
        // Ensure the initial alpha is set to 0 before starting the fade-in
        canvasGroup.blocksRaycasts = true;
        mat.SetFloat("_alpha", 0);
        canvasGroup.alpha = 0;
        LeanTween.value(gameObject, 0, 1, fadeTime)
            .setOnUpdate((float val) => {
                mat.SetFloat("_alpha", val);
                canvasGroup.alpha = val;
            });
    }


}
