using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 

[RequireComponent(typeof(CardDisplay))]
public class PlayableCard : MonoBehaviour
    , IPointerClickHandler 
    , IDragHandler
    , IPointerEnterHandler
    , IPointerExitHandler
{
    public CardInfo cardInfo;
    private EnemyManager enemyManager;
    private CompanionManager companionManager;
    private CombatEntityInEncounterStats companionFromStats;
    private InCombatDeck deckFrom; 

    [SerializeField]
    private float hoverScale = 30f;
    [SerializeField]
    private float nonHoverScale = 20f;
    [SerializeField]
    public float hoverYDiff = 185f;
    private int preHoverSiblingIndex;
    // Start is called before the first frame update
    public bool hovered = false;

    private CardCaster caster;

    void Start()
    {
        cardInfo = GetComponent<CardDisplay>().cardInfo;
        GameObject enemyManagerGO = GameObject.Find("EnemyManager");
        GameObject companionManagerGO = GameObject.Find("CompanionManager");
        GameObject cardCasterGO = GameObject.Find("CardCaster");
        // My attempt at null safing. We should def talk about how we want to 
        // do this generally because it'll happen a lot with the modular scenes
        if(enemyManagerGO) enemyManager = enemyManagerGO.GetComponent<EnemyManager>();
        else Debug.LogError("EnemyManager not found");
        if(companionManagerGO) companionManager = companionManagerGO.GetComponent<CompanionManager>();
        else Debug.LogError("CompanionManager not found");
        if(cardCasterGO) caster = cardCasterGO.GetComponent<CardCaster>();
        else Debug.LogError("CardCaster not found");
    }

    public void OnPointerClick(PointerEventData eventData) 
    {
        // I think there's a good possibility that we'll want to pass the 
        // whole companionStats here at some point, but for now we'll just
        // pass each field individually
        CardCastArguments args = new CardCastArguments(companionFromStats);
        // Cast event handler in PlayerHand.cs will handle the card 
        // being removed from the hand

        caster.cardClickHandler(cardInfo, args, transform);
    }

    public void discard() {
        deckFrom.discardPile.Add(cardInfo);
    }


    // Keeping these here for reference as they will almost certainly
    // be needed for UI effects in the future
    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        preHoverSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
        transform.localScale = new Vector3(hoverScale, hoverScale, 1);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + hoverYDiff, transform.localPosition.z);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if(!hovered) return;
        hovered = false;
        transform.SetSiblingIndex(preHoverSiblingIndex);
        transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - hoverYDiff, transform.localPosition.z);
    }

    

    // Used when instantiating the card after Start has run
    // See PrefabInstantiator.cs
    public void setCardInfo(CardInfo card){
        this.cardInfo = card;
    }
    
    // Should pass by reference so that the values stay updated
    public void setCompanionFrom(CombatEntityInEncounterStats companionStats){
        this.companionFromStats = companionStats;
    }

    public void setDeckFrom(InCombatDeck deck){
        this.deckFrom = deck;
    }

}
