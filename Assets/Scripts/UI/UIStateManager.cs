using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIStateEventListener))]
[RequireComponent(typeof(EffectTargetRequestEventListener))]
[RequireComponent(typeof(EffectTargetSuppliedEventListener))]
[RequireComponent(typeof(CardCastEventListener))]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class UIStateManager : GenericSingleton<UIStateManager>
{
    public UIState currentState;
    [SerializeField]
    private UIStateEvent uiStateEvent;

    // Start is called before the first frame update
    void Update()
    {
        if(Input.GetMouseButtonDown(1) 
            && currentState == UIState.EFFECT_TARGETTING){
            setState(UIState.DEFAULT);
        }
    }

    public void setState(UIState newState) {
        StartCoroutine(uiStateEvent
            .RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(newState)));
    }

    public void uiStageChangeEventHandler(UIStateEventInfo info) {
        Debug.Log("UI State Change Event Handler new state: " + info.newState);
        currentState = info.newState;
    }

    public void effectTargetSuppliedEventHandler(EffectTargetSuppliedEventInfo info) {
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info) {
        setState(UIState.EFFECT_TARGETTING);
    }

    public void cardCastEventListener(CardCastEventInfo info) {
        setState(UIState.DEFAULT);
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info) {
        if(info.newPhase == TurnPhase.PLAYER_TURN){
            Debug.Log("UI State Manager: Player Turn");
            setState(UIState.DEFAULT);
        }
    }

}
