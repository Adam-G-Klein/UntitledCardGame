using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIStateEventListener))]
[RequireComponent(typeof(EffectTargetRequestEventListener))]
[RequireComponent(typeof(EffectTargetSuppliedEventListener))]
[RequireComponent(typeof(CardCastEventListener))]
[RequireComponent(typeof(TurnPhaseEventListener))]
[RequireComponent(typeof(CardSelectionRequestEventListener))]
[RequireComponent(typeof(CardSelectionRequestSuppliedEventListener))]
public class UIStateManager : MonoBehaviour
{
    public UIState currentState;
    [SerializeField]
    private UIStateEvent uiStateEvent;

    // Start is called before the first frame update
    void Update()
    {
        if(Input.GetMouseButtonDown(1) 
            && currentState == UIState.EFFECT_TARGETTING){
            cancelTargetting();
        }
    }

    private void cancelTargetting(){
        StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.DEFAULT)));
    }

    public void uiStageChangeEventHandler(UIStateEventInfo info) {
        Debug.Log("UI State Change Event Handler new state: " + info.newState);
        currentState = info.newState;
    }

    public void cardSelectionRequestEventHandler(CardSelectionRequestEventInfo info) {
        StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.CARD_SELECTION_DISPLAY)));
    }

    public void cardSelectionRequestSuppliedEventHandler(CardSelectionRequestSuppliedEventInfo info) {
        StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.DEFAULT)));
    }

    public void effectTargetSuppliedEventHandler(EffectTargetSuppliedEventInfo info) {
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info) {
        StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.EFFECT_TARGETTING)));
    }

    public void cardCastEventListener(CardCastEventInfo info) {
        StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.DEFAULT)));
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info) {
        if(info.newPhase == TurnPhase.PLAYER_TURN){
            Debug.Log("UI State Manager: Player Turn");
            StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.DEFAULT)));
        }
    }

}
