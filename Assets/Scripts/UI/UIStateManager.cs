using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIStateEventListener))]
[RequireComponent(typeof(EffectTargetRequestEventListener))]
[RequireComponent(typeof(EffectTargetSuppliedEventListener))]
public class UIStateManager : MonoBehaviour
{
    public UIState currentState;
    [SerializeField]
    private UIStateEvent uiStateEvent;
    // Start is called before the first frame update
    void Update()
    {
        if(Input.GetKey(KeyCode.Q) && currentState == UIState.EFFECT_TARGETTING){
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

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info) {
        StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.EFFECT_TARGETTING)));
    }

    public void effectTargetSuppliedEventHandler(EffectTargetSuppliedEventInfo info) {
        StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.DEFAULT)));
    }

    /*
    public void effectTargetCancelledEventHandler(EffectTargetCancelledEventInfo info) {
        StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.DEFAULT)));
    }
    */
}
