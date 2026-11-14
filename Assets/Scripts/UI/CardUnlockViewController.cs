using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// The "New Cards!" half of the end of run progress screen. Shown by EndOfRunProgressViewController when cards were unlocked.
public class CardUnlockViewController : MonoBehaviour
{
    public GameStateVariableSO gameState;
    public UIDocument endOfRunProgressUIDocument;

    [Header("Text")]
    [SerializeField] private string titleText = "New Cards!";
    [SerializeField] private string continueButtonText = "Continue";

    [Header("Envelopes")]
    [Tooltip("Which booster pack envelope art to use for each rarity, looked up in GameplayConstants with the card's pack")]
    [SerializeField] private BoosterPack.BoosterPackType commonEnvelopeType = BoosterPack.BoosterPackType.PackBasicBooster;
    [SerializeField] private BoosterPack.BoosterPackType uncommonEnvelopeType = BoosterPack.BoosterPackType.PackBasicBooster;
    [SerializeField] private BoosterPack.BoosterPackType rareEnvelopeType = BoosterPack.BoosterPackType.PackDeluxeBooster;
    [Tooltip("Used when GameplayConstants has no envelope for the card's pack")]
    [SerializeField] private Sprite fallbackEnvelopeSprite;
    [SerializeField] private Vector2 envelopeSize = new Vector2(243f, 338f);
    [SerializeField] private float envelopeSpacing = 67f;
    [SerializeField] private int maxEnvelopesPerRow = 5;
    [SerializeField] private float rowSpacing = 30f;
    [Tooltip("Envelope size multiplier when they don't fit on one row")]
    [SerializeField] private float multiRowScale = 0.8f;

    [Header("Screen fade in")]
    [SerializeField] private float screenFadeDuration = 0.3f;

    [Header("Envelope drop in")]
    [SerializeField] private float dropDistance = 30f;
    [SerializeField] private float dropDuration = 0.4f;
    [SerializeField] private float dropStagger = 0.12f;
    [SerializeField] private LeanTweenType dropEase = LeanTweenType.easeOutQuad;

    [Header("Envelope shake (until opened)")]
    [SerializeField] private float shakeIntervalMin = 0.8f;
    [SerializeField] private float shakeIntervalMax = 2f;
    [SerializeField] private float shakeDuration = 0.5f;
    [Tooltip("Degrees")]
    [SerializeField] private float shakeAngle = 6f;
    [Tooltip("Back and forth wiggles per shake")]
    [SerializeField] private float shakeCount = 3f;

    [Header("Envelope open")]
    [SerializeField] private float openPunchScale = 1.15f;
    [SerializeField] private float openPunchDuration = 0.12f;
    [SerializeField] private float envelopeFadeDuration = 0.25f;
    [SerializeField] private float envelopeFadeScale = 1.3f;
    [SerializeField] private float cardStartScale = 0.6f;
    [SerializeField] private float cardRevealDuration = 0.45f;
    [SerializeField] private LeanTweenType cardRevealEase = LeanTweenType.easeOutBack;
    [Tooltip("FMOD event played when an envelope opens, leave empty for none")]
    [SerializeField] private string openSfx = "";

    [Header("Continue button")]
    [SerializeField] private float pauseBeforeContinue = 0.3f;
    [SerializeField] private float continueFadeDuration = 0.3f;

    private class Envelope
    {
        public Button button;
        public VisualElement art;
        public VisualElement cardSlot;
        public bool opened;
        public Coroutine shakeRoutine;
    }

    private readonly List<Envelope> envelopes = new();
    private bool envelopesReady;
    private int openedCount;
    private Button continueButton;

    public void Show(List<CardType> cards)
    {
        StartCoroutine(ShowSequence(cards));
    }

    private IEnumerator ShowSequence(List<CardType> cards)
    {
        VisualElement root = endOfRunProgressUIDocument.rootVisualElement;
        VisualElement screen = root.Q<VisualElement>("cardUnlockScreen");
        VisualElement envelopeArea = root.Q<VisualElement>("envelopeArea");
        root.Q<Label>("cardUnlockTitle").text = titleText;
        continueButton = root.Q<Button>("cardUnlockContinueButton");
        continueButton.text = continueButtonText;
        continueButton.RegisterOnSelected(ContinueButtonHandler);
        continueButton.SetEnabled(false);
        continueButton.style.opacity = 0f;

        BuildEnvelopes(cards, envelopeArea);

        screen.style.opacity = 0f;
        screen.style.display = DisplayStyle.Flex;
        yield return Tween(0f, 1f, screenFadeDuration, LeanTweenType.linear, value => screen.style.opacity = value);

        for (int i = 0; i < envelopes.Count; i++)
        {
            Envelope envelope = envelopes[i];
            StartCoroutine(Tween(0f, 1f, dropDuration, dropEase, value => SetDropProgress(envelope.button, value)));
            if (i < envelopes.Count - 1) yield return new WaitForSeconds(dropStagger);
        }
        yield return new WaitForSeconds(dropDuration);

        envelopesReady = true;
        foreach (Envelope envelope in envelopes)
        {
            FocusManager.Instance.RegisterFocusableTarget(envelope.button.AsFocusable());
            envelope.shakeRoutine = StartCoroutine(ShakeUntilOpened(envelope));
        }
        if (envelopes.Count == 0) StartCoroutine(ShowContinueButton());
    }

