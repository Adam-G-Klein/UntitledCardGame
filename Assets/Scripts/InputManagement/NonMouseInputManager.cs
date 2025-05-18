using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public enum GFGInputAction {
    UP,
    DOWN,
    LEFT,
    RIGHT,
    SELECT,
    BACK,
    END_TURN,
    OPEN_COMPANION_1_DRAW, // Deprecated
    OPEN_COMPANION_2_DRAW, // Deprecated
    OPEN_COMPANION_3_DRAW, // Deprecated
    OPEN_COMPANION_4_DRAW, // Deprecated
    OPEN_COMPANION_5_DRAW, // Deprecated
    NONE, // used for the controller mapping of the stick being in the center of its range
    CUTSCENE_SKIP,
    SECONDARY_UP,
    SECONDARY_DOWN,
    SECONDARY_RIGHT,
    SECONDARY_LEFT,
    SELECT_DOWN,
    SELECT_UP,
    VIEW_DECK,
    VIEW_DISCARD,
    SELL_COMPANION,
    OPTIONS
}

public enum InputMethod {
    Mouse,
    Controller,
    Keyboard
}

// Temp name, just being extra explicit what functionality this class is for
// Using it to be able to play the game with just a keyboard, as a precursor 
// to using a controller
// brainstorming doc: https://docs.google.com/document/d/1-HkEMw6Go4Lw7kFVd3A6gOE2FjSb1qToeDbyvLiJyzc/edit?tab=t.0
public class NonMouseInputManager : GenericSingleton<NonMouseInputManager>, IControlsReceiver {

    [SerializeField]
    private int hoveredCardIndex = -1;
    private List<Hoverable> allHoverables = new List<Hoverable>();
    public Hoverable currentlyHovered;
    public GameObject hoverIndicator;
    private float hoverIndicatorZDelta = -1;

    // -- Effect Targetting -- //
    // Only to be checked when UIState.EFFECT_TARGETTING
    private List<Targetable.TargetType> currentValidTargets; 
    private List<Hoverable> currentTargetFilteredHoverables;

    // holder of this all-important state for now
    public InputMethod inputMethod = InputMethod.Mouse;
    private Hoverable lastHoveredCard;
    [SerializeField]
    [Header("Uses game state to determine the active encounter type.\n" +
        "Shouldn't try to access turn manager or UIState manager if we're in the shop")]
    public GameStateVariableSO gameState;

    // -- Shop -- //
    [SerializeField]
    [Header("ONLY ACCURATE IN SHOP. UIStateManager has the source of truth for state in combat")]
    private UIState uiState;

    // -- Companion Dragging -- //
    private ICompanionManagementViewDelegate companionManagementViewDelegate;
    private CompanionManagementView companionManagementView;

    private CardInShopWithPrice cardPurchasingFor; 
    private bool inDeckPreview = false;

    void Awake()
    {
        // inititalization only needed when we're the source of truth
        if(gameState.activeEncounter.GetValue().getEncounterType() == EncounterType.Shop) {
            uiState = UIState.DEFAULT;
        }        
    }
        

    void Start()
    {
        // Doesn't work right now, the card is still animating/not placed so the hover doesn't go through.
        TurnManager.Instance.addTurnPhaseTrigger(new TurnPhaseTrigger(TurnPhase.PLAYER_TURN,startTurnTrigger()));
    }

    public void ClearHoverState() {
        if(currentlyHovered != null) {
            currentlyHovered.onUnhover();
            currentlyHovered = null;
        }
        if(hoverIndicator != null)
            hoverIndicator.SetActive(false);

    }

    public void RegisterHoverable(Hoverable hoverable) {
        allHoverables.Add(hoverable);
    }   

    public void UnregisterHoverable(Hoverable hoverable) {
        allHoverables.Remove(hoverable);
    }

    /*
    Find the closest hoverable in the direction provided
    Directions:
           0,1 
    -1, 0 ----- 1, 0
          -1, 0

    */

