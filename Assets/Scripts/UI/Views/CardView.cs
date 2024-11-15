using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Playables;
using Unity.VisualScripting;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

public class CardView {
    public VisualElement cardContainer;
    // TODO, could require the stylesheet in the constructor and fetch these from there
    public static int CARD_DESC_SIZE = 26; //px
    public static int CARD_TITLE_SIZE = 44; //px
    public static int CARD_DESC_MAX_FULL_SIZE_CHARS = 18; // guess
    public static int CARD_TITLE_MAX_FULL_SIZE_CHARS = 8; // guess
    public CardView(CardType cardType) {
        cardContainer = makeWorldspaceCardView(cardType);
    }

    private VisualElement makeWorldspaceCardView(CardType card) {
        var container = new VisualElement();
        container.AddToClassList("card-container");

        var image = new VisualElement();
        image.AddToClassList("card-image");
        image.style.backgroundImage = new StyleBackground(card.Artwork);
        container.Add(image);

        var name = new Label();
        name.AddToClassList("card-title-label");
        name.text = card.Name;
        name.style.fontSize = getTitleFontSize(card.Name);
        container.Add(name);

        var desc = new Label();
        desc.AddToClassList("card-desc-label");
        desc.text = card.Description;
        desc.style.fontSize = getDescFontSize(card.Description);
        container.Add(desc);

        var manaContainer = new VisualElement();
        manaContainer.AddToClassList("mana-container");
    
        var manaCost = new Label();
        manaCost.AddToClassList("mana-card-label");
        manaCost.text = card.Cost.ToString();
        manaContainer.Add(manaCost);
        container.Add(manaContainer);

        return container;
    }

    private int getDescFontSize(string desc) {
        if (desc.Length > CARD_DESC_MAX_FULL_SIZE_CHARS) {
            // Debug.Log("desc.Length: " + desc.Length + " CARD_DESC_MAX_FULL_SIZE_CHARS: " + CARD_DESC_MAX_FULL_SIZE_CHARS + " CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length: " + (CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length) + " CARD_DESC_SIZE: " + CARD_DESC_SIZE + " CARD_DESC_SIZE * (CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length): " + (CARD_DESC_SIZE * (CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length)) + " (int)(CARD_DESC_SIZE * (CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length)): " + (int)(CARD_DESC_SIZE * (CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length)));
            float textSizeRatio = (float) CARD_DESC_MAX_FULL_SIZE_CHARS / (float) desc.Length;
            double scalingRatio = Math.Pow(textSizeRatio, (float)1/ (float)4);
            return (int)Math.Floor(CARD_DESC_SIZE * scalingRatio);
        }
        return CARD_DESC_SIZE;
    }

    private int getTitleFontSize(string title) {
        if (title.Length > CARD_TITLE_MAX_FULL_SIZE_CHARS) {
            float textSizeRatio = (float) CARD_TITLE_MAX_FULL_SIZE_CHARS / (float) title.Length;
            return (int)Math.Floor(CARD_TITLE_SIZE * textSizeRatio);
        }
        return CARD_TITLE_SIZE;
    }
}