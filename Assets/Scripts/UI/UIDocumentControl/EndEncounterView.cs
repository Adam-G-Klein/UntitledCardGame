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
    private int goldEarned;
    private int interestEarned;


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
    }

    public void Setup(int baseGoldEarnedPerBattle, int interestEarned, int interestCap, float interestPercentage)
    {
        goldEarnedContainer.Clear();
        interestEarnedContainer.Clear();

        this.goldEarned = baseGoldEarnedPerBattle;
        this.interestEarned = interestEarned;
        /*doc.rootVisualElement.Q<Label>("interest-help").text = "(You earn " +
            interestPercentage.ToString("P0") + 
            " of your current Gold as Interest, capped at " +
            interestCap.ToString() + " gold per combat)";*/
        // add the above back in if we need modularity in interest cap/percent
    }

    public void Show(bool keepButtonDisabled = false) {
        nextSceneButton.SetEnabled(false);
        // Ensure the initial alpha is set to 0 before starting the fade-in
        canvasGroup.blocksRaycasts = true;
        mat.SetFloat("_alpha", 0);
        canvasGroup.alpha = 0;
        LeanTween.value(gameObject, 0, 1, fadeTime)
            .setOnUpdate((float val) => {
                mat.SetFloat("_alpha", val);
                canvasGroup.alpha = val;
            })
            .setOnComplete(() => {
                StartCoroutine(AnimateText());
                if (keepButtonDisabled) return;
                nextSceneButton.SetEnabled(true);
                FocusManager.Instance.EnableFocusableTarget(nextSceneButton.AsFocusable());
                FocusManager.Instance.SetFocus(nextSceneButton.AsFocusable());
            });
    }

    public void EnableNextButton() {
        nextSceneButton.SetEnabled(true);
        FocusManager.Instance.EnableFocusableTarget(nextSceneButton.AsFocusable());
        FocusManager.Instance.SetFocus(nextSceneButton.AsFocusable());
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
