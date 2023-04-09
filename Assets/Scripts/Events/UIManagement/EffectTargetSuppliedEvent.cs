using UnityEngine;


[System.Serializable]
public class EffectTargetSuppliedEventInfo {
    public TargettableEntity target;

    public EffectTargetSuppliedEventInfo(TargettableEntity target){
        this.target = target;
    }
}

[CreateAssetMenu(
    fileName = "NewEffectTargetSuppliedEvent",
    menuName = "Events/UI Management/Effect Target Supplied Event")]
public class EffectTargetSuppliedEvent : BaseGameEvent<EffectTargetSuppliedEventInfo> { }
