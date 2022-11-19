using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyEffectEventInfo {
    public EnemyEffectName effectName;
    public int scale;
    public List<string> targets;

    public EnemyEffectEventInfo(EnemyEffectName effectName, int scale, List<string> targetIds){
        this.effectName = effectName;
        this.scale = scale;
        this.targets = targetIds;
    }

}
[CreateAssetMenu(
    fileName = "EnemyEffectEvent", 
    menuName = "Events/Game Event/Enemy Effect Event")]
public class EnemyEffectEvent : BaseGameEvent<EnemyEffectEventInfo> { }
