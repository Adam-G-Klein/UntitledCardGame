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
    public IEntityViewDelegate viewDelegate;
    public VisualElementFocusable focusable;

    public static string STATUS_EFFECTS_CONTAINER_SUFFIX = "-status-effects";

    private float SCREEN_WIDTH_PERCENT = 0.15f;
    private float ENEMY_RATIO = 1.15f;

    private Enemy enemy = null;
    public Enemy GetEnemy() {
        return this.enemy;
    }
    private EnemyInstance enemyInstance = null;
    private CombatInstance combatInstance = null;
    private VisualTreeAsset template;
    private int index;
    private bool isDead = false;

    private List<VisualElement> pickingModePositionList = new List<VisualElement>();

    private Vector3 originalScale;
    private Vector2 originalElementScale;
    private int ENTITY_NAME_MAX_CHARS = 6;
    private int ENTITY_NAME_FONT_SIZE = 20;
    public GameObject tweenTarget;

    private VisualElement statusArea;
    private VisualElement spriteElement;
    private Label name;
    private VisualElement healthBarFill;
    private Label healthBarLabel;
    private VisualElement selectedIndicator;
    private Label intentLabel;
    private VisualElement intentImage;
    private VisualElement intentContainer;
    private bool isTweening = false;

    private int lastHealthValue;
    private bool isHealthTweening = false;

    private static string HEALTH_LABEL_STRING = "{0}/{1}";

    private Sprite currentEnemySprite;


    public EnemyView(
        Enemy enemy,
        int index,
        IEntityViewDelegate viewDelegate,
        EnemyInstance enemyInstance = null)
    {
        this.enemy = enemy;
        currentEnemySprite = enemy.enemyType.sprite;
        this.enemyInstance = enemyInstance;
        this.index = index;
        this.viewDelegate = viewDelegate;
        this.template = GameplayConstantsSingleton.Instance.gameplayConstants.enemyTemplate;

        if (this.enemyInstance != null)
        {
            this.combatInstance = this.enemyInstance.combatInstance;
        }

        SetupEnemyView();

        if (this.combatInstance != null)
        {
            combatInstance.onDamageHandler += DamageScaleBump;
            combatInstance.onDeathHandler += OnDeathHandler;
            combatInstance.SetVisualElement(this.spriteElement);
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
        UpdateHealth();
        SetupStatusIndicators();
        if (enemyInstance) {
            SetupEnemyIntent();
        }
    }

    private void SetupEnemyView() {
        VisualElement enemyRoot = this.template.CloneTree();

        this.statusArea = enemyRoot.Q<VisualElement>("enemy-view-status-area");
        this.spriteElement = enemyRoot.Q<VisualElement>("enemy-view-sprite");
        this.name = enemyRoot.Q<Label>("enemy-view-name-label");
        this.healthBarFill = enemyRoot.Q<VisualElement>("enemy-view-health-bar-fill");
        this.healthBarLabel = enemyRoot.Q<Label>("enemy-view-health-bar-label");
        this.selectedIndicator = enemyRoot.Q<VisualElement>("enemy-view-selected-indicator");
        this.intentContainer = enemyRoot.Q<VisualElement>("enemy-view-intent-container");
        this.intentImage = enemyRoot.Q<VisualElement>("enemy-view-intent-image");
        this.intentLabel = enemyRoot.Q<Label>("enemy-view-intent-label");

        // Moving past the random VisualElement parent CloneTree() creates
        this.container = enemyRoot.Children().First();
        this.container.name = container.name + this.index;
        this.pickingModePositionList.Add(container);
        SetupMainContainer();
        SetupSprite();
        SetupName();
        SetupHealth();
        SetupStatusIndicators();
        UpdateWidthAndHeight();
    }

    private void SetupSprite()
    {
        this.spriteElement.style.backgroundImage = new StyleBackground(currentEnemySprite);
    }

    public void UpdateSprite(Sprite newSprite)
    {
        currentEnemySprite = newSprite;
        // Force a clear then set to ensure UI Toolkit recognizes the change
        this.spriteElement.style.backgroundImage = StyleKeyword.Null;
        this.spriteElement.style.display = DisplayStyle.None;
        this.spriteElement.style.backgroundImage = new StyleBackground(newSprite);
        this.spriteElement.style.display = DisplayStyle.Flex;
        // Mark this as dirty so it's redrawn.
        this.spriteElement.MarkDirtyRepaint();
    }

    private void SetupName() {
        this.name.text = this.enemy.GetName();
    }

    private void SetupHealth() {
        int currentHealth;
        int maxHealth;
        if (this.enemyInstance == null) {
            currentHealth = this.enemy.GetCurrentHealth();
            maxHealth = this.enemy.GetCombatStats().getMaxHealth();
        } else {
            currentHealth = this.combatInstance.combatStats.currentHealth;
            maxHealth = this.combatInstance.combatStats.maxHealth;
        }
        this.healthBarLabel.text = String.Format(HEALTH_LABEL_STRING, currentHealth, maxHealth);
        float healthPercent = (float) currentHealth / (float) maxHealth;
        this.healthBarFill.style.width = Length.Percent(healthPercent * 100);
        lastHealthValue = currentHealth;
    }

    private void UpdateHealth() {
        if (isHealthTweening) return;

        int currentHealth;
        int maxHealth;
        if (this.enemyInstance == null) {
            currentHealth = this.enemy.GetCurrentHealth();
            maxHealth = this.enemy.GetCombatStats().getMaxHealth();
        } else {
            currentHealth = this.combatInstance.combatStats.currentHealth;
            maxHealth = this.combatInstance.combatStats.maxHealth;
        }

        if (currentHealth == lastHealthValue) return;

        isHealthTweening = true;

        HealthBarUtils.UpdateHealth(lastHealthValue, currentHealth, maxHealth, healthBarFill, healthBarLabel, () => {
            isHealthTweening = false;
            lastHealthValue = currentHealth;
            // In case multiple instances of damage come through in close timing
            UpdateHealth();
        });
    }

    private void SetupStatusIndicators()
    {
        this.statusArea.Clear();

        if (this.combatInstance == null) return;

        foreach (KeyValuePair<StatusEffectType, int> kvp in combatInstance.GetDisplayedStatusEffects())
        {
            this.statusArea.Add(CreateStatusIndicator(viewDelegate.GetStatusEffectSprite(kvp.Key), kvp.Value.ToString()));
        }

        List<DisplayedCacheValue> cacheValues = combatInstance.GetDisplayedCacheValues();
        foreach (DisplayedCacheValue cacheValue in cacheValues)
        {
            this.statusArea.Add(CreateStatusIndicator(cacheValue.sprite, cacheValue.value.ToString()));
        }
    }

    private VisualElement CreateStatusIndicator(Sprite icon, string textValue)
    {
        VisualElement statusIndicator = new VisualElement();
        statusIndicator.AddToClassList("entity-view-status-indicator");

        Label statusLabel = new Label();
        statusLabel.AddToClassList("entity-view-status-indicator-label");
        statusLabel.text = textValue;

        VisualElement statusIcon = new VisualElement();
        statusIcon.AddToClassList("entity-view-status-indicator-icon");
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
            if (enemyInstance == null) return;
            Targetable targetable = enemyInstance.GetTargetable();
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
            if (enemyInstance == null) return;
            Targetable targetable = enemyInstance.GetTargetable();
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
            if (enemyInstance == null) return;
            Targetable targetable = enemyInstance.GetTargetable();
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


    private void SetupEnemyIntent()
    {
        if (enemyInstance.currentIntent != null)
        {
            this.intentContainer.style.display = DisplayStyle.Flex;
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

    public void DamageScaleBump(int scale = -1)
    {
        if (scale == 0 || this.isTweening) return;

        float duration = 0.125f;  // Total duration for the scale animation
        float minScale = .8f; // (float)Math.Min(.75, .9 - scale / 500);  // scale bump increases in intensity if entity takes more damage (haven't extensively tested this)

        Vector2 originalElementScale = new Vector2(
            this.spriteElement.style.scale.value.value.x,
            this.spriteElement.style.scale.value.value.y
        );

        LeanTween.value(1f, minScale, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(1) // inverse tween is called when this tween completes. On complete below is called after both tweens complete
            .setOnUpdate((float currentScale) =>
            {
                this.spriteElement.style.scale = new StyleScale(new Scale(originalElementScale * currentScale));
            })
            .setOnStart(() =>
            {
                this.isTweening = true;
            })
            .setOnComplete(() =>
            {
                this.isTweening = false;
                this.spriteElement.style.scale = new StyleScale(new Scale(originalElementScale));
            });
    }

    public void BossFrameDestructionRotationShake(float scale, float duration, int pingpongs)
    {
        float originalElementRotation = this.container.style.rotate.value.angle.value;
        float maxRotation = originalElementRotation + scale;

        LeanTween.value(originalElementRotation, maxRotation, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(pingpongs) // inverse tween is called when this tween completes. On complete below is called after both tweens complete
            .setOnUpdate((float currentRotation) =>
            {
                this.container.style.rotate = new StyleRotate(new Rotate(currentRotation));
            })
            .setOnStart(() =>
            {
                this.isTweening = true;
            })
            .setOnComplete(() =>
            {
                this.isTweening = false;
                this.container.style.rotate = new StyleRotate(new Rotate(originalElementRotation));
            });
    }

    public void BossFrameDestructionPositionShake(float scale, float duration, int pingpongs, float delay = 0f)
    {
        /*
        if(delay > 0f)
        {
            LeanTween.delayedCall(delay, () => {
                BossFrameDestructionPositionShake(scale, duration, pingpongs, 0f);
            });
            return;
        }
        */

        float originalElementPosition =
            this.container.style.right.value.value;

        // just do x for now, need to do a full vector tween to do more
        LeanTween.value(originalElementPosition, scale, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(pingpongs) // inverse tween is called when this tween completes. On complete below is called after both tweens complete
            .setOnUpdate((float currentPosition) =>
            {
                this.container.style.right = currentPosition;
            })
            .setOnStart(() =>
            {
                this.isTweening = true;
            })
            .setOnComplete(() =>
            {
                this.isTweening = false;
                this.container.style.right = originalElementPosition;
            });
    }

    public void DisableInteractions() {
        UIDocumentUtils.SetAllPickingMode(this.container, PickingMode.Ignore);
    }

    private IEnumerator OnDeathHandler(CombatInstance killer)
    {
        this.selectedIndicator.style.visibility = Visibility.Hidden;
        FocusManager.Instance.UnregisterFocusableTarget(this.focusable);
        isDead = true;
        yield return null;
    }

    public void HideIntent()
    {
        intentContainer.style.display = DisplayStyle.None;
    }

    public IEnumerator AbilityActivatedVFX() {
        VisualElement spriteCopy = new VisualElement();
        spriteCopy.style.backgroundImage = new StyleBackground(this.currentEnemySprite);
        spriteCopy.style.width = new Length(100, LengthUnit.Percent);
        spriteCopy.style.height = new Length(100, LengthUnit.Percent);
        yield return EntityAbilityInstance.GenericAbilityTriggeredVFX(this.spriteElement, spriteCopy);
    }
}