using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UIElements;
using System.Collections;

public class CompanionManagementView : IControlsReceiver {
    public VisualElement container;
    public Companion companion;

    private ICompanionManagementViewDelegate viewDelegate;

    private VisualElement spriteElement;
    private VisualElement bronzeFrame;
    private VisualElement silverFrame;
    private VisualElement goldFrame;
    private Label name;
    private VisualElement healthBarFill;
    private Label healthBarLabel;
    private VisualElement darkBox;
    private IconButton viewDeckButton = null;
    private IconButton sellCompanionButton = null;
    private VisualElement companionBoundingBox = null;
    private VisualElement rarityIndicator = null;
    // private CompanionViewOld companionView;

    private bool draggingThisCompanion = false;
    private bool isSellingDisabled = false;
    private bool upgradeAnimationPlaying = false;
    private bool interactionsEnabled = true;

    private int lastHealthValue;
    private bool isHealthTweening = false;

    private static string HEALTH_LABEL_STRING = "{0}/{1}";

    public CompanionManagementView(Companion companion, VisualTreeAsset template, ICompanionManagementViewDelegate viewDelegate) {
        this.viewDelegate = viewDelegate;
        this.companion = companion;
        container = MakeCompanionManagementView(companion, template);
    }

    public VisualElement MakeCompanionManagementView(Companion companion, VisualTreeAsset template) {
        VisualElement managementRoot = template.CloneTree();

        this.spriteElement = managementRoot.Q<VisualElement>("management-view-sprite");
        this.bronzeFrame = managementRoot.Q<VisualElement>("management-view-bronze-frame");
        this.silverFrame = managementRoot.Q<VisualElement>("management-view-silver-frame");
        this.goldFrame = managementRoot.Q<VisualElement>("management-view-gold-frame");
        this.name = managementRoot.Q<Label>("management-view-name-label");
        this.healthBarFill = managementRoot.Q<VisualElement>("management-view-health-bar-fill");
        this.healthBarLabel = managementRoot.Q<Label>("management-view-health-bar-label");
        this.rarityIndicator = managementRoot.Q<VisualElement>("management-view-rarity-indicator");

        VisualElement container = managementRoot.Children().First();

        SetupRarityIndicator();
        SetupCompanionSprite();
        SetupName();
        SetupHealth();
        SetupLevelIndicator();

        container.RegisterCallback<ClickEvent>(CompanionManagementOnClick);

        container.RegisterCallback<PointerDownEvent>((evt) => CompanionManagementOnPointerDown(evt, true));
        container.RegisterCallback<PointerMoveEvent>(CompanionManagementOnPointerMove);
        container.RegisterCallback<PointerUpEvent>(ComapnionManagementOnPointerUp);

        container.RegisterCallback<PointerLeaveEvent>(ComapnionManagementOnPointerLeave);
        container.RegisterCallback<PointerEnterEvent>(CompanionManagementOnPointerEnter);

        container.name = companion.companionType.name;

        return container;
    }

    private void SetupLevelIndicator() {
        bronzeFrame.style.visibility = Visibility.Hidden;
        silverFrame.style.visibility = Visibility.Hidden;
        goldFrame.style.visibility = Visibility.Hidden;
        switch (this.companion.companionType.level) {
            case CompanionLevel.LevelThree:
                goldFrame.style.visibility = Visibility.Visible;
            break;

            case CompanionLevel.LevelTwo:
                silverFrame.style.visibility = Visibility.Visible;
            break;

            case CompanionLevel.LevelOne:
            default:
                bronzeFrame.style.visibility = Visibility.Visible;
            break;
        }
    }

    private void SetupRarityIndicator() {
        Sprite rarityIndicator = null;
        PackSO pack = companion.companionType.pack;
        switch(this.companion.companionType.rarity) {
            case CompanionRarity.COMMON:
                rarityIndicator = pack.commonIcon;
                break;
            case CompanionRarity.UNCOMMON:
                rarityIndicator = pack.uncommonIcon;
                break;
            case CompanionRarity.RARE:
                rarityIndicator = pack.rareIcon;
                break;
        }
        this.rarityIndicator.style.backgroundImage = new StyleBackground(rarityIndicator);
    }

