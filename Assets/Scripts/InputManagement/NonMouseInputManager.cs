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
    private List<Hoverable> hoverables = new List<Hoverable>();
    private Hoverable currentlyHovered;

    void Update() {
    }

    public void ClearHoverState() {
        currentlyHovered.onUnhover();
        currentlyHovered = null;
    }

    public void RegisterHoverable(Hoverable hoverable) {
        hoverables.Add(hoverable);
    }   

    public void UnregisterHoverable(Hoverable hoverable) {
        hoverables.Remove(hoverable);
    }

    /*
    Find the closest hoverable in the direction provided
    Directions:
           0,1 
    -1, 0 ----- 1, 0
          -1, 0

    */

    private void hover(Vector2 direction) {
        if(hoverables.Count <= 0) return;
        // handle the base case, where nothing is currently hovered
        // for now, just hover the first element that registered
        // TODO: Make this a card, or the middle of the hand
        // Or have an element register as the default hover? Like in the shop, 
        // should probably be a the left-most shop card
        if(currentlyHovered == null) {
            currentlyHovered = hoverables[0];
            currentlyHovered.onHover();
            return;
        }
        
        List<Hoverable> candidates = new List<Hoverable>();
        foreach(Hoverable hoverable in hoverables) {

        }
    }

    private List<Hoverable> filterHoverablesByDirectionFromCurrent(Vector2 direction) {
        List<Hoverable> candidates = new List<Hoverable>();
        foreach(Hoverable hoverable in hoverables) {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(hoverable.transform.position);
            Vector2 hoverablePos = new Vector2(screenPos.x, screenPos.y);
            Vector2 currentPos = new Vector2(Screen.width / 2, Screen.height / 2);
            Vector2 hoverableDirection = hoverablePos - currentPos;
            // Ty copilot for remembering the linear algebra that I could not :')
            if(Vector2.Dot(hoverableDirection, direction) > 0) {
                candidates.Add(hoverable);
            }
        }
        return candidates;
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

    private void processInputForDefaultState(InputAction action) {
        switch(action) {
            case InputAction.UP:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: UP, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case InputAction.DOWN:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: DOWN, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case InputAction.LEFT:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: LEFT, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case InputAction.RIGHT:
                Debug.Log("[NonMouseInputManager] State: DEFAULT, Action: RIGHT, hoveredCardIndex: " + hoveredCardIndex);
                break;
            case InputAction.SELECT:
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