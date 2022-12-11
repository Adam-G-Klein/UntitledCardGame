using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyInstantiatedEventInfo {

    public EnemyInstance enemyInstance;

    public EnemyInstantiatedEventInfo(EnemyInstance enemyInstance) {
        this.enemyInstance = enemyInstance;
    }
}
[CreateAssetMenu(
    fileName = "EnemyInstantiatedEvent", 
    menuName = "Events/Game Event/Enemy Instantiated Event")]
public class EnemyInstantiatedEvent : BaseGameEvent<EnemyInstantiatedEventInfo> { }
