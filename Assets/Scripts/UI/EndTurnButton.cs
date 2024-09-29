using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(TurnPhaseEventListener))]
public class EndTurnButton : MonoBehaviour
{
    [SerializeField] 
    private TurnPhaseEvent turnPhaseEvent;
    private bool endTurnButtonEnabled = true;
    private Button button;

    void Start() {
        button = GetComponent<Button>();
    }

    public void endTurnButtonHandler() {
        if(endTurnButtonEnabled)
            StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.BEFORE_END_PLAYER_TURN)));
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info)
    {
        if(info.newPhase == TurnPhase.PLAYER_TURN)
        {
            button.interactable = true;
            endTurnButtonEnabled = true;
        }
        else
        {
            button.interactable = false;
            endTurnButtonEnabled = false;
        }

    }
    
}