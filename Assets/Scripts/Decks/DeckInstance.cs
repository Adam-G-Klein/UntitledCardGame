using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// gross, but now cards won't be able to display break if we don't have this component. Still have error checks everywhere for it not being present
// arch was gonna rot over time, might as well use it to speed up the demo right
[RequireComponent(typeof(CompanionInstance))]
public class DeckInstance : MonoBehaviour
{
    public Deck sourceDeck;
    public List<Card> drawPile;
    public List<Card> discardPile;
    public List<Card> inHand;
    public List<Card> exhaustPile;
    public CombatInstance combatInstance;

    private TurnPhaseTrigger drawCardsTurnPhaseTrigger;
    private TurnPhaseTrigger resetTempCardModificationsTrigger;

    private float nextMinionSpawnTheta = Mathf.PI/2f;
    private float minionSpawnRadius = 3f;

    public void Start() {
        SetupPiles(sourceDeck);
        RegisterDrawTrigger();
        RegisterEndTurnTrigger();
        RegisterEndOfEncounterHandler();
        combatInstance.onDeathHandler += OnDeath;
    }

    private void RegisterEndOfEncounterHandler() {
        EnemyEncounterManager.Instance.onEncounterEndHandler += OnEndEncounter;
    }

    private void RegisterDrawTrigger() {
        drawCardsTurnPhaseTrigger = new TurnPhaseTrigger(
            TurnPhase.START_PLAYER_TURN, DealStartPlayerTurnCards());
        TurnManager.Instance.addTurnPhaseTrigger(drawCardsTurnPhaseTrigger);
    }

    private void RegisterEndTurnTrigger() {
        resetTempCardModificationsTrigger = new TurnPhaseTrigger(
            TurnPhase.END_PLAYER_TURN, ResetTempCardModifications());
        TurnManager.Instance.addTurnPhaseTrigger(resetTempCardModificationsTrigger);
    }


    private void UnregisterDrawTrigger() {
        TurnManager.Instance.removeTurnPhaseTrigger(drawCardsTurnPhaseTrigger);
    }

    private IEnumerator OnDeath(CombatInstance killer) {
        UnregisterDrawTrigger();
        yield return null;
    }

    public IEnumerable DealStartPlayerTurnCards() {
        DealCardsToPlayerHand(sourceDeck.cardsDealtPerTurn);
        yield return null;
    }

    private void SetupPiles(Deck sourceDeck) {
        this.sourceDeck = sourceDeck;
        this.drawPile = new List<Card>();
        foreach(Card card in sourceDeck.cards) {
            Card newCard = new Card(card);
            SetCompanionFromOnCard(newCard);
            this.drawPile.Add(newCard);
        }
        this.discardPile = new List<Card>();
        this.inHand = new List<Card>();
        ShuffleDeck();
    }

    private void SetCompanionFromOnCard(Card card) {
        CompanionInstance companionInstance = GetComponent<CompanionInstance>();
        if(companionInstance == null) {
            Debug.LogError("DeckInstance " + this + " does not have a companion instance, cannot set companion from");
            return;
        } else {
            card.setCompanionFrom(companionInstance.companion.companionType);
        }
    }

    public List<PlayableCard> DealCardsToPlayerHand(int numCards) {
        List<Card> cards = DealCardsFromDeck(numCards);
        return PlayerHand.Instance.DealCards(cards, this);
    }

    public void AddCardFromDeckToHand(Card card) {
        if (drawPile.Contains(card)) {
            drawPile.Remove(card);
            PlayerHand.Instance.DealCards(new List<Card>() {card}, this);
        }
    }

    public List<Card> DealCardsFromDeck(int numCards, bool withReplacement = false){
        List<Card> returnList = new List<Card>();
        for(int i = 0; i < numCards; i++){
            DealCardFromDeckToList(returnList, withReplacement);
        }
        inHand.AddRange(returnList);
        return returnList;
    }

    private void DealCardFromDeckToList(List<Card> toList, bool withReplacement = false) {
        // Check to see if the draw pile is empty
        if (drawPile.Count == 0) {
            // If draw pile is empty, the options are shuffle in the discard pile then draw,
            // or we just get no draw since both are empty :(
            if (discardPile.Count == 0) {
                // big loser moment
                return;
            }
            ShuffleDiscardIntoDraw();
        }
        Card card = drawPile[0];
        if (!withReplacement)
            drawPile.Remove(card);
        if (!toList.Contains(card)) {
            toList.Add(card);
        }
    }

