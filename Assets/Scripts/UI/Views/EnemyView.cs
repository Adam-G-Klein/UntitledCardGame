using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class EnemyView : IUIEventReceiver
{
    public VisualElement container;
    public VisualElementFocusable elementFocusable;
    public IEntityViewDelegate viewDelegate;

    public static string STATUS_EFFECTS_CONTAINER_SUFFIX = "-status-effects";

    private float SCREEN_WIDTH_PERCENT = 0.175f;
    private float ENEMY_RATIO = 1f;

    private IUIEntity uiEntity;
    private int index;
    private bool isDead = false;

    private List<VisualElement> pickingModePositionList = new List<VisualElement>();
    private List<VisualElement> elementsKeepingDrawDiscardVisible = new List<VisualElement>();

    private VisualElement pillar;
    private VisualElement drawDiscardContainer;
    private VisualElement healthAndBlockTab = null;
    private VisualElement statusEffectsTab = null;
    private VisualElement descriptionContainer = null;
    private CombatInstance combatInstance = null;
    private Vector3 originalScale;
    private Vector2 originalElementScale;
    private int ENTITY_NAME_MAX_CHARS = 6;
    private int ENTITY_NAME_FONT_SIZE = 20;
    public GameObject tweenTarget;

    private VisualElement parentContainer;
    private VisualElement statusVertical;
    private VisualElement statusContainer;
    private VisualElement extraSpace;
    private VisualElement solidBackground;
    private VisualElement imageElement;
    private VisualElement lowerHoverDetector;
    private VisualElement viewDeckContainer;
    private IconButton drawPileButton;
    private IconButton discardPileButton;
    private IconButton viewDeckButton;
    private Label primaryNameLabel;
    private Label secondaryNameLabel;
    private Label blockLabel;
    private Label healthLabel;
    private Label maxHealthLabel;
    private VisualElement maxHealthContainer;
    private VisualElement selectedIndicator;
    private VisualTreeAsset template;
    public VisualElementFocusable focusable;
    private Label intentLabel;
    private VisualElement intentImage;
    private VisualElement intentContainer;
    private bool isTweening = false;


    public EnemyView(
        IUIEntity entity,
        int index,
        IEntityViewDelegate viewDelegate)
    {
        this.uiEntity = entity;
        this.index = index;
        this.viewDelegate = viewDelegate;
        this.template = GameplayConstantsSingleton.Instance.gameplayConstants.enemyTemplate;
        setupEntity(entity, index);

        this.combatInstance = entity.GetCombatInstance();
        if (this.combatInstance)
        {
            combatInstance.onDamageHandler += DamageScaleBump;
            combatInstance.onDeathHandler += OnDeathHandler;
            combatInstance.SetVisualElement(this.container);
        }
    }

    public void UpdateWidthAndHeight()
    {
        Tuple<int, int> entityWidthHeight = GetWidthAndHeight();
        this.container.style.width = entityWidthHeight.Item1;
        this.container.style.height = entityWidthHeight.Item2;
    }

    public void UpdateView()
    {
        SetupBlockAndHealth();
        SetupStatusIndicators();
        if (uiEntity.GetEnemyInstance())
        {
            setupEnemyIntent(uiEntity.GetEnemyInstance());
        }
        ;
    }

    private void setupEntity(IUIEntity entity, int index)
    {
        VisualElement enemyRoot = this.template.CloneTree();

        this.parentContainer = enemyRoot.Q<VisualElement>("companion-view-parent-container");
        this.statusContainer = enemyRoot.Q<VisualElement>("companion-view-status-container");
        this.statusVertical = enemyRoot.Q<VisualElement>("companion-view-status-vertical");
        this.extraSpace = enemyRoot.Q<VisualElement>("companion-view-extra-space");
        this.solidBackground = enemyRoot.Q<VisualElement>("companion-view-solid-background");
        this.imageElement = enemyRoot.Q<VisualElement>("companion-view-companion-image");
        this.primaryNameLabel = enemyRoot.Q<Label>("companion-view-primary-name-label");
        this.secondaryNameLabel = enemyRoot.Q<Label>("companion-view-secondary-name-label");
        this.blockLabel = enemyRoot.Q<Label>("companion-view-block-value-label");
        this.healthLabel = enemyRoot.Q<Label>("companion-view-health-value-label");
        this.selectedIndicator = enemyRoot.Q<VisualElement>("companion-view-selected-indicator");
        this.intentLabel = enemyRoot.Q<Label>("enemy-intent-label");
        this.intentImage = enemyRoot.Q<VisualElement>("enemy-intent-image");
        this.intentContainer = enemyRoot.Q<VisualElement>("intentContainer");
        // Moving past the random VisualElement parent CloneTree() creates
        this.container = enemyRoot.Children().First();
        this.container.name = container.name + this.index;
        this.pickingModePositionList.Add(container);
        SetupMainContainer();
        SetupBackground();
        SetupSprite();
        SetupBlockAndHealth();
        SetupStatusIndicators();
        UpdateWidthAndHeight();
    }


    private void SetupBackground()
    {
        this.solidBackground.style.backgroundImage = new StyleBackground(this.uiEntity.GetBackgroundImage());
    }

    private void SetupSprite()
    {
        Sprite sprite = null;
        if (uiEntity is Enemy enemy)
        {
            sprite = enemy.enemyType.sprite;
        }
        else if (uiEntity is EnemyInstance enemyInstance)
        {
            sprite = enemyInstance.enemy.enemyType.sprite;
        }
        this.imageElement.style.backgroundImage = new StyleBackground(sprite);
    }



    private void SetupBlockAndHealth()
    {
        if (this.combatInstance == null)
        {
            this.healthLabel.text = this.uiEntity.GetCurrentHealth().ToString();
            this.blockLabel.style.visibility = Visibility.Hidden;
        }
        else
        {
            this.healthLabel.text = this.combatInstance.combatStats.currentHealth.ToString();
            this.blockLabel.text = this.combatInstance.GetStatus(StatusEffectType.Defended).ToString();
        }

        this.pickingModePositionList.Add(this.healthLabel);
    }

    private void SetupStatusIndicators()
    {
        this.statusContainer.Clear();

        if (this.combatInstance == null) return;

        foreach (KeyValuePair<StatusEffectType, int> kvp in combatInstance.GetDisplayedStatusEffects())
        {
            if (kvp.Key == StatusEffectType.Defended) continue;
            this.statusContainer.Add(CreateStatusIndicator(viewDelegate.GetStatusEffectSprite(kvp.Key), kvp.Value.ToString()));
        }

        List<DisplayedCacheValue> cacheValues = combatInstance.GetDisplayedCacheValues();
        foreach (DisplayedCacheValue cacheValue in cacheValues)
        {
            this.statusContainer.Add(CreateStatusIndicator(cacheValue.sprite, cacheValue.value.ToString()));
        }
    }

    private VisualElement CreateStatusIndicator(Sprite icon, string textValue)
    {
        VisualElement statusIndicator = new VisualElement();
        statusIndicator.AddToClassList("companion-view-status-indicator");

        Label statusLabel = new Label();
        statusLabel.AddToClassList("companion-view-status-indicator-label");
        statusLabel.text = textValue;

        VisualElement statusIcon = new VisualElement();
        statusIcon.AddToClassList("companion-view-status-indicator-icon");
        statusIcon.style.backgroundImage = new StyleBackground(icon);

        statusIndicator.Add(statusLabel);
        statusIndicator.Add(statusIcon);

        return statusIndicator;
    }

    private void SetupMainContainer()
    {
        this.container.RegisterOnSelected(() => ContainerPointerClick(null));
        this.container.RegisterCallback<PointerEnterEvent>(ContainerPointerEnter);
        this.container.RegisterCallback<PointerLeaveEvent>(ContainerPointerLeave);

        this.focusable = this.container.AsFocusable();
        this.focusable.additionalFocusAction += () => ContainerPointerEnter(null);
        this.focusable.additionalUnfocusAction += () => ContainerPointerLeave(null);
    }

    public void SetSelectionIndicatorVisibility(bool visible)
    {
        this.selectedIndicator.visible = visible;
    }

    private void ContainerPointerClick(ClickEvent evt)
    {
        try
        {
            Targetable targetable = this.uiEntity.GetTargetable();
            if (targetable == null) return;
            targetable.OnPointerClickUI(evt);
        }
        catch (Exception e)
        {
            Debug.Log("EntityView: Caught exception while trying to get entity targetable," +
                " might be due to the entity GO being destroyed");
            Debug.LogException(e);
        }
    }

    private void ContainerPointerEnter(PointerEnterEvent evt)
    {
        if (isDead) return;

        this.selectedIndicator.style.visibility = Visibility.Visible;

        try
        {
            Targetable targetable = this.uiEntity.GetTargetable();
            if (targetable == null) return;
            targetable.OnPointerEnterUI(evt);
        }
        catch (Exception e)
        {
            Debug.Log("EntityView: Caught exception while trying to get entity targetable," +
                " might be due to the entity GO being destroyed");
            Debug.LogException(e);
        }
    }

    private void ContainerPointerLeave(PointerLeaveEvent evt)
    {
        if (isDead) return;

        this.selectedIndicator.style.visibility = Visibility.Hidden;

        try
        {
            Targetable targetable = this.uiEntity.GetTargetable();
            if (targetable == null) return;
            targetable.OnPointerLeaveUI(evt);
        }
        catch (Exception e)
        {
            Debug.Log("EntityView: Caught exception while trying to get entity targetable," +
                " might be due to the entity GO being destroyed");
            Debug.LogException(e);
        }
    }


    private void setupEnemyIntent(EnemyInstance enemyInstance)
    {

        if (enemyInstance.currentIntent != null)
        {
            if (enemyInstance.currentIntent.GetDisplayValue() != 0)
            {
                this.intentLabel.style.display = DisplayStyle.Flex;
                this.intentLabel.text = enemyInstance.currentIntent.GetDisplayValue().ToString();
            }
            else
            {
                this.intentLabel.style.display = DisplayStyle.None;
            }
            intentImage.style.backgroundImage = new StyleBackground(viewDelegate.GetEnemyIntentImage(enemyInstance.currentIntent.intentType));
        }
    }

    private Tuple<int, int> GetWidthAndHeight()
    {
        int width = (int)(Screen.width * SCREEN_WIDTH_PERCENT);
        int height = (int)(width * ENEMY_RATIO);

        // This drove me insane btw
#if UNITY_EDITOR
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint windowWidth, out uint windowHeight);
        width = (int)(windowWidth * SCREEN_WIDTH_PERCENT);
        height = (int)(width * ENEMY_RATIO);
#endif

        return new Tuple<int, int>(width, height);
    }

    public void SetPickingModes(bool enable)
    {
        foreach (VisualElement ve in pickingModePositionList)
        {
            UIDocumentUtils.SetAllPickingMode(ve, enable ? PickingMode.Position : PickingMode.Ignore);
        }
    }

    private void DamageScaleBump(int scale)
    {
        if (scale == 0 || this.isTweening) return; // this could mean the damage didn't go through the block or that the companion died while taking damage

        float duration = 0.125f;  // Total duration for the scale animation
        float minScale = .8f; // (float)Math.Min(.75, .9 - scale / 500);  // scale bump increases in intensity if entity takes more damage (haven't extensively tested this)

        Vector2 originalElementScale = new Vector2(
            this.container.style.scale.value.value.x,
            this.container.style.scale.value.value.y
        );

        LeanTween.value(1f, minScale, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(1) // inverse tween is called when this tween completes. On complete below is called after both tweens complete
            .setOnUpdate((float currentScale) =>
            {
                this.container.style.scale = new StyleScale(new Scale(originalElementScale * currentScale));
            })
            .setOnStart(() =>
            {
                this.isTweening = true;
            })
            .setOnComplete(() =>
            {
                this.isTweening = false;
                this.container.style.scale = new StyleScale(new Scale(originalElementScale));
            });
    }

    private IEnumerator OnDeathHandler(CombatInstance killer)
    {
        FocusManager.Instance.UnregisterFocusableTarget(this.elementFocusable);
        isDead = true;
        yield return null;
    }

    public void HideIntent()
    {
        intentContainer.style.display = DisplayStyle.None;
    }
}