using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIStateEventListener))]
[RequireComponent(typeof(EffectTargetRequestEventListener))]
[RequireComponent(typeof(EffectTargetSuppliedEventListener))]
[RequireComponent(typeof(CardCastEventListener))]
public class UIStateManager : MonoBehaviour
{
    public UIState currentState;
    [SerializeField]
    private UIStateEvent uiStateEvent;

    // A flag to prevent cancelling a cast between targetted effects
    // Because we currently execute effects as they're targetted, and 
    // we're not casting them all at once at the end of the card
    private bool targetSelectedForCurrentCast = false;
    // Start is called before the first frame update
    void Update()
    {
        if(Input.GetKey(KeyCode.Q) 
            && currentState == UIState.EFFECT_TARGETTING
            && !targetSelectedForCurrentCast){
            cancelTargetting();
        }
    }

    private void cancelTargetting(){
        StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.DEFAULT)));
        targetSelectedForCurrentCast = false;
    }

    public void uiStageChangeEventHandler(UIStateEventInfo info) {
        Debug.Log("UI State Change Event Handler new state: " + info.newState);
        currentState = info.newState;
    }

    public void effectTargetSuppliedEventHandler(EffectTargetSuppliedEventInfo info) {
        targetSelectedForCurrentCast = true;
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info) {
        StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.EFFECT_TARGETTING)));
    }

    public void cardCastEventListener(CardCastEventInfo info) {
        StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.DEFAULT)));
        targetSelectedForCurrentCast = false;
    }

}
