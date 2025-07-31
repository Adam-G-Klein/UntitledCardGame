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
    }

    public void Setup(int baseGoldEarnedPerBattle, int interestEarned, int interestCap, float interestPercentage)
    {
        doc.rootVisualElement.Q<Label>("base-gold").text = "Base Gold Earned: " + baseGoldEarnedPerBattle.ToString();
        doc.rootVisualElement.Q<Label>("interest").text = "Interest Earned: " + interestEarned.ToString();
        doc.rootVisualElement.Q<Label>("interest-help").text = "(You earn " +
            interestPercentage.ToString("P0") + 
            " of your current Gold as Interest, capped at " +
            interestCap.ToString() + " gold per combat)";
    }

    public void Show() {
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
                nextSceneButton.SetEnabled(true);
                FocusManager.Instance.SetFocus(nextSceneButton.AsFocusable());
            });
    }

    private void NextClick(ClickEvent evt) {
        // Prevent double clicks because that will advance to the next scene!!!!
        nextSceneButton.SetEnabled(false);
        gameState.LoadNextLocation();
    }
}