    private void ShuffleDeck() {
         System.Random _random = new System.Random();
         Card temp;

         int n = drawPile.Count;
         for (int i = 0; i < n; i++)
         {
             // NextDouble returns a random number between 0 and 1
             int r = i + (int)(_random.NextDouble() * (n - i));
             temp = drawPile[r];
             drawPile[r] = drawPile[i];
             drawPile[i] = temp;
         }
     }

    public void ShuffleDiscardIntoDraw(){
        Debug.Log("Shuffling discard pile into draw pile, triggering downstream effects");
        EnemyEncounterManager.Instance.DeckShuffled(this);
        PlayerHand.Instance.StartCoroutine(PlayerHand.Instance.OnDeckShuffled(this));
        drawPile.AddRange(discardPile);
        drawPile.Shuffle();
        discardPile.Clear();
    }

    public void DiscardCards(List<Card> cards){
        inHand.RemoveAll(c => cards.Contains(c));
        discardPile.AddRange(cards);
    }

    public void ShuffleIntoDraw(List<Card> cards){
        Debug.Log("Shuffling " + cards.Count + " cards into draw pile");
        drawPile.AddRange(cards);

        drawPile.Shuffle();
    }

    public void AddCardsToTopOfDeck(List<Card> cards){
        Debug.Log("Adding " + cards.Count + " cards to top of draw pile");
        List<Card> newDrawPile = new List<Card>();
        newDrawPile.AddRange(cards);
        newDrawPile.AddRange(drawPile);
        drawPile = newDrawPile;
    }

    public void AddCardsToBottomOfDeck(List<Card> cards){
        Debug.Log("Adding " + cards.Count + " cards to the bottom of the draw pile");
        List<Card> newDrawPile = new List<Card>();
        newDrawPile.AddRange(drawPile);
        newDrawPile.AddRange(cards);
        drawPile = newDrawPile;
    }

    public bool ContainsCardById(string id){
        return drawPile.Exists(c => c.id == id) || discardPile.Exists(c => c.id == id);
    }

    public Card GetCardById(string id){
        Card card = drawPile.Find(c => c.id == id);
        if(card == null){
            card = discardPile.Find(c => c.id == id);
        }
        return card;
    }

    public void ExhaustCard(Card card){
        if(drawPile.Contains(card)){
            Debug.Log("Exhausting card " + card.id + " with name " + card.name + " from draw pile");
            drawPile.Remove(card);
        }
        else if(discardPile.Contains(card)){
            Debug.Log("Exhausting card " + card.id + " with name " + card.name + " from discard pile");
            discardPile.Remove(card);
        } else if (inHand.Contains(card)) {
            Debug.Log("Exhausting card " + card.id + " with name " + card.name + " from hand");
            inHand.Remove(card);
        }
        exhaustPile.Add(card);
        if (card.cardType.onExhaustEffectWorkflow != null) {
            EffectDocument document = new EffectDocument();
            document.originEntityType = EntityType.Unknown;
            EffectManager.Instance.QueueEffectWorkflow(
                new EffectWorkflowClosure(document, card.cardType.onExhaustEffectWorkflow, null)
            );
        }
        StartCoroutine(PlayerHand.Instance.OnCardExhaust(this, card));
    }

    public void DiscardCard(Card card){
        if(drawPile.Contains(card)){
            Debug.Log("Discarding card " + card.id + " from draw pile");
            drawPile.Remove(card);
            discardPile.Add(card);
        } else if (inHand.Contains(card)) {
            Debug.Log("Discarding card " + card.id + " from hand");
            inHand.Remove(card);
            discardPile.Add(card);
        }
    }

    public void PurgeCard(Card card){
        Debug.Log("Purging card " + card.id + " from deck");
        if(drawPile.Contains(card)){
            drawPile.Remove(card);
        }
        else if(discardPile.Contains(card)){
            discardPile.Remove(card);
        } else if (inHand.Contains(card)) {
            inHand.Remove(card);
        }
        sourceDeck.PurgeCard(card.id);
    }

