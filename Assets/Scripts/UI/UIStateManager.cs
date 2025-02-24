using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIStateEventListener))]
[RequireComponent(typeof(CardCastEventListener))]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class UIStateManager : GenericSingleton<UIStateManager>
{
    // huge hack to fix the purge card issue, but I know targetting
    // changing soon anyways
    public static UIStateManager Instance { get; private set; }
    public UIDocumentScreenspace screenspaceDoc;
    public bool constantStateUpdate = false;
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

        screenspaceDoc = GetComponentInChildren<UIDocumentScreenspace>();
    }
    public UIState currentState;
    [SerializeField]
    private UIStateEvent uiStateEvent;
    public bool targettingCancellable = true; 

    public void SetUIDocDirty() {
        screenspaceDoc.SetStateDirty();
    }

    // Start is called before the first frame update
    void Update()
    {
        if(Input.GetMouseButtonDown(1)) {
            TryCancelTargetting();
        }
        if(constantStateUpdate){
            SetUIDocDirty();
        }
    }

    public void TryCancelTargetting() {
        if(currentState == UIState.EFFECT_TARGETTING){
            CancelContext context = new CancelContext();
            TargettingManager.Instance.cancelTargettingHandler.Invoke(context);
            if (context.canCancel) {
                setState(UIState.DEFAULT);                   
            }
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

    public void cardCastEventListener(CardCastEventInfo info) {
        setState(UIState.DEFAULT);
        EnemyEncounterManager.Instance.SetCastingCard(false);
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info) {
        if(info.newPhase == TurnPhase.PLAYER_TURN){
            Debug.Log("UI State Manager: Player Turn");
            setState(UIState.DEFAULT);
        }
    }

}