    private void SetupHealth() {
        int currentHealth;
        int maxHealth;
        currentHealth = this.companion.GetCurrentHealth();
        maxHealth = this.companion.GetCombatStats().getMaxHealth();
        this.healthBarLabel.text = String.Format(HEALTH_LABEL_STRING, currentHealth, maxHealth);
        float healthPercent = (float) currentHealth / (float) maxHealth;
        this.healthBarFill.style.width = Length.Percent(healthPercent * 100);
    }

    private void UpdateHealth() {
        if (isHealthTweening) return;

        int currentHealth;
        int maxHealth;
        currentHealth = this.companion.GetCurrentHealth();
        maxHealth = this.companion.GetCombatStats().getMaxHealth();

        if (currentHealth == lastHealthValue) return;

        isHealthTweening = true;

        HealthBarUtils.UpdateHealth(lastHealthValue, currentHealth, maxHealth, healthBarFill, healthBarLabel, () => {
                isHealthTweening = false;
                lastHealthValue = currentHealth;
                // In case multiple instances of damage come through in close timing
                UpdateHealth();
            });
    }

    private void SetupName() {
        this.name.text = this.companion.GetName();
    }

    private void SetupCompanionSprite() {
        Sprite sprite = this.companion.companionType.fullSprite;
        if (sprite == null) {
            sprite = this.companion.getSprite();
        }
        this.spriteElement.style.backgroundImage = new StyleBackground(sprite);
    }

    public void CompanionManagementOnPointerEnter(PointerEnterEvent evt)
    {
        if (!interactionsEnabled) return;
        MusicController.Instance.PlaySFX("event:/SFX/SFX_UIHover");
        if (viewDelegate == null) return;
        if (viewDelegate.IsSellingCompanions() || viewDelegate.IsDraggingCompanion() || upgradeAnimationPlaying) return;
        CreateViewDeckButton();
        if (!isSellingDisabled) CreateSellCompanionButton();
        CreateCompanionBoundingBox();
        viewDelegate.DisplayTooltip(container, companion.companionType.GetTooltip(), TooltipContext.CompanionManagementView);
    }

    public void CompanionManagementNonMouseSelect() {
        if (!viewDelegate.CanDragCompanions()) {
            Debug.Log("Companion Management On Click not dragging");
            CompanionManagementOnClick(null);
        }
    }

    public void CompanionManagementOnClick(ClickEvent evt) {
        if (!interactionsEnabled) return;
        viewDelegate.CompanionManagementOnClick(this);
    }

    public void CompanionManagementOnPointerDown(PointerDownEvent evt, bool usingMouse) {
        if (!interactionsEnabled) return;
        Debug.Log("Companion on pointer down");
        RemoveCompanionHoverButtons();
        viewDelegate.DestroyTooltip(container);
        draggingThisCompanion = true;
        if (usingMouse) {
            viewDelegate.CompanionManagementOnPointerDown(this, evt.position);
        } else {
            // Handling dragging this companion using keyboard/controller
            FocusManager.Instance.onFocusDelegate += FocusChangedWhileDragging;
            viewDelegate.CompanionManagementOnPointerDown(this, (evt.target as VisualElement).worldBound.center);
            ControlsManager.Instance.RegisterControlsReceiver(this);
        }
    }

    public void FocusChangedWhileDragging(IFocusableTarget focusable) {
        viewDelegate.CompanionManagementOnPointerMove(this, focusable.GetUIPosition());
    }

    private void CompanionManagementOnPointerMove(PointerMoveEvent evt) {
        if (!interactionsEnabled) return;
        viewDelegate?.CompanionManagementOnPointerMove(this, evt.position);
    }

    public void ComapnionManagementOnPointerUp(PointerUpEvent evt) {
        if (!interactionsEnabled) return;
        viewDelegate.ComapnionManagementOnPointerUp(this, evt.position);
        draggingThisCompanion = false;
    }