    private void hoverInDirection(Vector2 direction, List<Hoverable> candidates) {
        if(candidates.Count <= 0) return;
        // handle the base case, where nothing is currently hovered
        // for now, just hover the first element that registered
        // TODO: Make this a card, or the middle of the hand
        // Or have an element register as the default hover? Like in the shop, 
        // should probably be a the left-most shop card
        if(currentlyHovered == null) {
            hover(candidates[0]);
            return;
        }
        List<Hoverable> directionalCandidates = filterHoverablesByDirectionFromCurrent(direction, candidates);
        if(directionalCandidates.Count <= 0) {
            // TODO: add wrapping around for cycling back to hoverable  furthest in other dir
            Debug.Log("[NonMouseInputManager] No candidates found in direction: " + direction);
            return;
        } 
        Hoverable closest = findClosestHoverableToCurrent(directionalCandidates);
        hover(closest);
    }

    public void hover(Hoverable hover) {
        if(currentlyHovered != null) {
            currentlyHovered.onUnhover();
        }
        currentlyHovered = hover;
        currentlyHovered.onHover();
        hoverIndicator.transform.position = new Vector3(currentlyHovered.transform.position.x, currentlyHovered.transform.position.y, currentlyHovered.transform.position.z + hoverIndicatorZDelta);
        Debug.Log("[NonMouseInputManager] Hovering over: " + currentlyHovered.name + " position: " + currentlyHovered.transform.position);
        // if we have an arrow active, point it at what's hovered
        TargettingArrowsController.Instance.freezeArrow(currentlyHovered.gameObject);
        hoverIndicator.SetActive(true);
        if(currentlyHovered.GetEntityType() == EntityType.Card) {
            lastHoveredCard = currentlyHovered;
        }
    }

    private List<Hoverable> filterHoverablesByEntityType(EntityType entityType, List<Hoverable> candidates) {
        List<Hoverable> filtered = new List<Hoverable>();
        foreach(Hoverable candidate in candidates) {
            if(candidate.GetEntityType() == entityType) {
                filtered.Add(candidate);
            }
        }
        return filtered;
    }

    private List<Hoverable> filterHoverablesByDirectionFromCurrent(Vector2 direction, List<Hoverable> candidates) {
        List<Hoverable> directionalCandidates = new List<Hoverable>();
        Vector2 currentPos = currentlyHovered.getScreenPosition();
        Debug.Log("[NonMouseInputManager] Filtering hoverables by direction: " + direction + " from currentPos: " + currentPos);
        foreach(Hoverable candidate in candidates) {
            // all positions in screen space
            Vector2 candidatePos = candidate.getScreenPosition();
            Vector2 candidateDirection = candidatePos - currentPos;
            Debug.Log("\t[NonMouseInputManager] candidate: " + candidate.name + " candidatePos: " + candidatePos + " candidateDirection: " + candidateDirection);
            // Ty copilot for remembering the linear algebra that I could not :')
            if(Vector2.Dot(candidateDirection, direction) > 0) {
                directionalCandidates.Add(candidate);
                Debug.Log("\t[NonMouseInputManager] Added hoverable to candidates: " + candidate);
            } else {
                Debug.Log("\t[NonMouseInputManager] NOT adding hoverable to candidates: " + candidate);
            }
        }
        return directionalCandidates;
    }

    private Hoverable findClosestHoverableToCurrent(List<Hoverable> candidates) {
        if(candidates.Count <= 0) return null;
        Hoverable closest = candidates[0];
        float closestDistance = Vector2.Distance(closest.transform.position, currentlyHovered.transform.position);
        foreach(Hoverable hoverable in candidates) {
            float distance = Vector2.Distance(hoverable.transform.position, currentlyHovered.transform.position);
            if(distance < closestDistance) {
                closest = hoverable;
                closestDistance = distance;
            }
        }
        return closest;
    }



    public void ProcessInput(GFGInputAction action) {
        UnityEngine.Cursor.visible = false;
        inputMethod = InputMethod.Keyboard;
        if(uiState == UIState.OPTIONS_MENU) {
            processInputForOptionsMenu(action);
            return;
        }
        // NOTE we can't actually get a CUTSCENE_SKIP action unless we're in a cutscene
        // we stuff it at the input manager level because if you spam the button our gamestate tries 
        // and fails to skip multiple scenes. 
        // Location changes instantly, but the scene load can't keep up 
        if(gameState.currentLocation == Location.INTRO_CUTSCENE) {
            processInputForCutscene(action);
            return;
        }
        if(gameState.currentLocation == Location.POST_COMBAT) {
            processInputSimple(action, filterHoverablesByHoverableType(HoverableType.PostCombat, allHoverables));
            return;
        }
        if(gameState.currentLocation == Location.MAIN_MENU || gameState.currentLocation == Location.TEAM_SIGNING || gameState.currentLocation == Location.TUTORIAL || gameState.currentLocation == Location.SHOP_TUTORIAL) { 
            processInputSimple(action);
            return;
        }
        if(gameState.activeEncounter.GetValue().getEncounterType() == EncounterType.Shop) {
            processInputForShop(action);
            return;
        } else if (gameState.activeEncounter.GetValue().getEncounterType() == EncounterType.Enemy) {
            processInputForCombat(action);
            return;
        } 
        Debug.LogError("Couldn't find which state to route input to");
    }

