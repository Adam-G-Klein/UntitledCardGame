using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.Linq;

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
    private Hoverable hoverable;

    // Checked by PlayerHand when discarding the whole hand
    // set back to false there when it's checked
    public bool retained = false;
    public AudioClip cardHover;
    public float hoverSFXVolume = 0.1f;
    public float hoverYOffset = .75f;
    public float hoverZOffset = 0.5f;
    public GameObject hoverObjectToMove;
    private float hoverAnimationTime = .2f;
    private Vector3 startPos;

    public GameObject cardCastVFXPrefab;
    public GameObject cardExhaustVFXPrefab;

    private UIDocumentCard docCard;
    private bool isCardCastPlaying = false;
    private bool isCardDiscardPlaying = false;
    public bool interactable = false;

    public void Awake()
    {
        docCard = GetComponent<UIDocumentCard>();
    }
    public void Start()
    {
        transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
        hoverable = GetComponent<Hoverable>();
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
            StartCoroutine(GenericEntityDialogueParticipant
                .Instance
                .SpeakCompanionLine(
                    "You don't have enough mana for me to cast that for you :(",
                    deckFrom.GetComponent<CompanionInstance>().companion.companionType, 3f));
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
        interactable = false;

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


        int cardPlayedIndex = PlayerHand.Instance.cardsInHand.IndexOf(this);
        ManaManager.Instance.updateMana(-card.GetManaCost());
        StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(card)));
        IncrementCastCount();
        EnemyEncounterManager.Instance.combatEncounterState.CastCard(card);
        yield return StartCoroutine(PlayerHand.Instance.OnCardCast(this));
        PlayerHand.Instance.StopCardDrawFX(gameObject);

        if (card.cardType.exhaustsWhenPlayed)
        {
            Debug.Log("STARTING exhaust when played coroutine");
            yield return PlayerHand.Instance.ExhaustCard(this);
            Debug.Log("DONE WITH exhaust when played coroutine");

            EnemyEncounterManager.Instance.SetCastingCard(false);
            PlayerHand.Instance.HoverNextCard(cardPlayedIndex);

            // Add a WaitForSeconds so that the target hovering does not break when using keyboard.
            // yield return new WaitForSeconds(0.5f);
            // Don't need to call ResizeHand, because ExhaustCard already does it!
        }
        else
        {
            // remove the card from the hand first so that resizing doesn't affect the card being cast
            yield return StartCoroutine(PlayerHand.Instance.SafeRemoveCardFromHand(this));

            EnemyEncounterManager.Instance.SetCastingCard(false);
            PlayerHand.Instance.HoverNextCard(cardPlayedIndex);

            yield return StartCoroutine(PlayerHand.Instance.ResizeHand(this));
            yield return StartCoroutine(PlayerHand.Instance.DiscardCard(this, true));
        }

        PlayerHand.Instance.UpdatePlayableCards();
        // If the hand is empty as a result of playing this card, invoke any subscribers.
        if (PlayerHand.Instance.cardsInHand.Count == 0)
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
        GameObject.Instantiate(
            cardExhaustVFXPrefab,
            this.transform.position,
            Quaternion.identity);
    }

    private IEnumerator CardCastVFX(GameObject cardGameObject)
    {
        this.isCardCastPlaying = true;
        FXExperience experience = PrefabInstantiator.instantiateFXExperience(cardCastVFXPrefab, cardGameObject.transform.position);

        experience.BindGameObjectsToTracks(new Dictionary<string, GameObject>() {
            { "CardAnimationTrack", cardGameObject },
            { "CardTweenTrack", cardGameObject },
        });
        experience.AddLocationToKey("Card", this.transform.position);
        experience.AddLocationToKey("Companion", this.deckFrom.transform.position);
        // This makes it so that we can use 0,0 as the "current position of the card"
        cardGameObject.transform.SetParent(experience.transform);
        experience.onExperienceOver += CardCastVFXFinished;
        Debug.Log("Started card cast VFX");
        experience.StartExperience();
        yield return new WaitUntil(() => this.isCardCastPlaying == false);
        Debug.Log("Finished card cast VFX");
    }

    private void CardCastVFXFinished()
    {
        // If we don't do this, then the crew (the card) goes down with the ship (the FXExperience)
        this.gameObject.transform.SetParent(null);
        this.isCardCastPlaying = false;
    }

    private IEnumerator CardDiscardVFX(GameObject cardGameObject)
    {
        FXExperience experience = PrefabInstantiator.instantiateFXExperience(cardCastVFXPrefab, cardGameObject.transform.position);

        experience.BindGameObjectsToTracks(new Dictionary<string, GameObject>() {
            { "CardAnimationTrack", cardGameObject },
            { "CardTweenTrack", cardGameObject },
        });
        experience.AddLocationToKey("Card", this.transform.position);
        experience.AddLocationToKey("Companion", this.deckFrom.transform.position);
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
        if (card.effectSteps != null && card.effectSteps.Count != 0)
        {
            invokeCalculationOnCardEffectWorkflow(card.effectSteps);
        }
        if (card.cardType.inPlayerHandEndOfTurnWorkflow != null && card.cardType.inPlayerHandEndOfTurnWorkflow.effectSteps.Count != 0)
        {
            invokeCalculationOnCardEffectWorkflow(card.cardType.inPlayerHandEndOfTurnWorkflow.effectSteps);
        }
        if (card.cardType.onExhaustEffectWorkflow != null && card.cardType.onExhaustEffectWorkflow.effectSteps.Count != 0)
        {
            invokeCalculationOnCardEffectWorkflow(card.cardType.onExhaustEffectWorkflow.effectSteps);
        }
        if (card.cardType.onDiscardEffectWorkflow != null && card.cardType.onDiscardEffectWorkflow.effectSteps.Count != 0)
        {
            invokeCalculationOnCardEffectWorkflow(card.cardType.onDiscardEffectWorkflow.effectSteps);
        }
    }

    private void invokeCalculationOnCardEffectWorkflow(List<EffectStep> steps)
    {
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Card;
        EffectManager.Instance.invokeEffectWorkflowForCalculation(
            document,
            steps,
            CardFinishedCalculatingCallback(document));
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
        deckFrom.ExhaustCard(card, this);
        cleanupAndDestroy();
        yield return null;
    }

    // Called by playerHand.discardCard
    public IEnumerator DiscardToDeck()
    {
        if (PlayerHand.Instance.cardsInHand.Contains(this))
        {
            yield return StartCoroutine(PlayerHand.Instance.SafeRemoveCardFromHand(this));
        }
        yield return CardDiscardVFX(this.gameObject);
        deckFrom.DiscardCards(new List<Card> { card });
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
        if (!interactable || EnemyEncounterManager.Instance.GetCastingCard() || !PlayerHand.Instance.GetCanPlayCards()) return;
        hovered = true;
        HoverAssociatedCompanion();
        if (hoverable != null)
        {
            // Make sure hoverable knows that we've already processed
            // this hover in case keyboard controls turn on
            hoverable.hovered = true;
        }
        // Set the volume first
        MusicController.Instance.PlaySFX("event:/SFX/SFX_UIHover");
        transform.SetAsLastSibling();

        //PlayerHand.Instance.HoverNextCard(-1); // this prevents card moving in hand from forcefully chaning hover target if playable has manually selected a new card

        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, new Vector3(2f, 2f, 1), hoverAnimationTime)
            .setEase(LeanTweenType.easeOutQuint);
        LeanTween.moveLocal(gameObject, new Vector3(0, hoverYOffset, hoverZOffset), hoverAnimationTime)
            .setEase(LeanTweenType.easeOutQuint);
    }

    public void OnPointerExitVoid()
    {
        OnPointerExit(null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!interactable || !hovered) return;
        UnHoverAssociatedCompanion();
        ResetCardScale();
    }

    public void ResetCardScale()
    {
        if (hovered)
        {
            hovered = false;

            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, new Vector3(1.5f, 1.5f, 1), hoverAnimationTime)
                .setEase(LeanTweenType.easeOutQuint);
            LeanTween.moveLocal(gameObject, new Vector3(0, 0, 0), hoverAnimationTime)
                .setEase(LeanTweenType.easeOutQuint);
        }
        interactable = true;
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