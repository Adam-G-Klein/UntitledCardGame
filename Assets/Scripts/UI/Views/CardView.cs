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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

public class CardView {
    public VisualElement cardContainer;
    // TODO, could require the stylesheet in the constructor and fetch these from there
    public static int CARD_DESC_SIZE = 12; //px
    public static int CARD_TITLE_SIZE = 22; //px
    public static int CARD_DESC_MAX_FULL_SIZE_CHARS = 18; // guess
    public static int CARD_TITLE_MAX_FULL_SIZE_CHARS = 8; // guess
    private Card cardInstance = null;
    public Color modifiedManaCostColor = Color.green;
    
    public CardView(CardType cardType, CompanionTypeSO companionType) {
        cardContainer = makeWorldspaceCardView(cardType, companionType);
    }

    public CardView(Card card, CompanionTypeSO companionType) {
        cardContainer = makeWorldspaceCardView(card.cardType, companionType);
        this.cardInstance = card;
    }

    private VisualElement makeWorldspaceCardView(CardType card, CompanionTypeSO companionType) {
        var container = new VisualElement();
        container.AddToClassList("card-container");

        var greenBorder = new VisualElement();
        greenBorder.AddToClassList("green-card-border");
        greenBorder.visible = false;
        container.Add(greenBorder);

        var image = new VisualElement();
        image.AddToClassList("card-image");
        image.style.backgroundImage = new StyleBackground(card.Artwork);
        container.Add(image);

        var companionImage = new VisualElement();
        companionImage.AddToClassList("companion-image");
        companionImage.style.backgroundImage = new StyleBackground(companionType.sprite);
        container.Add(companionImage);

        var name = new Label();
        name.AddToClassList("card-title-label");
        name.text = card.Name;
        name.style.fontSize = getTitleFontSize(card.Name);
        container.Add(name);

        var desc = new Label();
        desc.AddToClassList("card-desc-label");

        string description = card.Description;
        foreach (var defaultValue in card.defaultValues){
            string styledValue = $"<b>{defaultValue.value}</b>";
            description = description.Replace($"{{{defaultValue.key}}}", styledValue);
        }
        desc.text = description;
        desc.style.fontSize = getDescFontSize(card.Description);
        container.Add(desc);

        var manaContainer = new VisualElement();
        manaContainer.AddToClassList("mana-container");

        var manaCost = new Label();
        setManaCost(manaCost, card);
        manaContainer.Add(manaCost);
        container.Add(manaContainer);

        return container;
    }

    private void setManaCost(Label manaCost, CardType card) {
        manaCost.AddToClassList("mana-card-label");
        if (cardInstance == null) {
             manaCost.text = card.Cost.ToString();
        } else {

            int manaCostValue = cardInstance.GetManaCost();
            manaCost.text = manaCostValue.ToString();
            if (manaCostValue < card.Cost) {
                manaCost.AddToClassList("mana-reduced");
            } 
        }
        
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

    public void UpdateCardText(string newText) {
        Label label = cardContainer.Q<Label>(null, "card-desc-label");
        label.text = newText;
        label.MarkDirtyRepaint();
    }

    public void UpdateManaCost() {
        Label manaLabel = cardContainer.Q<Label>(null, "mana-card-label");
        setManaCost(manaLabel, cardInstance.cardType);
        manaLabel.MarkDirtyRepaint();
    }

    public void SetHighlight(bool isHighlightVisible) {
        if (cardContainer == null) return;
        VisualElement ve = cardContainer.Q<VisualElement>(null, "green-card-border");
        if (isHighlightVisible) {
            //UpdateCardText("active");
            ve.visible = true;
        } else {
            //UpdateCardText("inactive");
            ve.visible = false;
        }
        ve.MarkDirtyRepaint();
    }
}