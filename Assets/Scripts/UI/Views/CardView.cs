using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;


// This class isn't expected to have a delegate view or delegate controller because it'll be wrapped
// by one that does
public class CardView {
    public VisualElement cardContainer;
    public VisualElementFocusable cardFocusable;

    public Card cardInstance = null;
    public Color modifiedManaCostColor = Color.green;

    private static float BASE_WIDTH = 543f;
    private static float BASE_HEIGHT = 832f;
    private CardType cardType;

    private VisualElement cardRoot;

    private Card.CardRarity rarity = Card.CardRarity.NONE;

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
        container.AddToClassList("card-view-container");
        container.AddToClassList("focusable");
        cardFocusable = container.AsFocusable();
        container.RegisterCallback<GeometryChangedEvent>(ScaleCardRootToFitContainer);

        cardRoot = container.Children().First();
        cardRoot.style.display = DisplayStyle.None;

        Label manaContainer = container.Q<Label>("manaLabel");
        setManaCost(manaContainer, card);

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
            // iconDescContainer.visible = true;
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

        // TODO: eek we need a neutral card back as well for cards without a PackSO :thinking:
        PackSO packFrom = packSO;
        if (packSO == null) {
            if (card.packFrom != null)  packFrom = card.packFrom;
            else if (companionType.pack != null) packFrom = companionType.pack ;
        }

        if (packFrom != null)
        {
            VisualElement backgroundTexture = container.Q<VisualElement>("cardBackgroundTexture");
            backgroundTexture.style.backgroundImage = new StyleBackground(packFrom.cardBack);
        }

        Label cardTypeLabel = container.Q<Label>("cardTypeLabel");
        cardTypeLabel.text = cardTypeLabel.text = System.Text.RegularExpressions.Regex.Replace(
            card.cardCategory.ToString(), "(?<!^)([A-Z])", " $1"
        ).Trim();

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

    private void ScaleCardRootToFitContainer(GeometryChangedEvent evt) {
        cardContainer.UnregisterCallback<GeometryChangedEvent>(ScaleCardRootToFitContainer);

        float width = cardContainer.resolvedStyle.width;
        float height = cardContainer.resolvedStyle.height;

        float scaleX = width / BASE_WIDTH;
        float scaleY = height / BASE_HEIGHT;
        float uniformScale = Mathf.Min(scaleX, scaleY);

        cardRoot.transform.scale = new Vector3(uniformScale, uniformScale, 1f);
        cardRoot.style.display = DisplayStyle.Flex;
    }

    public CardType GetCardType()
    {
        return cardType;
    }
}