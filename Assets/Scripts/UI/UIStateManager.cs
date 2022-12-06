using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
    I'm conflicted between using a script like this to 
    track state and just having all of the UI elements
    listen to published events
    Going with the published event route for now.
    Going to need some stuff here still
*/


public class UIStateManager : MonoBehaviour
{
    public UIState currentState;
    [SerializeField]
    private UIStateEvent uiStateEvent;
    // Start is called before the first frame update
    void Update()
    {
        if(Input.GetKey(KeyCode.Q) && currentState == UIState.EFFECT_TARGETTING){
            StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.DEFAULT)));
        }
        
    }

    public void uiStageChangeEventHandler(UIStateEventInfo info) {
        currentState = info.newState;
    }
}
