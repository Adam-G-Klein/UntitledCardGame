using UnityEngine;

[System.Serializable]
public class TargettableEntityListEventInfo {
    public TargettableEntity target;

    public TargettableEntityListEventInfo(TargettableEntity target){
        this.target = target;
    }
}

[CreateAssetMenu(
    fileName = "NewTargettableEntityListEvent",
    menuName = "Events/Combat Entity/Targettable Entity List Event")]
public class TargettableEntityListEvent : BaseGameEvent<TargettableEntityListEventInfo> { }
