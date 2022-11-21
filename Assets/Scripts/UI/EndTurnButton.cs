using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class EndTurnButton : MonoBehaviour
{
    [SerializeField] 
    private TurnPhaseEvent turnPhaseEvent;

    public void onClick()
    {
        turnPhaseEvent.Raise(new TurnPhaseEventInfo(TurnPhase.END_PLAYER_TURN));
    }



}