    public void TransformAllCardsOfType(
        CardType target,
        CardType destType,
        bool includeCardsInHand,
        bool includeDrawPile,
        bool includeDiscardPile

    ) {
        if (includeCardsInHand) {
            List<Card> newCardsToDeal = new();
            for (int i = 0; i < inHand.Count; i++) {
                if (inHand[i].cardType == target) {
                    PlayableCard card = PlayerHand.Instance.GetCardById(inHand[i].id);
                    Destroy(card.gameObject);
                    PlayerHand.Instance.SafeRemoveCardFromHand(inHand[i]);
                    inHand[i] = new Card(destType, inHand[i].getCompanionFrom());
                    newCardsToDeal.Add(inHand[i]);
                }
            }
            PlayerHand.Instance.DealCards(newCardsToDeal, this);
        }
        if (includeDrawPile) {
            for (int i = 0; i < drawPile.Count; i++) {
                if (drawPile[i].cardType == target) {
                    drawPile[i] = new Card(destType, drawPile[i].getCompanionFrom());
                }
            }
        }
        if (includeDiscardPile) {
            for (int i = 0; i < discardPile.Count; i++) {
                if (discardPile[i].cardType == target) {
                    discardPile[i] = new Card(destType, discardPile[i].getCompanionFrom());
                }
            }
        }
    }

    public void AddToDiscard(Card card){
        discardPile.Add(card);
        if(inHand.Contains(card)) {
            inHand.Remove(card);
        }
    }

    public List<Card> GetShuffledDrawPile(){
        List<Card> shuffledDrawPile = new List<Card>();
        shuffledDrawPile.AddRange(drawPile);
        shuffledDrawPile.Shuffle();
        return shuffledDrawPile;
    }

    public List<Card> GetShuffledDiscardPile(){
        List<Card> shuffledDiscardPile = new List<Card>();
        shuffledDiscardPile.AddRange(discardPile);
        shuffledDiscardPile.Shuffle();
        return shuffledDiscardPile;
    }

    public List<Card> GetAllCards(){
        List<Card> cards = new List<Card>();
        cards.AddRange(drawPile);
        cards.AddRange(discardPile);
        cards.AddRange(inHand);
        return cards;
    }

    public bool Contains(Card card){
        return drawPile.Contains(card) || discardPile.Contains(card);
    }

    public Vector2 getNextMinionSpawnPosition() {
        Vector2 center = transform.position;
        // from copilot and https://answers.unity.com/questions/1545128/how-can-i-get-a-point-position-in-circle-line.html
        Vector2 spawnLoc = new Vector2(
            center.x + minionSpawnRadius * Mathf.Cos(nextMinionSpawnTheta),
            center.y + minionSpawnRadius * Mathf.Sin(nextMinionSpawnTheta)
        );
        nextMinionSpawnTheta += 2 * Mathf.PI / GameplayConstantsSingleton.Instance.gameplayConstants.MAX_MINIONS_PER_COMPANION;
        return spawnLoc;
    }

    public CompanionTypeSO GetCompanionTypeSO() {
        CompanionInstance companionInstance = GetComponent<CompanionInstance>();
        if(companionInstance == null) {
            Debug.LogError("DeckInstance " + this + " does not have a companion instance, cannot get companion type");
            return null;
        } else {
            return companionInstance.companion.companionType;
        }
    }

    private void OnEndEncounter() {
        // Ensure we reset the modifications for all cards that were present on this companion,
        // including temporary generated cards.
        List<Card> allCards = new();
        allCards.AddRange(inHand);
        allCards.AddRange(drawPile);
        allCards.AddRange(discardPile);
        allCards.AddRange(exhaustPile);
        foreach (Card card in allCards) {
            card.ResetCardModifications();
            card.cardType.ResetCardModifications();
        }
    }

    private IEnumerable ResetTempCardModifications() {
        foreach (Card card in drawPile) {
            card.ResetTempCardModifications();
        }

        foreach (Card card in discardPile) {
            card.ResetTempCardModifications();
        }

        foreach (Card card in inHand) {
            card.ResetTempCardModifications();
        }

        foreach (Card card in exhaustPile) {
            card.ResetTempCardModifications();
        }
        yield break;
    }
}
