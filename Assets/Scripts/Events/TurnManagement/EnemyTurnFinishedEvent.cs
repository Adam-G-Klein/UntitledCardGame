using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Event that tells the enemy manager
// (or in the future, potentially the other enemies and the turn manager's counter)
// that this single enemy is done with its turn 
// not currently in use, just having all the enemies attack at once
// Will be needed when we want to do attack animations
[System.Serializable]
public class EnemyTurnFinishedEventInfo {
    public string id;

    public EnemyTurnFinishedEventInfo (string id){
        this.id = id;
    }
}
[CreateAssetMenu(
    fileName = "EnemyTurnFinishedEvent", 
    menuName = "Events/Game Event/Enemy Turn Finished Event")]
public class EnemyTurnFinishedEvent : BaseGameEvent<EnemyTurnFinishedEventInfo> { }