    private void BuildEnvelopes(List<CardType> cards, VisualElement envelopeArea)
    {
        envelopeArea.Clear();
        envelopes.Clear();
        if (cards.Count == 0) return;

        int rowCount = Mathf.CeilToInt(cards.Count / (float)Mathf.Max(1, maxEnvelopesPerRow));
        int perRow = Mathf.CeilToInt(cards.Count / (float)rowCount);
        float scale = rowCount > 1 ? multiRowScale : 1f;

        VisualElement row = null;
        for (int i = 0; i < cards.Count; i++)
        {
            if (i % perRow == 0)
            {
                row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList("eor-envelope-row");
                envelopeArea.Add(row);
            }
            Envelope envelope = BuildEnvelope(cards[i], envelopeSize * scale);
            row.Add(envelope.button);
            envelopes.Add(envelope);
        }
    }

    private Envelope BuildEnvelope(CardType card, Vector2 size)
    {
        FindCardSource(card, out PackSO pack, out CompanionTypeSO companion, out Card.CardRarity rarity);

        Button button = new Button();
        button.AddToClassList("eor-envelope");
        button.AddToClassList("focusable");
        button.style.width = size.x;
        button.style.height = size.y;
        button.style.marginLeft = envelopeSpacing / 2f;
        button.style.marginRight = envelopeSpacing / 2f;
        button.style.marginTop = rowSpacing / 2f;
        button.style.marginBottom = rowSpacing / 2f;
        SetDropProgress(button, 0f);

        VisualElement cardSlot = new VisualElement { pickingMode = PickingMode.Ignore };
        cardSlot.AddToClassList("eor-envelope-layer");
        cardSlot.style.opacity = 0f;
        CardView cardView = new CardView(card, companion, rarity, true, pack);
        cardView.cardContainer.focusable = false;
        cardView.cardContainer.RemoveFromClassList("focusable");
        cardView.cardContainer.style.width = Length.Percent(100);
        cardView.cardContainer.style.height = Length.Percent(100);
        UIDocumentUtils.SetAllPickingMode(cardView.cardContainer, PickingMode.Ignore);
        cardSlot.Add(cardView.cardContainer);

        VisualElement art = new VisualElement { pickingMode = PickingMode.Ignore };
        art.AddToClassList("eor-envelope-layer");
        Sprite envelopeSprite = GetEnvelopeSprite(pack, rarity);
        if (envelopeSprite != null) art.style.backgroundImage = new StyleBackground(envelopeSprite);
        else Debug.LogWarning($"No envelope sprite for {card.Name} (pack: {(pack != null ? pack.name : "none")}), assign a Fallback Envelope Sprite on CardUnlockViewController");

        button.Add(cardSlot);
        button.Add(art);

        Envelope envelope = new Envelope { button = button, art = art, cardSlot = cardSlot };
        button.RegisterOnSelected(() => OpenEnvelope(envelope));
        return envelope;
    }

    // Unlocked cards come from the run's pack pools or the team's companion pools, which also decides the card back and envelope
    private void FindCardSource(CardType card, out PackSO pack, out CompanionTypeSO companion, out Card.CardRarity rarity)
    {
        companion = null;
        foreach (PackSO activePack in gameState.activePacks)
        {
            if (TryGetRarity(activePack.packCardPoolSO, card, out rarity))
            {
                pack = activePack;
                return;
            }
        }
        foreach (Companion activeCompanion in gameState.companions.activeCompanions)
        {
            if (TryGetRarity(activeCompanion.companionType.cardPool, card, out rarity))
            {
                companion = activeCompanion.companionType;
                pack = companion.pack;
                return;
            }
        }
        Debug.LogWarning($"Couldn't find {card.Name} in any of the run's card pools, defaulting to common");
        pack = card.packFrom;
        rarity = Card.CardRarity.COMMON;
    }

