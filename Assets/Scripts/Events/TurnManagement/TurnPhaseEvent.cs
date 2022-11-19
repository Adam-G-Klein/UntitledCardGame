using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnPhase {
    START_PLAYER_TURN,
    PLAYER_DRAW,
    END_PLAYER_TURN,
    START_ENEMY_TURN,
    ENEMY_ATTACK,
    END_ENEMY_TURN
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
