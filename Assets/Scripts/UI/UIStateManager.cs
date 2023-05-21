using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIStateEventListener))]
[RequireComponent(typeof(EffectTargetRequestEventListener))]
[RequireComponent(typeof(EffectTargetSuppliedEventListener))]
[RequireComponent(typeof(CardCastEventListener))]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class UIStateManager : MonoBehaviour
{
    // huge hack to fix the purge card issue, but I know targetting
    // changing soon anyways
    public static UIStateManager Instance { get; private set; }
    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }
    public UIState currentState;
    [SerializeField]
    private UIStateEvent uiStateEvent;
    public bool targettingCancellable = true; 

    // Start is called before the first frame update
    void Update()
    {
        if(Input.GetMouseButtonDown(1) 
            && currentState == UIState.EFFECT_TARGETTING
            && targettingCancellable){
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
