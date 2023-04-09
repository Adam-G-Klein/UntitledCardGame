using UnityEngine;

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
