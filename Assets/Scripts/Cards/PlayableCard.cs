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
    private float hoverScale = 1.5f;
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
    public float hoverSFXVolume = 0.1f;
    public float hoverYOffset = 1.5f;
    public float hoverZOffset = 0.5f;

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
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Card clicked");
        Debug.Log(currentState);
        if (currentState != UIState.DEFAULT || !interactable) return;

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
        if (eventData.button != PointerEventData.InputButton.Left) return;
        interactable = false;

        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Card;
        EffectManager.Instance.invokeEffectWorkflow(document, card.effectSteps, CardFinishCastingCallback());
    }

    private IEnumerator CardFinishCastingCallback() {
        ManaManager.Instance.updateMana(-card.GetManaCost());
        StartCoroutine(cardCastEvent.RaiseAtEndOfFrameCoroutine(new CardCastEventInfo(card)));
        IncrementCastCount();
        EnemyEncounterManager.Instance.combatEncounterState.CastCard(card);
        yield return StartCoroutine(PlayerHand.Instance.OnCardCast(this));
        PlayerHand.Instance.DiscardCard(this);
        PlayerHand.Instance.UpdatePlayableCards();
        if (card.cardType.exhaustsWhenPlayed) {
            CardExhaustVFX();
            ExhaustCard();
        } else {
            yield return StartCoroutine(CardCastVFX(this.gameObject));
            DiscardCardFromHand();
        }
        // If the hand is empty as a result of playing this card, invoke any subscribers.
        if (PlayerHand.Instance.cardsInHand.Count == 0) {
            Debug.Log("Hand is empty, triggering downstream OnHandEmpty subscribers");
            PlayerHand.Instance.OnHandEmpty();
        }
    }

    private void CardExhaustVFX() {
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

    public void DiscardCardFromHand() {
        if (this.gameObject.activeSelf) {
            DiscardToDeck();
        }
    }

    public void ExhaustCard() {
        deckFrom.ExhaustCard(card);
        cleanupAndDestroy();
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
        if(!interactable) return;
        hovered = true;
        //MusicController.Instance.PlaySFX(cardHover, hoverSFXVolume);
        //Replace with FMOD Event
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/SFX_UIHover");
        transform.localScale = new Vector3(hoverScale, hoverScale, 1);
        transform.position = new Vector3(transform.position.x, transform.position.y + hoverYOffset, transform.position.z + hoverZOffset);
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!interactable || !hovered) return;
        hovered = false;
        transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
        transform.position = new Vector3(transform.position.x, transform.position.y - hoverYOffset, transform.position.z - hoverZOffset);
    }

    public void ResetCardScale() {
        if (hovered) {
            hovered = false;
            interactable = true;
            transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
            transform.position = new Vector3(transform.position.x, transform.position.y - hoverYOffset, transform.position.z - hoverZOffset);
        }
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
}
