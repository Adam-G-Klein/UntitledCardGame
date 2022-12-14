using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(TurnPhaseEventListener))]
[RequireComponent(typeof(Button))]
public class EndTurnButton : MonoBehaviour
{
    [SerializeField] 
    private TurnPhaseEvent turnPhaseEvent;
    [SerializeField]
    private UIStateEvent uiStateEvent;
    private Button button;
    void Start() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {

            StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.END_PLAYER_TURN)));
            StartCoroutine(uiStateEvent.RaiseAtEndOfFrameCoroutine(new UIStateEventInfo(UIState.DEFAULT)));
        });
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info)
    {
        if(info.newPhase == TurnPhase.PLAYER_TURN)
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }



}
