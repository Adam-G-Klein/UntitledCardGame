using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnPhase {
    START_ENCOUNTER,
    START_PLAYER_TURN,
    PLAYER_TURN,
    BEFORE_END_PLAYER_TURN,
    END_PLAYER_TURN,
    START_ENEMY_TURN,
    ENEMIES_TURN,
    END_ENEMY_TURN,
    END_ENCOUNTER
}
[System.Serializable]
public class TurnPhaseEventInfo {
    public TurnPhase newPhase;

    public TurnPhaseEventInfo (TurnPhase newPhase){
        this.newPhase = newPhase;
    }
}
[CreateAssetMenu(
    fileName = "TurnPhaseEvent", 
    menuName = "Events/Game Event/Turn Phase Event")]
public class TurnPhaseEvent : BaseGameEvent<TurnPhaseEventInfo> { }
