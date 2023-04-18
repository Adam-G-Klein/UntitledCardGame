using UnityEngine;

// Need to combine this with instantiated event,
// and can also delete the event Info and just pass the instance
// not getting distracted by that right now though
[System.Serializable]
public class CombatEntityDeathEventInfo {

    public CombatEntityInstance instance;

    public CombatEntityDeathEventInfo(CombatEntityInstance instance) {
        this.instance = instance;
    }
}

[CreateAssetMenu(
    fileName = "NewCombatEntityDeathEvent", 
    menuName = "Events/Combat Entity/Combat Entity Death")]
public class CombatEntityDeathEvent : BaseGameEvent<CombatEntityDeathEventInfo> { }
