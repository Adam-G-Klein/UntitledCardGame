using UnityEngine;
using System;
using UnityEngine.UIElements;


// This class isn't expected to have a delegate view or delegate controller because it'll be wrapped
// by one that does
public class CardView {
    public VisualElement cardContainer;
    public VisualElementFocusable cardFocusable;
    // TODO, could require the stylesheet in the constructor and fetch these from there
    public static int CARD_DESC_SIZE_FULL_TEXTURE = 40; //px
    public static int CARD_TITLE_SIZE_FULL_TEXTURE = 60; //px
    public static int CARD_DESC_SIZE_SHOP_SCREEN = 16; //px
    public static int CARD_TITLE_SIZE_SHOP_SCREEN = 18; //px
    public static int CARD_DESC_MAX_FULL_SIZE_CHARS_FULL_TEXTURE = 60; // guess
    public static int CARD_TITLE_MAX_FULL_SIZE_CHARS_FULL_TEXTURE = 7; // guess
    public static int CARD_DESC_MAX_FULL_SIZE_CHARS_SHOP_SCREEN = 30; // guess
    public static int CARD_TITLE_MAX_FULL_SIZE_CHARS_SHOP_SCREEN = 5; // guess
    public static int COMPANION_AND_MANA_INDICATOR_WIDTH_HEIGHT_COMBAT = 80;
    public static int COMPANION_AND_MANA_INDICATOR_WIDTH_HEIGHT_SHOP = 30;

    // Trying to define single pixel sizes for cards to bring some conformity to the look. Changing text sizes reads a little messy imo
    private static int CARD_TITLE_COMBAT = 32;
    private static int CARD_DESC_COMBAT = 28;
    private static int CARD_TITLE_SHOP = 12;
    private static int CARD_DESC_SHOP = 10; 

    public Card cardInstance = null;
    public Color modifiedManaCostColor = Color.green;

    private float SCREEN_WIDTH_PERCENT = 0.11f;
    private float RATIO = 1.53f;
    private CardType cardType;

    private Card.CardRarity rarity = Card.CardRarity.NONE;
    // fillUIDocument - in some cases (like the current shop and the intro screen) we don't want this card to
    // take up its whole ui doc. In others, like combat (where the card is in worldspace splatted to a texture)
    // we do.
    public CardView(CardType cardType, CompanionTypeSO companionType, Card.CardRarity rarity, bool cardInShop = false, PackSO packSO = null) {
        this.rarity = rarity;
        cardContainer = makeCardView(cardType, companionType, cardInShop, packSO);
    }

    public CardView(Card card, CompanionTypeSO companionType, bool cardInShop = false) {
        this.cardType = card.cardType;
        rarity = card.shopRarity;
        cardContainer = makeCardView(card.cardType, companionType, cardInShop);
        this.cardInstance = card;
    }

    private VisualElement makeCardView(CardType card, CompanionTypeSO companionType, bool cardInShop = false, PackSO packSO = null) {
        VisualTreeAsset visualTreeAsset = GameplayConstantsSingleton.Instance.gameplayConstants.cardTemplate;
        VisualElement container = visualTreeAsset.CloneTree();
        container.AddToClassList("card-container");
        cardFocusable = container.AsFocusable();

        Label manaContainer = container.Q<Label>("manaLabel"); 
        setManaCost(manaContainer, card);
        if (cardInShop)
        {
            manaContainer.AddToClassList("smaller-mana-label");
        }

        VisualElement rarityGem = container.Q("rarityGem");
        switch (rarity) {
            case Card.CardRarity.COMMON:
            case Card.CardRarity.NONE:
                rarityGem.AddToClassList("rarity-gem-common");
                break;
            case Card.CardRarity.UNCOMMON:
                rarityGem.AddToClassList("rarity-gem-uncommon");
                break;
            case Card.CardRarity.RARE:
                rarityGem.AddToClassList("rarity-gem-rare");
                break;
        }

        Label title = container.Q<Label>("cardName");
        //int fontSize = getTitleFontSize(card.Name, cardInShop);
        int fontSize = cardInShop ? CARD_TITLE_SHOP : CARD_TITLE_COMBAT;
        if (!cardInShop) title.text = $"<line-height={75}%>{card.Name}</line-height>";
        else title.text = card.Name;
        title.style.fontSize = fontSize;

        Label description = container.Q<Label>("cardDesc");
        string desc = card.GetDescription();
        //fontSize = getDescFontSize(desc, cardInShop);
        fontSize = cardInShop ? CARD_DESC_SHOP : CARD_DESC_COMBAT;
        //if (!cardInShop) description.text = $"<line-height={60}%>{desc}</line-height>";
        description.text = desc;
        description.style.fontSize = fontSize;

        Label companionNameLabel = container.Q<Label>("companionNameLabel");
        if (!cardInShop) companionNameLabel.AddToClassList("card-type-label-large");
        if (packSO != null)
        {
            container.Q("cardBackground").style.backgroundColor = packSO.packColor;
            companionNameLabel.text = packSO.packName + " Pack";
        }
        else if (companionType != null)
        {
            container.Q("cardBackground").style.backgroundColor = companionType.pack.packColor;
            companionNameLabel.text = companionType.companionName;
        }
        else
        {
            companionNameLabel.text = "ANY";
            companionNameLabel.AddToClassList("card-type-label-any");
        }

        Label cardTypeLabel = container.Q<Label>("cardTypeLabel");
        cardTypeLabel.text = cardTypeLabel.text = System.Text.RegularExpressions.Regex.Replace(
            card.cardCategory.ToString(), "(?<!^)([A-Z])", " $1"
        ).Trim();
        switch (card.cardCategory)
        {
            case CardCategory.Attack:
                cardTypeLabel.AddToClassList("attack-card-label-color");
                break;
            case CardCategory.NonAttack:
                cardTypeLabel.AddToClassList("non-attack-card-label-color");
                break;
            case CardCategory.Status:
                cardTypeLabel.AddToClassList("status-card-label-color");
                break;
        }
        if (!cardInShop) cardTypeLabel.AddToClassList("card-type-label-large");

        if (cardInShop)
        {
            Tuple<int, int> cardWidthHeight = GetWidthAndHeight();
            container.style.width = cardWidthHeight.Item1;
            container.style.height = cardWidthHeight.Item2;
        }
        return container;
    }

    private void setManaCost(Label manaCost, CardType card) {
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

    private int getDescFontSize(string desc, bool cardInShop) {
        int maxChars = cardInShop ? CARD_DESC_MAX_FULL_SIZE_CHARS_SHOP_SCREEN : CARD_DESC_MAX_FULL_SIZE_CHARS_FULL_TEXTURE;
        int fontSize = cardInShop ? CARD_DESC_SIZE_SHOP_SCREEN : CARD_DESC_SIZE_FULL_TEXTURE;

        return UIDocumentUtils.UpdateTextSize(desc, maxChars, fontSize);
    }

    private int getTitleFontSize(string title, bool cardInShop) {
        int maxChars = cardInShop ? CARD_TITLE_MAX_FULL_SIZE_CHARS_SHOP_SCREEN : CARD_TITLE_MAX_FULL_SIZE_CHARS_FULL_TEXTURE;
        int fontSize = cardInShop ? CARD_TITLE_SIZE_SHOP_SCREEN : CARD_TITLE_SIZE_FULL_TEXTURE;

        return UIDocumentUtils.UpdateTextSize(title, maxChars, fontSize, 2);
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

    public CardType GetCardType() {
        return cardType;
    }
}