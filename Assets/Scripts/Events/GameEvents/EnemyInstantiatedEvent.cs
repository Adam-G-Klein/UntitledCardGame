using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyInstantiatedEventInfo {

    // EnemyInstance seems to be where we're keeping the id right now 
    // and that is the only thing I'm using this event for currently
    public EnemyInstance enemy;

    public EnemyInstantiatedEventInfo(EnemyInstance enemy) {
        this.enemy = enemy;
    }
}
[CreateAssetMenu(
    fileName = "EnemyInstantiatedEvent", 
    menuName = "Events/Game Event/Enemy Instantiated Event")]
public class EnemyInstantiatedEvent : BaseGameEvent<EnemyInstantiatedEventInfo> { }