    private static bool TryGetRarity(CardPoolSO pool, CardType card, out Card.CardRarity rarity)
    {
        rarity = Card.CardRarity.NONE;
        if (pool == null) return false;
        if (pool.unlockableRareCards.Contains(card) || pool.rareCards.Contains(card)) rarity = Card.CardRarity.RARE;
        else if (pool.unlockableUncommonCards.Contains(card) || pool.uncommonCards.Contains(card)) rarity = Card.CardRarity.UNCOMMON;
        else if (pool.unlockableCommonCards.Contains(card) || pool.commonCards.Contains(card)) rarity = Card.CardRarity.COMMON;
        return rarity != Card.CardRarity.NONE;
    }

    private Sprite GetEnvelopeSprite(PackSO pack, Card.CardRarity rarity)
    {
        BoosterPack.BoosterPackType envelopeType = rarity switch
        {
            Card.CardRarity.RARE => rareEnvelopeType,
            Card.CardRarity.UNCOMMON => uncommonEnvelopeType,
            _ => commonEnvelopeType,
        };
        Sprite sprite = null;
        if (pack != null)
        {
            GameplayConstantsSO constants = GameplayConstantsSingleton.Instance.gameplayConstants;
            sprite = constants.GetBoosterPackSprite(envelopeType, pack);
            if (sprite == null) sprite = constants.GetBoosterPackSprite(commonEnvelopeType, pack);
        }
        return sprite != null ? sprite : fallbackEnvelopeSprite;
    }

    private IEnumerator ShakeUntilOpened(Envelope envelope)
    {
        while (!envelope.opened)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(shakeIntervalMin, shakeIntervalMax));
            yield return Tween(0f, 1f, shakeDuration, LeanTweenType.linear, t =>
            {
                if (envelope.opened) return;
                float angle = shakeAngle * Mathf.Sin(t * shakeCount * 2f * Mathf.PI) * (1f - t);
                SetRotation(envelope.art, angle);
            });
        }
    }

    private void OpenEnvelope(Envelope envelope)
    {
        if (!envelopesReady || envelope.opened) return;
        envelope.opened = true;
        if (envelope.shakeRoutine != null) StopCoroutine(envelope.shakeRoutine);
        FocusManager.Instance.UnregisterFocusableTarget(envelope.button.AsFocusable());
        envelope.button.focusable = false;
        envelope.button.pickingMode = PickingMode.Ignore;
        StartCoroutine(OpenSequence(envelope));
    }

    private IEnumerator OpenSequence(Envelope envelope)
    {
        SetRotation(envelope.art, 0f);
        if (!string.IsNullOrEmpty(openSfx)) MusicController.Instance.PlaySFX(openSfx);

        yield return Tween(1f, openPunchScale, openPunchDuration, LeanTweenType.easeOutQuad, value => SetScale(envelope.art, value));

        StartCoroutine(Tween(0f, 1f, envelopeFadeDuration, LeanTweenType.easeInQuad, t =>
        {
            envelope.art.style.opacity = 1f - t;
            SetScale(envelope.art, Mathf.Lerp(openPunchScale, envelopeFadeScale, t));
        }));
        yield return Tween(0f, 1f, cardRevealDuration, cardRevealEase, t =>
        {
            envelope.cardSlot.style.opacity = Mathf.Clamp01(t);
            SetScale(envelope.cardSlot, Mathf.LerpUnclamped(cardStartScale, 1f, t));
        });
        envelope.art.style.display = DisplayStyle.None;

        openedCount++;
        if (openedCount == envelopes.Count) StartCoroutine(ShowContinueButton());
    }

    private IEnumerator ShowContinueButton()
    {
        yield return new WaitForSeconds(pauseBeforeContinue);
        yield return Tween(0f, 1f, continueFadeDuration, LeanTweenType.linear, value => continueButton.style.opacity = value);
        continueButton.SetEnabled(true);
        FocusManager.Instance.RegisterFocusableTarget(continueButton.AsFocusable());
    }

    private void ContinueButtonHandler()
    {
        continueButton.SetEnabled(false);
        SceneTransitionManager.LoadScene("MainMenu", .5f);
    }

    private void SetDropProgress(VisualElement element, float progress)
    {
        element.style.opacity = progress;
        element.style.translate = new Translate(0, -dropDistance * (1f - progress), 0);
    }

    private static void SetScale(VisualElement element, float scale)
    {
        element.style.scale = new Scale(new Vector3(scale, scale, 1f));
    }

    private static void SetRotation(VisualElement element, float degrees)
    {
        element.style.rotate = new Rotate(new Angle(degrees, AngleUnit.Degree));
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
}
