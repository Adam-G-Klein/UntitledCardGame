using UnityEngine;

[RequireComponent(typeof(TurnPhaseEventListener))]
public class HotkeyManager : GenericSingleton<HotkeyManager> {

    [SerializeField]
    private TurnPhaseEvent turnPhaseEvent;
    bool endTurnHotkeyEnabled = false;

    void Update() {

        if(endTurnHotkeyEnabled && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.JoystickButton3))) {
            StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.BEFORE_END_PLAYER_TURN)));
        }
    }
    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info)
    {
        endTurnHotkeyEnabled = info.newPhase == TurnPhase.PLAYER_TURN;
    }
}