using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Linq;
using Unity.VisualScripting;
using System.Collections.Generic;


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

    private static float SCREEN_WIDTH_PERCENT = 0.11f;
    private static float RATIO = 832f/543f;
    private CardType cardType;

    private Card.CardRarity rarity = Card.CardRarity.NONE;
    // fillUIDocument - in some cases (like the current shop and the intro screen) we don't want this card to
    // take up its whole ui doc. In others, like combat (where the card is in worldspace splatted to a texture)
    // we do.
    private static Dictionary<String, Sprite> silhouetteCache = new Dictionary<String, Sprite>();

    public CardView(CardType cardType, CompanionTypeSO companionType, Card.CardRarity rarity, bool cardInShop = false, PackSO packSO = null, bool hideBackground = false) {
        this.cardType = cardType;
        this.rarity = rarity;
        cardContainer = makeCardView(cardType, companionType, cardInShop, packSO, hideBackground);
    }

    public CardView(Card card, CompanionTypeSO companionType, bool cardInShop = false) {
        this.cardType = card.cardType;
        rarity = card.shopRarity;
        cardContainer = makeCardView(card.cardType, companionType, cardInShop);
        this.cardInstance = card;
    }

    private VisualElement makeCardView(CardType card, CompanionTypeSO companionType, bool cardInShop = false, PackSO packSO = null, bool hideBackground = false) {
        VisualTreeAsset visualTreeAsset = GameplayConstantsSingleton.Instance.gameplayConstants.cardTemplate;
        VisualElement container = visualTreeAsset.CloneTree();
        container.focusable = true;
        container.AddToClassList("card-container");
        container.AddToClassList("focusable");
        cardFocusable = container.AsFocusable();

        Label manaContainer = container.Q<Label>("manaLabel");
        setManaCost(manaContainer, card);

        /*VisualElement rarityGem = container.Q("rarityGem");
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
        }*/

        Label title = container.Q<Label>("cardName");
        title.text = card.GetName();
        Label description = container.Q<Label>("cardDesc");
        description.text = card.GetDescription();

        VisualElement iconDescContainer = container.Q<VisualElement>("cardIconDesc");
        if (card.HasIconDescription())
        {
            // Override the text description with the icon description.
            description.visible = false;

            // It would be helpful to parse the the Icon description and divide it into separate lines.
            iconDescContainer.visible = true;
            iconDescContainer.Clear();
            // Create the first row.
            VisualElement row = new VisualElement();
            row.AddToClassList("iconDescRow");
            iconDescContainer.Add(row);

            List<DescriptionToken> tokens = card.GetIconDescriptionTokens();
            for (int i = 0; i < tokens.Count; i++)
            {
                DescriptionToken token = tokens[i];
                switch (token.tokenType)
                {
                    case DescriptionToken.TokenType.Icon:
                        VisualElement icon = new VisualElement();
                        icon.AddToClassList("iconDescSprite");
                        if (cardInShop)
                        {
                            icon.AddToClassList("iconDescSpriteShop");
                        }
                        Sprite s = GameplayConstantsSingleton.Instance.gameplayConstants.descriptionIconSprites[token.icon];
                        icon.style.backgroundImage = new StyleBackground(s.texture);
                        row.Add(icon);
                        break;
                    case DescriptionToken.TokenType.Text:
                        Label textLbl = new Label();
                        textLbl.AddToClassList("iconDescLabel");
                        textLbl.text = token.text;
                        textLbl.name = "iconDescText" + i;
                        row.Add(textLbl);
                        break;
                    case DescriptionToken.TokenType.NewLine:
                        // Start a new row.
                        row = new VisualElement();
                        row.AddToClassList("iconDescRow");
                        iconDescContainer.Add(row);
                        break;
                }
            }
        } else
        {
            iconDescContainer.visible = false;
        }

        /*if (packSO != null)
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
        }*/

        VisualElement cardBackground = container.Q("cardBackground");
        if (hideBackground)
        {
            cardBackground.visible = false;
        } else {
            if (companionType != null) {
                Sprite silhouette = GetSilhouette(companionType.fullSprite, companionType.companionName);
                cardBackground.style.backgroundImage = new StyleBackground(silhouette.texture);
            } else if (packSO != null && packSO.packIcon != null)
            {
                Sprite silhouette = GetSilhouette(packSO.packIcon, packSO.packName);
                cardBackground.style.backgroundImage = new StyleBackground(silhouette.texture);
            }
            else {
                Sprite silhouette = GetSilhouette(GameplayConstantsSingleton.Instance.gameplayConstants.neutralPackIcon, "neutral");
                cardBackground.style.backgroundImage = new StyleBackground(silhouette.texture);
            }   
        }

        VisualElement rarityIndicator = container.Q("rarityIndicator");
        Sprite rarityIndicatorSprite = null;
        switch(rarity) {
            case Card.CardRarity.NONE:
            case Card.CardRarity.COMMON:
                if (cardType.packFrom != null) rarityIndicatorSprite = cardType.packFrom.commonIcon;
                else if (companionType != null) rarityIndicatorSprite = companionType.pack.commonIcon;
                else rarityIndicatorSprite = GameplayConstantsSingleton.Instance.gameplayConstants.neutralCommonIcon;
                break;
            case Card.CardRarity.UNCOMMON:
                if (cardType.packFrom != null) rarityIndicatorSprite = cardType.packFrom.uncommonIcon;
                else if (companionType != null) rarityIndicatorSprite = companionType.pack.uncommonIcon;
                else rarityIndicatorSprite = GameplayConstantsSingleton.Instance.gameplayConstants.neutralUncommonIcon;
                break;
            case Card.CardRarity.RARE:
                if (cardType.packFrom != null) rarityIndicatorSprite = cardType.packFrom.rareIcon;
                else if (companionType != null) rarityIndicatorSprite = companionType.pack.rareIcon;
                else rarityIndicatorSprite = GameplayConstantsSingleton.Instance.gameplayConstants.neutralRareIcon;
                break;
        }
        rarityIndicator.style.backgroundImage = new StyleBackground(rarityIndicatorSprite);

        VisualElement cardBackgroundTexture = container.Q("cardBackgroundTexture");
        float xScale = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        float yScale = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;

        // Set the scale of the cardBackgroundTexture
        cardBackgroundTexture.style.scale = new StyleScale(new Scale(new Vector2(xScale, yScale)));

        Label cardTypeLabel = container.Q<Label>("cardTypeLabel");
        cardTypeLabel.text = cardTypeLabel.text = System.Text.RegularExpressions.Regex.Replace(
            card.cardCategory.ToString(), "(?<!^)([A-Z])", " $1"
        ).Trim();

        if (cardInShop)
        {
            Tuple<int, int> cardWidthHeight = GetWidthAndHeight();
            container.style.width = cardWidthHeight.Item1;
            container.style.height = cardWidthHeight.Item2;

            title.AddToClassList("cardNameShop");
            description.AddToClassList("cardDescriptionShop");
            cardTypeLabel.AddToClassList("cardTypeLabelShop");
            manaContainer.AddToClassList("manaLabelShop");
            container.Q("cardOutline").AddToClassList("cardOutlineShop");
            iconDescContainer.AddToClassList("iconDescContainerShop");
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
        Label label = cardContainer.Q<Label>("cardDesc");
        label.text = newText;
        label.MarkDirtyRepaint();
    }

    public void UpdateCardIconDescription(List<DescriptionToken> newIconDesc) {
        VisualElement iconDescContainer = cardContainer.Q<VisualElement>("cardIconDesc");

        for (int i = 0; i < newIconDesc.Count; i++)
        {
            DescriptionToken token = newIconDesc[i];
            if (token.tokenType == DescriptionToken.TokenType.Text)
            {
                string name = "iconDescText" + i;
                cardContainer.Q<Label>(name).text = token.text;
            }
        }

        iconDescContainer.MarkDirtyRepaint();
    }

    public void UpdateManaCost() {
        Label manaLabel = cardContainer.Q<Label>(null, "manaLabel");
        setManaCost(manaLabel, cardInstance.cardType);
        manaLabel.MarkDirtyRepaint();
    }

    public void SetHighlight(bool isHighlightVisible) {
        if (cardContainer == null) return;
        VisualElement ve = cardContainer.Q<VisualElement>("greenBorder");
        if (isHighlightVisible) {
            //UpdateCardText("active");
            ve.visible = true;
        } else {
            //UpdateCardText("inactive");
            ve.visible = false;
        }
        ve.MarkDirtyRepaint();
    }

    public void SetLocked() {
        VisualElement lockedIndicator = cardContainer.Q<VisualElement>("card-locked-indicator");
        lockedIndicator.style.display = DisplayStyle.Flex;
    }

    public static Tuple<int, int> GetWidthAndHeight() {
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

    public CardType GetCardType()
    {
        return cardType;
    }

    public static Sprite GetSilhouette(Sprite originalSprite, String UID, float alpha = 0.4f)
    {
        Sprite original = originalSprite;
        // Check if we've already processed this sprite
        if (silhouetteCache.ContainsKey(UID))
            return silhouetteCache[UID];

        Sprite silhouette = CreateSilhouette(original, alpha);

        // Step 7: Cache it so we don't have to do this again
        silhouetteCache[UID] = silhouette;

        return silhouette;
    }

    public static Sprite CreateSilhouette(Sprite original, float alpha)
    {
        // Step 1: Get the original texture from the sprite
        Texture2D originalTex = original.texture;

        // Step 2: Create a new texture with same dimensions
        Texture2D newTex = new Texture2D(
            (int)original.textureRect.width,   // Width of the sprite's region in the texture
            (int)original.textureRect.height,  // Height of the sprite's region
            TextureFormat.RGBA32,              // Color format (Red, Green, Blue, Alpha - 32 bits)
            false                              // No mipmaps (we don't need them for UI)
        );

        // Step 3: Get the pixels from the SPECIFIC REGION of the texture that this sprite uses
        // (Important because sprite atlases can have multiple sprites in one texture)
        Color[] pixels = originalTex.GetPixels(
            (int)original.textureRect.x,      // Start X in texture
            (int)original.textureRect.y,      // Start Y in texture
            (int)original.textureRect.width,  // How many pixels wide
            (int)original.textureRect.height  // How many pixels tall
        );

        // Step 4: Process each pixel - convert to black with modified alpha
        for (int i = 0; i < pixels.Length; i++)
        {
            // This is doing exactly what the shader fragment shader did:
            // Keep the original alpha, multiply by our desired alpha, make RGB black
            pixels[i] = new Color(
                0,                      // R = black
                0,                      // G = black
                0,                      // B = black
                pixels[i].a * alpha     // A = original alpha × our control value
            );
        }

        // Step 5: Write the processed pixels into our new texture
        newTex.SetPixels(pixels);
        newTex.Apply();  // Actually upload to GPU memory

        // Step 6: Create a new sprite from this texture
        return Sprite.Create(
            newTex,                                          // The texture we just created
            new Rect(0, 0, newTex.width, newTex.height),   // Use the entire texture
            new Vector2(0.5f, 0.5f),                       // Pivot point (center)
            original.pixelsPerUnit                          // Match original's pixel density
        );
    }
}