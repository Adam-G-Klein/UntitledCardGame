using UnityEngine;

[System.Serializable]
public class CombatEntityInstantiatedEventInfo {

    public CombatEntityInstance instance;

    public CombatEntityInstantiatedEventInfo(CombatEntityInstance instance) {
        this.instance = instance;
    }
}

[CreateAssetMenu(
    fileName = "NewCombatEntityInstantiatedEvent", 
    menuName = "Events/Combat Entity/Combat Entity Instantiated Event")]
public class CombatEntityInstantiatedEvent : BaseGameEvent<CombatEntityInstantiatedEventInfo> { }
