using UnityEngine;

[RequireComponent(typeof(TurnPhaseEventListener))]
public class HotkeyManager : GenericSingleton<HotkeyManager> {

    [SerializeField]
    private TurnPhaseEvent turnPhaseEvent;
    public bool endTurnHotkeyEnabled = false;

    void Update() {
        if(endTurnHotkeyEnabled && Input.GetKeyDown(KeyCode.LeftShift)) {
            EndTurn();
        }
    }

    public void EndTurn() {
        StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.BEFORE_END_PLAYER_TURN)));
    }
    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info)
    {
        endTurnHotkeyEnabled = info.newPhase == TurnPhase.PLAYER_TURN;
    }
}