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
    private float hoverAnimationTime = .2f;
    private Vector3 startPos;

    public GameObject cardCastVFXPrefab;
    public GameObject cardExhaustVFXPrefab;

    private UIDocumentCard docCard;
    private bool isCardCastPlaying = false;
    public bool interactable = false;

    public void  Awake() {
        docCard = GetComponent<UIDocumentCard>();
    }
    public void Start()
    {
        transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
        hoverable = GetComponent<Hoverable>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Card clicked");
        Debug.Log(
            "CURRENT STATE: " + currentState +
            "Interactable: " + interactable +
            " EnemyEncounterManager.GetCastingCard(): " + EnemyEncounterManager.Instance.GetCastingCard().ToString()
        );
        if (currentState != UIState.DEFAULT || !interactable || EnemyEncounterManager.Instance.GetCastingCard() || EnemyEncounterManager.Instance.GetCombatOver()) return;
        if (card.GetManaCost() > ManaManager.Instance.currentMana) {
                StartCoroutine(GenericEntityDialogueParticipant
                    .Instance
                    .SpeakCompanionLine(
                        "You don't have enough mana for me to cast that for you :(",
                        deckFrom.GetComponent<CompanionInstance>().companion.companionType, 3f));
            return;
        }

        if (!card.cardType.playable) {
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
    }

    private IEnumerator CardFinishCastingCallback() {
        Debug.Log("STARTING CardFinishCastingCallback");
        ManaManager.Instance.updateMana(-card.GetManaCost());
        StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(card)));
        IncrementCastCount();
        EnemyEncounterManager.Instance.combatEncounterState.CastCard(card);
        yield return StartCoroutine(PlayerHand.Instance.OnCardCast(this));
        if (card.cardType.exhaustsWhenPlayed) {
            yield return StartCoroutine(PlayerHand.Instance.ExhaustCard(this));
            // Don't need to call ResizeHand, because ExhaustCard already does it!
        } else {
            yield return StartCoroutine(PlayerHand.Instance.ResizeHand(this));
            yield return StartCoroutine(CardCastVFX(this.gameObject));
            yield return StartCoroutine(PlayerHand.Instance.DiscardCard(this));
        }

        PlayerHand.Instance.UpdatePlayableCards();
        // If the hand is empty as a result of playing this card, invoke any subscribers.
        if (PlayerHand.Instance.cardsInHand.Count == 0) {
            Debug.Log("Hand is empty, triggering downstream OnHandEmpty subscribers");
            yield return PlayerHand.Instance.OnHandEmpty();
        } else if(NonMouseInputManager.Instance.inputMethod != InputMethod.Mouse) {
            NonMouseInputManager.Instance.hoverACard(new List<PlayableCard> { this });
        }
        Debug.Log("FINISHED CardFinishCastingCallback");
    }

    public void CardExhaustVFX() {
        GameObject.Instantiate(
            cardExhaustVFXPrefab,
            this.transform.position,
            Quaternion.identity);
    }

    private IEnumerator CardCastVFX(GameObject cardGameObject) {
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

    private void CardCastVFXFinished() {
        // If we don't do this, then the crew (the card) goes down with the ship (the FXExperience)
        this.gameObject.transform.SetParent(null);
        this.isCardCastPlaying = false;
    }

    public void UpdateCardText() {
        //if (card.cardType.values.Count == 0) return;
        //String newVal = "";
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Card;
        Debug.Log("trying to update card text");
        if (card.effectSteps != null && card.effectSteps.Count != 0) {
            EffectManager.Instance.invokeEffectWorkflowForCalculation(
                document,
                card.effectSteps,
                CardFinishedCalculatingCallback(document));
        }
        if (card.cardType.inPlayerHandEndOfTurnWorkflow != null && card.cardType.inPlayerHandEndOfTurnWorkflow.effectSteps.Count != 0) {
            EffectManager.Instance.invokeEffectWorkflowForCalculation(
                document,
                card.cardType.inPlayerHandEndOfTurnWorkflow.effectSteps,
                CardFinishedCalculatingCallback(document));
        }
    }

    private IEnumerator CardFinishedCalculatingCallback(EffectDocument document) {
        //document should now have a value for the output and the multiplicity of that output;
        docCard.UpdateCardText(document);
        yield return null;
    }

    private void IncrementCastCount(){
        card.castCount += 1;
    }

    public void ExhaustCard() {
        deckFrom.ExhaustCard(card, this, OnCardExhaustHandler());
        StartCoroutine(PlayerHand.Instance.ResizeHand(this));
    }

    private IEnumerator OnCardExhaustHandler() {
        cleanupAndDestroy();
        yield return null;
    }

    // Called by playerHand.discardCard
    public void DiscardToDeck() {
        deckFrom.DiscardCards(new List<Card> { card });
        cleanupAndDestroy();
    }

    public void cleanupAndDestroy() {
        docCard.Cleanup(() => {
            Destroy(this.gameObject);
        });
    }

    // Keeping these here for reference as they will almost certainly
    // be needed for UI effects in the future
    public void OnDrag(PointerEventData eventData) { }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!interactable || EnemyEncounterManager.Instance.GetCastingCard() || EnemyEncounterManager.Instance.GetCombatOver()) return;
        hovered = true;
        if(hoverable != null) {
            // Make sure hoverable knows that we've already processed
            // this hover in case keyboard controls turn on
            hoverable.hovered = true;
        }
        //MusicController.Instance.PlaySFX(cardHover, hoverSFXVolume);
        //Replace with FMOD Event
        // Set the volume first
        MusicController2.Instance.PlaySFX("event:/SFX/SFX_UIHover");
        //transform.localScale = new Vector3(hoverScale, hoverScale, 1);
        //transform.position = new Vector3(startPos.x, startPos.y + hoverYOffset, startPos.z + hoverZOffset);
        transform.SetAsLastSibling();

        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, new Vector3(2f, 2f, 1), hoverAnimationTime)
            .setEase(LeanTweenType.easeOutQuint);
        LeanTween.move(gameObject, new Vector3(startPos.x, startPos.y + hoverYOffset, startPos.z + hoverZOffset), hoverAnimationTime)
            .setEase(LeanTweenType.easeOutQuint);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!interactable || !hovered) return;
        hovered = false;
        //transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
        //transform.position = new Vector3(transform.position.x, transform.position.y - hoverYOffset, transform.position.z - hoverZOffset);

        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, new Vector3(1.5f, 1.5f, 1), hoverAnimationTime)
            .setEase(LeanTweenType.easeOutQuint);
        LeanTween.move(gameObject, new Vector3(startPos.x, startPos.y, startPos.z), hoverAnimationTime)
            .setEase(LeanTweenType.easeOutQuint);
    }

    public void ResetCardScale() {
        if (hovered) {
            hovered = false;
            transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
            transform.position = new Vector3(transform.position.x, transform.position.y - hoverYOffset, transform.position.z - hoverZOffset);
        }
        interactable = true;
    }

    // Used when instantiating the card after Start has run
    // See PrefabInstantiator.cs
    public void SetCardInfo(Card card){
        this.card = card;
    }

    // Should pass by reference so that the values stay updated
    public void SetDeckFrom(DeckInstance deckFrom) {
        this.deckFrom = deckFrom;
    }

    public void uiStateEventHandler(UIStateEventInfo eventInfo) {
        currentState = eventInfo.newState;
    }

    public void SetBasePosition() {
        startPos = transform.position;
    }
}
