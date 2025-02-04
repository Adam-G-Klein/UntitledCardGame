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
    private Card cardInstance = null;
    public Color modifiedManaCostColor = Color.green;

    private float SCREEN_WIDTH_PERCENT = 0.11f;
    private float RATIO = 1.4f;
    
    // fillUIDocument - in some cases (like the current shop and the intro screen) we don't want this card to 
    // take up its whole ui doc. In others, like combat (where the card is in worldspace splatted to a texture)
    // we do.
    public CardView(CardType cardType, CompanionTypeSO companionType, bool fillUIDocument = false) {
        cardContainer = makeCardView(cardType, companionType, fillUIDocument);
    }

    public CardView(CardType cardType, Sprite genericSprite, bool fillUIDocument = false) {
        cardContainer = makeCardView(cardType, null, fillUIDocument, genericSprite);
    }

    public CardView(Card card, CompanionTypeSO companionType, bool fillUIDocument = false) {
        cardContainer = makeCardView(card.cardType, companionType, fillUIDocument);
        this.cardInstance = card;
    }

    private VisualElement makeCardView(CardType card, CompanionTypeSO companionType, bool fillUIDocument = false, Sprite genericSprite = null) {
        Debug.Log("goobie woobie");
        Debug.Log(companionType);
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
        if (companionType != null) {
            companionImage.style.backgroundImage = new StyleBackground(companionType.sprite);
        } else {
            companionImage.style.backgroundImage = new StyleBackground(genericSprite);
        }
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

        if(!fillUIDocument) {
            Tuple<int, int> cardWidthHeight = GetWidthAndHeight();
            container.style.width = cardWidthHeight.Item1;
            container.style.height = cardWidthHeight.Item2;
        }

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