    public void ComapnionManagementOnPointerLeave(PointerLeaveEvent evt) {
        if (!interactionsEnabled) return;
        if (viewDelegate == null) return;
        viewDelegate.CompanionManagementOnPointerLeave(this, evt);
        viewDelegate.DestroyTooltip(container);
    }

    public void CompanionManagementOnUnfocus() {
        viewDelegate.CompanionManagementOnPointerLeave(this, null);
        viewDelegate.DestroyTooltip(container);
        RemoveCompanionHoverButtons();
    }

    private void CreateViewDeckButton() {
        if (viewDeckButton != null) {
            viewDeckButton.RemoveFromHierarchy();
        }
        viewDeckButton = new IconButton();
        // viewDeckButton.AddToClassList("shopButton");
        viewDeckButton.AddToClassList("companion-view-deck-button");
        viewDeckButton.AddToClassList("icon-button-absolute");
        viewDeckButton.SetIconHeight(1f);
        viewDeckButton.text = "Deck";
        viewDeckButton.SetIcon(GFGInputAction.VIEW_DECK, ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.VIEW_DECK));
        ControlsManager.Instance.RegisterIconChanger(viewDeckButton);

        viewDeckButton.style.width = container.worldBound.width * 0.45f;
        viewDeckButton.style.top = container.worldBound.yMin - container.worldBound.height * 0.3f;
        viewDeckButton.style.left = container.worldBound.xMin - 4;
        viewDeckButton.style.height = container.worldBound.height * 0.3f;
        viewDeckButton.name = "viewdeck";

        viewDeckButton.RegisterCallback<ClickEvent>((evt) => {ViewDeckButtonOnClick();});

