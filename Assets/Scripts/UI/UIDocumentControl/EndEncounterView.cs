using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EndEncounterView : MonoBehaviour
{
    [SerializeField]
    private float fadeTime = .1f;
    [SerializeField]
    private GameStateVariableSO gameState;

    private UIDocument doc;
    private UIDocumentScreenspace docRenderer;
    private VisualElement nextSceneButton;

    private Material mat;
    private CanvasGroup canvasGroup;
    private VisualElement goldEarnedContainer;
    private VisualElement interestEarnedContainer;
    private VisualElement bonusEarnedContainer;
    private int goldEarned;
    private int interestEarned;
    private float interestRate;
    private int interestCap;
    private int bonusTeamSizeReward;
    private int bonusManaReward;


    void OnEnable()
    {
        doc = GetComponent<UIDocument>();
        docRenderer = GetComponent<UIDocumentScreenspace>();
        // Set initial alpha to 0
        mat = GetComponent<RawImage>().material;
        mat.SetFloat("_alpha", 0);

        // Ensure the CanvasGroup alpha is set to 0 and blocks raycasts
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;

        nextSceneButton = doc.rootVisualElement.Q<VisualElement>("next-scene");
        nextSceneButton.RegisterOnSelected(NextClick);
        VisualElementFocusable nextSceneButtonFocusable = nextSceneButton.AsFocusable();
        FocusManager.Instance.RegisterFocusableTarget(nextSceneButtonFocusable);
        goldEarnedContainer = doc.rootVisualElement.Q("base-gold-container");
        interestEarnedContainer = doc.rootVisualElement.Q("interest-container");
        bonusEarnedContainer = doc.rootVisualElement.Q("bonus-elite-reward-container");
    }

    public void Setup(int baseGoldEarnedPerBattle, int interestEarned, int interestCap, float interestPercentage, int bonusManaReward = 0, int bonusTeamSizeReward = 0)
    {
        goldEarnedContainer.Clear();
        interestEarnedContainer.Clear();

        this.goldEarned = baseGoldEarnedPerBattle;
        this.interestEarned = interestEarned;
        this.interestCap = interestCap;
        this.interestRate = interestPercentage;
        this.bonusManaReward = bonusManaReward;
        this.bonusTeamSizeReward = bonusTeamSizeReward;

        int earnOneInterestEveryXGold = (int)(1 / interestPercentage);

        string templateText = doc.rootVisualElement.Q<Label>("interest-help").text;
        doc.rootVisualElement.Q<Label>("interest-help").text = string.Format(templateText, earnOneInterestEveryXGold, interestCap);
        // add the above back in if we need modularity in interest cap/percent
    }

    public void Show(bool keepButtonDisabled = false) {
        nextSceneButton.SetEnabled(false);
        // Ensure the initial alpha is set to 0 before starting the fade-in
        canvasGroup.blocksRaycasts = true;
        mat.SetFloat("_alpha", 0);
        canvasGroup.alpha = 0;

        if (this.bonusManaReward > 0 || this.bonusTeamSizeReward > 0) {
            bonusEarnedContainer.visible = true;
            Label bonusLabel = bonusEarnedContainer.Q<Label>("bonus-elite-reward-text");
            string bonusText = "";
            if (this.bonusManaReward > 0) {
                bonusText = $"+{this.bonusManaReward} Energy";
            }
            if (this.bonusTeamSizeReward > 0) {
                bonusText = $"+{this.bonusTeamSizeReward} Team Size";
            }
            bonusLabel.text = bonusText;
        } else {
            bonusEarnedContainer.visible = false;
        }

        LeanTween.value(gameObject, 0, 1, fadeTime)
            .setOnUpdate((float val) => {
                mat.SetFloat("_alpha", val);
                canvasGroup.alpha = val;
            })
            .setOnComplete(() => {
                StartCoroutine(AnimateText());
                StartCoroutine(PostShowCoroutine(keepButtonDisabled));
            });
    }

    public void EnableNextButton() {
        nextSceneButton.SetEnabled(true);
        FocusManager.Instance.EnableFocusableTarget(nextSceneButton.AsFocusable());
        FocusManager.Instance.SetFocus(nextSceneButton.AsFocusable());
    }

    private IEnumerator PostShowCoroutine(bool keepButtonDisabled) {
        if (gameState.BuildTypeDemoOrConvention()
                && DemoDirector.Instance != null
                && !DemoDirector.Instance.IsStepCompleted(DemoStepName.PostCombatRewardsDialogue)) {
            yield return DemoDirector.Instance.InvokeDemoStepCoroutine(DemoStepName.PostCombatRewardsDialogue);
        }
        if (!keepButtonDisabled) {
            EnableNextButton();
        }
    }

    private IEnumerator AnimateText()
    {
        for (int i = 0; i < interestEarned; i++)
        {
            Label label = MakeMoneyLabel();
            interestEarnedContainer.Add(label);
            AnimateDollar(label);
            MusicController.Instance.PlaySFX("event:/SFX/SFX_EarnMoney");
            yield return new WaitForSeconds(.1f);
        }
        for (int i = 0; i < goldEarned; i++)
        {
            Label label = MakeMoneyLabel();
            goldEarnedContainer.Add(label);
            AnimateDollar(label);
            MusicController.Instance.PlaySFX("event:/SFX/SFX_EarnMoney");
            yield return new WaitForSeconds(.1f);
        }
    }

    private Label MakeMoneyLabel() {
        Label label = new Label();
        label.text = "$";
        label.AddToClassList("post-combat-text");
        label.AddToClassList("post-combat-reward-dollar");
        return label;
    }

    private void AnimateDollar(VisualElement visualElement)
    {
        LeanTween.value(0f, 1f, .1f)
        .setEase(LeanTweenType.easeOutQuad)
        .setOnUpdate((float val) =>
        {
            visualElement.style.scale = new StyleScale(new Scale(new Vector2(val, val)));
        });

        LeanTween.value(45, 0, .1f)
        .setEase(LeanTweenType.easeInBack)
        .setOnUpdate((float val) =>
        {
            visualElement.style.rotate = new StyleRotate(new Rotate(val));
        });
    }

    private void NextClick(ClickEvent evt)
    {
        // trying out putting the update here when you click the button so the amount of interest you make makes a little more sense
        gameState.playerData.GetValue().gold += goldEarned + interestEarned;
        // Prevent double clicks because that will advance to the next scene!!!!
        nextSceneButton.SetEnabled(false);
        gameState.LoadNextLocation();
    }
}
