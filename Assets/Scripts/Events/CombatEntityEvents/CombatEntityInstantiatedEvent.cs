using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CombatEntityInstantiatedEventInfo {

    public CombatEntityInstance instance;

    public CombatEntityInstantiatedEventInfo(CombatEntityInstance instance) {
        this.instance = instance;
    }
}
[CreateAssetMenu(
    fileName = "CombatEntityInstantiatedEvent", 
    menuName = "Events/Combat Entity/Combat Entity Instantiated")]
public class CombatEntityInstantiatedEvent : BaseGameEvent<CombatEntityInstantiatedEventInfo> { }
