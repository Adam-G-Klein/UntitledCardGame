using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CreditsView : MonoBehaviour, IControlsReceiver
{
    public UIDocument uiDoc;
    public List<CreditPage> creditPages = new List<CreditPage>();

    private ScrollView creditsScrollView;
    private Action callback;
    private VisualElement hintIndicator;
    private float lastHintTime = -Mathf.Infinity;
    private float hintCooldown = 1.5f;
    private IVisualElementScheduledItem hideHintTask;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Init")]
    public void Init() {
        Init(null);
    }

    public void Init(Action callback) {
        this.callback = callback;
        lastHintTime = Time.time;
        FocusManager.Instance.StashFocusables(this.GetType().Name);
        ControlsManager.Instance.RegisterControlsReceiver(this);
        creditsScrollView = uiDoc.rootVisualElement.Q<ScrollView>();
        foreach (CreditPage creditPage in creditPages) {
            SetupCreditPage(creditPage);
        }

        hintIndicator = uiDoc.rootVisualElement.Q<VisualElement>("credits-skip-credits");
        IconVisualElement iconVE = new IconVisualElement(uiDoc.rootVisualElement.Q<VisualElement>("credits-skip-credits-icon"));
        iconVE.SetIcon(
            GFGInputAction.CUTSCENE_SKIP,
            ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.CUTSCENE_SKIP));
        ControlsManager.Instance.RegisterIconChanger(iconVE);
        creditsScrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void OnGeometryChanged(GeometryChangedEvent evt) {
        VisualElement spacer = new VisualElement();
        spacer.style.height = creditsScrollView.contentViewport.resolvedStyle.height;
        creditsScrollView.contentContainer.Add(spacer);
        creditsScrollView.contentContainer.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        creditsScrollView.schedule.Execute(RollCredits).StartingIn(1500); // 1.5s of just seeing the initial page
    }

    public void RollCredits() {
        float startY = creditsScrollView.scrollOffset.y;
        float endY = creditsScrollView.verticalScroller.highValue;
        float duration = 40f;

        LeanTween.value(this.gameObject, startY, endY, duration)
            .setEase(LeanTweenType.linear)
            .setOnUpdate((float y) => {
                creditsScrollView.scrollOffset = new Vector2(creditsScrollView.scrollOffset.x, y);
            })
            .setOnComplete(() => OnCompleted());
    }

    private void SetupCreditPage(CreditPage creditPage) {
        VisualElement creditsLogoParent = new VisualElement();
        creditsLogoParent.AddToClassList("credits-line-parent");
        VisualElement logo = new VisualElement();
        logo.AddToClassList("credits-line-logo");
        logo.style.backgroundImage = new StyleBackground(creditPage.creditPageLogo);
        creditsLogoParent.Add(logo);
        creditsScrollView.contentContainer.Add(creditsLogoParent);

        bool spriteOnLeft = false;
        foreach (CreditLine creditLine in creditPage.creditLines) {
            creditsScrollView.contentContainer.Add(CreateCreditLine(creditLine, spriteOnLeft));
            spriteOnLeft = !spriteOnLeft;
        }
    }

    private VisualElement CreateCreditLine(CreditLine creditLine, bool spriteOnLeft) {
        VisualElement creditLineVE = new VisualElement();
        creditLineVE.AddToClassList("credits-line-parent");

        VisualElement leftSprite = new VisualElement();
        leftSprite.AddToClassList("credits-line-sprite");

        VisualElement titleParent = new VisualElement();
        titleParent.AddToClassList("credits-line-title-parent");
        Label titleLabel = new Label();
        titleLabel.AddToClassList("credits-line-title-label");
        titleLabel.text = creditLine.title.ToUpper();
        titleParent.Add(titleLabel);

        VisualElement nameParent = new VisualElement();
        nameParent.AddToClassList("credits-line-name-parent");
        Label nameLabel = new Label();
        nameLabel.AddToClassList("credits-line-name-label");
        nameLabel.text = creditLine.name;
        nameParent.Add(nameLabel);

        VisualElement rightSprite = new VisualElement();
        rightSprite.AddToClassList("credits-line-sprite");

        if (spriteOnLeft) {
            leftSprite.style.backgroundImage = new StyleBackground(creditLine.rat);
        } else {
            rightSprite.style.backgroundImage = new StyleBackground(creditLine.rat);
        }

        creditLineVE.Add(leftSprite);
        creditLineVE.Add(titleParent);
        creditLineVE.Add(nameParent);
        creditLineVE.Add(rightSprite);

        return creditLineVE;
    }

    public void OnCompleted() {
        LeanTween.cancel(this.gameObject);
        FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        ControlsManager.Instance.UnregisterControlsReceiver(this);
        callback?.Invoke();
        Destroy(this.gameObject);
    }

    private void TryShowSkipHint() {
        if (Time.time - lastHintTime < hintCooldown) return;

        lastHintTime = Time.time;
        hintIndicator.style.visibility = Visibility.Visible;
        hideHintTask?.Pause();
        hideHintTask = hintIndicator.schedule.Execute(() => hintIndicator.style.visibility = Visibility.Hidden)
            .StartingIn((long)(hintCooldown * 1000));
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (action == GFGInputAction.CUTSCENE_SKIP) {
            OnCompleted();
        } else {
            TryShowSkipHint();
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        return;
    }

    [Serializable]
    public class CreditPage {
        public Sprite creditPageLogo;
        public List<CreditLine> creditLines;
        [TextArea(3, 5)]
        public List<string> additionalCredits;
    }

    [Serializable]
    public class CreditLine {
        public string name;
        [TextArea(3, 5)]
        public string title;
        public Sprite rat;
    }
}
