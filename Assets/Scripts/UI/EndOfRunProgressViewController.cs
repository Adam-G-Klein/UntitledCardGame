using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EndOfRunProgressViewController : MonoBehaviour
{
    public GameStateVariableSO gameState;
    public UIDocument endOfRunProgressUIDocument;
    [SerializeField] private CardUnlockViewController cardUnlockView;

    [Header("Testing")]
    [Tooltip("Shown as unlocked cards when this scene is played directly, without a finished run")]
    [SerializeField] private List<CardType> debugUnlockedCards = new();

    [Header("Text")]
    [SerializeField] private string victoryTitle = "Victory!";
    [SerializeField] private string defeatTitle = "Defeat";
    [SerializeField] private string unlockButtonText = "Unlock!";
    [SerializeField] private string noUnlockButtonText = "Progress";
    [SerializeField] private string maxLevelText = "Max Level";
    [Tooltip("{0} is the number of levels left until max level")]
    [SerializeField] private string unlocksRemainingFormat = "{0} unlocks remaining";
    [SerializeField] private string oneUnlockRemainingText = "1 unlock remaining";

    [Header("Row drop in")]
    [SerializeField] private float initialDelay = 0.5f;
    [SerializeField] private float rowDropDistance = 30f;
    [SerializeField] private float rowDropDuration = 0.4f;
    [Tooltip("Delay between each row starting its drop in")]
    [SerializeField] private float rowStagger = 0.15f;
    [SerializeField] private LeanTweenType rowDropEase = LeanTweenType.easeOutQuad;

    [Header("Total count up")]
    [SerializeField] private float pauseBeforeCountUp = 0.2f;
    [SerializeField] private float countUpDuration = 1f;
    [SerializeField] private LeanTweenType countUpEase = LeanTweenType.easeOutQuad;

    [Header("New stake unlocked drop in")]
    [SerializeField] private float pauseBeforeStakeUnlock = 0.3f;

    [Header("Progress bar")]
    [SerializeField] private float pauseBeforeProgressBar = 0.4f;
    [Tooltip("Seconds to fill a whole level's worth of the bar, partial fills take proportionally less")]
    [SerializeField] private float fullBarFillDuration = 1.2f;
    [SerializeField] private float minFillDuration = 0.3f;
    [Tooltip("How long a full bar holds before resetting for the next level")]
    [SerializeField] private float pauseOnLevelUp = 0.3f;
    [SerializeField] private LeanTweenType fillEase = LeanTweenType.easeInOutQuad;
    [Tooltip("Where the fill art starts and ends within the bar image, as a fraction of the bar's width")]
    [SerializeField, Range(0f, 1f)] private float fillStartFraction = 0.067f;
    [SerializeField, Range(0f, 1f)] private float fillEndFraction = 0.956f;

    [Header("Card unlock transition")]
    [SerializeField] private float progressScreenFadeDuration = 0.3f;

    private RunProgressSummary summary;
    private PlayerLevelingSO leveling;

    private VisualElement progressScreen;
    private Label titleLabel;
    private VisualElement combatRow;
    private VisualElement bossRow;
    private VisualElement stakeBonusRow;
    private VisualElement divider;
    private VisualElement totalRow;
    private VisualElement stakeUnlockRow;
    private Label totalRowValue;
    private VisualElement progressBarFillMask;
    private Label xpLabel;
    private Label unlocksRemainingLabel;
    private Button continueButton;

    void Start()
    {
        StartCoroutine(SetupWhenReady());
    }

    private IEnumerator SetupWhenReady()
    {
        yield return new WaitUntil(() =>
            UIDocumentUtils.ElementIsReady(endOfRunProgressUIDocument.rootVisualElement)
            && ProgressManager.Instance != null);
        Setup();
        yield return PlaySequence();
    }

    private void Setup()
    {
        leveling = ProgressManager.Instance.playerLevelingSO;
        summary = ProgressManager.Instance.progressSO.lastRunSummary ?? BuildEmptySummary();

        foreach (AchievementSO achievementSO in ProgressManager.Instance.achievementSOList)
        {
            achievementSO.lockedInProgress = achievementSO.currentProgress;
        }
        SaveManager.Instance.SavePlayerProgress();

        VisualElement root = endOfRunProgressUIDocument.rootVisualElement;
        progressScreen = root.Q<VisualElement>("progressScreen");
        titleLabel = root.Q<Label>("titleLabel");
        combatRow = root.Q<VisualElement>("combatRow");
        bossRow = root.Q<VisualElement>("bossRow");
        stakeBonusRow = root.Q<VisualElement>("stakeBonusRow");
        divider = root.Q<VisualElement>("divider");
        totalRow = root.Q<VisualElement>("totalRow");
        stakeUnlockRow = root.Q<VisualElement>("stakeUnlockRow");
        totalRowValue = root.Q<Label>("totalRowValue");
        progressBarFillMask = root.Q<VisualElement>("progressBarFillMask");
        xpLabel = root.Q<Label>("xpLabel");
        unlocksRemainingLabel = root.Q<Label>("unlocksRemainingLabel");
        continueButton = root.Q<Button>("continueButton");

        titleLabel.text = summary.wonRun ? victoryTitle : defeatTitle;
        root.Q<Label>("combatRowLabel").text = $"Combats Won ({summary.combatsWon})";
        root.Q<Label>("combatRowValue").text = summary.combatXP.ToString();
        root.Q<Label>("bossRowLabel").text = $"Bosses Defeated ({summary.bossesCleared})";
        root.Q<Label>("bossRowValue").text = summary.bossXP.ToString();
        root.Q<Label>("stakeBonusRowLabel").text = $"Stake {summary.ascensionLevel} Bonus (x{summary.ascensionMultiplier:0.##})";
        root.Q<Label>("stakeBonusRowValue").text = $"+{summary.ascensionBonusXP}";
        totalRowValue.text = "0";

        foreach (VisualElement element in new[] { combatRow, bossRow, stakeBonusRow, divider, totalRow, stakeUnlockRow })
        {
            SetDropProgress(element, 0f);
        }

        if (leveling != null)
        {
            UpdateProgressBar(summary.totalXPBefore, summary.levelBefore);
        }

        continueButton.text = summary.cardsUnlocked.Count > 0 ? unlockButtonText : noUnlockButtonText;
        continueButton.RegisterOnSelected(ContinueButtonHandler);
        continueButton.SetEnabled(false);
    }

    private RunProgressSummary BuildEmptySummary()
    {
        int totalXP = ProgressManager.Instance.progressSO.totalXP;
        int level = leveling != null ? leveling.GetLevel(totalXP) : 1;
        return new RunProgressSummary
        {
            wonRun = gameState.currentRunOutcome == RunOutcome.Victory,
            ascensionLevel = gameState.ascensionLevel,
            ascensionMultiplier = 1f,
            totalXPBefore = totalXP,
            totalXPAfter = totalXP,
            levelBefore = level,
            levelAfter = level,
            cardsUnlocked = new List<CardType>(debugUnlockedCards),
        };
    }

    private IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(initialDelay);

        List<VisualElement> rows = new List<VisualElement> { combatRow, bossRow };
        if (summary.ascensionBonusXP > 0) rows.Add(stakeBonusRow);
        rows.Add(divider);
        rows.Add(totalRow);
        for (int i = 0; i < rows.Count; i++)
        {
            StartCoroutine(DropIn(rows[i]));
            if (i < rows.Count - 1) yield return new WaitForSeconds(rowStagger);
        }
        yield return new WaitForSeconds(rowDropDuration);

        yield return new WaitForSeconds(pauseBeforeCountUp);
        yield return Tween(0f, summary.xpGained, countUpDuration, countUpEase,
            value => totalRowValue.text = Mathf.RoundToInt(value).ToString());

        if (summary.newStakeUnlocked)
        {
            yield return new WaitForSeconds(pauseBeforeStakeUnlock);
            yield return DropIn(stakeUnlockRow);
        }

        if (leveling != null)
        {
            yield return new WaitForSeconds(pauseBeforeProgressBar);
            yield return AnimateProgressBar();
        }
        else
        {
            Debug.LogError("ProgressManager has no PlayerLevelingSO assigned, skipping the progress bar");
        }

        continueButton.SetEnabled(true);
        FocusManager.Instance.RegisterFocusables(endOfRunProgressUIDocument);
    }

    private IEnumerator DropIn(VisualElement element)
    {
        yield return Tween(0f, 1f, rowDropDuration, rowDropEase, value => SetDropProgress(element, value));
    }

    private void SetDropProgress(VisualElement element, float progress)
    {
        element.style.opacity = progress;
        element.style.translate = new Translate(0, -rowDropDistance * (1f - progress), 0);
    }

    private IEnumerator AnimateProgressBar()
    {
        int level = summary.levelBefore;
        float xp = summary.totalXPBefore;
        UpdateProgressBar(xp, level);

        while (true)
        {
            int nextLevelXP = leveling.GetNextLevelXP(level);
            if (nextLevelXP < 0) break;

            bool levelsUp = summary.totalXPAfter >= nextLevelXP;
            float target = levelsUp ? nextLevelXP : summary.totalXPAfter;
            if (target > xp)
            {
                float levelSpan = nextLevelXP - leveling.GetLevelStartXP(level);
                float duration = Mathf.Max(minFillDuration, fullBarFillDuration * (target - xp) / levelSpan);
                int levelForTween = level;
                yield return Tween(xp, target, duration, fillEase, value => UpdateProgressBar(value, levelForTween));
                xp = target;
            }

            if (!levelsUp) break;
            yield return new WaitForSeconds(pauseOnLevelUp);
            level++;
            UpdateProgressBar(xp, level);
        }
    }

    private void UpdateProgressBar(float totalXP, int level)
    {
        int levelStartXP = leveling.GetLevelStartXP(level);
        int nextLevelXP = leveling.GetNextLevelXP(level);
        float fraction;
        if (nextLevelXP < 0)
        {
            fraction = 1f;
            xpLabel.text = maxLevelText;
        }
        else
        {
            int levelSpan = nextLevelXP - levelStartXP;
            fraction = Mathf.Clamp01((totalXP - levelStartXP) / levelSpan);
            xpLabel.text = $"[{Mathf.FloorToInt(totalXP - levelStartXP)}/{levelSpan}]";
        }
        progressBarFillMask.style.width = Length.Percent(Mathf.Lerp(fillStartFraction, fillEndFraction, fraction) * 100f);

        int unlocksRemaining = leveling.MaxLevel - level;
        unlocksRemainingLabel.text = unlocksRemaining == 1 ? oneUnlockRemainingText : string.Format(unlocksRemainingFormat, unlocksRemaining);
    }

    private IEnumerator Tween(float from, float to, float duration, LeanTweenType ease, Action<float> onUpdate)
    {
        if (duration <= 0f)
        {
            onUpdate(to);
            yield break;
        }
        bool done = false;
        LeanTween.value(gameObject, from, to, duration)
            .setEase(ease)
            .setOnUpdate(onUpdate)
            .setOnComplete(() => done = true);
        yield return new WaitUntil(() => done);
    }

    private void ContinueButtonHandler()
    {
        continueButton.SetEnabled(false);
        if (summary.cardsUnlocked.Count == 0)
        {
            SceneTransitionManager.LoadScene("MainMenu", .5f);
            return;
        }
        if (cardUnlockView == null)
        {
            Debug.LogError("EndOfRunProgressViewController has no CardUnlockViewController assigned, skipping the card unlock view");
            SceneTransitionManager.LoadScene("MainMenu", .5f);
            return;
        }
        StartCoroutine(GoToCardUnlocks());
    }

    private IEnumerator GoToCardUnlocks()
    {
        FocusManager.Instance.UnregisterFocusables(progressScreen);
        yield return Tween(1f, 0f, progressScreenFadeDuration, LeanTweenType.linear, value => progressScreen.style.opacity = value);
        progressScreen.style.display = DisplayStyle.None;
        cardUnlockView.Show(summary.cardsUnlocked);
    }
}