    private void processInputForOptionsMenu(GFGInputAction action) {
        // todo
    }

    private void processInputForCutscene(GFGInputAction action) {
        switch(action) {
            case GFGInputAction.SELECT:
                CutsceneStartStopper.Instance.playableDirector.Play();
                Debug.Log("[NonMouseInputManager] Action: SELECT");
                break;
            case GFGInputAction.CUTSCENE_SKIP:
                Debug.Log("[NonMouseInputManager] Action: CUTSCENE_SKIP");
                gameState.LoadNextLocation();
                break;
            default:
                Debug.Log("[NonMouseInputManager] UNIMPLEMENTED ACTION: " + action);
                break;
        }
    }

    private void processInputSimple(GFGInputAction action, List<Hoverable> hoverableSubset = null) {
        List<Hoverable> subset = hoverableSubset == null ? allHoverables : hoverableSubset;
        switch(action) {
            case GFGInputAction.UP:
                hoverInDirection(Vector2.up, subset);
                Debug.Log("[NonMouseInputManager] Action: UP, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.DOWN:
                hoverInDirection(Vector2.down, subset);
                Debug.Log("[NonMouseInputManager] Action: DOWN, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.LEFT:
                hoverInDirection(Vector2.left, subset); 
                Debug.Log("[NonMouseInputManager] Action: LEFT, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.RIGHT:
                hoverInDirection(Vector2.right, subset); 
                Debug.Log("[NonMouseInputManager] Action: RIGHT, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.SELECT:
                currentlyHovered.onSelect();
                Debug.Log("[NonMouseInputManager] Action: SELECT");
                break;
            case GFGInputAction.BACK:
                Debug.Log("[NonMouseInputManager] Action: BACK");
                break;
            case GFGInputAction.END_TURN:
                Debug.Log("[NonMouseInputManager] Action: END_TURN");
                break;
            case GFGInputAction.OPEN_COMPANION_1_DRAW:
                Debug.Log("[NonMouseInputManager] Action: OPEN_COMPANION_1_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_2_DRAW:
                Debug.Log("[NonMouseInputManager] Action: OPEN_COMPANION_2_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_3_DRAW:
                Debug.Log("[NonMouseInputManager] Action: OPEN_COMPANION_3_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_4_DRAW:
                Debug.Log("[NonMouseInputManager] Action: OPEN_COMPANION_4_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_5_DRAW:
                Debug.Log("[NonMouseInputManager] Action: OPEN_COMPANION_5_DRAW");
                break;
            default:
                Debug.Log("[NonMouseInputManager] UNIMPLEMENTED ACTION: " + action);
                break;
        }
    }

    private void processInputForCombat(GFGInputAction action) {
        switch(GetUIState()) {
            case UIState.DEFAULT:
                processInputForDefaultStateCombat(action);
                break;
            case UIState.EFFECT_TARGETTING:
                processInputForEffectTargettingState(action);
                break;
            case UIState.CARD_SELECTION_DISPLAY:
                processInputForDefaultStateCombat(action, filterHoverablesByHoverableType(HoverableType.CardSelection, allHoverables));
                break;
            default:
                Debug.Log("[NonMouseInputManager] Can't yet process input for state: " + UIStateManager.Instance.currentState);
                break;
        }
    }

    private List<Hoverable> filterHoverablesByHoverableType(HoverableType hoverableType, List<Hoverable> candidates) {
        List<Hoverable> filtered = new List<Hoverable>();
        foreach(Hoverable candidate in candidates) {
            if(candidate.hoverableType == hoverableType) {
                filtered.Add(candidate);
            }
        }
        return filtered;
    }

    // cardInShopWith.sourcecompanion price may be null, in which case it's and "ANY" card
    private List<Hoverable> filterHoverablesByApplicableCompanionType(CardInShopWithPrice cardInShopWithPrice, List<Hoverable> candidates) {
        List<Hoverable> filtered = new List<Hoverable>();
        foreach(Hoverable candidate in candidates) {
            if(candidate.hoverableType == HoverableType.CompanionManagement) {
                if(cardInShopWithPrice.sourceCompanion == null) {
                    filtered.Add(candidate);
                } else if(candidate.companionTypeSO == cardInShopWithPrice.sourceCompanion) {
                    filtered.Add(candidate);
                }

            }
        }
        return filtered;

    }
    private List<Hoverable> filterHoverablesByTargetType(List<Targetable.TargetType> targetTypes, List<Hoverable> candidates) {
        List<Hoverable> filtered = new List<Hoverable>();
        foreach(Hoverable candidate in candidates) {
            if(targetTypes.Contains(candidate.GetTargetType())) {
                filtered.Add(candidate);
            }
        }
        return filtered;
    }

    private List<Hoverable> filterOutHoverablesByTypes(List<HoverableType> typesToFilterOut, List<Hoverable> candidates) {
        List<Hoverable> filtered = new List<Hoverable>();
        foreach (Hoverable candidate in candidates) {
            if (!typesToFilterOut.Contains(candidate.hoverableType)) {
                filtered.Add(candidate);
            }
        }
        return filtered;
    }

    private void processInputForShop(GFGInputAction action) {
        List<HoverableType> typesToFilterOut = new List<HoverableType> { HoverableType.SellingCompanion, HoverableType.UpgradingCompanion };
        List<Hoverable> filteredHoverables = filterOutHoverablesByTypes(typesToFilterOut, allHoverables);

        switch(uiState) {
            case UIState.DEFAULT:
                processInputForShopDefaultState(action, filteredHoverables);
                break;
            case UIState.DRAGGING_COMPANION:
                processInputForShopDraggingCompanionState(action);
                break;
            case UIState.CARD_SELECTION_DISPLAY:
                processInputForShopDefaultState(action,
                    filterHoverablesByHoverableType(HoverableType.CardSelection, filteredHoverables));
                break;
            case UIState.PURCHASING_CARD:
                Debug.Log("[NonMouseInputManager] processing input for state: PURCHASING_CARD");
                processInputForShopDefaultState(action, 
                    filterHoverablesByApplicableCompanionType(cardPurchasingFor, filteredHoverables));
                break;
            case UIState.SELLING_COMPANION:
                processInputForShopDefaultState(action,
                    filterHoverablesByHoverableType(HoverableType.SellingCompanion, allHoverables));
                break;
            case UIState.UPGRADING_COMPANION:
                processInputForShopDefaultState(action,
                    filterHoverablesByHoverableType(HoverableType.UpgradingCompanion, allHoverables));
                break;
            case UIState.REMOVING_CARD:
                processInputForShopDefaultState(action,
                    filterHoverablesByHoverableType(HoverableType.CompanionManagement, filteredHoverables));
                break;
            default:
                Debug.LogError("[NonMouseInputManager] I'm in the shop so I can't process input for state: " + uiState);
                break;
        }
    }

    private void processInputForShopDefaultState(GFGInputAction action, List<Hoverable> hoverableSubset = null) {
        List<Hoverable> subset = hoverableSubset == null ? allHoverables : hoverableSubset;
        switch(action) {
            case GFGInputAction.UP:
                hoverInDirection(Vector2.up, subset);
                Debug.Log("[NonMouseInputManager] State: SHOP, Action: UP, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.DOWN:
                hoverInDirection(Vector2.down, subset);
                Debug.Log("[NonMouseInputManager] State: SHOP, Action: DOWN, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.LEFT:
                hoverInDirection(Vector2.left, subset); 
                Debug.Log("[NonMouseInputManager] State: SHOP, Action: LEFT, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.RIGHT:
                hoverInDirection(Vector2.right, subset); 
                Debug.Log("[NonMouseInputManager] State: SHOP, Action: RIGHT, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.SELECT:
                currentlyHovered.onSelect();
                Debug.Log("[NonMouseInputManager] State: SHOP, Action: SELECT");
                break;
            case GFGInputAction.BACK:
                Debug.Log("[NonMouseInputManager] State: SHOP, Action: BACK");
                break;
            case GFGInputAction.END_TURN:
                Debug.Log("[NonMouseInputManager] State: SHOP, Action: END_TURN");
                break;
            default:
                Debug.Log("[NonMouseInputManager] State: SHOP, UNIMPLEMENTED Action: " + action);
                break;
        }

    }

    public void CompanionDragACTIVATE(CompanionManagementView companionView, 
        ICompanionManagementViewDelegate viewDelegate){
        companionManagementView = companionView;
        companionManagementViewDelegate = viewDelegate;
        // viewDelegate.CompanionManagementOnPointerDown(companionView, null, currentlyHoveredScreenPosUiDoc());
        SetUIState(UIState.DRAGGING_COMPANION);
    }

    private void moveDraggedCompanionToCurrentHoverable() {
        if(companionManagementViewDelegate == null) {
            Debug.LogError("[NonMouseInputManager] State: SHOP, DRAGGING_COMPANION, but no delegate found");
            return;
        } else {
            // companionManagementViewDelegate.CompanionManagementOnPointerMove(companionManagementView, 
            //     null, 
            //     currentlyHoveredScreenPosUiDoc());
        }
    }

    private void processInputForShopDraggingCompanionState(GFGInputAction action) {
        List<Hoverable> companionManagementSlots = filterHoverablesByHoverableType(HoverableType.CompanionManagement, allHoverables);
        switch(action) {
            case GFGInputAction.UP:
                hoverInDirection(Vector2.up, companionManagementSlots);
                moveDraggedCompanionToCurrentHoverable();
                Debug.Log("[NonMouseInputManager] State: SHOP, DRAGGING_COMPANION, Action: UP");
                break;
            case GFGInputAction.DOWN:
                hoverInDirection(Vector2.down, companionManagementSlots);
                moveDraggedCompanionToCurrentHoverable();
                Debug.Log("[NonMouseInputManager] State: SHOP, DRAGGING_COMPANION, Action: DOWN");
                break;
            case GFGInputAction.LEFT:
                hoverInDirection(Vector2.left, companionManagementSlots);
                moveDraggedCompanionToCurrentHoverable();
                Debug.Log("[NonMouseInputManager] State: SHOP, DRAGGING_COMPANION, Action: LEFT");
                break;
            case GFGInputAction.RIGHT:
                hoverInDirection(Vector2.right, companionManagementSlots);
                moveDraggedCompanionToCurrentHoverable();
                Debug.Log("[NonMouseInputManager] State: SHOP, DRAGGING_COMPANION, Action: RIGHT");
                break;
            case GFGInputAction.SELECT:
                // companionManagementViewDelegate.ComapnionManagementOnPointerUp(companionManagementView, null, currentlyHoveredScreenPosUiDoc());
                Debug.Log("[NonMouseInputManager] State: SHOP, DRAGGING_COMPANION, Action: SELECT");
                break;
            case GFGInputAction.BACK:
                Debug.Log("[NonMouseInputManager] State: SHOP, DRAGGING_COMPANION, Action: BACK");
                break;
            default:
                Debug.Log("[NonMouseInputManager] State: SHOP, DRAGGING_COMPANION, UNIMPLEMENTED Action: " + action);
                break;
        }
    }
    private void processInputForDefaultStateCombat(GFGInputAction action, List<Hoverable> hoverableSubset = null) {
        List<Hoverable> subset = hoverableSubset == null ? allHoverables : hoverableSubset;
        switch(action) {
            case GFGInputAction.UP:
                hoverInDirection(Vector2.up, subset);
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: UP, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.DOWN:
                hoverInDirection(Vector2.down, subset);
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: DOWN, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.LEFT:
                hoverInDirection(Vector2.left, subset); 
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: LEFT, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.RIGHT:
                hoverInDirection(Vector2.right, subset); 
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: RIGHT, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case GFGInputAction.SELECT:
                if(currentlyHovered != null)
                    currentlyHovered.onSelect();
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: SELECT");
                break;
            case GFGInputAction.BACK:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: BACK");
                break;
            case GFGInputAction.END_TURN:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: END_TURN");
                break;
            case GFGInputAction.OPEN_COMPANION_1_DRAW:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: OPEN_COMPANION_1_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_2_DRAW:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: OPEN_COMPANION_2_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_3_DRAW: 
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: OPEN_COMPANION_3_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_4_DRAW: 
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: OPEN_COMPANION_4_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_5_DRAW: 
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: OPEN_COMPANION_5_DRAW");
                break;
            default:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, UNIMPLEMENTED Action: " + action);
                break;
        }
    }

    private void processInputForEffectTargettingState(GFGInputAction action) {
        if(UIStateManager.Instance.currentState != UIState.EFFECT_TARGETTING) {
            Debug.LogError("[NonMouseInputManager] State: EFFECT_TARGETTING, but UIStateManager is not in that state");
            // Do this anyways to keep the game mfrom softlocking
            processInputForDefaultStateCombat(action);
            return;
        }
        switch(action) {
            case GFGInputAction.UP:
                hoverInDirection(Vector2.up, currentTargetFilteredHoverables);
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: UP");
                break;
            case GFGInputAction.DOWN:
                hoverInDirection(Vector2.down, currentTargetFilteredHoverables);

                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: DOWN");
                break;
            case GFGInputAction.LEFT:
                hoverInDirection(Vector2.left, currentTargetFilteredHoverables);

                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: LEFT");
                break;
            case GFGInputAction.RIGHT:
                hoverInDirection(Vector2.right, currentTargetFilteredHoverables);
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: RIGHT");
                break;
            case GFGInputAction.SELECT:
                currentlyHovered.onSelect();
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: SELECT");
                break;
            case GFGInputAction.BACK:
                UIStateManager.Instance.TryCancelTargetting();
                hover(lastHoveredCard);
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: BACK");
                break;
            case GFGInputAction.END_TURN:
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: END_TURN");
                break;
            case GFGInputAction.OPEN_COMPANION_1_DRAW:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: OPEN_COMPANION_1_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_2_DRAW:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: OPEN_COMPANION_2_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_3_DRAW:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: OPEN_COMPANION_3_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_4_DRAW:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: OPEN_COMPANION_4_DRAW");
                break;
            case GFGInputAction.OPEN_COMPANION_5_DRAW:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: OPEN_COMPANION_5_DRAW");
                break;
            default:

                Debug.Log("[NonMouseInputManager] UNIMPLEMENTED ACTION State: EFFECT_TARGETTING, Action: " + action);
                break;
        }
    }

    public void SetValidTargets(List<Targetable.TargetType> validTargets) {
        currentValidTargets = validTargets;
        currentTargetFilteredHoverables = filterHoverablesByTargetType(validTargets, allHoverables);
        if(currentTargetFilteredHoverables.Count > 0) {
            hover(currentTargetFilteredHoverables[0]);
        }
    }

    private IEnumerable startTurnTrigger(){
        hoverACard();
        yield return null;
    }

    // If currently finishing casting card is eligible, it could get hovered
    // not what we want, cuz it's about to get destroyed. So explicitly exclude it
    public void hoverACard(List<PlayableCard> exclude = null){
        List<Hoverable> cards = filterHoverablesByEntityType(EntityType.Card, allHoverables);
        // filter cards by whether they're playable
        List<PlayableCard> playableCards = new List<PlayableCard>();
        foreach(Hoverable card in cards) {
            PlayableCard playableCard = card.GetComponent<PlayableCard>();
            if(playableCard != null 
                && (exclude == null || !exclude.Contains(playableCard))
                && playableCard.card.cardType.playable) {
                playableCards.Add(playableCard);
            }
        }

        // filter cards by whether we have enough mana to play them
        List<PlayableCard> affordableCards = new List<PlayableCard>();
        foreach(PlayableCard card in playableCards) {
            if(card.card.GetManaCost() <= ManaManager.Instance.currentMana) {
                affordableCards.Add(card);
            }
        }
        if(affordableCards.Count > 0) {
           Debug.Log("[NonMouseInputManager] Found a playable and affordable card, hovering card: " + affordableCards[0].name);
            hover(affordableCards[0].GetComponent<Hoverable>());
        }
    }


    public Vector2 currentlyHoveredScreenPosition() {
        if(currentlyHovered != null) {
            return currentlyHovered.getScreenPosition();
        } else {
            return Vector2.zero;
        }
    }

    public Vector2 currentlyHoveredScreenPosUiDoc() {
        if(currentlyHovered == null) {
            return Vector2.zero;
        }
        Vector2 screenPos = currentlyHovered.getScreenPosition();
        return new Vector2(screenPos.x, Screen.height - screenPos.y);
    }

    public UIState GetUIState() {
        // gotta go fast zoom zoom 
        if(gameState.currentLocation == Location.INTRO_CUTSCENE || gameState.currentLocation == Location.MAIN_MENU || gameState.currentLocation == Location.TEAM_SIGNING) {
            return uiState;
        }
        if (gameState.activeEncounter.GetValue().getEncounterType() == EncounterType.Enemy) {
            return UIStateManager.Instance.currentState;
        } else {
            return uiState;
        }
    }

    public void SetUIState(UIState newState) {
        if(gameState.activeEncounter.GetValue().getEncounterType() != EncounterType.Shop) {
            UIStateManager.Instance.setState(newState);
        } else {
            // lol get special cased nerd
            if(newState == UIState.CARD_SELECTION_DISPLAY && uiState == UIState.UPGRADING_COMPANION) {
                inDeckPreview = true;
            }
            if(newState == UIState.DEFAULT && inDeckPreview) {
                inDeckPreview = false;
                newState = UIState.UPGRADING_COMPANION;
            }
            uiState = newState;
        }
    }

    public void SetPurchasingCard(CardInShopWithPrice cardInShopWithPrice) {
        cardPurchasingFor = cardInShopWithPrice;
        uiState = UIState.PURCHASING_CARD;
    }

    public void UnSetPurchasingCard() {
        cardPurchasingFor = null;
        uiState = UIState.DEFAULT;
    }

    public void hoverACardSelection() {
        List<Hoverable> cards = filterHoverablesByHoverableType(HoverableType.CardSelection, allHoverables);
        if(cards.Count > 0) {
            hover(cards[0]);
        }
    }

    public Hoverable GetHoverableForElement(VisualElement element) {
        foreach (Hoverable hoverable in allHoverables) {
            if (hoverable.associatedUIDocElement == element) {
                return hoverable;
            }
        }
        return null;
    }

    public void UpdateCurrentlyHovered() {
        if (currentlyHovered != null) {
            Hoverable newHoverable = GetHoverableForElement(currentlyHovered.associatedUIDocElement);
            if (newHoverable != null) {
                hover(newHoverable);
            }
        }
    }

    public void CallIfHoverableNotNextHovered(Action action, List<Hoverable> hoverables) {
        StartCoroutine(CallIfHoverableNotNextHoveredCorout(action,hoverables));
    }

    private IEnumerator CallIfHoverableNotNextHoveredCorout(Action action, List<Hoverable> hoverables) {
        Debug.Log("[NonMouseInputManager] Starting CallIfHoverableNotNextHoveredCorout");
        Hoverable startHoverable = currentlyHovered;
        yield return new WaitUntil(() => (currentlyHovered != startHoverable) && (currentlyHovered != null));
        
        Debug.Log("[NonMouseInputManager] Hoverables we wont' call for: " + string.Join(", ", hoverables) + ", Currently Hovered: " + currentlyHovered + ", Start Hoverable: " + startHoverable + " contains? " + hoverables.Contains(currentlyHovered));
        if(!hoverables.Contains(currentlyHovered)) {
            action.Invoke();
        }
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        UnityEngine.Cursor.visible = false;
        inputMethod = InputMethod.Keyboard;
        if(uiState == UIState.OPTIONS_MENU) {
            processInputForOptionsMenu(action);
            return;
        }
        // NOTE we can't actually get a CUTSCENE_SKIP action unless we're in a cutscene
        // we stuff it at the input manager level because if you spam the button our gamestate tries 
        // and fails to skip multiple scenes. 
        // Location changes instantly, but the scene load can't keep up 
        if(gameState.currentLocation == Location.INTRO_CUTSCENE) {
            processInputForCutscene(action);
            return;
        }
        if(gameState.currentLocation == Location.POST_COMBAT) {
            processInputSimple(action, filterHoverablesByHoverableType(HoverableType.PostCombat, allHoverables));
            return;
        }
        if(gameState.currentLocation == Location.MAIN_MENU || gameState.currentLocation == Location.TEAM_SIGNING || gameState.currentLocation == Location.TUTORIAL || gameState.currentLocation == Location.SHOP_TUTORIAL) { 
            processInputSimple(action);
            return;
        }
        if(gameState.activeEncounter.GetValue().getEncounterType() == EncounterType.Shop) {
            processInputForShop(action);
            return;
        } else if (gameState.activeEncounter.GetValue().getEncounterType() == EncounterType.Enemy) {
            processInputForCombat(action);
            return;
        } 
        Debug.LogError("Couldn't find which state to route input to");
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        throw new NotImplementedException();
    }
}