        viewDelegate.AddToRoot(viewDeckButton);
    }

    private void CreateSellCompanionButton() {
        if (sellCompanionButton != null) {
            sellCompanionButton.RemoveFromHierarchy();
        }
        sellCompanionButton = new IconButton();
        // sellCompanionButton.AddToClassList("shopButton");
        sellCompanionButton.AddToClassList("companion-sell-button");
        sellCompanionButton.AddToClassList("icon-button-absolute");
        sellCompanionButton.SetIconHeight(1f);
        sellCompanionButton.text = "Sell";
        sellCompanionButton.SetIcon(GFGInputAction.SELL_COMPANION, ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.SELL_COMPANION));
        ControlsManager.Instance.RegisterIconChanger(sellCompanionButton);

        sellCompanionButton.style.width = container.worldBound.width * 0.45f;
        sellCompanionButton.style.top = container.worldBound.yMin - container.worldBound.height * 0.3f;
        sellCompanionButton.style.left = container.worldBound.xMin + (container.worldBound.width * 0.5f) + 4;
        sellCompanionButton.style.height = container.worldBound.height * 0.3f;
        sellCompanionButton.name = "sellcompanion";

        sellCompanionButton.RegisterCallback<ClickEvent>((evt) => { SellCompanionButtonOnClick(); });

        viewDelegate.AddToRoot(sellCompanionButton);
    }

    public void ViewDeckButtonOnClick() {
        RemoveCompanionHoverButtons();
        viewDelegate.ShowCompanionDeckView(companion);
    }

    public void SellCompanionButtonOnClick() {
        RemoveCompanionHoverButtons();
        viewDelegate.SellCompanion(this);
    }

    private void CreateCompanionBoundingBox() {
        companionBoundingBox = new VisualElement();
        companionBoundingBox.style.position = Position.Absolute;
        companionBoundingBox.pickingMode = PickingMode.Ignore;
        float width = container.worldBound.width * 1.1f;
        float height = container.worldBound.height * 1.5f;
        companionBoundingBox.style.width = width;
        companionBoundingBox.style.height = height;
        Vector2 containerCenter = new Vector2(container.worldBound.x + container.worldBound.width * 0.5f,
            container.worldBound.y + container.worldBound.height * 0.5f);
        companionBoundingBox.style.left = containerCenter.x - (width * 0.5f);
        companionBoundingBox.style.top = containerCenter.y - (height * 0.5f);
        viewDelegate.AddToRoot(companionBoundingBox);
        viewDelegate.GetMonoBehaviour().StartCoroutine(RegisterMoveCallback(companionBoundingBox.parent));
    }

    private IEnumerator RegisterMoveCallback(VisualElement parent) {
        yield return new WaitForEndOfFrame();
        parent.RegisterCallback<PointerMoveEvent>(BoundingBoxParentOnPointerMove);
    }

    public void UpdateView()
    {
        UpdateHealth();
    }


    private void BoundingBoxParentOnPointerMove(PointerMoveEvent evt)
    {
        if (companionBoundingBox != null && !companionBoundingBox.worldBound.Contains(evt.position))
        {
            RemoveCompanionHoverButtons();
        }
    }

    private void RemoveCompanionHoverButtons() {
        if (sellCompanionButton != null) {
            sellCompanionButton.style.visibility = Visibility.Hidden;
            sellCompanionButton.RemoveFromHierarchy();
            sellCompanionButton = null;
        }
        if (viewDeckButton != null) {
            viewDeckButton.style.visibility = Visibility.Hidden;
            viewDeckButton.RemoveFromHierarchy();
            viewDeckButton = null;
        }
        if (companionBoundingBox != null) {
            companionBoundingBox.parent.UnregisterCallback<PointerMoveEvent>(BoundingBoxParentOnPointerMove);
            companionBoundingBox.RemoveFromHierarchy();
            companionBoundingBox = null;
        }
    }


    public void ShowNotApplicable() {
        darkBox = new VisualElement();
        container.Add(darkBox);
        darkBox.style.position = Position.Absolute;
        darkBox.style.top = 0;
        darkBox.style.left = 0;
        darkBox.style.right= 0;
        darkBox.style.bottom = 0;
        darkBox.style.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
    }

    public void ResetApplicable() {
        if (darkBox != null) {
            darkBox.RemoveFromHierarchy();
            darkBox = null;
        }
    }

    public void DisableSelling() {
        isSellingDisabled = true;
    }

    public void EnableSelling() {
        isSellingDisabled = false;
    }

    public void SetUpdateAnimationPlaying(bool isItPlaying) {
        this.upgradeAnimationPlaying = isItPlaying;
    }

    public void ResetToNeutral() {
        viewDelegate.DestroyTooltip(container);
        RemoveCompanionHoverButtons();
    }

    public void UpdateWidthAndHeight(float screenWidthPercent) {
        Tuple<int, int> entityWidthHeight = GetWidthAndHeight(screenWidthPercent);
        container.style.width = entityWidthHeight.Item1;
        container.style.height = entityWidthHeight.Item2;
    }

    private Tuple<int, int> GetWidthAndHeight(float screenWidthPercent) {
        int width = (int)(Screen.width * screenWidthPercent);

        // This drove me insane btw
        #if UNITY_EDITOR
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint windowWidth, out uint windowHeight);
        width = (int)(windowWidth * screenWidthPercent);
        #endif

        int height = (int) ((float) width * (160f/260f)); // 1:1 aspect ratio

        return new Tuple<int, int>(width, height);
    }

    public void DisableInteractions() {
        interactionsEnabled = false;
        // The below all exists because when UI is disabled, this might be hovered
        ComapnionManagementOnPointerLeave(null);
        RemoveCompanionHoverButtons();
        viewDelegate.DestroyTooltip(container);
    }

    public void EnableInteractions() {
        interactionsEnabled = true;
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (!draggingThisCompanion) return;

        if (action == GFGInputAction.SELECT_UP) {
            viewDelegate.ComapnionManagementOnPointerUp(this, FocusManager.Instance.GetCurrentFocus().GetUIPosition());
            FocusManager.Instance.onFocusDelegate -= FocusChangedWhileDragging;
            ControlsManager.Instance.UnregisterControlsReceiver(this);
            viewDelegate.DestroyTooltip(container);
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        return;
    }
}