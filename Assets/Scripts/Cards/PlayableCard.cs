using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(UIDocumentCard))]
[RequireComponent(typeof(UIStateEventListener))]
[RequireComponent(typeof(Targetable))]
public class PlayableCard : MonoBehaviour,
    IPointerClickHandler,
    IDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Card card;
    public DeckInstance deckFrom;

    [SerializeField]
    private float hoverScale = 2f;
    [SerializeField]
    private float nonHoverScale = 1f;
    [SerializeField]
    private CardCastEvent cardCastEvent;
    public bool hovered = false;

    private UIState currentState;

    // Checked by PlayerHand when discarding the whole hand
    // set back to false there when it's checked
    public bool retained = false;
    public AudioClip cardHover;
    // public float hoverSFXVolume = 0.1f;
    public float hoverYOffset = .75f;
    public float hoverZOffset = 0.5f;
    public GameObject hoverObjectToMove;
    private float hoverAnimationTime = .2f;
    private Vector3 startPos;

    public GameObject cardCastVFXPrefab;
    public GameObject cardExhaustVFXPrefab;

    private UIDocumentCard docCard;
    public bool interactable = false; // To be set to true when clicking on the card shouldn't do anything
    public bool hoverable = true; // To be set to true when hovering/focusing the card shouldn't make the card do anything
    public bool currentlyCastingCard = false;
    public bool hoverInPlace = false;

    private bool cardExhaustVFXFinished = false;

    private const string CardCalculationDamageKey = "card_calculation_rpl_damage_key";

    private Vector3 discardDest;

    public void Awake()
    {
        docCard = GetComponent<UIDocumentCard>();
    }
    public void Start()
    {
        transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
        discardDest = this.deckFrom.transform.position;
    }

    public void OnPointerClickVoid()
    {
        OnPointerClick(null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Card clicked");
        Debug.Log(
            "CURRENT STATE: " + currentState +
            "Interactable: " + interactable +
            " EnemyEncounterManager.GetCastingCard(): " + EnemyEncounterManager.Instance.GetCastingCard().ToString()
        );
        if (currentState != UIState.DEFAULT || !interactable || EnemyEncounterManager.Instance.GetCastingCard() || !PlayerHand.Instance.GetCanPlayCards()) return;
        if (card.GetManaCost() > ManaManager.Instance.currentMana)
        {
            // StartCoroutine(GenericEntityDialogueParticipant
            //     .Instance
            //     .SpeakCompanionLine(
            //         "You don't have enough mana for me to cast that for you :(",
            //         deckFrom.GetComponent<CompanionInstance>().companion.companionType, 3f));
            DialogueView.Instance.gameObject.SetActive(true);
            DialogueView.Instance.SpeakLine(deckFrom.GetComponent<CompanionInstance>().companion.companionType,
                    "You don't have enough mana for me to cast that for you.");
            return;
        }

        if (!card.cardType.playable)
        {
            StartCoroutine(GenericEntityDialogueParticipant
                .Instance
                .SpeakCompanionLine(
                    "Not sure what I can do with that one :(",
                    deckFrom.GetComponent<CompanionInstance>().companion.companionType, 3f));
            return;
        }
        if (eventData != null && eventData.button != PointerEventData.InputButton.Left) return;
        EnemyEncounterManager.Instance.SetCastingCard(true);
        PlayerHand.Instance.SetHoverable(false);
        interactable = false;
        currentlyCastingCard = true;

        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Card;
        EffectManager.Instance.invokeEffectWorkflow(document, card.effectSteps, CardFinishCastingCallback());
        UnHoverAssociatedCompanion();
    }

    private IEnumerator CardFinishCastingCallback()
    {
        // report card cast for achievements
        if (card.cardType.cardCategory == CardCategory.Attack && card.GetManaCost() == 0)
        {
            ProgressManager.Instance.ReportProgressEvent(GameActionType.ZERO_COST_ATTACKS_PLAYED, 1);
        }

        List<PlayableCard> cardsInHand = PlayerHand.Instance.GetCardsOrdered();
        int cardPlayedIndex = cardsInHand.IndexOf(this);
        ManaManager.Instance.updateMana(-card.GetManaCost());
        StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(card)));
        IncrementCastCount();
        EnemyEncounterManager.Instance.combatEncounterState.CastCard(card);
        PlayerHand.Instance.SetHoverable(true);
        yield return StartCoroutine(PlayerHand.Instance.OnCardCast(this));

        if (card.cardType.exhaustsWhenPlayed)
        {
            Debug.Log("STARTING exhaust when played coroutine");
            yield return PlayerHand.Instance.ExhaustCard(this);
            Debug.Log("DONE WITH exhaust when played coroutine");

            EnemyEncounterManager.Instance.SetCastingCard(false);
            PlayerHand.Instance.HoverNextCardAfterCast();

            // Add a WaitForSeconds so that the target hovering does not break when using keyboard.
            // yield return new WaitForSeconds(0.5f);
            // Don't need to call ResizeHand, because ExhaustCard already does it!
        }
        else
        {
            // remove the card from the hand first so that resizing doesn't affect the card being cast
            yield return StartCoroutine(PlayerHand.Instance.SafeRemoveCardFromHand(this));

            EnemyEncounterManager.Instance.SetCastingCard(false);
            PlayerHand.Instance.HoverNextCardAfterCast();

            yield return StartCoroutine(PlayerHand.Instance.ResizeHand(this));

            // Non-power cards should discard to deck after they are played.
            if (card.cardType.cardCategory == CardCategory.Passive)
            {
                cleanupAndDestroy();
            }
            else
            {
                yield return StartCoroutine(PlayerHand.Instance.DiscardCard(this, true));
            }
        }

        // If the hand is empty as a result of playing this card, invoke any subscribers.
        if (cardsInHand.Count == 0)
        {
            Debug.Log("Hand is empty, triggering downstream OnHandEmpty subscribers");
            yield return PlayerHand.Instance.OnHandEmpty();
        }
        else if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.KeyboardController)
        {
            Debug.Log("Trying to a hover a new card now that the card has been played");
            //PlayerHand.Instance.FocusACard(this);
        }
        Debug.Log("FINISHED CardFinishCastingCallback");
    }

    public void CardExhaustVFX()
    {
        // GameObject.Instantiate(
        //     cardExhaustVFXPrefab,
        //     this.transform.position,
        //     Quaternion.identity);
        FXExperience experience = PrefabInstantiator.instantiateFXExperience(cardExhaustVFXPrefab, gameObject.transform.position);

        experience.BindGameObjectsToTracks(new Dictionary<string, GameObject>() {
            { "CardAnimationTrack", gameObject },
        });
        // experience.AddLocationToKey("Card", this.transform.position);
        // experience.AddLocationToKey("Companion", discardDest);
        // This makes it so that we can use 0,0 as the "current position of the card"
        gameObject.transform.SetParent(experience.transform);
        experience.onExperienceOver += () => {
            cardExhaustVFXFinished = true;
            this.gameObject.transform.SetParent(null);
        };
        experience.StartExperience();
    }

    private IEnumerator CardDiscardVFX(GameObject cardGameObject)
    {
        FXExperience experience = PrefabInstantiator.instantiateFXExperience(cardCastVFXPrefab, cardGameObject.transform.position);

        experience.BindGameObjectsToTracks(new Dictionary<string, GameObject>() {
            { "CardAnimationTrack", cardGameObject },
            { "CardTweenTrack", cardGameObject },
            { "CardScaleTrack", cardGameObject },
        });
        experience.AddLocationToKey("Card", this.transform.position);
        experience.AddLocationToKey("Companion", discardDest);
        // This makes it so that we can use 0,0 as the "current position of the card"
        cardGameObject.transform.SetParent(experience.transform);
        experience.onExperienceOver += CardDiscardVFXFinished;
        Debug.Log("Started card discard VFX");
        experience.StartExperience();
        yield return null;
        Debug.Log("Finished card discard VFX");
    }

    private void CardDiscardVFXFinished()
    {
        // If we don't do this, then the crew (the card) goes down with the ship (the FXExperience)
        this.gameObject.transform.SetParent(null);
        cleanupAndDestroy();
    }

    public void UpdateCardText()
    {
        Debug.Log("trying to update card text");
        StartCoroutine(UpdateCardTextCoroutine());
    }

    public IEnumerator UpdateCardTextCoroutine()
    {
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Card;

        if (card.effectSteps != null && card.effectSteps.Count != 0)
        {
            document.stringMap[CardCalculationDamageKey] = "rpl_damage";
            yield return EffectManager.Instance.invokeEffectWorkflowForCalculation(
                document,
                card.effectSteps,
                null
            );
        }
        if (card.cardType.inPlayerHandEndOfTurnWorkflow != null && card.cardType.inPlayerHandEndOfTurnWorkflow.effectSteps.Count != 0)
        {
            document.stringMap[CardCalculationDamageKey] = "inhand_rpl_damage";
            yield return EffectManager.Instance.invokeEffectWorkflowForCalculation(
                document,
                card.cardType.inPlayerHandEndOfTurnWorkflow.effectSteps,
                null
            );
        }
        if (card.cardType.onExhaustEffectWorkflow != null && card.cardType.onExhaustEffectWorkflow.effectSteps.Count != 0)
        {
            document.stringMap[CardCalculationDamageKey] = "onexhaust_rpl_damage";
            yield return EffectManager.Instance.invokeEffectWorkflowForCalculation(
                document,
                card.cardType.onExhaustEffectWorkflow.effectSteps,
                null
            );
        }
        if (card.cardType.onDiscardEffectWorkflow != null && card.cardType.onDiscardEffectWorkflow.effectSteps.Count != 0)
        {
            document.stringMap[CardCalculationDamageKey] = "ondiscard_rpl_damage";
            yield return EffectManager.Instance.invokeEffectWorkflowForCalculation(
                document,
                card.cardType.onDiscardEffectWorkflow.effectSteps,
                null
            );
        }
        if (card.cardType.onDrawEffectWorkflow != null && card.cardType.onDrawEffectWorkflow.effectSteps.Count != 0)
        {
            document.stringMap[CardCalculationDamageKey] = "ondraw_rpl_damage";
            yield return EffectManager.Instance.invokeEffectWorkflowForCalculation(
                document,
                card.cardType.onDrawEffectWorkflow.effectSteps,
                null
            );
        }
        yield return CardFinishedCalculatingCallback(document);
    }


    private IEnumerator CardFinishedCalculatingCallback(EffectDocument document)
    {
        //document should now have a value for the output and the multiplicity of that output;
        docCard.UpdateCardText(document);
        yield return null;
    }

    private void IncrementCastCount()
    {
        card.castCount += 1;
    }

    public IEnumerator ExhaustCard()
    {
        if (deckFrom != null)
        {
            deckFrom.ExhaustCard(card, this);
        }
        else
        {
            Debug.LogWarning($"Trying to exhaust card {card.cardType.GetName()}, but the deck no longer exists");
        }
        yield return new WaitUntil(() => cardExhaustVFXFinished == true);
        cleanupAndDestroy();
        yield return null;
    }

    // Called by playerHand.discardCard
    public IEnumerator DiscardToDeck()
    {
        if (PlayerHand.Instance.GetCardsOrdered().Contains(this) ||
            (PlayerHand.Instance.cardsInSelectionSpline != null && PlayerHand.Instance.cardsInSelectionSpline.Contains(this)))
        {
            yield return StartCoroutine(PlayerHand.Instance.SafeRemoveCardFromHand(this));
        }
        yield return CardDiscardVFX(this.gameObject);
        if (deckFrom != null)
        {
            deckFrom.DiscardCards(new List<Card> { card });
        }
        else
        {
            Debug.LogWarning($"Trying to discard card {card.cardType.GetName()}, but the deck no longer exists");
        }
    }

    public void cleanupAndDestroy()
    {
        docCard.Cleanup(() =>
        {
            Destroy(hoverObjectToMove);
            Destroy(this.gameObject);
        });
    }

    // Keeping these here for reference as they will almost certainly
    // be needed for UI effects in the future
    public void OnDrag(PointerEventData eventData) { }

    public void OnPointerEnterVoid()
    {
        OnPointerEnter(null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!hoverable || !PlayerHand.Instance.GetCanPlayCards()) return;
        hovered = true;
        HoverAssociatedCompanion();
        // Set the volume first
        MusicController.Instance.PlaySFX("event:/SFX/SFX_UIHover");
        transform.SetAsLastSibling();
        PlayerHand.Instance.HoverCard(this);
    }

    public void OnPointerExitVoid()
    {
        OnPointerExit(null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentlyCastingCard || !hovered) return;
        UnHoverAssociatedCompanion();
        hovered = false;
        PlayerHand.Instance.UnhoverCard(this);
    }


    // TODO: Move over last use of this card
    public void ResetCardScale(bool focus = false)
    {
        interactable = true;
        currentlyCastingCard = false;
        if (focus == false && hovered)
        {
            hovered = false;
            PlayerHand.Instance.UnhoverCard(this);
        }
        else if (focus == true)
        {
            if (gameObject.TryGetComponent<GameObjectFocusable>(out GameObjectFocusable goFocusable))
            {
                FocusManager.Instance.SetFocus(goFocusable);
            }
            OnPointerEnterVoid();
        }
    }

    // Used when instantiating the card after Start has run
    // See PrefabInstantiator.cs
    public void SetCardInfo(Card card)
    {
        this.card = card;
    }

    // Should pass by reference so that the values stay updated
    public void SetDeckFrom(DeckInstance deckFrom)
    {
        this.deckFrom = deckFrom;
    }

    public void uiStateEventHandler(UIStateEventInfo eventInfo)
    {
        currentState = eventInfo.newState;
    }

    public void SetBasePosition(Vector3 position)
    {
        startPos = position;
    }

    private void HoverAssociatedCompanion()
    {
        if (deckFrom == null)
        {
            Debug.LogError("[PlayableCard] deckfrom is null, can't highlight associated companion");
            return;
        }
        CompanionInstance companionInstanceFrom = this.deckFrom.GetComponent<CompanionInstance>();
        if (companionInstanceFrom == null)
        {
            Debug.LogError("[PlayableCard] deckfrom doesn't have a companion instance, can't highlight selected companion");
            return;
        }
        companionInstanceFrom.companionView.SetSelectionIndicatorVisibility(true);
    }

    private void UnHoverAssociatedCompanion()
    {
        if (deckFrom == null)
        {
            Debug.LogError("[PlayableCard] deckfrom is null, can't highlight associated companion");
            return;
        }
        CompanionInstance companionInstanceFrom = this.deckFrom.GetComponent<CompanionInstance>();
        if (companionInstanceFrom == null)
        {
            Debug.LogError("[PlayableCard] deckfrom doesn't have a companion instance, can't highlight selected companion");
            return;
        }
        companionInstanceFrom.companionView.SetSelectionIndicatorVisibility(false);
    }

}