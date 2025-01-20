using UnityEngine;
using System;
using UnityEngine.UIElements;


// This class isn't expected to have a delegate view or delegate controller because it'll be wrapped
// by one that does
public class CardView {
    public VisualElement cardContainer;
    // TODO, could require the stylesheet in the constructor and fetch these from there
    public static int CARD_DESC_SIZE = 26; //px
    public static int CARD_TITLE_SIZE = 44; //px
    public static int CARD_DESC_MAX_FULL_SIZE_CHARS = 18; // guess
    public static int CARD_TITLE_MAX_FULL_SIZE_CHARS = 8; // guess

    private float SCREEN_WIDTH_PERCENT = 0.11f;
    private float RATIO = 1.4f;
    
    public CardView(CardType cardType, CompanionTypeSO companionType) {
        cardContainer = makeWorldspaceCardView(cardType, companionType);
    }

    private VisualElement makeWorldspaceCardView(CardType card, CompanionTypeSO companionType) {
        var container = new VisualElement();
        container.AddToClassList("card-container");

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

        Tuple<int, int> cardWidthHeight = GetWidthAndHeight();
        container.style.width = cardWidthHeight.Item1;
        container.style.height = cardWidthHeight.Item2;

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

    private Tuple<int, int> GetWidthAndHeight() {
        int width = (int)(Screen.width * SCREEN_WIDTH_PERCENT);
        int height = (int)(width * RATIO);

        // This drove me insane btw
        #if UNITY_EDITOR
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint windowWidth, out uint windowHeight);
        width = (int)(windowWidth * SCREEN_WIDTH_PERCENT);
        height = (int)(width * RATIO);
        #endif

        return new Tuple<int, int>(width, height);
    }
}