using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public enum InputAction {
    UP,
    DOWN,
    LEFT,
    RIGHT,
    SELECT,
    BACK,
    END_TURN,
    OPEN_COMPANION_1_DRAW,
    OPEN_COMPANION_2_DRAW,
    OPEN_COMPANION_3_DRAW,
    OPEN_COMPANION_4_DRAW,
    OPEN_COMPANION_5_DRAW,
}

// Temp name, just being extra explicit what functionality this class is for
// Using it to be able to play the game with just a keyboard, as a precursor 
// to using a controller
// brainstorming doc: https://docs.google.com/document/d/1-HkEMw6Go4Lw7kFVd3A6gOE2FjSb1qToeDbyvLiJyzc/edit?tab=t.0
public class NonMouseInputManager : GenericSingleton<NonMouseInputManager> {

    [SerializeField]
    private int hoveredCardIndex = -1;
    public PlayableCard hoveredCard;
    private List<Hoverable> hoverables = new List<Hoverable>();

    void Update() {
    }

    public void ClearHoverState() {
        foreach(PlayableCard card in PlayerHand.Instance.cardsInHand) {
            card.OnPointerExit(null);
        }
        hoveredCardIndex = -1;
    }

    public void RegisterHoverable(Hoverable hoverable) {
        hoverables.Add(hoverable);
    }   

    public void UnregisterHoverable(Hoverable hoverable) {
        hoverables.Remove(hoverable);
    }

    public void ProcessInput(InputAction action) {
        Cursor.visible = false;
        switch(UIStateManager.Instance.currentState) {
            case UIState.DEFAULT:
                processInputForDefaultState(action);
                break;
            case UIState.EFFECT_TARGETTING:
                processInputForEffectTargettingState(action);
                break;
            default:
                Debug.Log("[NonMouseInputManager] Can't yet process input for state: " + UIStateManager.Instance.currentState);
                break;
        }

    }

    // card_hovered should probably be it's own UI state at some point soon
    // too lazy to do it rn
    private void hoverCard(int cardToHover = 0) {
        if(PlayerHand.Instance.cardsInHand.Count <= 0) return;
        if(hoveredCard && cardToHover == hoveredCardIndex) return; // already hovering the card
        if(hoveredCard) hoveredCard.OnPointerExit(null);
        hoveredCardIndex = cardToHover;
        hoveredCard = PlayerHand.Instance.cardsInHand[hoveredCardIndex];
        if(hoveredCard.hovered) return; // oneeee more check because I found an edge case I couldn't reproduce
        hoveredCard.OnPointerEnter(null);
    }

    private void unHoverCardWhileMaintainingState() {
        if(hoveredCard) {
            hoveredCard.OnPointerExit(null);
            hoveredCard = null;
        } 
    }

    private void processInputForDefaultState(InputAction action) {
        switch(action) {
            case InputAction.UP:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: UP, hoveredCardIndex: " + hoveredCardIndex);
                hoverCard(hoveredCardIndex == -1 ? 0 : hoveredCardIndex);
                break;
            case InputAction.DOWN:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: DOWN, hoveredCardIndex: " + hoveredCardIndex);
                unHoverCardWhileMaintainingState();
                break;
            case InputAction.LEFT:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: LEFT, hoveredCardIndex: " + hoveredCardIndex);
                if(hoveredCardIndex == -1) {
                    hoverCard(0);
                } else if (hoveredCardIndex > 0) {
                    hoverCard(hoveredCardIndex - 1);
                } else {
                    hoverCard(PlayerHand.Instance.cardsInHand.Count - 1);
                }
                break;
            case InputAction.RIGHT:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: RIGHT, hoveredCardIndex: " + hoveredCardIndex);
                if(hoveredCardIndex == -1) {
                    hoverCard(PlayerHand.Instance.cardsInHand.Count - 1);
                } else if (hoveredCardIndex < PlayerHand.Instance.cardsInHand.Count - 1) {
                    hoverCard(hoveredCardIndex + 1);
                } else {
                    hoverCard(0);
                }
                break;
            case InputAction.SELECT:
                hoverCard();
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: SELECT");
                break;
            case InputAction.BACK:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: BACK");
                break;
            case InputAction.END_TURN:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: END_TURN");
                break;
            case InputAction.OPEN_COMPANION_1_DRAW:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: OPEN_COMPANION_1_DRAW");
                break;
            case InputAction.OPEN_COMPANION_2_DRAW:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: OPEN_COMPANION_2_DRAW");
                break;
            case InputAction.OPEN_COMPANION_3_DRAW: 
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: OPEN_COMPANION_3_DRAW");
                break;
            case InputAction.OPEN_COMPANION_4_DRAW: 
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: OPEN_COMPANION_4_DRAW");
                break;
            case InputAction.OPEN_COMPANION_5_DRAW: 
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: OPEN_COMPANION_5_DRAW");
                break;
            default:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, UNIMPLEMENTED Action: " + action);
                break;
        }
    }

    private void processInputForEffectTargettingState(InputAction action) {
        switch(action) {
            case InputAction.UP:
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: UP");
                break;
            case InputAction.DOWN:

                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: DOWN");
                break;
            case InputAction.LEFT:

                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: LEFT");
                break;
            case InputAction.RIGHT:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: RIGHT");
                break;
            case InputAction.SELECT:

                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: SELECT");
                break;
            case InputAction.BACK:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: BACK");
                break;
            case InputAction.END_TURN:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: END_TURN");
                break;
            case InputAction.OPEN_COMPANION_1_DRAW:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: OPEN_COMPANION_1_DRAW");
                break;
            case InputAction.OPEN_COMPANION_2_DRAW:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: OPEN_COMPANION_2_DRAW");
                break;
            case InputAction.OPEN_COMPANION_3_DRAW:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: OPEN_COMPANION_3_DRAW");
                break;
            case InputAction.OPEN_COMPANION_4_DRAW:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: OPEN_COMPANION_4_DRAW");
                break;
            case InputAction.OPEN_COMPANION_5_DRAW:
            
                Debug.Log("[NonMouseInputManager] State: EFFECT_TARGETTING, Action: OPEN_COMPANION_5_DRAW");
                break;
            default:

                Debug.Log("[NonMouseInputManager] UNIMPLEMENTED ACTION State: EFFECT_TARGETTING, Action: " + action);
                break;
        }
    }
}