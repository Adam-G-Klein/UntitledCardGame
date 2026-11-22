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
    private static float BASE_HEIGHT = 760f;
    private CardType cardType;

    private VisualElement cardRoot;

    private Card.CardRarity rarity = Card.CardRarity.NONE;

    // Matches the greenBorder color in CardTemplateV2.uxml
    private static readonly Color DEFAULT_HIGHLIGHT_COLOR = new Color(0f, 1f, 21f / 255f, 1f);
    private bool effectHighlight = false;
    private bool selectionHighlight = false;
    private Color selectionHighlightColor = DEFAULT_HIGHLIGHT_COLOR;

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

        // TODO: eek we need a neutral card back as well for cards without a PackSO :thinking:
        PackSO packFrom = packSO;
        if (packSO == null) {
            if (card.packFrom != null)  packFrom = card.packFrom;
            else if (companionType != null) packFrom = companionType.pack;
        }

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
            int rowStartTokenIdx = 0;
            for (int i = 0; i < tokens.Count; i++)
            {
                DescriptionToken token = tokens[i];
                int nextNewLineIndex = tokens.FindIndex(i + 1, t => t.tokenType == DescriptionToken.TokenType.NewLine);
                int rowSize = (nextNewLineIndex == -1) ? tokens.Count - rowStartTokenIdx : nextNewLineIndex - rowStartTokenIdx;
                switch (token.tokenType)
                {
                    case DescriptionToken.TokenType.Icon:
                        VisualElement icon = new VisualElement();
                        icon.AddToClassList("iconDescSprite");
                        if (cardInShop)
                        {
                            icon.AddToClassList("iconDescSpriteShop");
                        }
                        Sprite s = GameplayConstantsSingleton.Instance.gameplayConstants.descriptionIconSprites[token.icon].sprite;
                        icon.style.backgroundImage = new StyleBackground(s.texture);
                        row.Add(icon);
                        break;
                    case DescriptionToken.TokenType.Text:
                        Label textLbl = new Label();
                        textLbl.AddToClassList("iconDescLabel");
                        textLbl.text = token.text;
                        textLbl.name = "iconDescText" + i;

                        // Font size logic to make sure the text fits in the row. This is a bit hacky, but it works for now.
                        if (rowSize > 3 || token.text.Length > 2)
                        {
                            textLbl.style.fontSize = 45;
                        }
                        else if (rowSize == 1)
                        {
                            textLbl.style.fontSize = 40;
                        }
                        else
                        {
                            textLbl.style.fontSize = 60;
                        }
                        // Remember the unscaled font size so FitIconRowToWidth can scale from it.
                        textLbl.userData = textLbl.style.fontSize.value.value;

                        row.Add(textLbl);
                        break;
                    case DescriptionToken.TokenType.NewLine:
                        // Start a new row.
                        row = new VisualElement();
                        row.AddToClassList("iconDescRow");
                        iconDescContainer.Add(row);
                        rowStartTokenIdx = i + 1;
                        break;
                    case DescriptionToken.TokenType.NewLineHorizontalDiv:
                        // Add a divider
                        VisualElement divider = new VisualElement();
                        divider.AddToClassList("iconDescDivider");
                        if (packFrom != null) divider.style.backgroundColor = packFrom.packSecondaryColor * new Color(0.6f, 0.6f, 0.6f, 1f);
                        iconDescContainer.Add(divider);

                        row = new VisualElement();
                        row.AddToClassList("iconDescRow");
                        iconDescContainer.Add(row);
                        rowStartTokenIdx = i + 1;
                        break;
                }
            }

            foreach (VisualElement iconRow in iconDescContainer.Query<VisualElement>(className: "iconDescRow").ToList())
            {
                SetupIconRowFitting(iconRow);
            }
        } else
        {
            iconDescContainer.visible = false;
        }

        if (packFrom != null)
        {
            VisualElement backgroundTexture = container.Q<VisualElement>("cardBackgroundTexture");
            Sprite cardBack = packFrom.cardBackCommon;
            if (rarity == Card.CardRarity.COMMON) cardBack = packFrom.cardBackCommon;
            else if (rarity == Card.CardRarity.UNCOMMON) cardBack = packFrom.cardBackUncommon;
            else if (rarity == Card.CardRarity.RARE) cardBack = packFrom.cardBackRare;
            backgroundTexture.style.backgroundImage = new StyleBackground(cardBack);
        }

        Label cardTypeLabel = container.Q<Label>("cardTypeLabel");
        cardTypeLabel.text = cardTypeLabel.text = System.Text.RegularExpressions.Regex.Replace(
            card.cardCategory.ToString(), "(?<!^)([A-Z])", " $1"
        ).Trim();

        return container;
    }

    // Matches the width of .iconDescSprite / .iconDescSpriteShop in cardTemplateV2.uss.
    private const float ICON_BASE_WIDTH_PERCENT = 25f;
    private const float MIN_ICON_ROW_SCALE = 0.4f;

    private class IconRowFitState {
        public float scale = 1f;
        public bool pending = false;
    }

    // Rows that contain icons must stay on one line. Instead of letting the labels wrap,
    // shrink the whole row (fonts and icons together) until it fits the card width.
    // Text-only rows (e.g. "N = ...") are left alone so they can still wrap.
    private static void SetupIconRowFitting(VisualElement row)
    {
        bool hasIcon = row.Children().Any(c => c.ClassListContains("iconDescSprite"));
        if (!hasIcon) return;

        row.userData = new IconRowFitState();
        foreach (VisualElement child in row.Children())
        {
            if (child is Label lbl)
            {
                lbl.style.whiteSpace = WhiteSpace.NoWrap;
                lbl.style.flexShrink = 0;
            }
            // Children change size when their text or font changes; the row itself only
            // changes size when the card does. Listen to both.
            child.RegisterCallback<GeometryChangedEvent>(_ => ScheduleIconRowFit(row));
        }
        row.RegisterCallback<GeometryChangedEvent>(_ => ScheduleIconRowFit(row));
    }

    // Several children fire GeometryChanged in the same layout pass; collapse them into one
    // fit that runs after layout has settled, so it always measures fresh sizes.
    private static void ScheduleIconRowFit(VisualElement row)
    {
        if (row.userData is not IconRowFitState state || state.pending) return;
        state.pending = true;
        row.schedule.Execute(() => {
            state.pending = false;
            FitIconRowToWidth(row, state);
        });
    }

    private static void FitIconRowToWidth(VisualElement row, IconRowFitState state)
    {
        float available = row.contentRect.width;
        if (available <= 0f || float.IsNaN(available)) return;

        float needed = 0f;
        foreach (VisualElement child in row.Children())
        {
            if (child.resolvedStyle.display == DisplayStyle.None) continue;
            needed += child.layout.width + child.resolvedStyle.marginLeft + child.resolvedStyle.marginRight;
        }
        if (needed <= 0f || float.IsNaN(needed)) return;

        // Estimate the width at full size, then pick the largest scale (up to 1) that fits.
        // Aim slightly under the available width so rounding doesn't cause a wrap.
        float neededAtFullSize = needed / state.scale;
        float targetScale = Mathf.Clamp(available * 0.98f / neededAtFullSize, MIN_ICON_ROW_SCALE, 1f);

        // Only re-apply when it overflows now, or when there's meaningful room to grow back.
        bool overflowing = needed > available + 0.5f;
        bool canGrow = targetScale > state.scale + 0.02f;
        if (!overflowing && !canGrow) return;

        state.scale = targetScale;
        foreach (VisualElement child in row.Children())
        {
            if (child is Label lbl && lbl.userData is float baseFontSize)
            {
                lbl.style.fontSize = baseFontSize * targetScale;
            }
            else if (child.ClassListContains("iconDescSprite"))
            {
                child.style.width = Length.Percent(ICON_BASE_WIDTH_PERCENT * targetScale);
            }
        }
    }

    private void setManaCost(Label manaCost, CardType card) {
        if (!card.playable)
        {
            manaCost.text = "-";
        }
        else if (cardInstance == null)
        {
            manaCost.text = card.Cost.ToString();
        }
        else {
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

    // Highlight driven by the card's calculation workflow ("highlightCard").
    public void SetHighlight(bool isHighlightVisible) {
        effectHighlight = isHighlightVisible;
        RefreshHighlight();
    }

    // Highlight driven by card selection (e.g. cards with on-exhaust/on-discard triggers).
    // Takes precedence over the effect highlight's color while active.
    public void SetSelectionHighlight(bool isHighlightVisible, Color color) {
        selectionHighlight = isHighlightVisible;
        selectionHighlightColor = color;
        RefreshHighlight();
    }

    private void RefreshHighlight() {
        if (cardContainer == null) return;
        VisualElement ve = cardContainer.Q<VisualElement>("greenBorder");
        if (ve == null) return;
        ve.visible = effectHighlight || selectionHighlight;
        Color borderColor = selectionHighlight ? selectionHighlightColor : DEFAULT_HIGHLIGHT_COLOR;
        ve.style.borderLeftColor = borderColor;
        ve.style.borderRightColor = borderColor;
        ve.style.borderTopColor = borderColor;
        ve.style.borderBottomColor = borderColor;
        ve.MarkDirtyRepaint();
    }

    public void SetBlackBorder(bool isVisible) {
        if (cardContainer == null) return;
        VisualElement ve = cardContainer.Q<VisualElement>("blackBorder");
        if (isVisible) {
            ve.visible = true;
        } else {
            ve.visible = false;
        }
    }

    public void SetLocked() {
        VisualElement lockedIndicator = cardContainer.Q<VisualElement>("card-locked-indicator");
        lockedIndicator.style.display = DisplayStyle.Flex;
    }

    private void ScaleCardRootToFitContainer(GeometryChangedEvent evt) {
        // cardContainer.UnregisterCallback<GeometryChangedEvent>(ScaleCardRootToFitContainer);

        float width = cardContainer.resolvedStyle.width;
        float height = cardContainer.resolvedStyle.height;
        // Not laid out yet (e.g. parent hidden or still being built): wait for the next geometry pass
        // instead of showing the card at scale 0 / NaN.
        if (float.IsNaN(width) || float.IsNaN(height) || width <= 0f || height <= 0f) return;

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