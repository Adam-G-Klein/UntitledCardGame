using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Combine with CardEffectEventInfo
[System.Serializable]
public class EnemyEffectEventInfo {
    public Dictionary<StatusEffect, int> statusEffects;
    public List<TargettableEntity> targets;
    public int damage;
    public EnemyIntent intent;

    public EnemyEffectEventInfo(EnemyIntent intent) {
        this.intent = intent;
        this.targets = intent.targets;
        this.damage = intent.damage;
        this.statusEffects = intent.statusEffects;
    }

}
[CreateAssetMenu(
    fileName = "EnemyEffectEvent", 
    menuName = "Events/Game Event/Enemy Effect Event")]
public class EnemyEffectEvent : BaseGameEvent<EnemyEffectEventInfo> { }
